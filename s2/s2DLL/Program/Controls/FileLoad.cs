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
    public class FileLoad : FrameworkElement, INotifyPropertyChanged, IAsyncObject
    {
        private static Log Log = Log.GetInstance("Com.Aote.Controls.FileLoad");

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

        /// <summary>
        /// 异步对象工作状态。
        /// </summary>
        private State state;
        public State State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(FileLoad),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
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
        
        #region 实体id
        public static readonly DependencyProperty BlobIdProperty =
            DependencyProperty.Register("BlobId", typeof(string), typeof(FileLoad), new PropertyMetadata(null));

        public string BlobId
        {
            get { return (string)GetValue(BlobIdProperty); }
            set { SetValue(BlobIdProperty, value); }
        }
        #endregion

        private string filter = "All Files(*.*)|*.*";
        public string Filter
        {
            get { return filter; }
            set { this.filter = value; }
        }

        private string limit;
        public string Limit
        {
            get { return limit; }
            set { this.limit = value; }
        }

        #region 文件名称
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(FileLoad), new PropertyMetadata(null));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }
        #endregion

        public string Path { get; set; }

        //实体名
        public string EntityName { get; set; }

        private string tempid = null;
        public void UpLoad()
        {
         
            if (tempid == null)
            {
                tempid = System.Guid.NewGuid().ToString();
            }
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = Filter,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                State = State.StartLoad;
                FileInfo fi = openFileDialog.File;
               // MessageBox.Show(fi.Length + "|||" + Int32.Parse(Limit) * 1000);
                //判断附件大小
                if (Limit != null)
                {
                    if (fi.Length >Int32.Parse(Limit)*1000)
                    {
                        MessageBox.Show(fi.Name + "附件大小超过限制，不能上传！");
                        State = State.LoadError;
                        return;
                    }
                }
                this.FileName = fi.Name;
                StreamReader sr = fi.OpenText();
                WebClient webclient = new WebClient();
                Uri uri = new Uri(Path + "?FileName=" + this.FileName + "&BlobId=" + tempid + "&EntityName=" + EntityName);
                webclient.OpenWriteCompleted += new OpenWriteCompletedEventHandler(webclient_OpenWriteCompleted);
                webclient.Headers["Content-Type"] = "multipart/form-data";
                webclient.OpenWriteAsync(uri, "POST", fi.OpenRead());
                webclient.WriteStreamClosed += new WriteStreamClosedEventHandler(webclient_WriteStreamClosed);
            }
        }

        //将文件数据流发送到服务器上
        void webclient_OpenWriteCompleted(object sender, OpenWriteCompletedEventArgs e)
        {
            IsBusy = true;
            // e.UserState - 需要上传的流（客户端流）
            Stream clientStream = e.UserState as Stream;
            // e.Result - 目标地址的流（服务端流）
            Stream serverStream = e.Result;
            byte[] buffer = new byte[clientStream.Length];
            int readcount = 0;
            // clientStream.Read - 将需要上传的流读取到指定的字节数组中
            while ((readcount = clientStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // serverStream.Write - 将指定的字节数组写入到目标地址的流
                serverStream.Write(buffer, 0, readcount);
            }
            serverStream.Close();
            clientStream.Close();
        }

        void webclient_WriteStreamClosed(object sender, WriteStreamClosedEventArgs e)
        {
            //该值指示异步操作是否已被取消
            bool Cancelled = true;
            //上传文件成功;
            if (e.Error == null)
            {
                //设置实体id
                this.BlobId = tempid;
                State = State.Loaded;
                tempid = null;
            }
            else
            {
                Cancelled = false;
                State = State.LoadError;
                tempid = null;
                Error = e.Error.Message;
            }
            AsyncCompletedEventArgs args = new AsyncCompletedEventArgs(e.Error, Cancelled, State);
            OnCompleted(args);
            IsBusy = false;
        }


        /// <summary>
        /// 工作完成事件
        /// </summary>
        public event AsyncCompletedEventHandler Completed;
        
        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }
    }
}
