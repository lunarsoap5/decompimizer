using System.Text.Json;
using RarcTools;

Console.WriteLine("Hello, World!");

// Open ISO Image
RarcTools.GCRebuilder.GCRebuilder.ExtractISO("decompimizer-GZ2E01.iso", @"extractedISO\");

// Decrypt, extract, and re-pack a stage archive
string archiveDirectory = RarcTools.RARCDump.DumpArchive(
    Yaz0dec.InitYaz0Decode(@"extractedISO\root\res\Stage\F_SP109\R00_00.arc")
);

// Read dzr file into a serialized var
var dzxContents = JsonSerializer.Deserialize<List<DZXDataBlock>>(
    LibStage.ExtractDZX(archiveDirectory + @"\dzr\room.dzr")
);

/*
// Modifying per section
var sclsSections = sections.Where(s => s.Tag == "SCLS");

foreach (var section in sclsSections)
{
    var entries = section.Entries.Select(e => e.Deserialize<SclsEntry>()).ToList();
    // work with entries
}

// adding per section
var sclsSections = sections.Where(s => s.Tag == "SCLS");


sclsSections.Add()
*/

string dzxString = JsonSerializer.Serialize(
    dzxContents,
    new JsonSerializerOptions { WriteIndented = true }
);
LibStage.PackageDZX(dzxString, archiveDirectory + @"\dzr\room.dzr", false);
RarcTools.RARCPacker.PackArchive(archiveDirectory);

// Decrypt, extract, and re-pack a bmg archive
//
archiveDirectory = RarcTools.RARCDump.DumpArchive(
    Yaz0dec.InitYaz0Decode(@"extractedISO\root\res\Msgus\bmgres.arc")
);
string bmgContents = BmgTools.DumpBmg(archiveDirectory + @"\zel_00.bmg");
BmgTools.PackBmg(bmgContents, archiveDirectory + @"\zel_00.bmg", "iso-8859-1", false);
RarcTools.RARCPacker.PackArchive(archiveDirectory);
RarcTools.GCRebuilder.GCRebuilder.RebuildISO(@"extractedISO\root\", "newISO.iso", false);
System.IO.Directory.Delete(@"extractedISO\", true); // delete the temp ISO directory once we are done with it.


/*
// Notes for putting everything together - DZX

// Deserialize DZX Patches file:
var patches = JsonSerializer.Deserialize<List<DZXPatch>>(patchJson);


// unpack dzx file
. . .
var dataBlocks = JsonSerializer.Deserialize<List<DZXDataBlock>>(json);

// Modify section
foreach (var patch in patches)
{
    // Verify that we have a type mapping for the block we are wanting to replace
    if (!TagTypes.TryGetValue(patch.DataBlock, out Type entryType))
        continue;

    // Loop through all of the dataBlock entries until we find the tag that we want to match
    var section = dataBlocks.First(s => s.Tag == patch.DataBlock);

    // Loop through all entries in the dataBlock
    for (int i = 0; i < section.Entries.Count; i++)
    {
        var entry = section.Entries[i].Deserialize(entryType);
        if (entry.ID == patch.ID)
        {
            var patched = ApplyChanges(entry, patch.Data);
            datablocks.Entries[i] = JsonSerializer.SerializeToElement(patched);
        }
    }
}

// add section
foreach (var patch in patches)
{
    // Verify that we have a type mapping for the block we are wanting to replace
    if (!TagTypes.TryGetValue(patch.DataBlock, out Type entryType))
        continue;

    // Loop through all of the dataBlock entries until we find the tag that we want to match
    var section = dataBlocks.First(s => s.Tag == patch.DataBlock);

    // Add the entry
    section.Entries.Add(patch.Data.Deserialize(entryType));
}

// Delete section
foreach (var patch in patches)
{
    // Verify that we have a type mapping for the block we are wanting to replace
    if (!TagTypes.TryGetValue(patch.DataBlock, out Type entryType))
        continue;

    // Loop through all of the dataBlock entries until we find the tag that we want to match
    var section = dataBlocks.First(s => s.Tag == patch.DataBlock);

    // Remove the entry that matches the patch ID
    section.Entries.RemoveAll(e => e.GetProperty("ID").GetInt32() == patch.ID);
}
*/


/*
// Notes for putting everything together - BMG

// Deserialize BMG Patches file:
List<BMGPatch> patches = JsonSerializer.Deserialize<List<BMGPatch>>(patchJson);


// unpack bmg file
. . .
List<BMGDataBlock> dataBlocks = JsonSerializer.Deserialize<List<BMGDataBlock>>(json);

// Modify section
foreach (BMGPatch patch in patches)
{
    // Loop through all of the dataBlock entries until we find the tag that we want to match
    var section = dataBlocks.First(s => ((s.Index == patch.Index) || (s.Section == patch.Section));

    var patched = ApplyChanges(entry, patch.Data);
    datablocks.Entries[i] = JsonSerializer.SerializeToElement(patched);
}
*/
