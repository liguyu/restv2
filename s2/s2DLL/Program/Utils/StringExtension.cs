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
using Com.Aote.ObjectTools;

namespace Com.Aote.Utils
{
    /// <summary>
    /// 对字符串类进行扩展，增加一些新的有用的方法
    /// </summary>
    public static class StringExtension
    {
        //把字符串对象转换成类型
        /// <summary>
        /// 把字符串格式的类型描述转换成实际类型。
        /// </summary>
        /// <param name="attrType">字符串形式的类型描述</param>
        /// <returns>实际类型</returns>
        public static Type ToType(this string attrType)
        {
            if (attrType == "integer")
            {
                return typeof(int?);
            }
            else if (attrType == "long")
            {
                return typeof(long?);
            }
            else if (attrType == "double")
            {
                return typeof(double?);
            }
            else if (attrType == "string")
            {
                return typeof(string);
            }
            else if (attrType == "date")
            {
                return typeof(DateTime?);
            }
            else if (attrType == "time")
            {
                return typeof(DateTime?);
            }
            else if (attrType == "boolean")
            {
                return typeof(bool?);
            }
            //是集合
            else if (attrType.EndsWith("[]"))
            {
                return typeof(BaseObjectList);
            }
            else
            {
                //多对一
                return typeof(GeneralObject);
            }
        }
    }
}
