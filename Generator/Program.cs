using RarcTools;

Console.WriteLine("Hello, World!");

// Open ISO Image
RarcTools.GCRebuilder.GCRebuilder.ExtractISO("decompimizer-GZ2E01.iso", @"extractedISO\");

// Decrypt, extract, and re-pack a stage archive
string archiveDirectory = RarcTools.RARCDump.DumpArchive(
    Yaz0dec.InitYaz0Decode(@"extractedISO\root\res\Stage\F_SP109\R00_00.arc")
);
string dzxContents = LibStage.ExtractDZX(archiveDirectory + @"\dzr\room.dzr");
LibStage.PackageDZX(dzxContents, archiveDirectory + @"\dzr\room.dzr", false);
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
