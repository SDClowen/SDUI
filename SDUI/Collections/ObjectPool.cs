using System;
using System.Collections.Concurrent;

namespace SDUI.Collections;

public class ObjectPool<T>
{
    private readonly ConcurrentBag<T> _objects;
    private readonly Func<T> _objectGenerator;

    public ObjectPool(Func<T> objectGenerator)
    {
        _objects = new ConcurrentBag<T>();
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
    }

    public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();

    public void Return(T item)
    {
        if (item != null)
            _objects.Add(item);
    }
}
