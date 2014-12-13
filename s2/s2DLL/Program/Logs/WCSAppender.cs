using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.Logs
{
    public class WCSAppender : IAppender
    {

        public void ShowMessage(string msg)
        {
            //检查是否有当天日期名称文件，没有创建，已创建，追加
            string fileName = DateTime.Now.ToString("yyyy-MM-dd");
            string path = "C:/WCSlogs/" + fileName + "WCS.txt";
            byte[] data = new UTF8Encoding().GetBytes(msg);
            StreamWriter sw = new StreamWriter(path, true, Encoding.GetEncoding("UTF-8"));
            sw.WriteLine(msg);
            sw.Close();
        }
    }
}
