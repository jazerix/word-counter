using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace WordCounter;

public class IndexDirectory
{
    private readonly string _path;
    ConcurrentDictionary<string, Count> occurrences = new();
    private ConcurrentDictionary<string, Count> excluded = new();

    public IndexDirectory(string path)
    {
        _path = path;
    }

    public IndexDirectory Index()
    {
        Directory
            .EnumerateFiles(_path, "*.txt", SearchOption.AllDirectories)
            .AsParallel()
            .ForAll(IndexFile);
        return this;
    }

    public IndexDirectory ExcludeFile(string path)
    {
        var words = new Regex(@"[^\w]+")
            .Split(File.ReadAllText(path).Trim())
            .ToList()
            .Where(word => word != string.Empty);
        foreach (var word in words)
            Exclude(word);
        return this;
    }

    public IndexDirectory Exclude(string word)
    {
        excluded[word.ToUpper()] = new Count();
        return this;
    }

    public void Persist()
    {
        var letterGroups = occurrences
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
    }

    public void PersistExcludeStats()
    {
        excluded.ToDisk("excluded-stats.txt");
    }

    private void IndexFile(string file)
    {
        using var content = File.OpenText(file);
        while (!content.EndOfStream)
        {
            var line = content.ReadLine();
            if (line == null)
                continue;

            foreach (var word in new Regex(@"[^\w]+").Split(line.Trim()))
            {
                var uppercaseWord = word.ToUpper().Trim();
                if (uppercaseWord == string.Empty)
                    continue;
                if (IsFilteredWord(uppercaseWord)) continue;

                var count = occurrences.GetOrAdd(uppercaseWord, new Count());
                count.Increment();
            }
        }
    }

    private bool IsFilteredWord(string uppercaseWord)
    {
        if (!excluded.TryGetValue(uppercaseWord, out var count)) return false;
        count.Increment();
        return true;
    }
}