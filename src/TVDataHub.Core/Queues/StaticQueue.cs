using System.Collections.Concurrent;

namespace TVDataHub.Application.Queues;

public static class StaticQueue<T>
{
    private static readonly ConcurrentQueue<T> _queue = new();

    public static void Enqueue(T item) => 
        _queue.Enqueue(item);

    public static void EnqueueMany(List<T> items) =>
        items.ForEach(Enqueue);

    public static bool TryDequeue(out T? item) => 
        _queue.TryDequeue(out item);

    public static bool IsEmpty =>
        _queue.IsEmpty;

    public static int Count =>
        _queue.Count;
}