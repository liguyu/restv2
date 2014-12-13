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
using System.IO;
using System.ComponentModel;
using Com.Aote.Logs;
using Com.Aote.ObjectTools;

namespace Com.Aote.Controls
{
    //在后台导出Excel
    public class Excel : FrameworkElement, INotifyPropertyChanged
    {
        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region Completed 导出完成事件
        /// <summary>
        /// 导出完成事件
        /// </summary>
        public event EventHandler Completed;
        public void OnCompleted()
        {
            if (Completed != null)
            {
                Completed(this, null);
            }
        }
        #endregion

        #region IsBusy 是否正在工作
        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }
        #endregion

        #region HQL 执行查询的语句
        private string hql;
        public string HQL
        {
            get { return hql; }
            set
            {
                if (hql == value)
                {
                    return;
                }
                hql = value;
            }
        }
        #endregion

        #region Path 后台访问路径
        private string path;
        public string Path
        {
            get { return path;}
            set
            {
                if(path == value)
                {
                    return;
                }
                path = value;
            }
        }
        #endregion

        #region FileName 产生的Excel文件名
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName == value)
                {
                    return;
                }
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }
        #endregion

        #region ToExcel 导出Excel
        public void ToExcel()
        {
            IsBusy = true;
            //将hql请求发送到后台，由后台执行查询，把查询结果写入Excel文件
            string uuid = System.Guid.NewGuid().ToString();
            string str = Path.Replace("|", "%7c") + "?uuid=" + uuid;
            Uri uri = new Uri(str);
            WebClient client = new WebClient();
            client.UploadStringCompleted += new UploadStringCompletedEventHandler(client_UploadStringCompleted);
            client.UploadStringAsync(uri, HQL);
        }

        void client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //获取Excel文件名
                FileName = e.Result;
                //触发导出完成事件
                OnCompleted();
                IsBusy = false;
            }
            else
            {
                MessageBox.Show("error:" + e.Error);
            }
        }
        #endregion
    }
}
