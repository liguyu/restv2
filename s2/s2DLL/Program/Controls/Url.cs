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
using System.ComponentModel;
using Com.Aote.ObjectTools;

namespace Com.Aote.Controls
{
    // 根据给定的Url执行错误，并返回操作执行结果
    public class UrlInvoke : FrameworkElement, INotifyPropertyChanged
    {
        // 要执行的Url
        public string Url { get; set; }

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

        #region IsBusy 是否正忙
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
        #endregion

        #region State 工作状态
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
        #endregion

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(UrlInvoke),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        #region Completed 工作完成事件
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
        #endregion

        #region CanSave 是否开始执行
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(UrlInvoke),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveChanged)));

        private static void OnCanSaveChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            UrlInvoke bea = (UrlInvoke)dp;
            if (bea.CanSave)
            {
                bea.Invoke();
            }
            bea.CanSave = false;
        }

        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set
            {
                SetValue(CanSaveProperty, value);
            }
        }
        #endregion

        // 执行给定的Url
        public void Invoke()
        {
            //传递到后台执行
            Uri uri = new Uri(Url);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, e) =>
            {
                IsBusy = false;
                //通知数据提交过程完成
                if (e.Error != null)
                {
                    State = State.Error;
                    Error = e.Error.Message;
                }
                else
                {
                    if (!e.Result.Equals("ok"))
                    {
                        State = State.Error;
                        Error = e.Result;
                    }
                    else
                    {
                        State = State.End;
                    }
                }
                OnCompleted(e);
            };
            IsBusy = true;
            State = State.Start;
            client.DownloadStringAsync(uri);
        }
    }
}
