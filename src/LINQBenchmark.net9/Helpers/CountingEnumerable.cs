using System.Collections;

namespace LINQBenchmark.Helpers;

public class CountingEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _source;
    public static int EnumeratorCount { get; set; }
    public static int MoveNextCount { get; set; }

    public CountingEnumerable(IEnumerable<T> source)
    {
        _source = source;
    }

    public IEnumerator<T> GetEnumerator() => new CountingEnumerator(_source.GetEnumerator());

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class CountingEnumerator : IEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        
        public CountingEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
            EnumeratorCount++;
        }

        public T Current => _inner.Current;
        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            MoveNextCount++;
            return _inner.MoveNext();
        }

        public void Reset() => _inner.Reset();
        public void Dispose() => _inner.Dispose();
    }

    public static void ResetCounters()
    {
        EnumeratorCount = 0;
        MoveNextCount = 0;
    }
}