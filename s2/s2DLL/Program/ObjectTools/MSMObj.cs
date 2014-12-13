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

namespace Com.Aote.ObjectTools
{
    //短信发送对象
    public class MSMObj : CustomTypeHelper, IAsyncObject
    {
        #region WebClientInfo 用于去后台获取数据的基本地址描述
        /// <summary>
        /// 用于去后台获取数据的基本地址描述
        /// </summary>
        public WebClientInfo WebClientInfo { get; set; }
        #endregion

        //电话号码
        public string Phone { get; set; }

        //发送信息 
        public string Message { get; set; }

        //发送短信方法
        public void SendMsg()
        {
            //传递到后台执行
            Uri uri = new Uri(WebClientInfo.BaseAddress+"/msm/"+Phone+"/"+Message+"/");
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    State = State.Loaded;
                }
                else
                {
                    State = State.LoadError;
                }
                State = State.End;
                IsBusy = false;
                //通知加载完成
                OnCompleted(a);
            };
            IsBusy = true;
            State = State.StartLoad;
            client.DownloadStringAsync(uri);
        }
        //发送表示
        private bool send = false;
        public bool Send
        {
            set
            {
                this.send = value;
                if (this.send)
                {
                    SendMsg();
                }
            }
            get { return this.send; }

        }


        public bool IsBusy
        {
            get;
            set;
        }

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

        public string Error
        {
            get;
            set;            
        }

        public event System.ComponentModel.AsyncCompletedEventHandler Completed;

        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        public string Name
        {
            get;
            set;
        }
    }
}
