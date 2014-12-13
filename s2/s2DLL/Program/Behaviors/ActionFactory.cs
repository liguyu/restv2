using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Interactivity;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Com.Aote.ObjectTools;
using Com.Aote.Utils;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 代表一个异步动作组，异步动作组里的异步对象即可以串行执行（由SyncActionFactory类实现）,
    /// 也可以并发执行（由AsyncActionFactory）类实现。
    /// 异步动作组可以互相包含，这样，就可以完成复杂的动作流程控制了。
    /// 从BaseAsyncAction继承，保证了他具有一般异步动作的特性。
    /// 从IList<IAction>，说明他可以把IAction作为自己的子。
    /// 从IList继承，保证了可以在xaml文件里添加IAction作为异步动作组的子。
    /// </summary>
    public abstract class ActionFactory : BaseAsyncAction, IList<IAction>, IList
    {
        /// <summary>
        /// 异步动作组的内部表示，所有异步动作将存放到这个集合中。
        /// </summary>
        private ObservableCollection<IAction> _actions = new ObservableCollection<IAction>();

        /// <summary>
        /// 在构造函数中，将监听_actions的集合变化，当有新的异步动作加入时，将监听这异步动作的完成事件。
        /// 当某个异步动作完成时，调用子类的OnActionCompleted，完成子异步动作完成后的处理过程。
        /// 串行与并发动作组处理子异步动作完成的方式是不同的。
        /// </summary>
        public ActionFactory()
        {
            _actions.CollectionChanged += (o, e) =>
            {
                foreach (IAction b in e.NewItems)
                {
                    // 只监听异步动作的操作完成事件，非异步动作不管。
                    if (b is IAsyncAction)
                    {
                        IAsyncAction a = b as IAsyncAction;
                        a.Completed += (o1, e1) =>
                        {
                            OnActionCompleted(a, e1);
                        };
                    }
                }
            };
        }

        /// <summary>
        /// 某个子动作完成后，接下来怎么做，由子类实现。串行异步动作组与并发异步动作组处理子动作完成事件的方式不同。
        /// </summary>
        /// <param name="action">完成工作的子动作</param>
        /// <param name="args">完成工作的子动作，工作完成时的事件参数</param>
        protected abstract void OnActionCompleted(IAsyncAction action, AsyncCompletedEventArgs args);

        //重载Init方法，通知子初始化
        public override void Init(object ui)
        {
            base.Init(ui);
            foreach(IInitable obj in (from p in _actions where p is IInitable select p))
            {
                obj.Init(ui);
            }
        }

        #region IList<IAction> Members

        public int IndexOf(IAction item)
        {
            return _actions.IndexOf(item);
        }

        public void Insert(int index, IAction item)
        {
            _actions.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _actions.RemoveAt(index);
        }

        public IAction this[int index]
        {
            get
            {
                return _actions[index];
            }
            set
            {
                _actions[index] = value;
            }
        }

        #endregion

        #region ICollection<IAction> Members

        public void Add(IAction item)
        {
            _actions.Add(item);
        }

        public void Clear()
        {
            _actions.Clear();
        }

        public bool Contains(IAction item)
        {
            return _actions.Contains(item);
        }

        public void CopyTo(IAction[] array, int arrayIndex)
        {
            _actions.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _actions.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(IAction item)
        {
            return _actions.Remove(item);
        }

        #endregion

        #region IEnumerable<IAction> Members

        public IEnumerator<IAction> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            _actions.Add((IAction)value);
            return _actions.Count;
        }

        public bool Contains(object value)
        {
            return _actions.Contains((IAction)value);
        }

        public int IndexOf(object value)
        {
            return _actions.IndexOf((IAction)value);
        }

        public void Insert(int index, object value)
        {
            _actions.Insert(index, (IAction)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            _actions.Remove((IAction)value);
        }

        object IList.this[int index]
        {
            get
            {
                return _actions[index];
            }
            set
            {
                _actions[index] = (IAction)value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            _actions.CopyTo((IAction[])array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        #endregion
    }
}
