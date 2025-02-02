using System.Collections.Generic;

namespace SDUI.Collections;

public class IndexedList<T> : List<T>
{
    public new int Add(T item)
    {
        Add(item);
        return Count - 1;
    }
}
