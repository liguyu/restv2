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
using System.Collections.Generic;
using System.Linq;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 自定义属性信息，包括属性名，属性类型，属性声明等。
    /// </summary>
    public class CustomPropertyInfoHelper : PropertyInfo
    {
        //所属对象类型
        public Type ObjectType;

        /// <summary>
        /// 属性名
        /// </summary>
        public string _name;

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type _type;

        /// <summary>
        /// 属性声明
        /// </summary>
        public List<Attribute> _attributes = new List<Attribute>();

        #region 普通构造函数

        /// <summary>
        /// 采用属性名及类型构造
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="type">类型</param>
        public CustomPropertyInfoHelper(string name, Type type, Type objectType)
        {
            _name = name;
            _type = type;
            ObjectType = objectType;
        }

        /// <summary>
        /// 采用属性名，类型，声明构造
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="type">类型</param>
        /// <param name="attributes">声明</param>
        public CustomPropertyInfoHelper(string name, Type type, List<Attribute> attributes, Type objectType)
        {
            _name = name;
            _type = type;
            _attributes = attributes;
            ObjectType = objectType;
        }
        #endregion

        #region 简单的实现及未实现的函数

        public override PropertyAttributes Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            MethodInfo mi = GetType().GetMethod("GetValue", new Type[]
                {typeof(object), typeof(BindingFlags), typeof(Binder), 
                    typeof(object[]), typeof(System.Globalization.CultureInfo)});
            return mi;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            List<ParameterInfo> list = new List<ParameterInfo>();
            return list.ToArray();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return GetType().GetMethod("SetValue");
        }

        public override Type PropertyType
        {
            get { return _type; }
        }

        public override Type DeclaringType
        {
            get { return ObjectType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            var attrs = from a in _attributes where a.GetType() == attributeType select a;
            return attrs.ToArray();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _attributes.ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type ReflectedType
        {
            get { throw new NotImplementedException(); }
        }

        internal List<Attribute> CustomAttributesInternal
        {
            get { return _attributes; }
        }
        #endregion

        /// <summary>
        /// 读属性值，直接转为调用对象的GetPropertyValue
        /// </summary>
        /// <param name="obj">要读取值的对象</param>
        /// <param name="invokeAttr">未知</param>
        /// <param name="binder">未知</param>
        /// <param name="index">未知</param>
        /// <param name="culture">未知</param>
        /// <returns> 属性值</returns>
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            return (obj as CustomTypeHelper).GetPropertyValue(_name);
        }

        /// <summary>
        /// 设置属性值，直接转为调用对象的SetPropertyValue方法。
        /// </summary>
        /// <param name="obj">要设置值的对象</param>
        /// <param name="value">设置的值</param>
        /// <param name="invokeAttr">未知</param>
        /// <param name="binder">未知</param>
        /// <param name="index">未知</param>
        /// <param name="culture">未知</param>
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            (obj as CustomTypeHelper).SetPropertyValue(_name, value, false);
        }
    }
}
