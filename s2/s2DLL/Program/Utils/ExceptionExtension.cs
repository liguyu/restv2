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

namespace Com.Aote.Utils
{
    /// <summary>
    /// 对异常进行扩展
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// 循环找错误信息，如果为空，或者是空串，继续找InnerException
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetMessage(this Exception e)
        {
            string msg = e.Message;
            while ((msg == null || msg.Equals("")) && e != null)
            {
                e = e.InnerException;
                msg = e.Message;
            }
            return msg;
        }
    }
}
