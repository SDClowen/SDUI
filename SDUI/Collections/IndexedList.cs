using System.Collections.Generic;

namespace SDUI.Collections;

public class IndexedList<T> : List<T>
{
    public new int Add(T item)
    {
        base.Add(item);
        return Count - 1;
    }
}