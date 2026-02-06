using SkiaSharp;
using System;
using System.Collections;
using System.ComponentModel;


namespace SDUI.Controls;

public partial class ListViewItem
{
    public class ListViewSubItemCollection : IList
    {
        private readonly ListViewItem _owner;

        // A caching mechanism for key accessor
        // We use an index here rather than control so that we don't have lifetime
        // issues by holding on to extra references.
        private int _lastAccessedIndex = -1;

        public ListViewSubItemCollection(ListViewItem owner)
        {
            _owner = owner;
        }

        /// <summary>
        ///     Returns a ListViewSubItem given it's zero based index into the ListViewSubItemCollection.
        /// </summary>
        public ListViewSubItem this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

                return _owner._subItems[index];
            }
            set
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

                var oldSubItem = _owner._subItems[index];

                _owner._subItems[index] = value;
                value._owner = _owner;

                oldSubItem._owner = null;
                oldSubItem.ReleaseUiaProvider();

                _owner.UpdateSubItems(index);
            }
        }

        /// <summary>
        ///     Retrieves the child control with the specified key.
        /// </summary>
        public virtual ListViewSubItem? this[string key]
        {
            get
            {
                // We do not support null and empty string as valid keys.
                if (string.IsNullOrEmpty(key)) return null;

                // Search for the key in our collection
                var index = IndexOfKey(key);
                if (!IsValidIndex(index)) return null;

                return this[index];
            }
        }

        /// <summary>
        ///     Returns the total number of items within the list view.
        /// </summary>
        [Browsable(false)]
        public int Count => _owner.SubItemCount;

        object ICollection.SyncRoot => this;

        bool ICollection.IsSynchronized => true;

        bool IList.IsFixedSize => false;

        public bool IsReadOnly => false;

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is not ListViewSubItem item) throw new ArgumentException(nameof(value));

                this[index] = item;
            }
        }

        int IList.Add(object? item)
        {
            if (item is not ListViewSubItem itemValue) throw new ArgumentException(nameof(item));

            return IndexOf(Add(itemValue));
        }

        public void Clear()
        {
            var oldCount = _owner.SubItemCount;
            if (oldCount > 0)
            {
                for (var i = 0; i < oldCount; i++)
                {
                    _owner.SubItems[i]._owner = null;
                    _owner._subItems[i].ReleaseUiaProvider();
                }

                _owner._subItems.Clear();
                _owner.SubItemCount = 0;
                _owner.UpdateSubItems(-1, oldCount);
            }
        }

        bool IList.Contains(object? item)
        {
            if (item is not ListViewSubItem itemValue) return false;

            return Contains(itemValue);
        }

        int IList.IndexOf(object? subItem)
        {
            if (subItem is not ListViewSubItem subItemValue) return -1;

            return IndexOf(subItemValue);
        }

        void IList.Insert(int index, object? item)
        {
            if (item is not ListViewSubItem subItem) throw new ArgumentException(nameof(item));

            Insert(index, subItem);
        }

        void IList.Remove(object? item)
        {
            if (item is ListViewSubItem itemValue) Remove(itemValue);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            // Remove ourselves as the owner.
            _owner._subItems[index]._owner = null;
            _owner._subItems[index].ReleaseUiaProvider();

            // Collapse the items
            _owner._subItems.RemoveAt(index);

            var oldCount = _owner.SubItemCount;
            _owner.SubItemCount--;
            _owner.UpdateSubItems(-1, oldCount);
        }

        void ICollection.CopyTo(Array dest, int index)
        {
            if (Count > 0) ((ICollection)_owner._subItems).CopyTo(dest, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _owner._subItems.GetEnumerator();
        }

        public ListViewSubItem Add(ListViewSubItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EnsureAdditionalCapacity(1);
            item._owner = _owner;
            _owner._subItems.Add(item);
            _owner.UpdateSubItems(_owner.SubItemCount++);
            return item;
        }

        public ListViewSubItem Add(string? text)
        {
            var item = new ListViewSubItem(_owner, text);
            Add(item);
            return item;
        }

        public ListViewSubItem Add(string? text, SKColor foreColor, SKColor backColor, Font font)
        {
            var item = new ListViewSubItem(_owner, text, foreColor, backColor, font);
            Add(item);
            return item;
        }

        public void AddRange(ListViewSubItem[] items)
        {
            ArgumentNullException.ThrowIfNull(items);

            EnsureAdditionalCapacity(items.Length);
            foreach (var item in items)
                if (item is not null)
                {
                    item._owner = _owner;
                    _owner._subItems.Add(item);
                    _owner.SubItemCount++;
                }

            _owner.UpdateSubItems(-1);
        }

        public void AddRange(string[] items)
        {
            ArgumentNullException.ThrowIfNull(items);

            EnsureAdditionalCapacity(items.Length);
            foreach (var item in items)
                if (item is not null)
                {
                    _owner._subItems.Add(new ListViewSubItem(_owner, item));
                    _owner.SubItemCount++;
                }

            _owner.UpdateSubItems(-1);
        }

        public void AddRange(string[] items, SKColor foreColor, SKColor backColor, Font font)
        {
            ArgumentNullException.ThrowIfNull(items);

            EnsureAdditionalCapacity(items.Length);
            foreach (var item in items)
                if (item is not null)
                {
                    _owner._subItems.Add(new ListViewSubItem(_owner, item, foreColor, backColor, font));
                    _owner.SubItemCount++;
                }

            _owner.UpdateSubItems(-1);
        }

        public bool Contains(ListViewSubItem? subItem)
        {
            return IndexOf(subItem) != -1;
        }

        /// <summary>
        ///     Returns true if the collection contains an item with the specified key, false otherwise.
        /// </summary>
        public virtual bool ContainsKey(string? key)
        {
            return IsValidIndex(IndexOfKey(key));
        }

        /// <summary>
        ///     Checks that the sub items list size does not exceed the MaxSubItems value
        ///     and ensures that it has the given capacity.
        /// </summary>
        private void EnsureAdditionalCapacity(int additionalCapacity)
        {
            if (_owner.SubItemCount >= MaxSubItems) throw new InvalidOperationException();

            _owner._subItems.EnsureCapacity(_owner._subItems.Count + additionalCapacity);
        }

        public int IndexOf(ListViewSubItem? subItem)
        {
            for (var index = 0; index < Count; ++index)
                if (_owner._subItems[index] == subItem)
                    return index;

            return -1;
        }

        /// <summary>
        ///     The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.
        /// </summary>
        public virtual int IndexOfKey(string? key)
        {
            if (string.IsNullOrEmpty(key)) return -1;

            if (IsValidIndex(_lastAccessedIndex))
                if (this[_lastAccessedIndex].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    return _lastAccessedIndex;

            for (var i = 0; i < Count; i++)
                if (this[i].Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    _lastAccessedIndex = i;
                    return i;
                }

            _lastAccessedIndex = -1;
            return -1;
        }

        /// <summary>
        ///     Determines if the index is valid for the collection.
        /// </summary>
        private bool IsValidIndex(int index)
        {
            return ((index >= 0) && (index < Count));
        }

        public void Insert(int index, ListViewSubItem item)
        {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));

            ArgumentNullException.ThrowIfNull(item);

            item._owner = _owner;

            EnsureAdditionalCapacity(1);

            // Insert new item
            _owner._subItems.Insert(index, item);
            _owner.SubItemCount++;
            _owner.UpdateSubItems(-1);
        }

        public void Remove(ListViewSubItem? item)
        {
            var index = IndexOf(item);
            if (index != -1) RemoveAt(index);
        }

        /// <summary>
        ///     Removes the child control with the specified key.
        /// </summary>
        public virtual void RemoveByKey(string? key)
        {
            var index = IndexOfKey(key);
            if (IsValidIndex(index)) RemoveAt(index);
        }
    }
}