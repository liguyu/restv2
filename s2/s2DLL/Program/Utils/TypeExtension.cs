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
using System.Json;

namespace Com.Aote.Utils
{
    public static class TypeExtension
    {
        //扩展的取属性方法
        public static PropertyInfo NewGetProperty(this Type type, string name)
        {
            PropertyInfo pi = type.GetProperty(name);
            if (pi != null)
            {
                return pi;
            }
            //没有找到，找其继承的类
            foreach (Type t in type.GetInterfaces())
            {
                pi = NewGetProperty(t, name);
                if (pi != null)
                {
                    return pi;
                }
            }
            return null;
        }
    }
}
