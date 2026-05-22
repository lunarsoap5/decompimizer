using System.Text.Json;
using System.Text.Json.Serialization;
using RarcTools;

Console.WriteLine("Hello, World!");

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
                    if (archiveDirectory.Contains(@"R_SP109\R03"))
                    {
                        sectionID = PatchFunctions.GetEntryParamID(
                            dzxContents[sectionIndex].Entries[i]
                        );
                    }
                    else
                    {
                        sectionID = PatchFunctions.GetEntryID(dzxContents[sectionIndex].Entries[i]);
                    }
                    Console.WriteLine(sectionID);
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
                        Console.WriteLine(
                            "Successfully Applied Patch to: "
                                + archiveDirectory
                                + "-"
                                + DZXChange.ID
                        );
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
                (bmgChange.Index != null && s.index?.ToLower() == bmgChange.Index.ToLower())
                || (bmgChange.Section != null && s.Section == bmgChange.Section)
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
                var patched = PatchFunctions.ApplyChanges(
                    bmgContents[sectionIndex],
                    bmgChange.ChangeData
                );
                bmgContents[sectionIndex] = (BMGDataBlock)patched;
                // Successfully patched the section, so break out.
                Console.WriteLine(
                    "Successfully Applied Patch to: " + archiveDirectory + "-" + bmgChange.Index
                );

                /*Console.WriteLine(
                    JsonSerializer.Serialize(
                        bmgContents[sectionIndex],
                        new JsonSerializerOptions { WriteIndented = true }
                    )
                );*/
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

RarcTools.GCRebuilder.GCRebuilder.RebuildISO(
    @"extractedISO\root\",
    "decompimizer-GZ2E01.iso",
    false
);
System.IO.Directory.Delete(@"extractedISO\", true); // delete the temp ISO directory once we are done with it.
