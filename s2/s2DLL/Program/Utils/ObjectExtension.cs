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
using System.Linq;
using Com.Aote.ObjectTools;
using System.Json;
using System.Globalization;
using System.Threading;
using Com.Aote.Reports;

namespace Com.Aote.Utils
{
    /// <summary>
    /// 对基本对象进行扩展
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// 新的获取类型的方法，如果对象实现了客户化类，则调用客户化类的方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Type NewGetType(this object o)
        {
            if (o is ICustomTypeProvider)
            {
                return (o as ICustomTypeProvider).GetCustomType();
            }
            return o.GetType();
        }

        //比较两个对象，两个对象都可以为空
        public static bool NewEquals(this object o, object other)
        {
            if (o == null && other == null)
            {
                return true;
            }
            if ((o != null && other == null) || (o == null && other != null))
            {
                return false;
            }
            return o.Equals(other);
        }

        //获取对象类型，把Json类型转换成相应的实际类型
        public static Type JsonGetType(this object o)
        {
            if (o == null) return typeof(object);
            if (o is JsonPrimitive)
            {
                JsonPrimitive json = (o as JsonPrimitive);
                if (json.JsonType == JsonType.Number)
                {
                    return typeof(decimal);
                }
                if (json.JsonType == JsonType.String)
                {
                    return typeof(string);
                }
                if (json.JsonType == JsonType.Boolean)
                {
                    return typeof(bool);
                }
            }
            return o.NewGetType();
        }

        //对象转换，把Json对象转换成普通对象
        public static object JsonConvert(this object value, Type type)
        {
            if (value == null) return null;
            if (value is JsonPrimitive)
            {
                JsonPrimitive json = (value as JsonPrimitive);
                if (json.JsonType == JsonType.Number)
                {
                    //数字行，根据具体C#数据类型转
                    if (type == typeof(int) || type == typeof(int?))
                        return (int)json;
                    if (type == typeof(double) || type == typeof(double?))
                        return (double)json;
                    if (type == typeof(decimal) || type == typeof(decimal?))
                        return (decimal)json;
                    if (type == typeof(DateTime) || type == typeof(DateTime?))
                    {
                      //  DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                        DateTime result = from1970.AddMilliseconds((long)json);
                        return result;
                    }
                }
                if (json.JsonType == JsonType.String)
                {
                    return (string)json;
                }
                if (json.JsonType == JsonType.Boolean)
                {
                    return (bool)json;
                }
            }
            //数字行，根据具体C#数据类型转
            if (   (type == typeof(int) || type == typeof(int?))
                 || (type == typeof(double) || type == typeof(double?))
                 || (type == typeof(decimal) || type == typeof(decimal?)))
                       return decimal.Parse(value + "");
            return value;
        }

        /// <summary>
        /// 查找资源，应用程序或者界面元素
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object FindResource(this object o, string name)
        {
            if (o is Table)
            {
                //如果是报表，调用报表自己的查找资源
                return (o as Table).FindResource(name);
            }
            else if (o is FrameworkElement)
            {
                return (o as FrameworkElement).FindResource(name);
            }
            else if (o is Application)
            {
                return (from r in (o as Application).Resources where r.Key.Equals(name) select r.Value).First();
            }
            else if (o is IInitable)
            {
                return (o as IInitable).FindResource(name);
            }
            else
            {
                throw new Exception("不支持资源查找, " + o);
            }
        }
    }
}
