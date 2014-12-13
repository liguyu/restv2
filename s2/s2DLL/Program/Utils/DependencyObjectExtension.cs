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
using System.Windows.Data;
using Com.Aote.Marks;
using System.Collections.Generic;

namespace Com.Aote.Utils
{
    public static class DependencyObjectExtension
    {
        //存放绑定到的对象，以免丢失
        private static List<BindingSlave> _bindings = new List<BindingSlave>();

        //监听依赖属性变化，调用属性变化方法
        public static void Watch(this DependencyObject source, string dependencyPropertyName, PropertyChangedCallback callback)
        {
            if (dependencyPropertyName == null || source == null || callback == null)
                throw new ArgumentNullException();
            Binding binding = new Binding(dependencyPropertyName) { Source = source, Mode = BindingMode.OneWay };
            BindingSlave bs = new BindingSlave();
            _bindings.Add(bs);
            bs.PropertyChanged += (o, e) =>
            {
                callback(source, new DependencyPropertyChangedEventArgs());
            };
            BindingOperations.SetBinding(bs, BindingSlave.ValueProperty, binding);
        }

    }
}
