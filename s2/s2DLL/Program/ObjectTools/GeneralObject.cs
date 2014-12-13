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
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Json;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using System.Windows.Browser;
using Com.Aote.Logs;
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 表示一个普通对象，普通对象可以到后台加载自己的属性数据。实现了动态属性机制。
    /// </summary>
    public class GeneralObject : CustomTypeHelper, IAsyncObject, ILoadable, IFromJson
    {
        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.GeneralObject");

        #region IAsyncObject Members

        /// <summary>
        /// 异步对象的名称，方便显示错误数据。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工作完成事件，包括数据加载
        /// </summary>
        public event AsyncCompletedEventHandler Completed;
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

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(GeneralObject),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        #endregion

        #region ILoadable Members
        public event EventHandler Loading;
        public void OnLoading()
        {
            if (Loading != null)
            {
                Loading(this, null);
            }
        }

        public event AsyncCompletedEventHandler DataLoaded;
        public void OnDataLoaded(AsyncCompletedEventArgs args)
        {
            if (DataLoaded != null)
            {
                DataLoaded(this, args);
            }
        }

        /// <summary>
        /// 开始加载数据
        /// </summary>
        public void Load()
        {
            string uuid = System.Guid.NewGuid().ToString();
            Uri uri;
            if (Path == null)
            {
                uri = new Uri(WebClientInfo.BaseAddress);
            }
            else if (Path != null && Path.Equals("null"))
            {
                return;
            }
            else
            {
                uri = new Uri(WebClientInfo.BaseAddress + "/" + Path.Replace("%", "%25").Replace("#", "%23") + "?uuid=" + uuid);
            }
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                Log.Debug("加载数据完成");
                if (a.Error == null)
                {
                    //更新数据
                    JsonObject item = JsonValue.Parse(a.Result) as JsonObject;
                    FromJson(item);
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
            Log.Debug("开始加载数据, 路径为: " + WebClientInfo.BaseAddress + "/" + Path);
            client.DownloadStringAsync(uri);
        }

        #endregion

        #region WebClientInfo 用于去后台获取数据的基本地址描述，在xaml文件中进行配置
        public WebClientInfo WebClientInfo { get; set; }
        #endregion

        #region LoadOnPathChanged 是否在Path改变时，主动获取数据，有些对象只能在外界要求下，才开始获取数据

        private bool loadOnPathChanged = true;

        /// <summary>
        /// 是否在Path改变时，主动获取数据，有些对象只能在外界要求下，才开始获取数据。默认情况为True，也就是在Path
        /// 改变时，将主动获取数据。
        /// </summary>
        public bool LoadOnPathChanged 
        {
            get { return loadOnPathChanged; }
            set { loadOnPathChanged = value; } 
        } 
        #endregion

        #region Path 获取属性的路径，一旦发生改变，将重新获取属性，但是如果指明属性路径改变时，不加载数据，则不会这么做。

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnPathChanged)));

        private static void OnPathChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject go = (GeneralObject)dp;
            //如果指明Path改变时，不加载数据，则只有当外界要求，加载数据时，才加载
            if (go.loadOnPathChanged)
            {
                go.Load();
            }
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        #endregion

        #region EntityType 对象类型，对应后台Hibernate类
        private string entityType;

        /// <summary>
        /// 对象类型，对应后台Hibernate类，设置时，从类型信息表中获取类型，设置本对象类型为获取到的类型。
        /// </summary>
        public string EntityType 
        {
            get { return this.entityType; }
            set
            {
                this.entityType = value;
                //设置类型
                SetCustomType(CustomTypes.GetInstance().GetType(value));
            }
        }
        #endregion

        #region Source 数据来源，主要用于列表中数据选中后编辑
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(GeneralObject), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// 当数据源发生变化时，复制数据源中的属性到本对象。
        /// </summary>
        /// <param name="dp">代表对象自身</param>
        /// <param name="args">改变的新值为获取数据的源</param>
        public static void OnSourceChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject edit = (GeneralObject)dp;
            GeneralObject source = args.NewValue as GeneralObject;
            if (source == null)
            {
                return;
            }
            edit.CopyFrom(source);
            //对象变为未修改，不是新的
            edit.IsNew = false;
            edit.IsModified = false;
        }

        public GeneralObject Source
        {
            get { return (GeneralObject)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region NotSave 不发送后台的属性名列表，逗号分隔。目的是提高性能。
        public static readonly DependencyProperty NotSaveProperty =
            DependencyProperty.Register("NotSave", typeof(string), typeof(GeneralObject), null);

        public string NotSave
        {
            get { return (string)GetValue(NotSaveProperty); }
            set { SetValue(NotSaveProperty, value); }
        }
        #endregion

        #region NotEmpty 不给一对多关系设置空集合。
        private bool notEmpty = false;
        public bool NotEmpty 
        {
            get { return notEmpty; }
            set
            {
                notEmpty = value;
            }
        }
        #endregion

        #region IsModified 是否修改过
        private bool isModified;

        /// <summary>
        /// 代表对象是否修改过，对象收到动态属性改变时，则认为对象修改过了。
        /// </summary>
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (isModified != value)
                {
                    isModified = value;
                    OnPropertyChanged("IsModified");
                }
            }
        }
        #endregion

        #region IsOld 对象是否没有反应数据的新状态，如果设置为true，自动加载，加载完成后，设置成false
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
                        Load();
                        isOld = false;
                    }
                }
            }
        }
        #endregion

        #region IsNew 是否新对象，也就是还没有提交到数据库的对象
        private bool isNew = true;
        
        /// <summary>
        /// 对象是否新建
        /// </summary>
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                if (isNew != value)
                {
                    isNew = value;
                    OnPropertyChanged("IsNew");
                }
            }
        }
        #endregion

        #region IsEmptyRow 是否空行对象
        public bool IsEmptyRow
        {
            get
            {
                if (List == null)
                {
                    return false;
                }
                return this == List.EmptyRow;
            }
        }
        #endregion

        #region IsInit 初始状态是指没有修改过的新对象，也就是调用new以后的状态
        public static readonly DependencyProperty IsInitProperty =
            DependencyProperty.Register("IsInit", typeof(bool), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnIsInitChanged)));

        private static void OnIsInitChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject go = (GeneralObject)dp;
            //如果指明Path改变时，不加载数据，则只有当外界要求，加载数据时，才加载
            if (go.IsInit)
            {
                go.New();
            }
        }

        public bool IsInit
        {
            get { return (bool)GetValue(IsInitProperty); }
            set { SetValue(IsInitProperty, value); }
        }
        #endregion

        //所属列表，添加到列表时，设置
        public BaseObjectList List { get; set; }

        #region Index 所属列表索引
        public int Index { get {
            //如果是翻页组件，根据行数和页数计算序号
            if (List is BasePagedList)
            {
                BasePagedList bpl = (BasePagedList)List;
                return (bpl.PageSize * bpl.PageIndex) + List.IndexOf(this) + 1;
            }
            return List.IndexOf(this) + 1; 
        
        } }
        #endregion


        

        #region IndexOf 返回该对象在某个列表中的索引号
        public int IndexOf(BaseObjectList list)
        {
            return list.IndexOf(this);
        }
        #endregion

        public GeneralObject()
        {
            //监听对象的动态属性变化事件，非动态属性不影响对象的是否修改状态。当对象产生动态属性变化事件时
            this.DynamicPropertyChanged += (o, e) => 
            { 
                IsModified = true;
                IsInit = false;
            };
            //监听初始话事件完成，完成后重新自己做一次New的过程
            this.InitFinished += (o, e) =>
            {
                this.New();
            };
        }

        #region 重载ToString()，返回实体类型及对象id号，以方便获取对象的唯一标识。
        public override string ToString()
        {
            //如果对象有配置的名字，返回名字
            if (Name != null)
            {
                return Name;
            }
            object id = GetPropertyValue("id");
            if (EntityType == null && id == null)
            {
                return base.ToString();
            }
            else if (EntityType != null)
            {
                return EntityType + ":" + id;
            }
            return id.ToString();
        }
        #endregion

        #region 重载对象相等方法(Equals)
        /// <summary>
        /// 重载对象相等方法，两个对象只要id号相同则相等。如果其中一个没有id号，则不相等。如果都没有id号，那么
        /// 只有引用相等时才相等。
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            //如果类型不同，或者对象为空，一定不相等
            if(obj == null || !(obj is GeneralObject))
            {
                return false;
            }
            GeneralObject go = obj as GeneralObject;
            object thisId = GetPropertyValue("id");
            object otherId = go.GetPropertyValue("id");
            //如果两个都没有id号或者都没有实体类型，看引用相等否
            if ((thisId == null && otherId == null) || (EntityType == null && go.EntityType == null))
            {
                return base.Equals(obj);
            }
            //如果其中有一个id或者实体类型为空，一定不相等
            if (thisId == null || otherId == null || EntityType == null || go.EntityType == null)
            {
                return false;
            }
            //看Id以及实体类型是否相等
            bool result = thisId.ToString().Equals(otherId.ToString()) && this.EntityType.Equals(go.EntityType);
            return result;
        }
        #endregion

        #region 重载获取hash值的方法(GetHashCode)
        /// <summary>
        /// 重载获取hash值的方法，返回id的hash值，如果没有，调用基本hash值计算方法
        /// </summary>
        /// <returns>计算后的hash值</returns>
        public override int GetHashCode()
        {
            object id = GetPropertyValue("id");
            if (id == null)
            {
                return base.GetHashCode();
            }
            return id.GetHashCode();
        }
        #endregion

        #region FromJson 由Json串给对象赋值
        /// <summary>
        /// 由Json串给对象赋值，将递归进行调用，碰到JsonArray自动把JsonArray转换成ObjectList。
        /// 碰到JsonObject，自动转换成GeneralOject。
        /// </summary>
        /// <param name="item">从这个json对象给对象属性赋值</param>
        public void FromJson(JsonObject item)
        {
            //通过获取类型，给_ctype赋值
            this.NewGetType();

            //如果有实体类型，则设置实体类型
            if (item.ContainsKey("EntityType"))
            {
                EntityType = item["EntityType"];
                item.Remove("EntityType");
            }
            foreach (string key in item.Keys)
            {
                object value = item[key];
                Log.Debug("from json name=" + this.Name);
                if (key.Equals("id"))
                {
                    Log.Debug("from json id=" + value);
                }
                //如果是数组，对数组中的每一个对象调用转换过程
                if (value is JsonArray)
                {
                    //数组转换成对象列表
                    ObjectList ol = new ObjectList();
                    ol.FromJson(value as JsonArray);
                    SetCollectionProperty(key, ol);
                }
                else if (value is JsonObject)
                {
                    //JsonObject转换成一般对象
                    GeneralObject go = new GeneralObject();
                    go.FromJson(value as JsonObject);
                    SetPropertyValue(key, go, true);
                }
                else if (value is JsonPrimitive)
                {
                    this.NewGetType().GetProperty(key).SetValue(this, value as JsonPrimitive, null);
                }
                else if (value == null)
                {
                    SetPropertyValue(key, null, true);
                }
                else 
                {
                    throw new Exception("类型错误");
                }
            }
            //新加载的对象为未修改
            IsModified = false;
        }

        //设置列表属性，设置时，将监听其IsModified属性改变事件
        private void SetCollectionProperty(string key, BaseObjectList ol)
        {
            //如果有默认对象，且要设置的列表为空，采用默认对象的复制结果
            var p = (from ps in PropertySetters where ps.PropertyName == key select ps).FirstOrDefault();
            if (p != null && p.DefaultObject != null && (ol == null || ol.Count == 0))
            {
                //复制默认对象到新对象
                ObjectList go = p.DefaultObject as ObjectList;
                ObjectList ngo = new ObjectList();
                ngo.CopyFrom(go);
                ol = ngo;
            }
            SetPropertyValue(key, ol, true);
            ol.Watch("IsModified", ol_PropertyChanged);
        }

        void ol_PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as BaseObjectList).IsModified)
            {
                IsModified = true;
            }
        }
        #endregion

        #region ToJson 把数据转换成Json串
        public JsonObject ToJson()
        {
            //对象转化成json
            JsonObject json = new JsonObject();
            //放置实体类型
            json["EntityType"] = EntityType;
            foreach (KeyValuePair<string, object> kvp in _customPropertyValues.AsEnumerable())
            {
                //该属性不用保存
                if (NotSave != null && NotSave.Split(',').Contains(kvp.Key))
                {
                    continue;
                }
                //空值直接赋值
                else if (kvp.Value == null)
                {
                    PropertyInfo pi = GetProperty(kvp.Key);
                    //空一对多关系不能发送到后台，否则会清空子
                    if (pi.PropertyType == typeof(BaseObjectList))
                    {
                        continue;
                    }
                    json[kvp.Key] = null;
                }
                //整数类型，可以为空
                else if (kvp.Value is int)
                    json[kvp.Key] = (int)kvp.Value;
                //double类型，可以为空
                else if (kvp.Value is double)
                    json[kvp.Key] = (double)kvp.Value;
                //decimal
                else if (kvp.Value is decimal)
                    json[kvp.Key] = (decimal)kvp.Value;
                //bool型
                else if (kvp.Value is bool)
                    json[kvp.Key] = (bool)kvp.Value;
                //字符串
                else if (kvp.Value is string)
                    json[kvp.Key] = kvp.Value as string;
                //日期
                else if (kvp.Value is DateTime)
                {
                    //DateTime from1970 = new DateTime(1970, 1, 1,0,0,0);
                    DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                    DateTime valDate = (DateTime)kvp.Value;
                    TimeSpan ts = valDate.ToUniversalTime() - from1970;
                    json[kvp.Key] = (Int64)(ts.TotalMilliseconds + 0.5);
                }
                //列表数据
                else if (kvp.Value is BaseObjectList)
                    json[kvp.Key] = (kvp.Value as BaseObjectList).ToJson();
                //其他对象
                else if (kvp.Value is GeneralObject)
                    json[kvp.Key] = (kvp.Value as GeneralObject).ToJson();
                else
                    throw new Exception("不认识的字段类型, " + kvp.Value.GetType());
            }
            return json;
        }
        #endregion


        #region 比较如果对象属性值发生变化，返回true
        public bool CompAttrsChanged(GeneralObject other)
        {
            string selfStr = this.ToJson().ToString();
            string otherStr = other.ToJson().ToString();
            return !selfStr.Equals(otherStr);
        }
        #endregion

        #region CopyFrom 从另一个对象复制属性到自身，只复制数据，不复制状态
        public void CopyFrom(GeneralObject go)
        {
            //复制实体类型
            if (go.entityType != null)
            {
                EntityType = go.EntityType;
            }
            foreach (string key in go._customPropertyValues.Keys)
            {
                object value = go._customPropertyValues[key];
                //是列表，进行列表复制
                if (value is BaseObjectList)
                {
                    //如果列表不存在，新建列表
                    if (!_customPropertyValues.ContainsKey(key) ||  _customPropertyValues[key] == null)
                    {
                        ObjectList ol = new ObjectList();
                        SetPropertyValue(key, ol, true);
                    }
                   
                        ObjectList nol = (ObjectList)_customPropertyValues[key];
                        nol.CopyFrom(value as BaseObjectList);
                  
                 
                }
                //是对象，进行对象复制
                else if (value is GeneralObject)
                {
                    SetPropertyValue(key, value, true);
                    ////对象不存在，新建对象
                    //if (!_customPropertyValues.ContainsKey(key) || _customPropertyValues[key] == null)
                    //{
                    //    GeneralObject obj = new GeneralObject();
                    //    SetPropertyValue(key, obj, true);
                    //}
                    ////进行对象复制
                    //GeneralObject ngo = (GeneralObject)_customPropertyValues[key];
                    //ngo.CopyFrom((GeneralObject)value);
                }
                //是一般属性，调用设置属性值的过程
                else
                {
                    this.NewGetType();
                    SetPropertyValue(key, value, true);
                }
            }
        }

        #endregion

        #region CopyDataFrom 从另一个对象复制属性到自身，只复制值
        public void CopyDataFrom(GeneralObject go)
        {
            //复制实体类型
            if (go.entityType != null)
            {
                EntityType = go.EntityType;
            }
            foreach (string key in go._customPropertyValues.Keys)
            {
                object value = go._customPropertyValues[key];
                //是列表，进行列表复制
                if (value is BaseObjectList)
                {
                    //如果列表不存在，新建列表
                    if (!_customPropertyValues.ContainsKey(key) || _customPropertyValues[key] == null)
                    {
                        ObjectList ol = new ObjectList();
                        SetPropertyValue(key, ol, true);
                    }

                    ObjectList nol = (ObjectList)_customPropertyValues[key];
                    nol.CopyFrom(value as BaseObjectList);


                }
                //是对象，进行对象复制
                else if (value is GeneralObject)
                {
                    GeneralObject newgo = new GeneralObject();
                    newgo.CopyFrom((GeneralObject)value);
                    SetPropertyValue(key, newgo, true);
                }
                //是一般属性，调用设置属性值的过程
                else
                {
                    this.NewGetType();
                    SetPropertyValue(key, value, true);
                }
            }
        }

        #endregion

        /// <summary>
        /// 根据模板复制对象
        /// </summary>
        /// <param name="tobj">模板对象</param>
        /// <param name="source">数据源对象</param>

        public void CopyFromTemple(GeneralObject tobj, GeneralObject source)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>(tobj._customPropertyValues);
            foreach (KeyValuePair<string, object> item in dic)
            {
                object value = source.GetPropertyValue(item.Key);
                SetPropertyValue(item.Key, value, true);
            }            /*
            foreach (KeyValuePair<string, object> item in tobj._customPropertyValues)
            {
                object value = source.GetPropertyValue(item.Value + "");
                SetPropertyValue(item.Key, value, true);
            }*/
        }

        #region NewPropertyValue 初始化属性值，如果配置里有默认值，设置属性值为默认值，否则为null
        public void NewPropertyValue(string propertyName)
        {
            //有属性默认值
            var p = (from ps in PropertySetters where ps.PropertyName == propertyName select ps).FirstOrDefault();
            if (p != null && p.Default != null)
            {
                //设置默认属性结果
                SetPropertyValue(propertyName, p.Default, true, true);
                //如果默认属性是列表，调用列表的New过程
                if (p.Default is BaseObjectList)
                {
                    (p.Default as BaseObjectList).New();
                }
            }
            //有默认对象，复制默认对象
            else if (p != null && p.DefaultObject != null)
            {
                if (p.DefaultObject is GeneralObject)
                {
                    //复制默认对象到新对象
                    GeneralObject go = p.DefaultObject as GeneralObject;
                    GeneralObject ngo = new GeneralObject();
                    ngo.CopyFrom(go);
                    p.Object.SetPropertyValue(p.PropertyName, ngo, false, true);
                }
                else if (p.DefaultObject is ObjectList)
                {
                    //复制默认对象到新对象
                    ObjectList go = p.DefaultObject as ObjectList;
                    ObjectList ngo = new ObjectList();
                    ngo.CopyFrom(go);
                    p.Object.SetPropertyValue(p.PropertyName, ngo, false, true);
                }
            }
            else
            {
                //如果属性是集合
                if (GetProperty(propertyName).PropertyType == typeof(BaseObjectList))
                {
                    //如果属性存在，清空集合
                    BaseObjectList old = (BaseObjectList)this.GetPropertyValue(propertyName);
                    if (old != null)
                    {
                        old.New();
                    }
                    //否则，无不置空标记，设置一个空集合
                    else if(!NotEmpty)
                    {
                        SetPropertyValue(propertyName, new ObjectList(), true);
                    }
                }
                else
                {
                    SetPropertyValue(propertyName, null, true, true);
                }
            }
        }
        #endregion

        #region SaveToJson 把对象要保持的状态转换成Json指令，由批处理一起出来
        /// <summary>
        /// 保存对象，返回保存对象的Json格式的指令，不执行实际的后台保存工作。统一由BatchExcuteAction
        /// 把要执行的数据库操作数据发送给后台服务。
        /// </summary>
        /// <returns>json格式的保存对象的操作数据</returns>
        public JsonObject SaveToJson()
        {
            JsonObject result = new JsonObject();
            //设置为保存给定实体
            result["operator"] = "save";
            result["entity"] = EntityType;
            result["data"] = ToJson();
            return result;
        }
        #endregion

        #region DeleteToJson 把删除指令转成Json串，给批处理执行
        /// <summary>
        /// 删除对象，返回删除操作的Json格式指令。不执行实际的删除操作。把操作指令交给批处理动作完成。
        /// </summary>
        /// <returns></returns>
        public JsonObject DeleteToJson()
        {
            JsonObject result = new JsonObject();
            //设置为删除给定实体
            result["operator"] = "delete";
            result["entity"] = EntityType;
            //获得对象id号
            int id = (int)GetPropertyValue("id");
            result["data"] = id;
            return result;
        }
        #endregion

        #region New 新建对象
        /// <summary>
        /// 新建对象，把对象的属性清空。调用NewPropertyValue方法，在清空对象属性时，可以根据
        /// 配置内容给对象赋默认值。
        /// </summary>
        public void New()
        {
            List<CustomPropertyInfoHelper> propInfos =this.GetPropertyInfos() ;
            foreach (CustomPropertyInfoHelper key in propInfos)
            {
                this.NewPropertyValue(key._name);
            }
            //设置对象为新对象，且未修改
            IsNew = true;
            IsModified = false;
            IsInit = true;
        }
        #endregion

        #region CanSave 是否保存对象属性到数据库
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveChanged)));

        private static void OnCanSaveChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject go = (GeneralObject)dp;
            if (go.CanSave && go.IsModified)
            {
                go.Save();
            }
            go.CanSave = false;
        }

        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set { SetValue(CanSaveProperty, value); }
        }
        #endregion

        #region Clone 复制自己
        /// <summary>
        /// 复制自己
        /// </summary>
        /// <returns>复制结果</returns>
        public GeneralObject Clone()
        {
            GeneralObject go = new GeneralObject();
            go.CopyFrom(this);
            return go;
        }
        #endregion

        #region CanSaveEx 是否保存对象属性到数据库
        public static readonly DependencyProperty CanSaveExProperty =
            DependencyProperty.Register("CanSaveEx", typeof(bool), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveExChanged)));

        private static void OnCanSaveExChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject go = (GeneralObject)dp;
            if (go.CanSaveEx)
            {
                go.Save();
            }
            go.CanSaveEx = false;
        }

        public bool CanSaveEx
        {
            get { return (bool)GetValue(CanSaveExProperty); }
            set { SetValue(CanSaveExProperty, value); }
        }
        #endregion

        #region Save 保存对象，直接保存到数据库
        public void Save()
        {
            //有错误，不保存
            _errors.ToString();
            this._errors.ToString();

            //如果页面有错误内容,不能保存,提示错误.
            if (this.HasErrors)
            {
                string msg = "";
                foreach (KeyValuePair<string, string> kv in _errors)
                {
                    msg += "key=" + kv.Key + ", value=" + kv.Value + "\n";
                }
                MessageBox.Show(msg);
                return;
            }

            DBService.Get(WebClientInfo.BaseAddress).Invoke(this, SaveToJson);
        }
        #endregion

        #region Delete 删除对象，直接从数据库中删除
        public void Delete()
        {
            DBService.Get(WebClientInfo.BaseAddress).Invoke(this, DeleteToJson);
        }
        #endregion

        #region Remove 把自己从List中移走
        public void Remove()
        {
            List.Remove(this);
            //如果有显示表格，从显示表中移走，显示表格用于层次数据显示
            if (OpenedList != null)
            {
                OpenedList.Remove(this);
            }
        }
        #endregion

        #region MonityList 所属列表数据个数发生变化时，通知自己的索引改变了
        public void MonityList()
        {
            List.CollectionChanged += (o, e) => { OnPropertyChanged("Index"); };
        }
        #endregion

        #region ToSource 用改变后的数据更新源
        public void ToSource()
        {
            //如果Source不为空，把数据复制到Source中
            if (Source != null)
            {
                Source.CopyFrom(this);
            }
            //如果有对应列表，复制一份到列表中
            else if (List != null)
            {
                GeneralObject go = new GeneralObject();
                go.CopyFrom(this);
                List.Add(go);
            }
        }
        #endregion

        #region MakeID 如果对象没有id，产生id，产生时会递归查找1对多的子
        public void MakeID()
        {
            if (!_customPropertyValues.ContainsKey("id") || _customPropertyValues["id"] == null || _customPropertyValues["id"].Equals(""))
            {
                SetPropertyValue("id", Guid.NewGuid().ToString(), false);
            }
            //对所有集合性质的子进行处理
            foreach (BaseObjectList list in (from p in _customPropertyValues.Values where p is BaseObjectList select p))
            {
                list.MakeID();
            }
        }
        #endregion

        #region ClearIsModified 清除修改标记，沿对象路径清除
        public void ClearIsModified()
        {
            var cs = (from p in _customPropertyValues.Values where p is BaseObjectList select p);
            foreach(BaseObjectList l in cs)
            {
                l.ClearIsModified();
            }
            IsModified = false;
        }
        #endregion

        #region OpenedList 树结构的对象展开时，子对象所加入的列表
        public static readonly DependencyProperty OpenedListProperty =
            DependencyProperty.Register("OpenedList", typeof(ObjectList), typeof(GeneralObject), null);

        public ObjectList OpenedList
        {
            get { return (ObjectList)GetValue(OpenedListProperty); }
            set { SetValue(OpenedListProperty, value); }
        }

        #endregion

        #region Level 树结构的对象所在层次
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(int), typeof(GeneralObject), new PropertyMetadata(0));

        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        #endregion

        #region ChildName 层次对象，子节点的属性名
        public string ChildName { get; set; }
        #endregion

        #region IsOpened 树结构的对象是否展开了
        public static readonly DependencyProperty IsOpenedProperty =
            DependencyProperty.Register("IsOpened", typeof(bool), typeof(GeneralObject),
            new PropertyMetadata(new PropertyChangedCallback(OnIsOpenedChanged)));

        private static void OnIsOpenedChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            GeneralObject go = (GeneralObject)dp;
            //如果有展开数据存放的列表，树结构对象根据是否打开状态进行数据切换
            if (go.OpenedList != null)
            {
                if (go.IsOpened)
                {
                    go.Extend(go.OpenedList, go.ChildName);
                }
                else
                {
                    go.Closed(go.OpenedList, go.ChildName);
                }
            }
        }

        //展开一个节点，如果展开的子本身是IsOpened，继续把这个字添加到显示列表中
        //返回的是当前展开的位置
        private int Extend(ObjectList openedList, string childName)
        {
            int index = openedList.IndexOf(this);
            ObjectList ol = (ObjectList)this.GetPropertyValue(childName);
            foreach (GeneralObject go in ol)
            {
                //已经在列表里，不重复增加
                if (!openedList.Contains(go))
                {
                    index++;
                    //子对象的层次为当前对象层次+1
                    go.Level = this.Level + 1;
                    openedList.Insert(index, go);
                }
                //如果子为可以展开，递归展开子，下一个展开的孩子要在展开的子后面添加
                if (go.IsOpened)
                {
                    index = go.Extend(openedList, childName);
                }
            }
            return index;
        }

        //关闭一个节点，关闭时，要递归从openedList里删除数据
        private void Closed(ObjectList openedList, string childName)
        {
            ObjectList ol = (ObjectList)this.GetPropertyValue(childName);
            foreach (GeneralObject go in ol)
            {
                go.Closed(openedList, childName);
                openedList.Remove(go);
            }
        }

        public bool IsOpened
        {
            get { return (bool)GetValue(IsOpenedProperty); }
            set { SetValue(IsOpenedProperty, value); }
        }

        #endregion

        #region Opened 强制展开一个节点
        public void Opened()
        {
            IsOpened = true;
            //如果以前已经展开了，设置属性后不起作用，新加节点可能无法显示，主动调用扩展方法，以显示所有节点
            Extend(this.OpenedList, this.ChildName);
        }
        #endregion
    }
}
