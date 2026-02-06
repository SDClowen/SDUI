using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SDUI.Controls;

namespace SDUI.Collections;

public class UIWindowElementCollection : IList<ElementBase>
{
    private readonly List<ElementBase> _items = new(32);
    private readonly UIWindowBase _owner;
    private int _maxZOrder;

    public UIWindowElementCollection(UIWindowBase owner)
    {
        _owner = owner;
    }

    public ElementBase this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            if (oldItem != value)
            {
                _owner.SuspendLayout();
                try
                {
                    if (oldItem != null)
                    {
                        oldItem.Parent = null;
                        if (_owner.FocusedElement == oldItem) _owner.FocusedElement = null;
                    }

                    _items[index] = value;
                    if (value != null)
                    {
                        value.Parent = _owner;
                        _maxZOrder++;
                        value.ZOrder = _maxZOrder;
                        if (_owner.FocusedElement == null && value.TabStop) _owner.FocusedElement = value;
                    }
                }
                finally
                {
                    _owner.ResumeLayout(true);
                }
            }
        }
    }

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public void Add(ElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _owner.SuspendLayout();
        try
        {
            item.Parent = _owner;
            _maxZOrder++;
            item.ZOrder = _maxZOrder;
            _items.Add(item);

            if (_owner.FocusedElement == null && item.TabStop) _owner.FocusedElement = item;
        }
        finally
        {
            _owner.ResumeLayout(true);
        }
    }

    public void Clear()
    {
        _owner.SuspendLayout();
        try
        {
            var itemsToRemove = _items.ToList();
            _items.Clear();
            foreach (var item in itemsToRemove)
            {
                item.Parent = null;
                if (_owner.FocusedElement == item) _owner.FocusedElement = null;
            }
        }
        finally
        {
            _owner.ResumeLayout(true);
        }
    }

    public bool Contains(ElementBase item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(ElementBase[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ElementBase> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public int IndexOf(ElementBase item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, ElementBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _owner.SuspendLayout();
        try
        {
            item.Parent = _owner;
            _maxZOrder++;
            item.ZOrder = _maxZOrder;
            _items.Insert(index, item);

            if (_owner.FocusedElement == null && item.TabStop) _owner.FocusedElement = item;
        }
        finally
        {
            _owner.ResumeLayout(true);
        }
    }

    public bool Remove(ElementBase item)
    {
        if (item == null)
            return false;

        _owner.SuspendLayout();
        try
        {
            var result = _items.Remove(item);
            if (result)
            {
                item.Parent = null;
                if (_owner.FocusedElement == item) _owner.FocusedElement = null;
            }

            return result;
        }
        finally
        {
            _owner.ResumeLayout(true);
        }
    }

    public void RemoveAt(int index)
    {
        _owner.SuspendLayout();
        try
        {
            var item = _items[index];
            _items.RemoveAt(index);
            item.Parent = null;
            if (_owner.FocusedElement == item) _owner.FocusedElement = null;
        }
        finally
        {
            _owner.ResumeLayout(true);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}