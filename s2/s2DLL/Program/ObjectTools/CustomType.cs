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
using Com.Aote.Logs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 自定义类型，包含一个基础类型，即对象自身的class类型，一些常用的内容都直接调用这个基础类型的方法
    /// 完成，只是在获取属性等方面才做自己的实现过程。
    /// </summary>
    public class CustomType : Type
    {
        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.CustomType");

        /// <summary>
        /// 基础类型，常用内容直接调用基础类型的方法完成。
        /// </summary>
        private Type _baseType;
        public Type InnerType { get { return _baseType; } }

        /// <summary>
        /// 用基础类型构造用户类型。
        /// </summary>
        /// <param name="delegatingType">基础类型</param>
        public CustomType(Type delegatingType)
        {
            _baseType = delegatingType;
        }

        /// <summary>
        /// 保存类型的属性信息，属性信息实现了PropertyInfo，可以当做正常的PropertyInfo使用。
        /// </summary>
        public List<CustomPropertyInfoHelper> _customProperties = new List<CustomPropertyInfoHelper>();

        #region 属性增加过程

        /// <summary>
        /// 增加一个属性，类型为string
        /// </summary>
        /// <param name="name">属性名</param>
        public void AddProperty(string name)
        {
            if (!CheckIfNameExists(name))
            {
                _customProperties.Add(new CustomPropertyInfoHelper(name, typeof(String), _baseType));
            }
        }

        /// <summary>
        /// 增加一个属性，类型为给的值
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="propertyType">属性类型</param>
        public void AddProperty(string name, Type propertyType)
        {
            if (!CheckIfNameExists(name))
            {
                _customProperties.Add(new CustomPropertyInfoHelper(name, propertyType, _baseType));
            }
        }

        /// <summary>
        /// 增加一个属性，除名称及类型外，还有声明，不太常用
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="attributes">声明</param>
        public void AddProperty(string name, Type propertyType, List<Attribute> attributes)
        {
            if (!CheckIfNameExists(name))
            {
                _customProperties.Add(new CustomPropertyInfoHelper(name, propertyType, attributes, _baseType));
            }
        }

        /// <summary>
        /// 检查属性是否存在，以避免重复添加属性。如果属性已经存在，将抛出属性名存在异常。
        /// </summary>
        /// <param name="name">属性名</param>
        /// <returns>是否存在，存在，抛出异常，不存在，返回false</returns>
        private bool CheckIfNameExists(string name)
        {
            if ((from p in _customProperties select p.Name).Contains(name) || (from p in _baseType.GetProperties() select p.Name).Contains(name))
                throw new Exception("The property with this name already exists: " + name);
            else return false;
        }
        #endregion

        #region 直接调用_baseType或者未实现的部分

        public override Assembly Assembly
        {
            get { return _baseType.Assembly; }
        }

        public override string AssemblyQualifiedName
        {
            get { return _baseType.AssemblyQualifiedName; }
        }

        public override Type BaseType
        {
            get { return _baseType.BaseType; }
        }

        public override string FullName
        {
            get { return _baseType.FullName; }
        }

        public override Guid GUID
        {
            get { return _baseType.GUID; }
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return _baseType.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {

            throw new NotImplementedException();
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return _baseType.GetConstructors(bindingAttr);
        }

        public override Type GetElementType()
        {
            return _baseType.GetElementType();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return _baseType.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return _baseType.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return _baseType.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return _baseType.GetFields(bindingAttr);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return _baseType.GetInterface(name, ignoreCase);
        }

        public override Type[] GetInterfaces()
        {
            return _baseType.GetInterfaces();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return _baseType.GetMembers(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return _baseType.GetMethods(bindingAttr);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return _baseType.GetNestedType(name, bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return _baseType.GetNestedTypes(bindingAttr);
        }
        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            return _baseType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            return _baseType.IsPrimitive;
        }

        public override Module Module
        {
            get { return _baseType.Module; }
        }

        public override string Namespace
        {
            get { return _baseType.Namespace; }
        }

        public override Type UnderlyingSystemType
        {
            get { return this; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _baseType.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _baseType.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _baseType.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return _baseType.Name; }
        }
        #endregion

        /// <summary>
        /// 获得所有属性信息，包括clr属性以及自定义属性。
        /// </summary>
        /// <param name="bindingAttr">未知</param>
        /// <returns>所有属性信息列表</returns>
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            //基本类型的属性信息与自定义属性信息合并
            PropertyInfo[] clrProperties = _baseType.GetProperties(bindingAttr);
            if (clrProperties != null)
                return clrProperties.Concat(_customProperties).ToArray();
            else
                return _customProperties.ToArray();
        }

        /// <summary>
        /// 实际获取某个属性的属性信息的实现过程。
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="bindingAttr">未知</param>
        /// <param name="binder">未知</param>
        /// <param name="returnType">未知</param>
        /// <param name="types">未知</param>
        /// <param name="modifiers">未知</param>
        /// <returns>根据名字获得的属性</returns>
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            // 看属性是否存在
            PropertyInfo propertyInfo = (from prop in GetProperties(bindingAttr) where prop.Name == name select prop).FirstOrDefault();
            if (propertyInfo == null)
            {
                // 如果属性信息不存在，返回临时属性，类型为一般类型
                return new CustomPropertyInfoHelper(name, typeof(object), _baseType);
            }
            Log.Debug("获取属性:" + name + ", " + propertyInfo);
            return propertyInfo;
        }
    }

}
