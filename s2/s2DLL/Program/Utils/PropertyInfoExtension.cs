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
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace Com.Aote.Utils
{
    /// <summary>
    /// 对PropertyInfo进行扩充
    /// </summary>
    public static class PropertyInfoExtension
    {
        /// <summary>
        /// 新的设置值的方法，将检查是否可以从给定对象转换，如何能够转换，调用转换器
        /// 进行工作，否则，才直接设置值。
        /// </summary>
        /// <param name="value"></param>
        public static void NewSetValue(this PropertyInfo pi, object obj, object value)
        {
            object v = GetValue(pi, value);
            pi.SetValue(obj, v, null);
        }

        /// <summary>
        /// 根据属性信息，对值进行转换，获得最终值
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object GetValue(PropertyInfo pi, object value)
        {
            if (value == null)
            {
                return null;
            }
            //如果是枚举，从字符串解析
            if (pi.PropertyType.IsEnum && value is string)
            {
                return Enum.Parse(pi.PropertyType, value as string, false);
            }
            //从字符串往基本类型转换
            if ((pi.PropertyType == typeof(bool) || pi.PropertyType == typeof(Nullable<bool>)) && value is string)
            {
                return bool.Parse(value.ToString());
            }
            //从其他类型往字符串转跑、
            if (pi.PropertyType == typeof(string) && value != null)
            {
                return value.ToString();
            }
            //如果结果类型可以从字符串转换，且转换出来的数据为字符串，结果值从字符串转换。
            object[] cusAttrs = pi.PropertyType.GetCustomAttributes(true);
            var typeAttr = (TypeConverterAttribute)(from t in cusAttrs where t is TypeConverterAttribute select t).FirstOrDefault();
            if (typeAttr != null)
            {
                Type type = Type.GetType(typeAttr.ConverterTypeName);
                TypeConverter t = (TypeConverter)Activator.CreateInstance(type);
                if (t.CanConvertFrom(value.GetType()))
                {
                    return t.ConvertFrom(value);
                }
            }
            //double往int转换
            if (value is double && pi.PropertyType == typeof(int))
            {
                int result;
                if (int.TryParse(value.ToString(), out result))
                {
                    return result;
                }
            }
            //decimal转double
            if (value is decimal && pi.PropertyType == typeof(double))
            {
                double result;
                if (double.TryParse(value.ToString(), out result))
                {
                    return result;
                }
            }
            return value;
        }
    }
}
