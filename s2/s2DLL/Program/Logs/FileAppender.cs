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
using System.IO.IsolatedStorage;
using Com.Aote.ObjectTools;
using System.IO;

namespace Com.Aote.Logs
{
    public class FileAppender : IAppender
    {

        public void ShowMessage(string msg)
        {
            //检查是否有当天日期名称文件，没有创建，已创建，追加
           IsolatedStorageFile isf =  IsolatedStorageFile.GetUserStoreForApplication();
           SystemTime st = new SystemTime();
           string fileName = st.Now.ToString("yyyy-MM-dd");
           if (isf.FileExists(fileName))
           {
               IsolatedStorageFileStream isfstream = isf.OpenFile(fileName, FileMode.Append);
               StreamWriter sw = new StreamWriter(isfstream);
               sw.WriteLine(msg);
               sw.Close();
           }
           else
           {
              IsolatedStorageFileStream  isfstream = isf.CreateFile(fileName);
              StreamWriter sw = new StreamWriter(isfstream);
              sw.WriteLine(msg);
              sw.Close();
           }
            
        }
    }
}
