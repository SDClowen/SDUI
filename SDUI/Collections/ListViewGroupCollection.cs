using SDUI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SDUI.Collections
{
    public class ListViewGroupCollection : IList
    {
        private readonly SDUI.Controls.ListView _listView;

        private List<Controls.ListViewGroup>? _list;

        internal ListViewGroupCollection(SDUI.Controls.ListView listView)
        {
            _listView = listView;
        }

        public int Count => List.Count;

        object ICollection.SyncRoot => this;

        bool ICollection.IsSynchronized => true;

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        private List<Controls.ListViewGroup> List => _list ??= [];

        public Controls.ListViewGroup this[int index]
        {
            get => List[index];
            set
            {
                if (List.Contains(value))
                {
                    return;
                }

                CheckListViewItems(value);
                value.ListView = _listView;
                List[index] = value;
            }
        }

        public Controls.ListViewGroup? this[string key]
        {
            get
            {
                if (_list is null)
                {
                    return null;
                }

                for (int i = 0; i < _list.Count; i++)
                {
                    if (string.Equals(key, this[i].Name, StringComparison.CurrentCulture))
                    {
                        return this[i];
                    }
                }

                return null;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                if (_list is null)
                {
                    // nothing to do
                    return;
                }

                int index = -1;
                for (int i = 0; i < _list.Count; i++)
                {
                    if (string.Equals(key, this[i].Name, StringComparison.CurrentCulture))
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    _list[index] = value;
                }
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is ListViewGroup group)
                {
                    this[index] = group;
                }
            }
        }

        public int Add(ListViewGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            if (Contains(group))
            {
                return -1;
            }

            CheckListViewItems(group);
            group.ListView = _listView;
            int index = ((IList)List).Add(group);
            //if (_listView.IsHandleCreated)
            //{
            //    _listView.InsertGroupInListView(List.Count, group);
            //    MoveGroupItems(group);
            //}

            return index;
        }

        public ListViewGroup Add(string? key, string? headerText)
        {
            ListViewGroup group = new ListViewGroup(key, headerText);
            Add(group);
            return group;
        }

        int IList.Add(object? value)
        {
            if (value is not ListViewGroup group)
            {
                return -1;
            }

            return Add(group);
        }

        public void AddRange(ListViewGroup[] groups)
        {
            ArgumentNullException.ThrowIfNull(groups);

            for (int i = 0; i < groups.Length; i++)
            {
                Add(groups[i]);
            }
        }

        public void AddRange(ListViewGroupCollection groups)
        {
            ArgumentNullException.ThrowIfNull(groups);

            for (int i = 0; i < groups.Count; i++)
            {
                Add(groups[i]);
            }
        }

        private void CheckListViewItems(ListViewGroup group)
        {
            for (int i = 0; i < group.Items.Count; i++)
            {
                ListViewItem item = group.Items[i];
                if (item.ListView is not null && item.ListView != _listView)
                {
                }
            }
        }

        public void Clear()
        {
            if (_listView.IsHandleCreated)
            {
                for (int i = 0; i < Count; i++)
                {
                    _listView.Groups.Remove(this[i]);
                }
            }

            // Dissociate groups from the ListView
            for (int i = 0; i < Count; i++)
            {
                this[i].ListView = null;
            }

            List.Clear();

            // we have to tell the listView that there are no more groups
            // so the list view knows to remove items from the default group
            _listView.Invalidate();//.UpdateGroupView();
        }

        public bool Contains(ListViewGroup value) => List.Contains(value);

        bool IList.Contains(object? value)
        {
            if (value is not ListViewGroup group)
            {
                return false;
            }

            return Contains(group);
        }

        public void CopyTo(Array array, int index) => ((ICollection)List).CopyTo(array, index);

        public IEnumerator GetEnumerator() => List.GetEnumerator();

        public int IndexOf(ListViewGroup value) => List.IndexOf(value);

        int IList.IndexOf(object? value)
        {
            if (value is not ListViewGroup group)
            {
                return -1;
            }

            return IndexOf(group);
        }

        public void Insert(int index, ListViewGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            if (Contains(group))
            {
                return;
            }

            CheckListViewItems(group);
            group.ListView = _listView;
            List.Insert(index, group);
            if (_listView.IsHandleCreated)
            {
                _listView.InsertGroupInListView(index, group);
                MoveGroupItems(group);
            }
        }

        void IList.Insert(int index, object? value)
        {
            if (value is ListViewGroup group)
            {
                Insert(index, group);
            }
        }

        private void MoveGroupItems(ListViewGroup group)
        {
            foreach (ListViewItem item in group.Items)
            {
                if (item.ListView == _listView)
                {
                    //item.UpdateStateToListView(item.Index);
                }
            }
        }

        public void Remove(ListViewGroup group)
        {
            if (!List.Remove(group))
            {
                return;
            }

            group.ListView = null;

            if (_listView.IsHandleCreated)
            {
                _listView.RemoveGroupFromListView(group);
            }
        }

        void IList.Remove(object? value)
        {
            if (value is ListViewGroup group)
            {
                Remove(group);
            }
        }

        public void RemoveAt(int index) => Remove(this[index]);

    }

}
