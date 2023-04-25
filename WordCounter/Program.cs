using WordCounter;

var indexDirectory = new IndexDirectory(".").ExcludeFile("exclude.txt").Index();
indexDirectory.Persist();
indexDirectory.PersistExcludeStats();

Console.WriteLine("DONE");