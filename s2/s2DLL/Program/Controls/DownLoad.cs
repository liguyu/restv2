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
    // 文件下载控件
    public class DownLoad : FrameworkElement, INotifyPropertyChanged
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

        #region Completed 下载完成事件
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

        #region Filter 文件过滤
        private string filter = "All Files(*.*)|*.*";
        public string Filter
        {
            get { return filter; }
            set { this.filter = value; }
        }
        #endregion

        #region Path 文件下载路径
        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if (path == value)
                {
                    return;
                }
                path = value;
                OnPropertyChanged("Path");
            }
        }
        #endregion

        #region Down 下载文件
        Stream fStream = null;
        public void Down()
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                Filter = Filter,
            };
            if (fileDialog.ShowDialog() == true)
            {
                fStream = fileDialog.OpenFile();
                //下载文件
                string uuid = System.Guid.NewGuid().ToString();
                string str = Path.Replace("\\", "%5E") + "?uuid=" + uuid;
                Uri uri = new Uri(str);
                WebClient client = new WebClient();
                client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
                IsBusy = true;
                client.OpenReadAsync(uri);
            }
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error + "");
                return;
            }
            byte[] buf = new byte[2048];
            int len = e.Result.Read(buf, 0, 2048);
            while (len != -1 && len != 0)
            {
                fStream.Write(buf, 0, len);
                len = e.Result.Read(buf, 0, 2048);
            }
            e.Result.Close();
            fStream.Flush();
            fStream.Close();
            //下载完成
            OnCompleted();
            IsBusy = false;
        }
        #endregion
    }
}
