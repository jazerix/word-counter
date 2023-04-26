using System.Collections.Concurrent;

namespace WordCounter;

public static class DictionaryExtension
{
    /// <summary>
    /// Saves the dictionary to disk using the output filename.
    /// </summary>
    /// <param name="dictionary">The dictionary to save to file.</param>
    /// <param name="outputFileName">The location on disk.</param>
    public static void ToDisk<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string outputFileName)
        where TKey : notnull
    {
        using var streamWriter = new StreamWriter(outputFileName);
        dictionary
            .ToList()
            .ForEach(result => streamWriter.WriteLine($"{result.Key} {result.Value}"));
    }

    /// <summary>
    /// Saves the dictionary to disk using the output filename.
    /// </summary>
    /// <param name="dictionary">The dictionary to save to file.</param>
    /// <param name="outputFileName">The location on disk.</param>
    public static void ToDisk<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, string outputFileName)
        where TKey : notnull
    {
        dictionary.ToDictionary(pair => pair.Key, x => x.Value).ToDisk(outputFileName);
    }
}