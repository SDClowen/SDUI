using System.Collections.Generic;

namespace SDUI.Collections;

public class ObjectReturnedList<T> : List<T>
{
    public new T Add(T item)
    {
        base.Add(item);
        return item;
    }
}