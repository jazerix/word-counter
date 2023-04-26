using System.Diagnostics;

namespace WordCounter;

/// <summary>
/// Wrapper class since ints are passed by reference. 
/// </summary>
[DebuggerDisplay("{_count}", Name = "LO")]
public class Count
{
    private int _count;

    public void Increment()
    {
        Interlocked.Increment(ref _count);
    }

    public override string ToString()
    {
        return $"{_count}";
    }
}