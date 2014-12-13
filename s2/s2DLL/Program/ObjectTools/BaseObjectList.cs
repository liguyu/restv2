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
using System.Collections.Specialized;
using System.Collections;
using System.Collections.ObjectModel;
using System.Json;
using System.ComponentModel;
using System.Linq;
using Com.Aote.Utils;
using Com.Aote.Marks;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 为分页列表及普通列表提供基础工作，分页列表与普通列表在加载数据的过程以及获取数据总个数方面不同。
    /// 这两项内容交给子类实现，其他对于对象的保存，Path改变时加载数据等都在这个基础类中实现了。
    /// 由于其自己也要登记一些求和等信息，所以从CustomTypeHelper继承
    /// </summary>
    public abstract class BaseObjectList : CustomTypeHelper, IList<GeneralObject>, IList, INotifyCollectionChanged, ILoadable, IAsyncObject
    {
        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }
        #endregion

        public string EntityType { get; set; }

        #region HasEmptyRow 是否额外添加一个空行，用于表格编辑时，额外添加空行，以便新添数据的操作
        public static readonly DependencyProperty HasEmptyRowProperty =
            DependencyProperty.Register("HasEmptyRow", typeof(bool), typeof(ObjectList),
            new PropertyMetadata(false, new PropertyChangedCallback(OnHasEmptyRowChanged)));
        private static void OnHasEmptyRowChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BaseObjectList list = (BaseObjectList)o;
            if (e.NewValue != null && (bool)e.NewValue)
            {
                list.CreateEmpty();
            }
        }
        public bool HasEmptyRow
        {
            get { return (bool)GetValue(HasEmptyRowProperty); }
            set { SetValue(HasEmptyRowProperty, value); }
        }

        #endregion

        #region EmptyRow 空行，用于表格编辑时，每次增加一个空行
        public static readonly DependencyProperty EmptyRowProperty =
            DependencyProperty.Register("EmptyRow", typeof(GeneralObject), typeof(BaseObjectList),
            new PropertyMetadata(new PropertyChangedCallback(OnEmptyRowChanged)));

        private static void OnEmptyRowChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BaseObjectList list = (BaseObjectList)o;
            //清除旧的空行的监听
            if (e.OldValue != null)
            {
                GeneralObject go = (GeneralObject)e.OldValue;
                go.DynamicPropertyChanged -= list.EmptyRowChanged;
            }
            //新加空行在动态属性变化后，在列表最后添加新空行
            if (e.NewValue != null)
            {
                GeneralObject go = (GeneralObject)e.NewValue;
                go.DynamicPropertyChanged += list.EmptyRowChanged;
            }
        }

        public void CreateEmpty()
        {
            GeneralObject go = new GeneralObject() { EntityType = this.EntityType };
            go.List = this;
            go.New();
            EmptyRow = go;
            //监听空行对象
            Monity(go);
            objects.Add(go);
        }

        private void EmptyRowChanged(object o, PropertyChangedEventArgs e)
        {
            CreateEmpty();
        }

        public GeneralObject EmptyRow
        {
            get { return (GeneralObject)GetValue(EmptyRowProperty); }
            set { SetValue(EmptyRowProperty, value); }
        }
        #endregion

        #region Source 从指定的Source处获得对象，放入自己的列表中
        private static DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(BaseObjectList), typeof(BaseObjectList),
            new PropertyMetadata(new PropertyChangedCallback(OnSourceChanged)));
        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BaseObjectList list = (BaseObjectList)o;
            list.OnSourceChanged();
        }
        private void OnSourceChanged()
        {
            if (Source == null)
            {
                return;
            }
            //复制过程中，不通知对象集合变化，复制完成后再通知
            objects.CollectionChanged -= OnCollectionChanged;
            //把数据源里的对象复制过来
            Clear();
            foreach (GeneralObject go in Source)
            {
                go.List = this;
                Monity(go);
                go.MonityList();
                objects.Add(go);
            }
            //复制完成，通知对象集合变化
            objects.CollectionChanged += OnCollectionChanged;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
        }
        public BaseObjectList Source
        {
            get { return (BaseObjectList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion

        #region NotSave 不发送后台的属性名列表，逗号分隔。目的是提高性能。
        public static readonly DependencyProperty NotSaveProperty =
            DependencyProperty.Register("NotSave", typeof(string), typeof(BaseObjectList), null);

        public string NotSave
        {
            get { return (string)GetValue(NotSaveProperty); }
            set { SetValue(NotSaveProperty, value); }
        }
        #endregion

        #region ToSource 把自己的数据复制到Source中去
        public void ToSource()
        {
            if (Source == null)
            {
                return;
            }
            Source.Clear();
            //空行不放
            var goes = (from p in objects where p != EmptyRow select p);
            foreach (GeneralObject go in goes)
            {
                Source.Add(go);
            }
        }
        #endregion

        #region TempletObject 模板对象
        public GeneralObject templetObject;
        public GeneralObject TempletObject
        {
            get { return templetObject; }
            set
            {
                templetObject = value;
            }
        }
        #endregion

        #region TempObj 临时对象，用于批量数据转换时，作为模板对象的临时绑定对象
        public GeneralObject TempObj { get; set; }
        #endregion

        #region ClearOnAdd 把原列表中数据往新列表中插入时，是否删除原来数据，默认为是。
        private bool clearOnAdd = true;
        public bool ClearOnAdd
        {
            get { return clearOnAdd; }
            set
            {
                if (clearOnAdd != value)
                {
                    clearOnAdd = value;
                }
            }
        }
        #endregion

        #region SelectObject 选中对象
        public object selectObject;
        public object SelectObject
        {
            get { return selectObject; }
            set
            {
                if (value != null)
                {
                    selectObject = value;
                    if (selectObject is IList)
                    {
                        //如果设定了往列表中添加时，清除原有数据，则清除
                        if (ClearOnAdd)
                        {
                            this.Clear();
                        }
                        objects.CollectionChanged -= OnCollectionChanged;
                        //因为临时对象再第二次添加数据（指的是只添加一条数据）时未发生变化，需要清除第一次临时对象
                        TempObj.New();
                        foreach (GeneralObject item in (IList)selectObject)
                        {
                            //模板对象
                            if (templetObject == null)
                            {
                                throw new Exception("模板对象不能为空!");
                            }
                            if (TempObj == null)
                            {
                                throw new Exception("临时对象不能为空!");
                            }
                            //将临时对象值赋值为要转换对象
                            TempObj.CopyFrom(item);
                            //产生新对象
                            GeneralObject go = new GeneralObject();
                            go.WebClientInfo = templetObject.WebClientInfo;
                            go.CopyFrom(templetObject);
                            var existObj = (from p in this.objects where p.Equals(go) select p).FirstOrDefault();
                            if (existObj == null)
                            {
                                this.Add(go);
                            }
                        }
                        objects.CollectionChanged += OnCollectionChanged;
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        OnPropertyChanged("Count");
                     
                    }
                }
            }

        }
        #endregion

        #region RemoveObjects 移走的对象列表，从列表中移走RemoveObjects中的对象
        private object removeObjects;
        public object RemoveObjects
        {
            get { return removeObjects; }
            set
            {
                removeObjects = value;

                if (value != null && value is IList)
                {
                    objects.CollectionChanged -= OnCollectionChanged;
                    foreach (GeneralObject item in (IList)removeObjects)
                    {
                        this.Remove(item);
                    }
                    objects.CollectionChanged += OnCollectionChanged;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    OnPropertyChanged("Count");
                }
            }
        }
        #endregion

        #region IsModified 列表是否修改过
        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register("IsModified", typeof(bool), typeof(BaseObjectList), null);

        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }
        #endregion

        #region Dirty 垃圾数据
        public static readonly DependencyProperty DirtyProperty =
            DependencyProperty.Register("Dirty", typeof(IEnumerable<object>), typeof(BaseObjectList), 
            new PropertyMetadata(new List<object>()));

        public IEnumerable<object> Dirty
        {
            get { return (IEnumerable<object>)GetValue(DirtyProperty); }
            set { SetValue(DirtyProperty, value); }
        }
        #endregion

        #region Changes 数据变更
        public static readonly DependencyProperty ChangesProperty =
            DependencyProperty.Register("Changes", typeof(BaseObjectList), typeof(BaseObjectList),
            new PropertyMetadata(new ObjectList()));

        public BaseObjectList Changes
        {
            get { return (BaseObjectList)GetValue(ChangesProperty); }
            set { SetValue(ChangesProperty, value); }
        }
        #endregion

        public string ChangesEntityType { set; get; }

        #region SortNames 排序列名与字段名对应关系，格式为 列名:字段名，每项之间用逗号分隔
        public string SortNames { get; set; }
        #endregion

        #region SortName 当前排序字段，可以进行默认设置
        public string sortName = null;
        public string SortName 
        {
            get { return sortName; }
            set
            {
                if (sortName == value)
                {
                    return;
                }
                sortName = value;
                OnPropertyChanged("SortName");
            }
        }
        #endregion

        #region Order 是否正序，True正序，False倒序
        public string order;
        public string Order 
        {
            get { return order; }
            set
            {
                if (order == value)
                {
                    return;
                }
                order = value;
                OnPropertyChanged("Order");
            }
        }
        #endregion

        #region ChangeSortName 根据给定列标题改变排序字段及正序倒序关系
        public void ChangeSortName(string colName)
        {
            //根据列名找到字段名
            string fName = null;
            foreach (string nps in SortNames.Split(','))
            {
                string[] np = nps.Split(':');
                if (np[0] == colName)
                {
                    fName = np[1];
                    break;
                }
            }
            //没找到，直接返回
            if (fName == null)
            {
                return;
            }
            //如果找到了，看当前字段名是否变化了，没变化，则修改排序规则（倒序、正序）
            if (fName == SortName)
            {
                //排序规则切换
                Order = Order == "asc" ? "desc" : "asc";
            }
            else
            {
                //设置排序字段，默认正序
                SortName = fName;
                Order = "asc";
            }
        }
        #endregion

        #region MakeChanges 产生数据变化记录，包含新建记录，删除的旧记录，修改的新旧记录
        public void MakeChanges()
        {
            this.Changes.Clear();
            this.Changes.State = State.StartLoad;
            //已经被删除的数据
            IEnumerable<object> delIEum = _copyed.Except(objects);
            List<object> dels = new List<object>(delIEum);
            foreach(GeneralObject go in dels)
            {
                GeneralObject newObj = new GeneralObject();
                newObj.CopyFrom(go);
                newObj.EntityType = ChangesEntityType;
                newObj.SetPropertyValue("id", null, false);
                newObj.SetPropertyValue("opertype", "删除", false);
                this.Changes.Add(newObj);
            }
            //新加数据
            foreach (GeneralObject go in this.objects)
            {
                //空行不处理
                if (go == EmptyRow) continue;
                //无id号,新加数据
                object id = go.GetPropertyValue("id");
                if (id == null || id.Equals(""))
                {
                    GeneralObject newObj = new GeneralObject();
                    newObj.CopyFrom(go);
                    newObj.EntityType = ChangesEntityType;
                    newObj.SetPropertyValue("id", null, false);
                    newObj.SetPropertyValue("opertype", "新增", false);
                    this.Changes.Add(newObj);
                }
                //有id号，比较是否修改了
                else
                {
                    var existObj = (from p in _objsCopyed where p.Equals(go) select p).FirstOrDefault();
                    bool isChanged = go.CompAttrsChanged(existObj);
                    if (isChanged)
                    {
                        GeneralObject oldObj = new GeneralObject();
                        oldObj.CopyFrom(existObj);
                        oldObj.EntityType = ChangesEntityType;
                        oldObj.SetPropertyValue("id", null, false);
                        oldObj.SetPropertyValue("oldid", id, false);
                        oldObj.SetPropertyValue("opertype", "修改前", false);
                        this.Changes.Add(oldObj);
                        GeneralObject newObj = new GeneralObject();
                        newObj.CopyFrom(go);
                        newObj.EntityType = ChangesEntityType;
                        newObj.SetPropertyValue("id", null, false);
                        newObj.SetPropertyValue("oldid", id, false);
                        newObj.SetPropertyValue("opertype", "修改后", false);
                        this.Changes.Add(newObj);

                    }
                }
            }
            this.Changes.State = State.Loaded;
        }
        #endregion

        #region ILoadable Members
        /// <summary>
        /// 开始加载事件，在开始加载数据时触发
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
        /// 数据加载完成事件，在数据加载完成后触发
        /// </summary>
        public event AsyncCompletedEventHandler DataLoaded;
        public void OnDataLoaded(AsyncCompletedEventArgs args)
        {
            if (DataLoaded != null)
            {
                DataLoaded(this, args);
            }
        }

        /// <summary>
        /// Load方法交给子类实现，翻页与普通列表加载数据的过程不同。
        /// </summary>
        abstract public void Load();
        #endregion

        #region IAsyncObject Members

        /// <summary>
        /// 本对象的名称，主要是方便显示。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 异步工作完成事件，加载完成后要触发。
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
        /// 是否忙
        /// </summary>
        public bool isBusy = false;
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

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(BaseObjectList),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        #endregion

        #region WebClientInfo 用于去后台获取数据的基本地址描述
        /// <summary>
        /// 用于去后台获取数据的基本地址描述
        /// </summary>
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

        #region Path 获取数据的路径，当路径发生变化时，将根据新路径到后台获取数据

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(BaseObjectList), 
            new PropertyMetadata(new PropertyChangedCallback(OnPathChanged)));

        /// <summary>
        /// 路径改变时，获取数据
        /// </summary>
        /// <param name="dp">自身</param>
        /// <param name="args">新值参数</param>
        public static void OnPathChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            //分页组件有MultiPath时，在MultiPath时加载数据，Path发生改变时不加载数据
            if (dp is PagedObjectList && ((PagedObjectList)dp).MultiPath != null)
            {
                return;
            }
            BaseObjectList ol = (BaseObjectList)dp;
            //如果指明Path改变时，不加载数据，则只有当外界要求，加载数据时，才加载
            if (ol.LoadOnPathChanged)
            {
                ol.Load();
            }
        }

        /// <summary>
        /// 获取数据的路径，当路径发生变化时，将根据新路径到后台获取数据
        /// </summary>
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        #endregion

        #region CurrentIndex 当前对象索引，添加对象时，如果对象存在，当前对象直接到要添加的对象，否则，到最后添加的对象
        public int currentIndex = -1;
        public int CurrentIndex {
            get { return currentIndex; }
            set
            {
                if (currentIndex != value)
                {
                    currentIndex = value;
                    //index变化后，当前选中项也变
                    OnPropertyChanged("CurrentIndex");
                    CurrentItem = this[currentIndex];
                }
            }

        }
        #endregion

        #region CurrentItem 当前对象
        public GeneralObject currentItem;
        public GeneralObject CurrentItem
        {
            get { return currentItem; }
            set
            {
                if (currentItem != value)
                {
                    currentItem = value;
                    if (value != null)
                    {
                        //当前选中项变化时，看集合中是否存在，不存在，添加进去，修改索引，存在，修改索引
                        int i = this.IndexOf(currentItem);
                        if (i == -1)
                        {
                            this.Add(currentItem);
                        }
                        OnPropertyChanged("CurrentItem");
                        CurrentIndex = this.IndexOf(currentItem);
                    }
                }
            }
        }
        #endregion

        #region Top 获取或者设置最顶层的对象
        private GeneralObject top = null;
        public GeneralObject Top
        {
            get { return top; }
            set
            {
                if (top == value)
                {
                    return;
                }
                top = value;
                //看是否存在，如果存在，已到顶端，否则，添加
                int index = this.objects.IndexOf(top);
                if (index == -1)
                {
                    if (top != null)
                    {
                        this.Add(top);
                    }
                }
                else
                {
                    this.RemoveAt(index);
                    this.Add(top);
                }
                //通知top改变
                OnPropertyChanged("Top");
            }
        }
        #endregion

        #region RemoveTop 移走顶层对象
        public void RemoveTop()
        {
            this.RemoveAt(this.Count - 1);
            if (this.Count == 0)
            {
                Top = null;
            }
            else
            {
                Top = this[this.Count - 1];
            }
            OnPropertyChanged("Top");
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

        #region Default 空列表可以有一个默认对象

        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.Register("Default", typeof(GeneralObject), typeof(BaseObjectList),
            new PropertyMetadata(new PropertyChangedCallback(OnDefaultChanged)));

        public GeneralObject Default
        {
            get { return (GeneralObject)GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        public static void OnDefaultChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            BaseObjectList ol = (BaseObjectList)dp;
            //如果列表为空，把默认值添加到列表中
            if (ol.Count == 0)
            {
                ol.Add(ol.Default);
            }
        }
        #endregion

        #region objects 保存所有对象
        protected ObservableCollection<GeneralObject> objects = new ObservableCollection<GeneralObject>();
        #endregion

        #region _copyed 保存最初复制过来的对象，在提交时，在这个集合里但是不在objects里的对象将被删除
        private List<GeneralObject> _copyed = new List<GeneralObject>();
        #endregion

        #region _objsCopyed 将对象属性值进行复制保存,
        private List<GeneralObject> _objsCopyed = new List<GeneralObject>();
        #endregion

        #region ExcelString excel字符串,变化时转换为数据对象,list对象需要配置EntityType
        public string ExcelString
        {
            set 
            {
                ExcelToDatas(value);
            }
        }

        //excel数据集主键
        private string excelKey;
        public string ExcelKey
        {
            get { return excelKey; }
            set { excelKey = value; }
        }

        //将excel格式字符串数据转为list数据
        private void ExcelToDatas(string excelStr)
        {
            if (excelStr == null || excelStr.Equals(""))
            {
                return;
            }
            string[] split = new string[] { "\r\n" };
            string[] objs = excelStr.Split(split, StringSplitOptions.RemoveEmptyEntries);
            //第一行为列名
            char[] cc = { '\t' };
            string[] names = objs[0].Split(cc);
            //整个复制过程完成后，再通知列表发生变化了
            this.objects.CollectionChanged -= this.OnCollectionChanged;
         
            //产生对象
            for (int i = 1; i < objs.Length; i++)
            {
                string temp = objs[i];
                string[] datas = temp.Split(cc);
                GeneralObject go = NewObject(names, datas);
                if (excelKey != null && !"".Equals(excelKey))
                {
                    char[] s = { ',' };
                    string[] excelKeys = excelKey.Split(s);
                    bool isHaving = false;
                    // 循环列表对象和要添加的对象比对是否主键相同的对象
                    // 如果要添加对象的excelKey键值和数据集对象的excelKey键值相等则覆盖该对象信息，否则添加对象到集合
                    foreach (GeneralObject o in this)
                    {
                        bool isEqu = true;
                        foreach (string str in excelKeys)
                        {
                            string excelKeyValue = "";
                            var existPro = (from p in go.GetPropertyInfos() where p.Name.Equals(str) select p).FirstOrDefault();
                            if (existPro != null)
                            {
                                excelKeyValue = go.GetPropertyValue(str) + "";
                            }
                            string thisKeyValue = "";
                            var thisPro = (from p in o.GetProperties() where p.Name.Equals(str) select p).FirstOrDefault();
                            if (thisPro != null)
                            {
                                thisKeyValue = o.GetPropertyValue(str) + "";
                            }
                            if (!excelKeyValue.Equals(thisKeyValue))
                            {
                                isEqu = false;
                                break;
                            }
                        }
                        if (isEqu)
                        {
                            o.CopyFrom(go);
                            isHaving = true;
                            break;
                        }
                    }
                    if (!isHaving)
                    {
                        this.Add(go);
                    }
                }
                else
                {
                    this.Add(go);
                }
            }
            //通知对象序号发生变化
            //foreach (GeneralObject go in objects)
            //{
            //    go.OnPropertyChanged("Index");
            //}
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(args);
            objects.CollectionChanged += this.OnCollectionChanged;
           // MessageBox.Show("ok"+this.objects.Count);
        }

        //产生新对象
        private GeneralObject NewObject(string[] names, string[] values)
        {
            GeneralObject go = new GeneralObject();
            go.EntityType = this.EntityType;
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                //超出值的个数，按空格算
                string value = "";
                if (i < values.Length)
                {
                    value = values[i];
                }
                go.SetPropertyValue(name, value, true);
            }
            return go;
        }
        #endregion

        #region New 清空所有数据
        public void New()
        {
            Clear();
            IsModified = false;
        }

        #endregion

        #region FromJson 从Json串构造
        /// <summary>
        /// 根据Json串构造列表中的子对象，递归调用GeneralObject本身的调用过程。
        /// </summary>
        /// <param name="array"></param>
        virtual public void FromJson(JsonArray array)
        {
            //整个复制过程完成后，再通知列表发生变化了
            objects.CollectionChanged -= this.OnCollectionChanged;
            List<GeneralObject> delObjs = new List<GeneralObject>(this.objects);
            //新增或重新给对象赋值
            foreach (JsonObject obj in array)
            {
                GeneralObject temp = new GeneralObject();
                temp.FromJson(obj);
                var existObj =  (from p in this.objects where p.Equals(temp) select p).FirstOrDefault();
                //对象已经存在，把原来的对象移走
                if (existObj != null)
                {
                    existObj.CopyFrom(temp);
                    //从删除列表里去掉，以便不删除这个对象
                    delObjs.Remove(existObj);
                }
                else
                {
                    //设置对象所属列表
                    this.Add(temp);
                }
            }
            //删除不在获取的数据中的对象
            foreach (GeneralObject go in delObjs)
            {
                //空行数据不删除
                if (go == EmptyRow)
                {
                    continue;
                }
                this.objects.Remove(go);
            }
            //通知对象序号发生变化
            foreach (GeneralObject go in objects)
            {
                go.OnPropertyChanged("Index");
            }
            //设置初始对象列表，用于最后确定数据库里要删除的对象
            _copyed.Clear();
            _copyed.AddRange(from p in objects where p != EmptyRow select p);
            //复制对象
            _objsCopyed.Clear();
            foreach (GeneralObject go in objects)
            {
                if (go == EmptyRow)
                {
                    continue;
                }
                GeneralObject newObj = new GeneralObject();
                newObj.CopyFrom(go);
                _objsCopyed.Add(newObj);
            }
            //发送列表变化通知, 新增对象为列表本身
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(args);
            //还原继续监听列表单个数据变化过程
            objects.CollectionChanged += this.OnCollectionChanged;
            IsOld = false;            
        }
        #endregion

        #region CopyFrom 从另外一个列表复制
        public void CopyFrom(BaseObjectList value)
        {
            //整个复制过程完成后，再通知列表发生变化了
            objects.CollectionChanged -= this.OnCollectionChanged;
            this.Clear();
            _copyed.Clear();
            foreach (GeneralObject obj in value)
            {
                //如果对象是列表中新增的编辑对象，不复制
                if (obj.List != null && obj.List.EmptyRow == obj)
                {
                    continue;
                }
                GeneralObject ngo = new GeneralObject();
                ngo.CopyFrom(obj);
                Add(ngo);
                _copyed.Add(ngo);
            }
            //发送列表变化通知, 新增对象为列表本身
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(args);
            //还原继续监听列表单个数据变化过程
            objects.CollectionChanged += this.OnCollectionChanged;
            IsModified = false;
        }
        #endregion

        #region CopyFrom 从另外一个列表复制
        public void CopyFrom(BaseObjectList value,int startIndex,int endIndex)
        {
            //整个复制过程完成后，再通知列表发生变化了
            objects.CollectionChanged -= this.OnCollectionChanged;
            this.Clear();
            _copyed.Clear();
            if (endIndex >this.objects.Count)
            {
                endIndex = value.objects.Count;
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                GeneralObject obj = value[i];
                GeneralObject ngo = new GeneralObject();
                ngo.CopyFrom(obj);
                Add(ngo);
                _copyed.Add(ngo);
            }
            //发送列表变化通知, 新增对象为列表本身
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(args);
            //还原继续监听列表单个数据变化过程
            objects.CollectionChanged += this.OnCollectionChanged;
            IsModified = false;
        }
        #endregion

        #region ToJson 把列表转换成Json串
        public JsonArray ToJson()
        {
            List<JsonObject> arrays = new List<JsonObject>();
            var goes = (from p in objects where p != EmptyRow select p);
            foreach (GeneralObject go in goes)
            {
                arrays.Add(go.ToJson());
            }
            JsonArray result = new JsonArray(arrays);
            return result;
        }
        #endregion

        #region MakeDirty 产生删除垃圾数据，列表保存时，把已经删除的数据当做垃圾数据
        public void MakeDirty()
        {
            IEnumerable<object> result = _copyed.Except(objects);
            Dirty = new List<object>(result);
        }
        #endregion

        #region Count 由子类决定如果计算数据总数。
        abstract public int Count { get; set; }
        #endregion

        #region Size objects列表里实际包含的对象数
        public int Size
        {
            get { return objects.Count; }
        }
        #endregion

        #region BaseObjectList 监听集合变化事件，将集合变化事件通知出去
        public BaseObjectList()
        {
            //构造时，监听对象集合变化，把对象集合变化通知出去
            objects.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object o, NotifyCollectionChangedEventArgs e)
        {
            //新添空行不应该认为对象发生了变化
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems.Count != 1 || e.NewItems[0] != EmptyRow)
            {
                IsModified = true;
            }
            OnCollectionChanged(e);
        }
        #endregion

        #region Monity and RemoveMonity 监听新增对象的动态属性变化以及HasErrors属性变化事件，以确定自身是否变化以及有没有错误
        public void Monity(GeneralObject item)
        {
            //监听新添加的数据，以便修改列表本身的状态
            item.DynamicPropertyChanged += new PropertyChangedEventHandler(OnItemDynamicPropertyChanged);
            item.PropertyChanged += new PropertyChangedEventHandler(OnItemPropertyChanged);
        }

        public void RemoveMonity(GeneralObject item)
        {
            item.DynamicPropertyChanged -= OnItemDynamicPropertyChanged;
            item.PropertyChanged -= OnItemPropertyChanged;
        }

        private void OnItemDynamicPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            IsModified = true;
        }

        private void OnItemPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            //修改错误状态
            if (e.PropertyName == "HasErrors")
            {
                //修改自身的错误状态
                HasErrors = (from p in objects where p != EmptyRow && p.HasErrors select p).Count() != 0;
            }
            //修改是否改过状态
            if (e.PropertyName == "IsModified" && (o as GeneralObject).IsModified)
            {
                IsModified = true;
            }
        }
        #endregion

        #region IList<BaseObjectList> Members

        public int IndexOf(GeneralObject item)
        {
            return objects.IndexOf(item);
        }

        public void Insert(int index, GeneralObject item)
        {
            objects.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            objects.RemoveAt(index);
        }

        public GeneralObject this[int index]
        {
            get
            {
                if (index < 0)
                    return null;
                return objects[index];
            }
            set
            {
                objects[index] = value;
            }
        }

        #endregion

        #region OnCountChanged 通知总数发生变化，子类可以重载此函数，以便根据情况决定是否通知
        public virtual void OnCountChanged()
        {
                OnPropertyChanged("Count");
        }
        #endregion

        #region ICollection<BaseObjectList> Members

        public void Add(GeneralObject item)
        {
            item.List = this;
            //不需要空行处理，直接添加，否则，添加到空行前面
            if (EmptyRow == null || item == EmptyRow)
            {
                objects.Add(item);
                OnCountChanged();
            }
            else
            {
                int index = objects.IndexOf(EmptyRow);
                objects.Insert(index, item);
            }
            Monity(item);
            item.MonityList();
            //新加对象取列表的PropertySetter
            foreach(PropertySetter ps in this.PropertySetters)
            {
                PropertySetter nps = ps.Clone();
                nps.Object = item;
                item.PropertySetters.Add(nps);
                //对ps中的每一个表达式，复制出一份
                foreach (Exp exp in ps.Exps)
                {
                    Exp nexp = exp.Clone();
                    nexp._targetObject = nps;
                    //触发nps的Loaded事件，让新表达式开始解析
                    nexp.OnLoaded(nps, new RoutedEventArgs());
                }
            }
        }


        #region IsClear 是否清除
        private bool isClear;
        public bool IsClear
        {
            get { return isClear; }
            set
            {
                if (isClear != value)
                {
                    isClear = value;
                    if (isClear)
                    {
                        Clear();
                    }
                }
            }
        }
        #endregion
      
        public void Clear()
        {
            List<GeneralObject> list = new List<GeneralObject>(objects);
            foreach (GeneralObject go in list)
            {
                RemoveMonity(go);
            }
            this.objects.Clear();
            OnPropertyChanged("Count");
            //重建空行
            if (HasEmptyRow)
            {
                CreateEmpty();
            }
        }

        public bool Contains(GeneralObject item)
        {
            return objects.Contains(item);
        }

        public void CopyTo(GeneralObject[] array, int arrayIndex)
        {
            objects.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(GeneralObject item)
        {
            RemoveMonity(item);
            bool result = objects.Remove(item);
            OnPropertyChanged("Count");
            return result;
        }

        #endregion

        #region IEnumerable<GeneralObject> Members

        public IEnumerator<GeneralObject> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            //是属性设置，调用属性设置过程
            if (value is PropertySetter)
            {
                base.Add(value as PropertySetter);
            }
            else if (value is GeneralObject)
            {
                this.objects.Add(value as GeneralObject);
            }
            else
            {
                throw new NotImplementedException();
            }
            return 0;
        }

        public bool Contains(object value)
        {
            return objects.Contains((GeneralObject)value);
        }

        public int IndexOf(object value)
        {
            return objects.IndexOf((GeneralObject)value);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            OnPropertyChanged("Count");
            objects.Remove((GeneralObject)value);
        }

        object IList.this[int index]
        {
            get
            {
                return objects[index];
            }
            set
            {
                objects[index] = (GeneralObject)value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            objects.CopyTo((GeneralObject[])array, index);
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

        #region Last 返回最后一个元素
        public GeneralObject Last()
        {
            if (objects.Count == 0)
                return null;
            return objects.Last();
        }
        #endregion

        #region Save方法
        public void Save()
        {
            DBService.Get(WebClientInfo.BaseAddress).Invoke(this, SaveToJson);
        }

        public JsonArray SaveToJson()
        {
            List<JsonObject> arrays = new List<JsonObject>();
            //空行不保存
            var goes = (from p in objects where p != EmptyRow select p);
            foreach (GeneralObject go in goes)
            {
                JsonObject json = new JsonObject();
                json["operator"] = "save";
                json["entity"] = go.EntityType;
                json["name"] = go.Name;
                //把列表不保存的属性传递给列表里的对象
                go.NotSave = this.NotSave;
                json["data"] = go.ToJson();
                arrays.Add(json);
            }
            JsonArray result = new JsonArray(arrays);
            return result;
        }

        public void SaveModified()
        {
            DBService.Get(WebClientInfo.BaseAddress).Invoke(this, SaveModifiedToJson);
        }

        public JsonArray SaveModifiedToJson()
        {
            List<JsonObject> arrays = new List<JsonObject>();
            //空行以及未修改的行不保存
            var goes = (from p in objects where p != EmptyRow && p.IsModified select p);
            foreach (GeneralObject go in goes)
            {
                JsonObject json = new JsonObject();
                json["operator"] = "save";
                json["entity"] = go.EntityType;
                json["name"] = go.Name;
                json["data"] = go.ToJson();
                arrays.Add(json);
            }
            JsonArray result = new JsonArray(arrays);
            return result;
        }

        #endregion

        #region MakeID 前台产生ID
        public void MakeID()
        {
            //对所有集合性质的子进行处理
            List<GeneralObject> l = new List<GeneralObject>();
            var goes = (from g in this.objects where g != this.EmptyRow select g);
            foreach (GeneralObject go in goes)
            {
                l.Add(go);
            }
            foreach (GeneralObject go in l)
            {
                go.MakeID();
            }
        }
        #endregion

        #region ClearIsModified 清除修改标记，用于保存成功后，演对象路径清除修改标记
        public void ClearIsModified()
        {
            //清零所有子的修改标记，包括空行
            foreach (GeneralObject go in objects)
            {
                go.ClearIsModified();
            }
            IsModified = false;
        }
        #endregion
    }
}
