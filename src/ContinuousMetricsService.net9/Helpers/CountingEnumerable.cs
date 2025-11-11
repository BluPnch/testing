using System.Collections;


namespace ContinuousMetricsService.net9.Helpers;

public class CountingEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _source;
    
    // Статические счетчики
    public static int EnumeratorCount { get; private set; }
    public static int MoveNextCount { get; private set; }

    public CountingEnumerable(IEnumerable<T> source)
    {
        _source = source;
    }

    // ✅ Реализация IEnumerable<T>.GetEnumerator()
    public IEnumerator<T> GetEnumerator()
    {
        EnumeratorCount++;
        return new CountingEnumerator(_source.GetEnumerator());
    }

    // ✅ Реализация IEnumerable.GetEnumerator() (явная реализация)
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static void ResetCounters()
    {
        EnumeratorCount = 0;
        MoveNextCount = 0;
    }

    // Внутренний класс-перечислитель
    private class CountingEnumerator : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public CountingEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public T Current => _enumerator.Current;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            MoveNextCount++;
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}