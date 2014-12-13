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
using System.ComponentModel;
using Com.Aote.ObjectTools;
using System.Windows.Interactivity;
using Com.Aote.Utils;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 为一般异步动作提供了基础实现。
    /// 对象从DependencyObject继承，以便所有异步动作都可以有依赖属性。
    /// 同时继承INotifyPropertyChanged，以便所有异步动作都能够对外通知属性改变
    /// 继承IAsyncAction，完成异步动作的常用处理。
    /// 异步动作的具体执行过程（Invoke方法）是抽象的，交给子类去实现。
    /// </summary>
    public abstract class BaseAsyncAction : DependencyObject,
        INotifyPropertyChanged, IAsyncAction, IInitable
    {
        #region PropertyChanged事件
        /// <summary>
        /// 属性改变事件，用于对象属性改变时，通知外界。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 提供属性改变事件的通知方法，子类及自身通过调用这个方法通知外界，属性改变了
        /// </summary>
        /// <param name="info">变化了的属性名称</param>
        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region IAsyncAction Members

        /// <summary>
        /// 是否正在工作属性的内部表示
        /// </summary>
        private bool isBusy = false;

        /// <summary>
        /// 实现了是否忙属性，当是否忙属性改变时，调用属性改变事件通知方法，通知外部属性改变了。
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        /// <summary>
        /// 实现了工作完成事件，当异步动作完成工作后，将触发该事件。
        /// </summary>
        public event AsyncCompletedEventHandler Completed;

        /// <summary>
        /// 提供工作完成事件的通知方法，子类及自身在工作完成后，调用这个方法通知外部工作完成了。
        /// </summary>
        /// <param name="args"></param>
        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }

        /// <summary>
        /// 异步对象工作状态。
        /// </summary>
        private State state;
        public State State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(BaseAsyncAction),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion
        
        /// <summary>
        /// 异步动作名称，可以在xaml文件中进行设置，以方便信息显示。
        /// </summary>
        public string Name { get; set; }

        #endregion

        public abstract void Invoke();

        #region CanSave 是否执行

        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(BaseAsyncAction), new PropertyMetadata(true));
        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set
            {
                SetValue(CanSaveProperty, value);
            }
        }
        #endregion

        #region IInitable Members

        //提供环境信息的对象，可以是应用程序或者界面元素之一
        private object UI;

        //Loaded事件，触发这个事件通知配置等对象开始工作
        public event RoutedEventHandler Loaded;
        private void OnLoaded()
        {
            if (Loaded != null)
            {
                Loaded(UI, new RoutedEventArgs());
            }
        }


        //是否进行初始化处理
        public bool IsInited { set; get; }

        virtual public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            this.IsInited = true;
            this.OnInitFinished();
        }


        //初始化完成事件
        public event RoutedEventHandler InitFinished;
        public void OnInitFinished()
        {
            if (this.InitFinished != null)
            {
                InitFinished(this, null);
            }
        }
        
        public object FindResource(string name)
        {
            if (name == "this")
                return this;
            return UI.FindResource(name);
        }

        #endregion
    }
}
