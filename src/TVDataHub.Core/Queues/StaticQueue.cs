using System.Collections.Concurrent;

namespace TVDataHub.Application.Queues;

public static class StaticQueue<T>
{
    private static readonly ConcurrentQueue<T> _queue = new();
    private static readonly SemaphoreSlim _signal = new(0);

    public static void Enqueue(T item)
    {
        _queue.Enqueue(item);
        _signal.Release();
    }

    public static void EnqueueMany(IEnumerable<T> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            _queue.Enqueue(item);
            count++;
        }
        _signal.Release(count);
    }

    public static async Task<T?> DequeueAsync()
    {
        await _signal.WaitAsync();

        if (_queue.TryDequeue(out var item))
            return item;

        return default;
    }

    public static bool IsEmpty =>
        _queue.IsEmpty;

    public static int Count =>
        _queue.Count;
}