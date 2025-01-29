using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDUI.Collections;

public class UIElementCollection : IList<UIElementBase>
{
    private readonly List<UIElementBase> _items = new(32);
    private readonly UIElementBase _owner;

    public UIElementCollection(UIElementBase owner)
    {
        _owner = owner;
    }

    public UIElementBase this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            if (oldItem != value)
            {
                if (oldItem != null)
                {
                    _owner.OnControlRemoved(new UIElementEventArgs(oldItem));
                }
                _items[index] = value;
                if (value != null)
                {
                    _owner.OnControlAdded(new UIElementEventArgs(value));
                }
            }
        }
    }

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public void Add(UIElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
        _owner.OnControlAdded(new UIElementEventArgs(item));
    }

    public void AddRange(UIElementBase[] items)
    {
        foreach (var item in items)
            this.Add(item);
    }

    public void Clear()
    {
        var itemsToRemove = _items.ToList();
        _items.Clear();
        foreach (var item in itemsToRemove)
        {
            _owner.OnControlRemoved(new UIElementEventArgs(item));
        }
    }

    public bool Contains(UIElementBase item) => _items.Contains(item);
    public void CopyTo(UIElementBase[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public IEnumerator<UIElementBase> GetEnumerator() => _items.GetEnumerator();
    public int IndexOf(UIElementBase item) => _items.IndexOf(item);

    public void Insert(int index, UIElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Insert(index, item);
        _owner.OnControlAdded(new UIElementEventArgs(item));
    }

    public bool Remove(UIElementBase item)
    {
        if (item == null)
            return false;

        var result = _items.Remove(item);
        if (result)
        {
            _owner.OnControlRemoved(new UIElementEventArgs(item));
        }
        return result;
    }

    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        _owner.OnControlRemoved(new UIElementEventArgs(item));
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}