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
using System.ComponentModel;
using Com.Aote.Marks;
using Com.Aote.Utils;
using System.Reflection;

namespace Com.Aote.ObjectTools
{
    public class PropertySetters : List<PropertySetter> { }

    /// <summary>
    /// 属性设置，可以在xaml里对属性进行设置。包括校验规则，错误信息，可否为空，默认值等。
    /// 
    /// 实现IInitable接口使得属性配置可以采用表达式的方式进行绑定。
    /// </summary>
    public class PropertySetter : DependencyObject, IInitable, INotifyPropertyChanged
    {
        #region PropertyName 属性名
        /// <summary>
        /// 要设置的属性名称
        /// </summary>
        public string PropertyName { get; set; }
        #endregion

        #region Validation 校验规则d
        /// <summary>
        /// 校验规则，是一段javascript语句，其中的this代表属性值本身，and代表&&，or代表||，^代表小于号。
        /// 例如属性必须在1到100之间，可以设置为 this >= 1 and this ^= 100。
        /// 校验规则设置应该是依赖属性，以便校验能根据其他属性值进行变化，这个功能，目前还没有。
        /// </summary>
        public Delegate Validation { get; set; }
        #endregion

        #region ErrorMessage 错误信息
        /// <summary>
        /// 不能满足校验规则时的出错信息
        /// </summary>
        public string ErrorMessage { get; set; }
        #endregion

        #region NotNull 是否不能为空
        /// <summary>
        /// 属性值是否不能为空，默认情况下，属性值可以为空
        /// </summary>
        private bool? notNull = false;
        public bool? NotNull
        {
            get { return notNull; }
            set 
            {
                if (notNull != value)
                {
                    notNull = value;
                    OnPropertyChanged("NotNull");
                }
            }
        }
        #endregion

        #region Default 清空时的默认值
        /// <summary>
        /// 可以通过这个配置，在清空属性值时，保留以前录入的结果。
        /// </summary>
        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.Register("Default", typeof(object), typeof(PropertySetter),
            new PropertyMetadata(new PropertyChangedCallback(OnDefaultChanged)));

        private static void OnDefaultChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            PropertySetter ps = (PropertySetter)o;
            if (ps.Object != null)
            {
                PropertyInfo pi = ps.Object.GetType().GetProperty(ps.PropertyName);
                //为动态属性
                if (pi == null || pi is CustomPropertyInfoHelper)
                {
                    //默认值改变了，但是对象还没有值，给对象设置值为默认值
                    if (ps.Object.GetPropertyValue(ps.PropertyName) == null)
                    {
                        ps.Object.SetPropertyValue(ps.PropertyName, ps.Default, false, true);
                    }
                }
                else
                {
                    if (pi.GetValue(ps.Object, null) == null)
                    {
                        pi.SetValue(ps.Object, ps.Default, null);
                    }
                }
            }
        }

        public object Default
        {
            get { return GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        #endregion

        #region DefaultObject 对象方式的默认值，包括列表。支持GeneralObject及ObjectList
        //对象默认值要深层复制到属性中去，不能简单引用。
        public static readonly DependencyProperty DefaultObjectProperty =
            DependencyProperty.Register("DefaultObject", typeof(object), typeof(PropertySetter),
            new PropertyMetadata(new PropertyChangedCallback(OnDefaultObjectChanged)));

        private static void OnDefaultObjectChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            PropertySetter ps = (PropertySetter)o;
            if (ps.Object != null)
            {
                //默认值改变了，但是对象还没有值，给对象设置值为默认值
                if (ps.Object.GetPropertyValue(ps.PropertyName) == null)
                {
                    if (ps.DefaultObject is GeneralObject)
                    {
                        //复制默认对象到新对象
                        GeneralObject go = ps.DefaultObject as GeneralObject;
                        GeneralObject ngo = new GeneralObject();
                        ngo.CopyFrom(go);
                        ps.Object.SetPropertyValue(ps.PropertyName, ngo, false, true);
                    }
                    else if (ps.DefaultObject is ObjectList)
                    {
                        //复制默认对象到新对象
                        ObjectList go = ps.DefaultObject as ObjectList;
                        ObjectList ngo = new ObjectList();
                        ngo.CopyFrom(go);
                        ps.Object.SetPropertyValue(ps.PropertyName, ngo, false, true);
                    }
                }
            }
        }

        public object DefaultObject
        {
            get { return GetValue(DefaultObjectProperty); }
            set { SetValue(DefaultObjectProperty, value); }
        }

        #endregion

        #region Value 属性值
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertySetter),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            //如果值没有发生变化，退出
            if ((args.NewValue != null && args.NewValue.Equals(args.OldValue)) || (args.NewValue == null && args.OldValue == null))
            {
                return;
            }
            PropertySetter ps = (PropertySetter)dp;
            
            //给属性赋值
            if (ps != null && ps.Object != null)
            {
                PropertyInfo pi = ps.Object.GetType().GetProperty(ps.PropertyName);
                //没有属性，或者是动态属性，调用动态属性的做法，否则，调用CLR属性的做法
                if (pi == null || pi is CustomPropertyInfoHelper)
                {
                    ps.Object.SetPropertyValue(ps.PropertyName, ps.Value, false);
                }
                else
                {
                    pi.SetValue(ps.Object, ps.Value, null);
                }
            }
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion

        #region DataContext 数据上下文，从UI的数据上下文中继承
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(object), typeof(PropertySetter),
            new PropertyMetadata(null));
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        #endregion

        #region NoDependValue 非依赖属性设值
        private object noDependValue;
        public object NoDependValue
        {
            get
            {
                return this.noDependValue;
            }
            set
            {
                if (this.Object != null)
                {
                    //取属性值，如果不一样，再赋值
                    object v = this.Object.GetPropertyValue(this.PropertyName);
                    if ((v != null && !v.Equals(value)) || (v == null && value != null))
                    {
                        //this.Object.SetPropertyValue(this.PropertyName, value, false);
                        SetPropertyValue(value);
                    }
                }
            }
        }

        //给属性设置值
        private void SetPropertyValue(object value)
        {
            //分割属性部分
            string[] names = this.PropertyName.Split('.');
            //循环获取对象
            CustomTypeHelper obj = this.Object;
            for (int i = 0; i < names.Length - 1; i++)
            {
                obj = (CustomTypeHelper)obj.GetPropertyValue(names[i]);
            }
            //给最后的对象赋值
            obj.SetPropertyValue(names[names.Length - 1], value, false);
        }
        #endregion

        #region 非依赖属性设值
        private bool validationVal;
        public bool ValidationVal
        {
            get
            {
                return this.validationVal;
            }
            set
            {
                if (this.Object != null)
                {
                    if (value)
                    {
                        this.Object.NotError(this.PropertyName);
                    }
                    else
                    {
                        this.Object.OnError(this.PropertyName, this.ErrorMessage);
                    }
                  }

            }
        }
        #endregion

        #region Operator 查询条件中的操作符
        /// <summary>
        /// 查询时的条件设置，专用于查询对象，如果没有设置，默认为查询对象对应属性与录入值相等。
        /// 采用HQL条件格式进行设置，其中this代表录入的数据值，^代表小于号。
        /// 例如查询条件录入的是开始时间，则应设置成 date>=this
        /// </summary>
        public string Operator { get; set; }
        #endregion

        #region Type 属性类型
        /// <summary>
        /// 属性类型，一般情况下不用，在查询对象中，可以通过这个设置迫使输入的查询条件满足一定的类型要求。
        /// </summary>
        public string Type { get; set; }
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

        #region Object 属性设置对应的对象
        /// <summary>
        /// 属性设置对应的对象
        /// </summary>
        public CustomTypeHelper Object { get; set; }
        #endregion

        #region IInitable Members

        //提供周围环境信息的对象，有可能是界面元素，也有可能是应用程序
        private object UI;

        //通过OnLoad事件通知其他对象环境已经具备
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

        public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            this.IsInited = true;
            this.OnInitFinished();
        }

        //查找资源，直接调用设置配置的对象的找资源方法
        public object FindResource(string name)
        {
            if (name == "this")
            {
                return Object;
            }
            return UI.FindResource(name);
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
        
        #endregion

        #region Exps 表达式列表，用于列表中对表达式进行复制
        public List<Exp> Exps = new List<Exp>();
        #endregion

        #region Clone 复制一个新的PropertySetter
        public PropertySetter Clone()
        {
            PropertySetter ps = new PropertySetter();
            ps.PropertyName = this.PropertyName;
            ps.NoDependValue = this.NoDependValue;
            ps.Default = this.Default;
            ps.NotNull = this.NotNull;
            ps.Validation = this.Validation;
            ps.ErrorMessage = this.ErrorMessage;
            ps.UI = this.UI;
            return ps;
        }
        #endregion

    }
}
