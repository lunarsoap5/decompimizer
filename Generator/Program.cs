using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Text.Json.Serialization;
using RarcTools;

Console.WriteLine("Hello, World!");

bool show_debug = false;

//==============================================================================================
// Open ISO Image
RarcTools.GCRebuilder.GCRebuilder.ExtractISO(@"decompimizer-GZ2E01.iso", @"extractedISO/");

var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };

//==============================================================================================
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
        archiveDirectory + ".arc", "",
        false,
        true
    );
}

//==============================================================================================
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
        archiveDirectory + ".arc", "",
        false,
        true
    );
}

//==============================================================================================
// Copy over custom files
// Deserialize asset manifest file:
string assetManifestContents = File.ReadAllText(Path.Combine("mod_assets","manifest.jsonc"));
var assetPatches = JsonSerializer.Deserialize<List<AssetPatch>>(assetManifestContents, options);

// Loop through all manifest items and apply them appropriately
foreach(var assetPatch in assetPatches)
{
    // If the manifest source directory contains an archive, we need to extract it before we modify it.
    if (assetPatch.Directory.Contains(".arc"))
    {
        string archiveDirectory = Tools.GetSubstringFromMarker(assetPatch.Directory, ".arc");
        string extractedDirectory = RARCDump.DumpArchive(
                Yaz0dec.InitYaz0Decode(@"extractedISO/root/" + archiveDirectory));
        

        foreach (AssetFiles file in assetPatch.Files)
        {
            
            string newDirectory =  Path.Combine(extractedDirectory,file.Subdirectory,file.FileName);
            //Console.WriteLine(extractedDirectory);
            File.Copy(Path.Combine("mod_assets", file.FileName), newDirectory, true);
            
            Console.WriteLine($"Successfully copied file: {file.FileName} to directory: {newDirectory}");

            
        }
            
        RARCPacker.PackArchive(
                extractedDirectory,
                extractedDirectory + ".arc", "",
                false,
                true
            );
    }
    // Otherwise, just move the custom file over, replacing anything as needed. 
    else
    {
        foreach (AssetFiles file in assetPatch.Files)
        {
            string newDirectory = Path.Combine(@"extractedISO/root/",assetPatch.Directory, file.FileName);
            Directory.CreateDirectory(Path.Combine(@"extractedISO/root/",assetPatch.Directory));
            File.Copy(Path.Combine("mod_assets", file.FileName),  newDirectory, true);
        
            Console.WriteLine($"Successfully copied file: {file.FileName} to directory: {newDirectory}");
        }
    }
    
}

//==============================================================================================
// Replace Assets stored in .bmd files
Console.WriteLine("Modifying BMD Textures");
BmdTools.replaceModdedTextures();

//==============================================================================================
// Generate Cosmetic Changes (i.e texture recoloring)
List<TextureRecolor> textureRecolors = CosmeticFunctions.GenerateTextureCosmetics();
foreach (TextureRecolor texRecolor in textureRecolors)
{
    // Dump the archive
    string texArchiveDirectory = RARCDump.DumpArchive(
                        Yaz0dec.InitYaz0Decode(texRecolor.ArchiveDirectory));

    foreach(TextureRecolorOptions texOptions in texRecolor.TextureOptions)
    {
        var texBmd = new BmdFile(Path.Combine(texArchiveDirectory,texOptions.FileName));
        switch(texOptions.RecolorType)
        {
            case TextureRecolorType.Greyscale:
                {
                    var textureToModify = texBmd.Textures[(int)texOptions.TextureIndex];
                    textureToModify.TintGrayscale(texOptions.NewColor);
                    break;
                }
            
            case TextureRecolorType.Palette:
                {
                    var textureToModify = texBmd.Textures[(int)texOptions.TextureIndex];
                    var map = new Dictionary<RgbaColor, RgbaColor>
                    {
                        {texOptions.OldColor, texOptions.NewColor}
                    };
                    textureToModify.Recolor(map, texOptions.Tolerance);
                    break;
                }
            
            case TextureRecolorType.Hue:
                {
                    var textureToModify = texBmd.Textures[(int)texOptions.TextureIndex];
                    textureToModify.RecolorByHue(
                        targetColor:  texOptions.OldColor,
                        replacementColor: texOptions.NewColor,
                        hueToleranceDegrees: texOptions.Tolerance);
                    break;
                }
            case TextureRecolorType.Material:
                {
                    byte[] mat3Bytes = texBmd.GetRawChunk("MAT3");
                    var mat3 = new Mat3Chunk(mat3Bytes);

                    uint value = texOptions.TextureIndex;

                    //int a = (byte)((value >> 24) & 0xFF); // unused for now
                    int konstIdx = (byte)((value >> 16) & 0xFF); 
                    int tevIdx = (byte)((value >> 8) & 0xFF);  
                    int idx = (byte)(value & 0xFF);         
                    
                    var material = mat3.Materials[idx];

                    // Not every material will modify a konst material and/or a tev color. So we check here to see if the value is set. 
                    if (konstIdx != 0xFF)
                    {
                        mat3.SetTevKonstColor(material.KonstColorIdx[konstIdx], texOptions.NewColor); 
                    }
                    if (tevIdx != 0xFF)
                    {
                        mat3.SetTevColor(material.TevColorIdx[tevIdx], texOptions.NewColor.R, texOptions.NewColor.G, texOptions.NewColor.B, 255);                      
                    }
                    texBmd.SetRawChunk("MAT3", mat3.GetPatchedChunkBytes());
                    break;
                }
            default:
                {
                    Console.WriteLine($"No recolor definition defined for type {texOptions.RecolorType}");
                    break;
                }
        }

        texBmd.Save(Path.Combine(texArchiveDirectory,texOptions.FileName));
        Console.WriteLine($"Modified texture in model {texOptions.FileName}");
    }

    RARCPacker.PackArchive(
                texArchiveDirectory,
                texArchiveDirectory + ".arc", "",
                false,
                true
            );
}

//==============================================================================================
// Generate custom asset/code files
Console.WriteLine("Generating Custom Asset Files");

// Generate portal.dat file that draw the portal icons on the map
FieldMapTools.generatePortalData();

//==============================================================================================
// Update the game code to the rando code - GZ2*99

string rawFilePath = @"extractedISO/root/&&systemdata/ISO.hdr";

// Read the file into a byte array
byte[] data = File.ReadAllBytes(rawFilePath);
data[0x4] = 0x39;
data[0x5] = 0x39;

// Write the modified bytes back to the file
File.WriteAllBytes(rawFilePath, data);
Console.WriteLine("Converted Game code to: GZ2E99");

//==============================================================================================
// Clean up any empty directories that are left over. 
foreach (string directory in Directory.GetDirectories("extractedISO", "*", SearchOption.AllDirectories)
                                          .OrderByDescending(d => d.Length))
{
    if (!Directory.EnumerateFileSystemEntries(directory).Any())
    {
        Directory.Delete(directory);
    }
}

//==============================================================================================
RarcTools.GCRebuilder.GCRebuilder.RebuildISO(
    @"extractedISO\root\",
    "decompimizer-GZ2E99.iso",
    false
);

System.IO.Directory.Delete(@"extractedISO\", true); // delete the temp ISO directory once we are done with it.


// Testing functions. Uncomment as needed. =============================================
// CosmeticFunctions.PrintMaterialDescriptions("O_gD_hutk.bmd");
