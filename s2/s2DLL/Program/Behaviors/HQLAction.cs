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
using System.Json;
using Com.Aote.ObjectTools;
using System.ComponentModel;
using Com.Aote.Logs;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 执行HQL语句，即支持自己单独执行，也支持批处理的执行。
    /// 支持自己单独执行的目的是，在某些场合，去后台进行数据库操作只有一条HQL语句。
    /// 这时，就不用配置BatchExcuteAction了，直接配置HQLAction即可。
    /// </summary>
    public class HQLAction : BaseAsyncAction
    {
        private static Log Log = Log.GetInstance("Com.Aote.Behaviors.HQLAction");

        /// <summary>
        /// 自己执行时，后台服务的地址信息，将根据这个信息构造Uri，调用后台服务，执行数据库操作。
        /// 这个信息在xaml文件中配置，可以引用已经定义好的静态资源。
        /// 比如，可以把数据库服务的基础地址放到应用程序资源里，这里只有通过StaticResource引用即可。
        /// </summary>
        public WebClientInfo WebClientInfo { get; set; }

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
            DependencyProperty.Register("HQL", typeof(string), typeof(HQLAction), 
            new PropertyMetadata(new PropertyChangedCallback(OnHQLChanged)));

        public static void OnHQLChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            Log.Debug("new hql=" + args.NewValue);
        }
        #endregion

        #region Type 语句类型，默认为HQL，也可以指定为SQL
        private string type = "hql";
        public string Type 
        {
            get { return type; }
            set { type = value; }
        }
        #endregion

        /// <summary>
        /// 动作的执行过程，首先调用获取执行语句的方法获得后台服务所需Json格式的语句。
        /// 然后把这个执行语句发送到统一的批量数据执行服务上，执行后台数据库操作。
        /// </summary>
        public override void Invoke()
        {
            DBService.Get(WebClientInfo.BaseAddress).Invoke(this, InvokeToJson);
        }

        /// <summary>
        /// 获得批量执行中执行HQL语句的Json格式的语句描述。格式为：
        /// {"operator":"hql", "data":"hql语句"}
        /// </summary>
        /// <returns></returns>
        public JsonObject InvokeToJson()
        {
            Log.Debug("hqlaction hql=" + HQL);
            //HQL语句为空，返回空
            if (HQL == null)
                return null;
            JsonObject result = new JsonObject();
            //设置为执行HQL语句
            result["operator"] = type;
            result["data"] = HQL;
            return result;
        }

        #region CanSave 是否保存对象属性到数据库
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(HQLAction),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveChanged)));

        private static void OnCanSaveChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            HQLAction bea = (HQLAction)dp;
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

        private bool isOld;
        public bool IsOld
        {
            get { return isOld; }
            set
            {
                if (isOld != value)
                {
                    isOld = value;
                    if (isOld)
                    {
                        Invoke();
                    }
                }
            }
        }
    }
}
