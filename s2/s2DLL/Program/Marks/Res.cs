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
using System.Xaml;
using System.Windows.Markup;
using Com.Aote.Utils;
using System.Reflection;
using System.Windows.Data;

namespace Com.Aote.Marks
{
    //获取资源，可以是我们自己定义的资源，也可以是元素
    public class Res : IMarkupExtension<object>
    {
        //资源名称
        public string Key { get; set; }

        //目标对象，标记所加入到的目标对象
        private object _targetObject = null;

        //目标对象属性，标记加入的目标对象属性
        private PropertyInfo _targetProperty;

        //附加属性的设置值的方法
        private MethodInfo _targetMethod;

        #region IMarkupExtension<object> Members

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var target =
               (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            _targetObject = target.TargetObject;
            //界面加载后，再去查找资源
            handler = new RoutedEventHandler(OnLoaded);
            _targetObject.GetType().GetEvent("Loaded").AddEventHandler(_targetObject, handler);
            object obj = target.TargetProperty;
            //非直接属性不支持
            if (obj is PropertyInfo)
            {
                _targetProperty = target.TargetProperty as PropertyInfo;
                //返回原来的值
                return _targetProperty.GetValue(_targetObject, null);
            }
            //是静态Get方法，依赖属性取值
            else if (obj is MethodInfo && (obj as MethodInfo).IsStatic && (obj as MethodInfo).Name.StartsWith("Get"))
            {
                MethodInfo mi = (MethodInfo)obj;
                //对象的Set属性方法，在赋值时调用
                _targetMethod = mi.ReflectedType.GetMethod("Set" + mi.Name.Substring(3));
                //返回原来的值
                object o = mi.Invoke(null, new object[] { _targetObject });
                return o;
            }
            else
            {
                throw new Exception("只支持属性及附加属性");
            }
        }

        private RoutedEventHandler handler;
        private void OnLoaded(object o, RoutedEventArgs e)
        {
            _targetObject.GetType().GetEvent("Loaded").RemoveEventHandler(_targetObject, handler);
            handler = null;
            //调用对象的找资源过程
            object res = _targetObject.FindResource(Key);
            if(res == null)
            {
                throw new Exception("资源找不到:" + Key);
            }
            if (_targetProperty != null)
            {
                _targetProperty.SetValue(_targetObject, res, null);
            }
            //附加属性的设置值过程
            else if (_targetMethod != null)
            {
                _targetMethod.Invoke(null, new object[] { _targetObject, res });
            }
            else
            {
                throw new Exception("无法赋值，没有找到赋值方法");
            }
        }

        #endregion
    }
}
