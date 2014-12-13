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
using System.Windows.Browser;
using System.Json;
using Com.Aote.Logs;
using Com.Aote.Utils;
using System.Windows.Markup;
using Com.Aote.Marks;


namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 代表一个能够提供用户自定义类型的对象。
    /// 实现ICustomTypeProvider，表明该对象可以提供用户自定义类型。通过该接口的GetCustomType方法，外界
    /// 可以获得该对象的客户化类型。
    /// 实现INotifyDataErrorInfo方法，表示这个对象支持异步校验，当对象发生错误时，可以通过事件的方式通知外部。
    /// 对象的类型，在类CustomTypes中进行管理，对象先在CustomTypes中根据类型名称（一般是实体名称），找到
    /// 对应的对象类型，并且设置对象类型为找到的类型。对于有些对象，并没有实体类型，这类对象每个对象自己拥有
    /// 一个匿名的对象类型，在调用对象的GetCustomType方法时，将为这类对象创建一个临时类型。
    /// 
    /// 对象支持在界面上对属性进行配置，这些配置本身可以绑定到其它对象的属性（一般是自己的某个属性），实现IInitable
    /// 方法可以让对象在界面加载完成后，获得界面环境信息。
    /// </summary>
    [ContentProperty("PropertySetters")]
    public class CustomTypeHelper : DependencyObject, 
        ICustomTypeProvider, INotifyPropertyChanged, INotifyDataErrorInfo, IInitable
    {
        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.CustomTypeHelper");

        #region _customPropertyValues 属性值存放处
        protected Dictionary<string, object> _customPropertyValues = new Dictionary<string, object>();
        #endregion

        #region PropertyStters 属性设置
        private List<PropertySetter> propertySetters = new List<PropertySetter>();
        public List<PropertySetter> PropertySetters
        {
            get { return propertySetters; }
        }

        //添加属性设置
        public void Add(PropertySetter ps)
        {
            //删除旧的同名属性设置
            var olds = from old in propertySetters where old.PropertyName.Equals(ps.PropertyName) select old;
            var oldPs = olds.FirstOrDefault();
            if (oldPs != null)
            {
                propertySetters.Remove(oldPs);
            }
            propertySetters.Add(ps);
            ps.Object = this;
            //有默认值，且对象的值为空，设置对象值为默认值
            if (ps.Default != null)
            {
                PropertyInfo pi = ps.Object.GetType().GetProperty(ps.PropertyName);
                //是动态属性
                if (pi == null || pi is CustomPropertyInfoHelper)
                {
                    if (GetPropertyValue(ps.PropertyName) == null)
                    {
                        SetPropertyValue(ps.PropertyName, ps.Default, false, true);
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
            //对值进行校验
            Validate(ps.PropertyName, GetPropertyValue(ps.PropertyName));
        }
        #endregion

        #region UI 对象所在环境，可以是应用程序或者界面元素之一
        private object UI;
        #endregion

        #region CustomType 对象对应的客户类型，对于临时对象，一个对象对应一个类

        /// <summary>
        /// 客户类型的内部表示。
        /// </summary>
        private CustomType _ctype;

        /// <summary>
        /// 获取客户类型，如果获取时，不存在，就建立一个，这样，每个没有实体类型的对象就做到了一个对象一个类。
        /// </summary>
        /// <returns>获取的对象类型</returns>
        public Type GetCustomType()
        {
            if (_ctype == null)
            {
                _ctype = new CustomType(GetType());
            }
            return _ctype;
        }

        /// <summary>
        /// 设置对象类型，从CustomTypes中找到类型的对象，可以调用这个方法设置对象类型。
        /// </summary>
        /// <param name="type"></param>
        public void SetCustomType(CustomType type)
        {
            _ctype = type;
        }
        #endregion

        #region 获得所有动态属性
        public  List<CustomPropertyInfoHelper>  GetPropertyInfos()
        {
             return this._ctype._customProperties;
        }
        
        #endregion

        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region DynamicPropertyChanged 动态属性改变事件，动态属性改变后，即发送一般属性改变事件，也发送动态属性改变事件

        /// <summary>
        /// 动态属性改变事件，动态属性改变后，即发送一般属性改变事件，也发送动态属性改变事件。有些过程只对动态属性
        /// 改变感兴趣，比如GeneralObject的是否修改属性，就只在动态属性改变时才进行变化。
        /// </summary>
        public event PropertyChangedEventHandler DynamicPropertyChanged;

        protected void OnDynamicPropertyChanged(string info)
        {
            if (DynamicPropertyChanged != null)
            {
                DynamicPropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region INotifyDataErrorInfo Members

        /// <summary>
        /// 属性及其错误列表，key为属性名，value为属性错误信息。
        /// </summary>
        public Dictionary<string, string> _errors = new Dictionary<string, string>();

        /// <summary>
        /// 某个属性发生了错误，在错误信息列表里进行注册
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="msg">错误信息</param>
        public void OnError(string propertyName, string msg)
        {
            //如果已经有错误信息，且错误信息没变，不重复通知
            if (_errors.ContainsKey(propertyName) && _errors[propertyName] == msg)
            {
                return;
            }
            _errors[propertyName] = msg;
            OnErrorsChanged(propertyName);
            //设置有错误
            HasErrors = true;
        }

        /// <summary>
        /// 当属性值经过校验，发现没有错误时，调用这个方法，通知外部某个属性没有错误了。
        /// </summary>
        /// <param name="propertyName">错误消失的属性名称</param>
        public void NotError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
            HasErrors = _errors.Count() != 0;
        }

        /// <summary>
        /// 属性错误通知事件，当某个属性错误状态发生变化时，通过这个事件通知外部
        /// 属性的错误状态发生变化了。
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public void OnErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 获取属性的错误信息列表，由于我们只保留属性的一条错误信息，在属性错误表中
        /// 获取到这个属性的一条错误信息，转换成列表，返回。
        /// </summary>
        /// <param name="propertyName">要获取错误信息的属性名称</param>
        /// <returns>为空，表示这个属性没有错误，否则，把属性唯一的一条错误转换成列表返回</returns>
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            //没有错误，返回空，binding开始会看对象是否有错误
            if (propertyName == null || !_errors.ContainsKey(propertyName))
            {
                return null;
            }
            List<string> result = new List<string>();
            result.Add(_errors[propertyName]);
            return result;
        }

        /// <summary>
        /// 有没有错误的内部表示
        /// </summary>
        private bool hasErrors;

        /// <summary>
        /// 表示这个对象是否有错误，如果所有属性都没有错误，对象就没有错误。
        /// </summary>
        public bool HasErrors
        {
            get { return hasErrors; }
            set
            {
                //值没变，不设置
                if (hasErrors != value)
                {
                    hasErrors = value;
                    OnPropertyChanged("HasErrors");
                }
            }
        }

        #endregion

        #region 简单调用对应的类型的方法

        /// <summary>
        /// 给对象添加一条属性描述，属性类型为默认的字符串型
        /// </summary>
        /// <param name="name">属性名</param>
        public void AddProperty(string name)
        {
            (GetCustomType() as CustomType).AddProperty(name);
        }

        /// <summary>
        /// 给对象添加一条属性描述，属性类型为给定类型
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="propertyType">属性类型</param>
        public void AddProperty(string name, Type propertyType)
        {
            (GetCustomType() as CustomType).AddProperty(name, propertyType);
        }

        /// <summary>
        /// 给对象添加一条属性描述，属性类型为给定类型，并可以附加一个声明列表，给属性加声明。
        /// 这个方法一般不会调用。
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="attributes">属性的声明</param>
        public void AddProperty(string name, Type propertyType, List<Attribute> attributes)
        {
            (GetCustomType() as CustomType).AddProperty(name, propertyType, attributes);
        }

        /// <summary>
        /// 获取所有属性信息，包括自定义的，也包括clr属性。
        /// </summary>
        /// <returns>所有属性信息</returns>
        public PropertyInfo[] GetProperties()
        {
            return (GetCustomType() as CustomType).GetProperties();
        }

        /// <summary>
        /// 根据名称，获取一条属性信息，即可以获取自定义属性，也可以获取clr属性。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PropertyInfo GetProperty(string name)
        {
            return (GetCustomType() as CustomType).GetProperty(name);
        }
        #endregion

        #region 设置值，包括校验过程
        public void SetPropertyValue(string propertyName, object value, bool isNew)
        {
            SetPropertyValue(propertyName, value, isNew, false);
        }

        /// <summary>
        /// 设置对象的动态属性值，clr属性值的设置不在这里处理。设置动态属性值之前，首先要进行各种校验。
        /// 为了在界面上体现对象的最新属性，即使属性值校验有问题，也会将错误值设置给属性。
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="isNew">新建时，即使校验错误，也要给数据</param>
        /// <param name="isDefault">设置默认值时，不发生动态属性改变事件</param>
        public void SetPropertyValue(string propertyName, object value, bool isNew, bool isDefault)
        {
            //如果类型为空，设置类型为值类型
            var r = (from p in _ctype._customProperties where p.Name == propertyName select p).FirstOrDefault();
            if (r == null)
            {
                //根据Json值设置数据类型
                AddProperty(propertyName, value.JsonGetType());
            }
            //值为空，且有默认值，取默认值
            var dValue = (from ps in PropertySetters where ps.PropertyName == propertyName && ps.Default != null select ps.Default).FirstOrDefault();
            if (value == null && dValue != null)
            {
                value = dValue;
            }
            //把Json值转换成对应类型
            value = value.JsonConvert(_ctype.GetProperty(propertyName).PropertyType);
            //校验，校验后对象的错误状态将保持
            Validate(propertyName, value);
            //不管校验是否通过，只要值确实改变了，赋值。此时对象为错误状态，由上层决定怎么做
            if (!_customPropertyValues.ContainsKey(propertyName) || !value.NewEquals(GetPropertyValue(propertyName)))
            {
                _customPropertyValues[propertyName] = value;
                //发送属性改变事件
                OnPropertyChanged(propertyName);
                if (!isDefault)
                {
                    //发送动态属性改变事件
                    OnDynamicPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// 校验执行过程，先检查是否有不能为空的校验，在看数据类型是否合适，最后调用配置的校验规则进行校验。
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        public bool Validate(string propertyName, object value)
        {
            var setter = (from ps in PropertySetters where ps.PropertyName == propertyName select ps).FirstOrDefault();
            //有不能为空校验，校验空值
            if (setter != null && setter.NotNull != null && (bool)setter.NotNull && value == null)
            {
                OnError(propertyName, "不能为空");
                return false;
            }
            //校验数据类型
            if (!ValidateValueType(value, GetCustomType().GetProperty(propertyName).PropertyType))
            {
                OnError(propertyName, "数据类型错误");
                return false;
            }
            //属性有校验要求，进行属性校验
            if (setter != null && setter.Validation != null && !ValidateValue(value, setter))
            {
                OnError(propertyName, setter.ErrorMessage);
                return false;
            }
            //校验成功，通知没有错误
            NotError(propertyName);
            return true;
        }

        /// <summary>
        /// 用配置的属性值校验规则校验属性值，校验规则用javascript代码书写。为了方便在xaml中书写，
        /// 用and代替&&，用or代替||，用^代替小于号，在调用javascript前注意转换。
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="setter">属性配置，里面有用javascript写的校验规则</param>
        /// <returns></returns>
        private bool ValidateValue(object value, PropertySetter setter)
        {
            //空值不用进行属性值校验
            if (value == null)
            {
                return true;
            }
            bool result = (bool)setter.Validation.DynamicInvoke(new object[] { value}); 
            return result;
        }

        /// <summary>
        /// 校验属性类型，主要调用类型本身的IsAssignableFrom方法看类型之间的兼容性。
        /// 其中，空值不用校验，空值可以赋给任何类型。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool ValidateValueType(object value, Type type)
        {
            // 空值不进行类型校验，空值可以给任何类型，空类型不校验
            // 字符串类型也不用校验，字符串是默认类型，可以把任意值给这种对象
            if (value == null)
            {
                return true;
            }
            //decimal可以给数字赋值
            if (value is decimal && (type == typeof(Nullable<int>) || type == typeof(long?) || type == typeof(Nullable<double>)))
            {
                return true;
            }
            bool result = type.IsAssignableFrom(value.GetType());
            return result;
        }
        #endregion

        #region SetValue 设置属性值，可以设置程序所写的属性值，也就是非动态属性值。这个方法调用提供了方便的设置任意属性值的方法。
        public void SetValue(string propertyName, object value)
        {
            //获取属性，调用属性的设置值过程
            GetCustomType().GetProperty(propertyName).SetValue(this, value, null);
        }
        #endregion

        #region GetPropertyValue 获取动态属性的属性值，属性不存在，则返回空。
        public object GetPropertyValue(string propertyName)
        {
            //如果属性不存在，返回空
            object value;
            _customPropertyValues.TryGetValue(propertyName, out value);
            return value;
        }
        #endregion

        #region IInitable Members

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

        //设置界面元素，触发Loaded事件，使得绑定到这个对象的Loaded事件上的配置开始工作。
        //调用属性设置的Init方法，让属性设置上的配置也开始工作。
        //获取属性配置，监听属性配置的变化，当属性配置改变后，做相应的工作。
        public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            //先从所有配置里获取属性初始配置信息，进行处理，然后监听属性配置信息变化过程。
            foreach (PropertySetter s in propertySetters)
            {
                s.Object = this;
                //先设置类型，再设置值，再校验
                if (s.Type != null)
                {
                    AddProperty(s.PropertyName, s.Type.ToType());
                }
                if (s.Value != null)
                {
                    SetPropertyValue(s.PropertyName, s.Value, false);
                }
                if (s.NotNull != null || s.Validation != null)
                {
                    Validate(s.PropertyName, GetPropertyValue(s.PropertyName));
                }
            }
            //监听属性变化，根据变化的属性，做对应动作
            foreach (PropertySetter s in propertySetters)
            {
                s.PropertyChanged += (o, e) => 
                {
                    //是NotNull，或者Validation发生变化，校验
                    if (e.PropertyName == "NotNull" || e.PropertyName == "Validation")
                    {
                        Validate(s.PropertyName, GetPropertyValue(s.PropertyName));
                    }
                    if (e.PropertyName == "Value")
                    {
                        SetPropertyValue(s.PropertyName, s.Value, false);
                    }
                };
            }
            //开始初始化，让绑定等开始工作
            foreach (PropertySetter s in propertySetters)
            {
                s.Init(ui);
            }
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
        

        //查找资源，如果指明this，返回自己，否则调用界面元素的找资源方法
        public object FindResource(string name)
        {
            if (name == "this")
                return this;
            return UI.FindResource(name);
        }
        #endregion
    }

 }
