using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Text.Json.Serialization;
using RarcTools;

Console.WriteLine("Hello, World!");

bool show_debug = false;

// Open ISO Image
RarcTools.GCRebuilder.GCRebuilder.ExtractISO(@"decompimizer-GZ2E01.iso", @"extractedISO/");

var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };

// Apply DZX Patches first
// Deserialize DZX Patches file:
string dzxPatchContents = File.ReadAllText(@"Generator/patch_files/dzx_patches.jsonc");
var dzxPatches = JsonSerializer.Deserialize<List<DZXPatch>>(dzxPatchContents, options);

// Modify section
foreach (var dzxPatch in dzxPatches)
{
    string archiveDirectory = RarcTools.RARCDump.DumpArchive(
        Yaz0dec.InitYaz0Decode(@"extractedISO/root/" + dzxPatch.FilePath + ".arc")
    );

    //Console.WriteLine("archive dir: " + archiveDirectory);
    // Read dzr file into a serialized var
    string filePath = "";
    if (dzxPatch.FilePath.Contains("STG_00"))
    {
        filePath = "/dzs/stage.dzs";
    }
    else
    {
        filePath = "/dzr/room.dzr";
    }
    //Console.WriteLine("File path: " + archiveDirectory + filePath);
    var dzxContents = JsonSerializer.Deserialize<List<DZXDataBlock>>(
        LibStage.ExtractDZX(archiveDirectory + filePath)
    );

    foreach (DZXChange DZXChange in dzxPatch.Changes)
    {
        bool didPatch = false;

        // Verify that we have a type mapping for the block we are wanting to replace
        Type entryType = DZX.GetEntryType(DZXChange.DataBlock);
        if (entryType == null)
        {
            Console.WriteLine("No entry type defined for: " + DZXChange.DataBlock);
            continue;
        }

        // Loop through all of the dataBlock entries until we find the tag that we want to match
        //Console.WriteLine("looking for: " + DZXChange.DataBlock);
        int sectionIndex = dzxContents.FindIndex(
            s => DZXChange.DataBlock != null && s.Tag == DZXChange.DataBlock
        );

        switch (DZXChange.Operation)
        {
            case "add":
            {
                dzxContents[sectionIndex].Entries.Add(
                    JsonSerializer.SerializeToElement(
                        JsonSerializer.Deserialize(DZXChange.Data, entryType)
                    )
                );
                break;
            }

            case "modify":
            {
                for (int i = 0; i < dzxContents[sectionIndex].Entries.Count; i++)
                {
                    //Console.WriteLine(section.Entries[i]);
                    var entry = dzxContents[sectionIndex].Entries[i].Deserialize(entryType);

                    // Currently an entry's ID is made up of Name@x,y,z
                    string sectionID = "";

                    // For Kak Malo Mart, the ID includes the param as multiple items can be in the same place.
                    if (archiveDirectory.Contains(@"R_SP109/R03"))
                    {
                        sectionID = PatchFunctions.GetEntryParamID(
                            dzxContents[sectionIndex].Entries[i]
                        );
                    }
                    else
                    {
                        sectionID = PatchFunctions.GetEntryID(dzxContents[sectionIndex].Entries[i]);
                    }
                    // Console.WriteLine(sectionID);
                    if (sectionID == DZXChange.ID)
                    {
                        var patched = PatchFunctions.ApplyChanges(
                            dzxContents[sectionIndex].Entries[i].Deserialize(entryType),
                            DZXChange.Data
                        );
                        dzxContents[sectionIndex].Entries[i] = JsonSerializer.SerializeToElement(
                            patched
                        );

                        // Successfully patched the section, so break out.
                        if (show_debug)
                        {
                        Console.WriteLine(
                            "Successfully Applied Patch to: "
                                + archiveDirectory
                                + "-"
                                + DZXChange.ID
                        );
                        }

                        didPatch = true;
                        break;
                    }
                }
                if (!didPatch)
                {
                    // If we make it to here, we did not find an actor in the section with the specified ID
                    Console.WriteLine(
                        "Unable to find patch for: " + archiveDirectory + "-" + DZXChange.ID
                    );
                }
                break;
            }
            case "remove":
            {
                dzxContents[sectionIndex].Entries.RemoveAll(
                    e => PatchFunctions.GetEntryID(e) == DZXChange.ID
                );
                break;
            }
        }
    }

    // Re-Package the Archive
    string dzxString = JsonSerializer.Serialize(
        dzxContents,
        new JsonSerializerOptions { WriteIndented = true }
    );
    LibStage.PackageDZX(dzxString, archiveDirectory + filePath, false);
    RarcTools.RARCPacker.PackArchive(
        archiveDirectory,
        archiveDirectory[..archiveDirectory.LastIndexOf("\\")] + ".arc",
        true
    );
}

// Apply BMG Patches
// Deserialize BMG Patches file:
string bmgPatchContents = File.ReadAllText(@"Generator/patch_files/bmg_patches.jsonc");
var bmgPatches = JsonSerializer.Deserialize<List<BMGPatch>>(bmgPatchContents, options);

// Modify section
foreach (var bmgPatch in bmgPatches)
{
    //Console.WriteLine(bmgPatch.FilePath);
    string archiveDirectory = RarcTools.RARCDump.DumpArchive(
        Yaz0dec.InitYaz0Decode(
            @"extractedISO/root/"
                + bmgPatch.FilePath[..bmgPatch.FilePath.LastIndexOf("/zel")]
                + ".arc"
        )
    );

    //Console.WriteLine("archive dir: " + archiveDirectory);
    //Console.WriteLine("File path: " + bmgPatch.FilePath);
    List<BMGDataBlock> bmgContents = JsonSerializer.Deserialize<List<BMGDataBlock>>(
        BmgTools.DumpBmg(archiveDirectory + @"\" + PatchFunctions.AfterLast(bmgPatch.FilePath, '/'))
    );

    foreach (BMGChange bmgChange in bmgPatch.Changes)
    {
        bool didPatch = false;
        // Loop through all of the dataBlock entries until we find the tag that we want to match
        //Console.WriteLine("looking for: " + bmgChange.Index);
        int sectionIndex = bmgContents.FindIndex(
    s =>
        (bmgChange.Section != null && s.Section == bmgChange.Section)
        || (bmgChange.Index != null && bmgChange.Section == null && s.index?.ToLower() == bmgChange.Index.ToLower())
);

        if (sectionIndex == -1)
        {
            Console.WriteLine("Unable to find section for: " + bmgChange.Index);
            continue;
        }

        switch (bmgChange.Operation)
        {
            case "modify":
            {
                if (bmgChange.Section != null)
                {
                    if (bmgChange.Section.Contains("FLW1") && bmgChange.Index != null)
                    {
                        int index = Convert.ToInt32(bmgChange.Index, 16);
                        BMGDataBlock target = bmgContents[sectionIndex];

                        BMGDataBlock changeData = JsonSerializer.Deserialize<BMGDataBlock>(
                            JsonSerializer.Serialize(bmgChange.ChangeData)
                        )!;

                        if (changeData.Data?.flwIndexTable is not null)
                            target.Data!.flwIndexTable![index] = changeData.Data.flwIndexTable[0];

                        if (changeData.Data?.flwTable is not null)
                            target.Data!.flwTable![index] = changeData.Data.flwTable[0];

                        if (show_debug)
                        {
                        Console.WriteLine(
                            "Successfully Applied Patch to: " + archiveDirectory + "-" + bmgChange.Section + "-" + bmgChange.Index
                        );
                        }
                    }
                    else if (bmgChange.Section == "FLI1" && bmgChange.Index != null)
                    {
                        int index = Convert.ToInt32(bmgChange.Index, 16);
                        BMGDataBlock target = bmgContents[sectionIndex];
                        BMGDataBlock changeData = JsonSerializer.Deserialize<BMGDataBlock>(
                                            JsonSerializer.Serialize(bmgChange.ChangeData)
                                        )!;

                        if (changeData.RawData is not null)
                            target.RawData![index] = changeData.RawData[0];

                        if (show_debug)
                        {
                        Console.WriteLine(
                            "Successfully Applied Patch to: " + archiveDirectory + "-"+ bmgChange.Section + "-" + bmgChange.Index
                        );
                        }
                    }
                }
                else
                {
                    
                
                var patched = PatchFunctions.ApplyChanges(
                    bmgContents[sectionIndex],
                    bmgChange.ChangeData
                );
                bmgContents[sectionIndex] = (BMGDataBlock)patched;
                // Successfully patched the section, so break out.
                if (show_debug)
                {
                if (bmgChange.Index != null)
                {
                    Console.WriteLine(
                        "Successfully Applied Patch to: " + archiveDirectory + "-" + bmgChange.Index
                    );
                }
                else
                {
                    Console.WriteLine(
                        "Successfully Applied Patch to: "
                            + archiveDirectory
                            + "-"
                            + bmgChange.Section
                    );
                }
                }
                }

                /*Console.WriteLine(
                    JsonSerializer.Serialize(
                        bmgContents[sectionIndex],
                        new JsonSerializerOptions { WriteIndented = true }
                    )
                );*/
                break;
            }

            case "add":
            {
                BMGDataBlock addition = JsonSerializer.Deserialize<BMGDataBlock>(
                    JsonSerializer.Serialize(bmgChange.ChangeData)
                )!;

                BMGDataBlock target = bmgContents[sectionIndex];

                if (addition.Data?.flwTable is not null)
                {
                    target.Data!.flwTable ??= [];
                    target.Data.flwTable.AddRange(addition.Data.flwTable);
                    target.Data.flwTableCount = target.Data.flwTable.Count;
                }

                if (addition.Data?.flwIndexTable is not null)
                {
                    target.Data!.flwIndexTable ??= [];
                    target.Data.flwIndexTable.AddRange(addition.Data.flwIndexTable);
                    target.Data.flwIndexCount = target.Data.flwIndexTable.Count;
                }
                if (show_debug)
                {
                Console.WriteLine(
                            "Successfully Added Patch to: " + archiveDirectory + "-"+ bmgChange.Section
                        );
                }
                break;
            }
        }
    }

    // Re-Package the Archive
    var bmgOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
    string bmgString = JsonSerializer.Serialize(bmgContents, bmgOptions);
    BmgTools.PackBmg(
        bmgString,
        archiveDirectory + @"\" + PatchFunctions.AfterLast(bmgPatch.FilePath, '/'),
        "iso-8859-1",
        false
    );
    RarcTools.RARCPacker.PackArchive(
        archiveDirectory,
        archiveDirectory[..archiveDirectory.LastIndexOf("\\")] + ".arc",
        true
    );
}

// Copy over custom files
// Deserialize asset manifest file:
string assetManifestContents = File.ReadAllText(Path.Combine("mod_assets","manifest.jsonc"));
var assetPatches = JsonSerializer.Deserialize<List<AssetPatch>>(assetManifestContents, options);

// Loop through all manifest items and apply them appropriately
foreach(var assetPatch in assetPatches)
{
    switch(assetPatch.Operation)
    {
        case "add":
        case "modify":
            {
                // If the manifest source directory contains an archive, we need to extract it before we modify it.
                if (assetPatch.Directory.Contains(".arc"))
                {
                    string archiveDirectory = Tools.GetSubstringFromMarker(assetPatch.Directory, ".arc");
                    string extractedDirectory = RARCDump.DumpArchive(
                        Yaz0dec.InitYaz0Decode(@"extractedISO/root/" + archiveDirectory), false);
                    string fileDirectory = @"archive/" + Tools.GetSuperstringAfterMarker(assetPatch.Directory,@".arc/");
                    string newDirectory =  Path.Combine(extractedDirectory,fileDirectory,assetPatch.FileName);
                    Console.WriteLine(extractedDirectory);
                    File.Copy(Path.Combine("mod_assets", assetPatch.FileName), newDirectory, true);
                    
                    Console.WriteLine($"Successfully copied file: {assetPatch.FileName} to directory: {newDirectory}");

                    RARCPacker.PackArchive(
                        Path.Combine(extractedDirectory,"archive"),
                        extractedDirectory + ".arc",
                        true
                    );
                }
                // Otherwise, just move the custom file over, replacing anything as needed. 
                else
                {
                    string newDirectory = Path.Combine(@"extractedISO/root/",assetPatch.Directory, assetPatch.FileName);
                    Directory.CreateDirectory(Path.Combine(@"extractedISO/root/",assetPatch.Directory));
                    File.Copy(Path.Combine("mod_assets", assetPatch.FileName),  newDirectory, true);
                
                    Console.WriteLine($"Successfully copied file: {assetPatch.FileName} to directory: {newDirectory}");
                }
                break;
            }
        default:
        {
            Console.WriteLine($"Error: no valid operation for: {assetPatch.Operation}");
            break;
        }
    }
}

// Clean up any empty directories that are left over. 
foreach (string directory in Directory.GetDirectories("extractedISO", "*", SearchOption.AllDirectories)
                                          .OrderByDescending(d => d.Length))
{
    if (!Directory.EnumerateFileSystemEntries(directory).Any())
    {
        Directory.Delete(directory);
    }
}

RarcTools.GCRebuilder.GCRebuilder.RebuildISO(
    @"extractedISO\root\",
    "decompimizer-GZ2E01.iso",
    false
);
System.IO.Directory.Delete(@"extractedISO\", true); // delete the temp ISO directory once we are done with it.
