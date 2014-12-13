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
using System.Collections.Generic;
using System.Json;
using Com.Aote.Logs;
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 管理实体类型等信息，这个对象到后台调用服务获取hibernate元数据信息。
    /// 把hibernate的元数据信息转换成客户类型信息，进行统一管理。
    /// </summary>
    public class CustomTypes : DependencyObject, ILoadable, IAsyncObject, INotifyPropertyChanged
    {
        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.CustomTypes");

        /// <summary>
        /// 用于去后台获取数据的基本地址描述，在xaml中进行配置。
        /// </summary>
        public WebClientInfo WebClientInfo { get; set; }

        //去后台加载地址的路径，一旦发生变化，直接调用加载过程
        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if (path != value)
                {
                    path = value;
                    if (path != null)
                    {
                        Load();
                    }
                }
            }
        }

        #region 单例
        private static CustomTypes instance;
        public static CustomTypes GetInstance()
        {
            return instance;
        }
        /// <summary>
        /// 对象可以在xaml文件中构造，只构造一个对象实例。以后对象可以通过单例模式
        /// 进行访问。也可以访问xaml文件中的StaticResource，获得对象。
        /// </summary>
        public CustomTypes()
        {
            instance = this;
        }
        #endregion

        /// <summary>
        /// 类型列表，按照名称进行管理，key存放的是类型名称，value存放具体类型
        /// </summary>
        private Dictionary<string, CustomType> _types = new Dictionary<string, CustomType>();

        /// <summary>
        /// 根据名称获取客户类型，如果列表里不存在，创建空客户类型，放到列表中。
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <returns>客户类型</returns>
        public CustomType GetType(string name)
        {
            CustomType type;

            if (!_types.TryGetValue(name, out type))
            {
                type = new CustomType(GetType());
                _types[name] = type;
            }
            return type;
        }

        #region ILoadable Members

        /// <summary>
        /// 开始加载事件，对象开始到后台加载元数据时，触发该事件。
        /// </summary>
        public event EventHandler Loading;
        public void OnLoading()
        {
            if (Loading != null)
            {
                Loading(this, null);
            }
        }

        /// <summary>
        /// 数据加载完成事件，对象加载完数据后，触发该事件。
        /// </summary>
        public event System.ComponentModel.AsyncCompletedEventHandler DataLoaded;
        public void OnDataLoaded(AsyncCompletedEventArgs args)
        {
            if (DataLoaded != null)
            {
                DataLoaded(this, args);
            }
        }

        /// <summary>
        /// 开始到后台加载类型信息
        /// </summary>
        public void Load()
        {
            Uri uri = new Uri(WebClientInfo.BaseAddress + Path);
            WebClient client = new WebClient();
            client.OpenReadCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    JsonObject types = JsonValue.Load(a.Result) as JsonObject;
                    //设置所有动态类型的属性
                    foreach (string type in types.Keys)
                    {
                        CustomType cType = new CustomType(typeof(GeneralObject));
                        JsonObject attrs = (JsonObject)types[type];
                        foreach (string attr in attrs.Keys)
                        {
                            string attrType = attrs[attr];
                            cType.AddProperty(attr, attrType.ToType());
                        }
                        //把类型放到类型表中
                        _types[type] = cType;
                    }
                    State = State.Loaded;
                }
                else
                {
                    State = State.LoadError;
                    Error = a.Error.GetMessage();
                }
                IsBusy = false;
                //通知加载完成
                OnCompleted(a);
                OnDataLoaded(a);
            };
            IsBusy = true;
            OnLoading();
            State = State.StartLoad;
            client.OpenReadAsync(uri);
        }
        #endregion

        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region IAsyncObject Members
        /// <summary>
        /// 异步操作名称，方便在错误时显示错误信息
        /// </summary>
        public string Name { get; set; }

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
            DependencyProperty.Register("Error", typeof(string), typeof(CustomTypes),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion
        
        #endregion
    }

}
