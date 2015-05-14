using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PanoramicDataWin8.controller.data
{
    public class AsyncVirtualizedCollection<T> : IList<DataWrapper<T>>, IList, INotifyCollectionChanged, INotifyPropertyChanged where T : class
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
                h(this, e);
        }

        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        private void FirePropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        private readonly IItemsProvider<T> _itemsProvider;
        public IItemsProvider<T> ItemsProvider
        {
            get { return _itemsProvider; }
        }


        private readonly int _pageSize = 100;
        public int PageSize
        {
            get { return _pageSize; }
        }

        private readonly long _pageTimeout = 10000;
        public long PageTimeout
        {
            get { return _pageTimeout; }
        }

        public AsyncVirtualizedCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
        {
            _itemsProvider = itemsProvider;
            _pageSize = pageSize;
            _pageTimeout = pageTimeout;
        }

        bool IList.Contains(object value)
        {
            return Contains((DataWrapper<T>)value);
        }
        public bool Contains(DataWrapper<T> item)
        {
            foreach (DataPage<T> page in _pages.Values)
            {
                if (page.Items.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(object value)
        {
            return IndexOf((DataWrapper<T>)value);
        }

        public int IndexOf(DataWrapper<T> item)
        {
            foreach (KeyValuePair<int, DataPage<T>> keyValuePair in _pages)
            {
                int indexWithinPage = keyValuePair.Value.Items.IndexOf(item);
                if (indexWithinPage != -1)
                {
                    return PageSize * keyValuePair.Key + indexWithinPage;
                }
            }
            return -1;
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }


        public IEnumerator<DataWrapper<T>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public DataWrapper<T> this[int index]
        {
            get
            {
                // remove stale pages
                CleanUpPages();

                // determine which page and offset within page
                int pageIndex = index / PageSize;
                int pageOffset = index % PageSize;

                // request primary page
                RequestPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                    RequestPage(pageIndex + 1);

                // if accessing lower 50% then request prev page
                if (pageOffset < PageSize / 2 && pageIndex > 0)
                    RequestPage(pageIndex - 1);

                // return requested item
                if (_pages[pageIndex].Items.Count > pageOffset)
                {
                    return _pages[pageIndex].Items[pageOffset];
                }
                return null;
            }
            set { throw new NotSupportedException(); }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        private Dictionary<int, DataPage<T>> _pages = new Dictionary<int, DataPage<T>>();

        public void CleanUpPages()
        {
            int[] keys = _pages.Keys.ToArray();
            foreach (int key in keys)
            {
                // page 0 is a special case, since WPF ItemsControl access the first item frequently
                if (PageTimeout != -1)
                {
                    if (key != 0 && (DateTime.Now - _pages[key].TouchTime).TotalMilliseconds > PageTimeout)
                    {
                        bool removePage = true;
                        DataPage<T> page;
                        if (_pages.TryGetValue(key, out page))
                        {
                            removePage = !page.IsInUse;
                        }

                        if (removePage)
                        {
                            _pages.Remove(key);
                            //Trace.WriteLine("Removed Page: " + key);
                        }
                    }
                }
            }
        }

        protected virtual void RequestPage(int pageIndex)
        {
            if (!_pages.ContainsKey(pageIndex))
            {
                // Create a page of empty data wrappers.
                int pageLength = Math.Min(this.PageSize, this.Count - pageIndex * this.PageSize);
                DataPage<T> page = new DataPage<T>(pageIndex * this.PageSize, pageLength);
                _pages.Add(pageIndex, page);
                //Trace.WriteLine("Added page: " + pageIndex);
                LoadPage(pageIndex, pageLength);
            }
            else
            {
                _pages[pageIndex].TouchTime = DateTime.Now;
            }
        }

        protected virtual void PopulatePage(int pageIndex, IList<T> dataItems)
        {
            //Trace.WriteLine("Page populated: " + pageIndex);
            DataPage<T> page;
            if (_pages.TryGetValue(pageIndex, out page))
            {
                page.Populate(dataItems);
            }
        }

        protected void EmptyCache()
        {
            _pages = new Dictionary<int, DataPage<T>>();
        }

        protected async virtual void LoadCount()
        {
            /*var t = new Task<int>(() =>
            {
                return ItemsProvider.FetchCount().Result;
            });
            t.ContinueWith(task =>
            {
                _count = task.Result;
                FirePropertyChanged("Count");
                FireCollectionReset();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            t.Start();*/
            int t = await Task.Run(() => ItemsProvider.FetchCount());
            _count = t;
            FirePropertyChanged("Count");
            FireCollectionReset();
        }

        protected async virtual void LoadPage(int pageIndex, int pageLength)
        {
            /*var t = new Task<IList<T>>(() =>
            {
                return ItemsProvider.FetchPage(pageIndex, pageLength).Result;
            });
            ItemsProvider.FetchPage(pageIndex, pageLength).ContinueWith(task =>
            {
                PopulatePage(pageIndex, task.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            t.Start();*/
            IList<T> t = await Task.Run(() => ItemsProvider.FetchPage(pageIndex, pageLength));
            PopulatePage(pageIndex, t);
        }

        private int _count = -1;
        public int Count
        {
            get
            {
                if (_count == -1)
                {
                    _count = 0;
                    LoadCount();
                }
                return _count;
            }
            protected set
            {
                _count = value;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, DataWrapper<T> item)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (DataWrapper<T>)value);
        }
        public void CopyTo(DataWrapper<T>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public void Add(DataWrapper<T> item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        public bool Remove(DataWrapper<T> item)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        bool IList.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
