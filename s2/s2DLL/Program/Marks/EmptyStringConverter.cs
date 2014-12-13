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

namespace Com.Aote.Marks
{
    /// <summary>
    /// 把空串转换成空值，文本框输入时，把空串按空值对待
    /// </summary>
    public class EmptyStringConverter : IValueConverter
    {
        /// <summary>
        /// 正向值不变
        /// </summary>
        /// <param name="value">转换的值</param>
        /// <param name="targetType">值的类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">未知</param>
        /// <returns>原来的值，不变</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //正向值不变
            return value;
        }

        /// <summary>
        /// 反向把空串转换成空
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">未知</param>
        /// <returns>空串转换成空，有需要过滤的字符，过滤掉</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            //空串转换成空值
            if (value is string)
            {
                if ((string)value == "")
                {
                    return null;
                }
                /*
                //过滤字符
                if (parameter != null && value != null)
                {
                    string result = (string)value;
                    string filter = (string)parameter;
                    //用每一个过滤字符替换value中的内容为空串
                    foreach (char ch in filter)
                    {
                        result = result.Replace("" + ch, "");
                    }
                    return result;
                }*/
            }
            return value;
        }
    }
}

