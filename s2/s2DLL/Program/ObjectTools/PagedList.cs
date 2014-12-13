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
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 重构后的分页列表，主要是hql语句或者sql语句按上传方式处理了。
    /// </summary>
    public class PagedList : BasePagedList
    {
        #region SumNames 求和字段名称，以","分隔
        /// <summary>
        /// 在开始加载总体信息时，要进行求和的字段名称。以","分隔
        /// </summary>
        public String SumNames { get; set; }
        #endregion

        #region Names SQL语句执行后，每列名称，以","分隔
        /// <summary>
        /// SQL语句执行时，每列名称
        /// </summary>
        public String Names { get; set; }
        #endregion

        #region Count 总共数据个数
        /// <summary>
        /// 总数据个数，在加载总体信息时获得，并赋值。
        /// </summary>
        private int count = -1;
        override public int Count
        {
            get {  if (count < 0) return 0;
                return count; }
            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged("Count");
                }
            }
        }
        #endregion

        #region HQL 要执行的SQL或者HQL语句，当其发生变化时，加载数据
        private string hql;
        public string HQL
        {
            get
            {
                return this.hql;
            }
            set
            {
                if (this.hql == value)
                {
                    return;
                }
                this.hql = value;
                //通知hql属性变化
                OnPropertyChanged("HQL");
                if (LoadOnPathChanged)
                {
                    Load();
                }
            }
        }
        #endregion

        #region SumHQL 求和所用的sql语句
        private string sumHql;
        public string SumHQL
        {
            get
            {
                return this.sumHql;
            }
            set
            {
                if (this.sumHql == value)
                {
                    return;
                }
                this.sumHql = value;
            }
        }
        #endregion

        #region CountHQL 产生加载总数的hql语句，子类可以重载此函数，以不同方式加载总数。
        public virtual string CountHQL
        {
            get
            {
                if (SumHQL != null)
                {
                    return SumHQL;
                }
                return HQL;
            }
        }
        #endregion

        #region PageHQL 产生加载每页数据所需hql语句，子类可以重载此函数，以不同方式加载每页数据。
        public virtual string PageHQL
        {
            get
            {
                return HQL;
            }
        }
        #endregion

        #region ReturnNames sql语句执行后，返回结果对应的列名称，子类可以重载。
        public virtual string ReturnNames
        {
            get
            {
                return Names;
            }
        }
        #endregion

        #region Load 加载合计数据
        public override void Load()
        {
            string countHQL = CountHQL;
            if (countHQL == null)
            {
                return;
            }

            int nowIndex = pageIndex;
            PageIndex = nowIndex;
            //Path后跟求和字段名称，代表求总数
            string uuid = System.Guid.NewGuid().ToString();
            WebClient client = new WebClient();
            IsBusy = true;
            OnLoading();
            State = State.StartLoad;
            //采用POST方式加载总数
            string str = WebClientInfo.BaseAddress + "/" + Path.Replace("%", "%25").Replace("#", "%23").Replace("^", "<") + "/" + SumNames + "?uuid=" + uuid;
            Uri uri = new Uri(str);
            client.UploadStringCompleted += new UploadStringCompletedEventHandler(Load_UploadStringCompleted);
            client.UploadStringAsync(uri, countHQL.Replace("^", "<"));
        }

        void Load_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //更新数据
                JsonObject item = JsonValue.Parse(e.Result) as JsonObject;
                FromJson(item);
                //如果没有数据，必须通知完成，因为不会再有页面变化通知，这时，认为发生了错误，错误状态为没有数据
                if (Count == 0)
                {
                    State = State.LoadError;
                    Error = "没有满足条件的数据";
                    OnDataLoaded(e);
                    OnCompleted(e);
                    //IsBusy在有数据的情况下，不发生变化
                    IsBusy = false;
                }
                //通知总数发生了变化
                OnPropertyChanged("Count");
            }
            else
            {
                IsBusy = false;
            }
        }
        #endregion

        #region LoadDetail 加载一页数据
        /// <summary>
        /// 加载当前页数据
        /// </summary>
        override public void LoadDetail()
        {
            string pageHQL = PageHQL;
            if (pageHQL == null)
            {
                return;
            }
            IsBusy = true;
            OnLoading();
            State = State.StartLoad;
            string uuid = System.Guid.NewGuid().ToString();
            string str;
            //如果，有名字，在路径中添加名字，SQL语句的查询需要自己指定名字，而HQL语句的查询则不需要
            if (Names != null)
            {
                str = WebClientInfo.BaseAddress + "/" + Path.Replace("%", "%25").Replace("#", "%23").Replace("^", "<") + "/" + Names + "/" + pageIndex + "/" + PageSize + "?uuid=" + uuid;
            }
            else
            {
                str = WebClientInfo.BaseAddress + "/" + Path.Replace("%", "%25").Replace("#", "%23").Replace("^", "<") + "/" + pageIndex + "/" + PageSize + "?uuid=" + uuid;
            }
            Uri uri = new Uri(str);
            WebClient client = new WebClient();
            client.UploadStringCompleted += new UploadStringCompletedEventHandler(LoadDetail_UploadStringCompleted);
            client.UploadStringAsync(uri, pageHQL.Replace("^", "<"));
        }

        void LoadDetail_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //更新数据
                JsonArray items = JsonValue.Parse(e.Result) as JsonArray;
                this.FromJson(items);
                State = State.Loaded;
            }
            else
            {
                State = State.LoadError;
                Error = e.Error.GetMessage();
            }
            IsBusy = false;
            //通知数据加载完成
            OnDataLoaded(e);
            OnCompleted(e);
        }
        #endregion

        #region FromJson 从Json串转换总体信息
        /// <summary>
        /// 把求和等总体数据从json串转换为对象属性，其中包括Count内容。
        /// </summary>
        /// <param name="obj"></param>
        private void FromJson(JsonObject item)
        {
            if (item == null) return;
            foreach (string key in item.Keys)
            {
                //JsonPrimitive value = (JsonPrimitive)item[key];
                //this.NewGetType().GetProperty(key).SetValue(this, value as JsonPrimitive, null);
                object value = item[key];
                value = value.JsonConvert(this.NewGetType().GetProperty(key).PropertyType);
                this.NewGetType().GetProperty(key).SetValue(this, value, null);
            }
        }
        #endregion

        #region OnCountChanged 重载通知总数变化过程，不产生总数变化通知，在总数加载完后，自己通知
        public override void OnCountChanged()
        {
        }
        #endregion
    }
}
