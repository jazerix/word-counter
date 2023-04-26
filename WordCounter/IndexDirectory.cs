using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace WordCounter;

public class IndexDirectory
{
    private readonly string _path;
    private readonly ConcurrentDictionary<string, Count> _occurrences = new();
    private readonly ConcurrentDictionary<string, Count> _excluded = new();

    public IndexDirectory(string path)
    {
        _path = path;
    }

    /// <summary>
    /// Begins the indexes of files concurrently. The methods blocks until all subdirectories have been indexed.
    /// </summary>
    /// <returns>Itself.</returns>
    public IndexDirectory Index()
    {
        Directory
            .EnumerateFiles(_path, "*.txt", SearchOption.AllDirectories)
            .AsParallel().ForAll(IndexFile);
        return this;
    }

    /// <summary>
    /// Examines input file and all words encountered will be added to the exclusion list.
    /// </summary>
    /// <param name="path">The path to the file that should be excluded.</param>
    /// <returns>Itself.</returns>
    public IndexDirectory ExcludeFromFile(string path)
    {
        var words = new Regex(@"[^\w]+")
            .Split(File.ReadAllText(path).Trim())
            .ToList()
            .Where(word => word != string.Empty);
        foreach (var word in words)
            Exclude(word);
        return this;
    }
    
    /// <summary>
    /// Excludes a single word.
    /// </summary>
    /// <param name="word">Word that is added to the exclusion list.</param>
    /// <returns>Itself.</returns>
    public IndexDirectory Exclude(string word)
    {
        _excluded[word.ToUpper()] = new Count();
        return this;
    }

    /// <summary>
    /// Persists results from the indexing process. Each word is grouped into their first letter and saved as file.
    /// If any words have been excluded an additional exclusion-stats.txt file will be created with the occurrences of
    /// each excluded word.
    /// </summary>
    public void SaveStats()
    {
        var letterGroups = _occurrences
            .ToList()
            .GroupBy(pair => pair.Key[..1]);

        Directory.CreateDirectory("results");
        foreach (var group in letterGroups)
        {
            group
                .OrderBy(x => x.Key)
                .ToDictionary(pair => pair.Key, x => x.Value)
                .ToDisk($"results/{group.Key}.txt");
        }
        PersistExcludeStats();
    }

    /// <summary>
    /// Saves a statistical file of the encountered filtered words.
    /// </summary>
    private void PersistExcludeStats()
    {
        if (_excluded.IsEmpty)
            return;
        _excluded.ToDisk("results/excluded-stats.txt");
    }

    /// <summary>
    /// A file that should be indexed.
    /// </summary>
    /// <param name="file">The file to index</param>
    private void IndexFile(string file)
    {
        var content = new StreamReader(File.OpenRead(file));
        while (!content.EndOfStream)
        {
            var line = content.ReadLine();
            if (line == null)
                continue;

            foreach (var word in new Regex(@"[^\w]+").Split(line.Trim()))
                IndexWord(word);
        }
    }

    /// <summary>
    /// Attempts to add the passed word to the list of occurrences. Excluded words will not be added to the list.
    /// </summary>
    /// <param name="word">Input word that should be indexed.</param>
    private void IndexWord(string word)
    {
        var uppercaseWord = word.ToUpper().Trim();
        if (uppercaseWord == string.Empty || IsFilteredWord(uppercaseWord))
            return;

        var count = _occurrences.GetOrAdd(uppercaseWord, new Count());
        count.Increment();
    }


    /// <summary>
    /// Checks the exclusion list to see if the passed word is contained.
    /// </summary>
    /// <param name="uppercaseWord">Word to validate.</param>
    /// <returns></returns>
    private bool IsFilteredWord(string uppercaseWord)
    {
        if (!_excluded.TryGetValue(uppercaseWord, out var count)) return false;
        count.Increment();
        return true;
    }
}