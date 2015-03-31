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
using System.Collections.Generic;
using System.Reflection;
using System.Json;
using Com.Aote.ObjectTools;
using System.ComponentModel;
using System.Windows.Interactivity;
using Com.Aote.Utils;
using System.Windows.Markup;
using System.Linq;
using Com.Aote.Logs;
using System.Text;

namespace Com.Aote.Behaviors
{
    public class Batchs : List<BatchInfo> { }

    /// <summary>
    /// 处理的业务模式：有个主对象，有一组从对象，主对象和从对象之间无关联，需要在主对象和从对象之间建立关联并保存
    /// 主对象由EntityType来决定，从对象集合由sql或hql查询得到，如果从sql得到，需要提供列名Names
    /// </summary>
    public class DeferredReferenceBatchInfo : BatchInfo
    {
        /// <summary>
        /// 如果是sql语句，需要给出各列列名
        /// </summary>
        public String Names { get; set; }

        /// <summary>
        /// 主实体名
        /// </summary>
        public String EntityType { get; set; }
        /// <summary>
        /// 从实体名
        /// </summary>
        public String ReferenceEntityType { get; set; }

        /// <summary>
        /// 值为sql或hql
        /// </summary>
        public String Path { get; set; }

        /// <summary>
        /// 主表实体hbm.xml中配的从表集合的名字
        /// </summary>
        public String Reference { get; set; }

        /// <summary>
        /// 返回ToJson方法
        /// </summary>
        /// 
        public override MethodInfo Method
        {
            get
            {
                return this.GetType().GetMethod("ToJson");
            }
        }

        #region HQL 要执行的HQL或者SQL语句
        /// <summary>
        /// 要执行的HQL语句，可以采用绑定机制，根据界面数据产生这个语句。
        /// </summary>
        public string HQL
        {
            get { return (string)GetValue(HQLProperty); }
            set { SetValue(HQLProperty, value); }
        }

        public static readonly DependencyProperty HQLProperty =
            DependencyProperty.Register("HQL", typeof(string), typeof(DeferredReferenceBatchInfo),
            null);
        #endregion

        /// <summary>
        /// 把需要的数据转成json对象
        /// </summary>
        /// <returns></returns>
        public JsonObject ToJson()
        {
            JsonObject result = new JsonObject();
            GeneralObject go = Source as GeneralObject;
            //设置为建立引用
            result["operator"] = "reference";
            //主对象
            result["entity"] = EntityType;
            result["data"] = go.ToJson();
            result["reference"] = Reference;
            //从对象
            result["entity2"] = ReferenceEntityType;
            result["names"] = Names;
            result["path"] = Path;
            result["hql"] = HQL;
            return result;
        }
    }

    /// <summary>
    /// 表示一个可以在xaml文件中进行配置的，把后台数据库操作数据交给批量执行动作的方法信息。
    /// 包括调用的对象以及对象的方法。
    /// </summary>
    public class BatchInfo : DependencyObject, IInitable
    {
        #region Source 调用的对象
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(IName), typeof(BatchInfo),
            new PropertyMetadata(null));

        public IName Source
        {
            get { return (IName)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion


        #region CanSave 是否保存对象属性到数据库
        //private bool canSave = true;
        
        //public bool CanSave
        //{
        //    get { return canSave; }
        //    set { canSave = value; }
        //} 

        
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(BatchInfo),new PropertyMetadata(true));
        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set
            {
                SetValue(CanSaveProperty, value);
            }
        } 
        #endregion

        /// <summary>
        /// 调用对象的方法名
        /// </summary>
        public string MethodName { get; set; } 

        /// <summary>
        /// 设置对象及方法名后，自动获得的方法，批处理动作将调用这个方法获得具体数据。
        /// </summary>
        public virtual MethodInfo Method
        {
            get
            {
                return Source.GetType().GetMethod(MethodName);
            }
        }

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

    /// <summary>
    /// 成批执行一批后台数据库操作，从要执行后台数据库操作的对象中获取Json格式的执行语句，一次性给后台服务。
    /// 后台批量数据库操作服务完成后，应当返回一个Json格式的结果。
    /// 在这个结果中表明哪些对象的属性在后台服务完成后要进行改变。
    /// 这个特点在数据保存后，返回数据编号等场合特别有用。目前，这个功能还没有实现。
    /// 要执行后台操作的对象已经获取后台操作数据的方法在xaml文件中，按BatchInfo组的方式提供。
    /// </summary>
    [ContentProperty("Batchs")]
    public class BatchExcuteAction : BaseAsyncAction
    {
        private static Log Log = Log.GetInstance("Com.Aote.Behaviors.BatchExcuteAction");

        /// <summary>
        /// 执行后台操作的地址信息，将根据这个信息构造Uri，调用后台服务，执行一批数据库操作。
        /// 这个信息在xaml文件中配置，可以引用已经定义好的静态资源。
        /// 比如，可以把数据库服务的基础地址放到应用程序资源里，这里只有通过StaticResource引用即可。
        /// </summary>
        public WebClientInfo WebClientInfo { get; set; }

        /// <summary>
        /// 要执行批量操作的对象以及获取批量操作数据的方法，每一项内容采用BatchInfo的方式给出。
        /// 这个信息在xaml文件中进行配置。
        /// </summary>
        private List<BatchInfo> batchs = new List<BatchInfo>();
        public List<BatchInfo> Batchs { get { return batchs; } }

        public override void Init(object ui)
        {
            base.Init(ui);
            //让Batchs获得初始化机会
            foreach (BatchInfo bi in Batchs)
            {
                bi.Init(ui);
            }
        }

        #region CanSave 是否保存对象属性到数据库
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(BatchExcuteAction),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveChanged)));

        private static void OnCanSaveChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            BatchExcuteAction bea = (BatchExcuteAction)dp;
            if (bea.CanSave)
            {
                bea.Invoke();
            }
            bea.CanSave = false;
        }

        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set
            {
                SetValue(CanSaveProperty, value);
            }
        }
        #endregion

        /// <summary>
        /// 执行批量数据操作，调用Batchs里配置的对象方法，获得批量操作数据。组织成整体，发送给后台服务。
        /// </summary>
        public override void Invoke()
        {
            //触发开始保存事件
            OnSaving();
            
            List<JsonObject> jsons = new List<JsonObject>();
            //调用每一个方法，获得要远程执行的json串
            foreach(BatchInfo batch in Batchs)
            {  
                //如果不允许保存
                if (!batch.CanSave)
                {
                    Log.Debug("batch.cansave=" + batch.CanSave);
                    continue;
                }

                object o = null;
                if (batch is DeferredReferenceBatchInfo)
                    o = batch.Method.Invoke(batch, null);
                else
                    o = batch.Method.Invoke(batch.Source, null);
                //如果返回空，这项批处理不执行
                if (o == null)
                {
                    Log.Debug("o=" + o);
                    continue;
                }

                Log.Debug("cansave=" + batch.CanSave + ", o=" + (o == null));

                //如果是Json对象，把Json对象加入列表中
                if (o is JsonObject)
                {
                    JsonObject json = (JsonObject)o;
                    //添加名字
                    json["name"] = batch.Source.Name;
                    jsons.Add(json);
                }
                //如果是Json数组，把数组里的对象添加进去
                else if (o is JsonArray)
                {
                    JsonArray ja = (JsonArray)o;
                    foreach (JsonObject obj in ja)
                    {
                        jsons.Add(obj);
                    }
                }
                else
                {
                    throw new Exception("必须是Json对象，或者Json数组");
                }
            }
            JsonArray array = new JsonArray(jsons);
            //传递到后台更新
            Uri uri = new Uri(WebClientInfo.BaseAddress);
            WebClient client = new WebClient();
            //准备好发送时，发送json数据
            client.UploadStringCompleted += (o, e) =>
            {
                IsBusy = false;
                if (e.Error != null)
                {
                    WebException we = (WebException)e.Error;
                    HttpWebResponse response = (System.Net.HttpWebResponse)we.Response;

                    String err = "网络连接错误，请检查。";
                    try
                    {
                        byte[] b = Convert.FromBase64String(response.Headers["Warning"]);
                        err = Encoding.UTF8.GetString(b, 0, b.Length);
                    }
                    catch(Exception)
                    {

                    }

                    Error = "错误码：" + (int)response.StatusCode + " 错误：" + err;
                    State = State.Error;
                    //MessageBox.Show("操作失败。" + Error);
                }
                else
                {
                    //把后台回送的数据重新发给相关对象执行
                    if (e.Result != null)
                    {
                        CallBack((JsonObject)JsonValue.Parse(e.Result));
                    }
                    State = State.End;
                }
                //通知数据提交过程完成
                OnCompleted(e);
            };
            IsBusy = true;
            State = State.Start;
            client.UploadStringAsync(uri, array.ToString());
        }

        //把后台回送的数据发给前台对象，回送数据格式为{对象名:{对象值}}
        private void CallBack(JsonObject array)
        {
            foreach (string key in array.Keys)
            {
                Log.Debug("key=" + key);
                var o = (from p in Batchs where p.Source.Name == key && p.Source is IFromJson select p.Source).FirstOrDefault();
                if (o != null)
                {
                    Log.Debug("o=" + o);
                    (o as IFromJson).FromJson((JsonObject)array[key]);
                }
            }
        }

        #region Saving 开始保存事件，在保存前触发
        /// <summary>
        /// 开始保存事件，在开始保存数据时触发
        /// </summary>
        public event EventHandler Saving;
        public void OnSaving()
        {
            if (Saving != null)
            {
                Saving(this, null);
            }
        }
        #endregion
    }
}
