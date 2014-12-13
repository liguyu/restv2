using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.ObjectTools
{
    public abstract class BasePagedList : BaseObjectList
    {
        #region PageIndex 当前页
        /// <summary>
        /// 当前页号，默认为-1，当页号发生变化后，重新加载某页数据。
        /// </summary>
        protected int pageIndex = -1;
        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                //if (pageIndex != value)
                //{
                pageIndex = value;
                //当前页改变，重新加载详细数据，pageIndex小于0时不加载
                if (pageIndex >= 0)
                {
                    LoadDetail();
                }
                //}
            }
        }
        #endregion

        #region PageSize 每页行数
        /// <summary>
        /// 每页行数
        /// </summary>
        public int PageSize { get; set; }
        #endregion

        public virtual void LoadDetail()
        {
        }

        #region PagedView 自带一个PagedCollectionView，方便DataPager进行绑定
        private PagedCollectionView pagedView;
        public PagedCollectionView PagedView
        {
            get 
            {
                if(pagedView == null)
                {
                    pagedView = new PagedCollectionView(new MyList(this));
                }
                return pagedView; 
            }
            set { pagedView = value; }
        }

        #endregion
    }

    #region 为翻页所做的虚拟列表
    class MyList : IEnumerable, IEnumerator, INotifyCollectionChanged
    {
        //对应翻页列表，该对象的Count属性随翻页组件的Count属性变化
        private BaseObjectList _list = null;

        private int _count;
        private int _current = -1;

        public MyList(BaseObjectList list)
        {
            _list = list;
            list.PropertyChanged += list_PropertyChanged;
        }

        void list_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Count")
            {
                Count = _list.Count;
            }
        }

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                _current = -1;
                OnCollectionChanged();
            }
        }
        public IEnumerator GetEnumerator()
        {
            _current = -1;
            return this;
        }

        public object Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            if (_current == _count - 1)
            {
                return false;
            }
            _current++;
            return true;
        }

        public void Reset()
        {
            _current = -1;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged() 
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
    #endregion
}
