using WordCounter;

if (args.Length == 0)
{
    Console.WriteLine("No input path given.");
    return;
}

var path = Path.GetFullPath(args[0]);
if (!Directory.Exists(path))
{
    Console.WriteLine("Invalid input path.");
    return;
}

var indexer = new IndexDirectory(path);

if (args.Length == 2)
{
    var excludePath = Path.GetFullPath(args[1]);
    if (!File.Exists(excludePath))
    {
        Console.WriteLine("Invalid path to exclude file path.");
        return;
    }

    indexer.ExcludeFromFile(excludePath);
}

indexer.Index();
Console.WriteLine("Indexing done. Persisting..."); 
indexer.SaveStats();
Console.WriteLine("Persisted. Results saved to 'results' directory."); 