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
using System.Collections;

namespace Com.Aote.Logs
{
    //把Exception记录到日志里，包括异常类型、异常信息、异常堆栈、异常附加信息等等。
    public class ExceptionLog
    {
        //记录异常日志
        public static void LogException(string logName, Exception e)
        {
            Log log = Log.GetInstance(logName);
            string str = "";
            //循环显示每一个异常
            while (e != null)
            {
                //显示异常类型，以及异常信息
                str += e.GetType().FullName + ": ";
                str += e.Message + "\n";
                //显示异常所有附加信息
                foreach(DictionaryEntry entry in e.Data)
                {
                    str += entry.Key + "=" + entry.Value + ",";
                }
                //显示异常堆栈
                str += "\n" + e.StackTrace + "\n";
                e = e.InnerException;
            }
            log.Error(str);
        }
    }
}
