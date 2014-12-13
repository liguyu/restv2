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

//应用程序更新对象，检查是否有更新
namespace Com.Aote.ObjectTools
{
    public class AppUpdateObj : DependencyObject, IAsyncObject, INotifyPropertyChanged
    {

        public AppUpdateObj()
        {
            //如果是安装状态，检查是否有自动更新
            if (Application.Current.InstallState == InstallState.Installed)
            {
                Application.Current.CheckAndDownloadUpdateCompleted += (o, a) =>
                {
                    if (a.UpdateAvailable == true && a.Error == null)
                    {
                        this.IsBusy = false;
                        this.State = State.End;
                        MessageBox.Show("检测应用更新并完成,请重启应用程序!");
                        Application.Current.MainWindow.Close();
                    }
                    else if(a.Error != null)
                    {
                        this.Error = "在检测应用更新时,出现以下错误信息:"+ a.Error.Message;
                        this.IsBusy = false;
                        this.State = State.Error;
                    }
                };
                this.IsBusy = true;
                this.State = State.Start;
                Application.Current.CheckAndDownloadUpdateAsync();
            }
            else
            {
                //不更新
                this.State = State.End;
            }
            
        }

        
        #region IAsyncObject Members
        /// <summary>
        /// 异步操作名称，方便在错误时显示错误信息
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否正在工作属性的内部表示
        /// </summary>
        private bool isBusy = false;

        /// <summary>
        /// 实现了是否忙属性，当是否忙属性改变时，调用属性改变事件通知方法，通知外部属性改变了。
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        /// <summary>
        /// 实现了工作完成事件，当异步动作完成工作后，将触发该事件。
        /// </summary>
        public event AsyncCompletedEventHandler Completed;

        /// <summary>
        /// 提供工作完成事件的通知方法，子类及自身在工作完成后，调用这个方法通知外部工作完成了。
        /// </summary>
        /// <param name="args"></param>
        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }

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
            DependencyProperty.Register("Error", typeof(string), typeof(AppUpdateObj),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

         #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        
 
       
 
        
    }
}
