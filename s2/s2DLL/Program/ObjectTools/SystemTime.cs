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
using Com.Aote.Utils;
using System.Windows.Data;
using System.Windows.Threading;

namespace Com.Aote.ObjectTools
{
    //系统时间，为用户登录的机器的时间
    public class SystemTime : DependencyObject, ILoadable, IAsyncObject, INotifyPropertyChanged
    {
        //与服务器之间的时间差
        private long _timeDiff;

        private int Month;

        private int Day;

        //当前时间
        public DateTime Now
        {
            get {
                return DateTime.Now.AddMilliseconds(_timeDiff);
            }
        }

        //当前日期前推几个月的日期
        public DateTime MonthsBeforeToday(int months)
        {
            return Now.AddMonths(-months);
        }

        //当天，当天=当前时间
        public DateTime Today
        {
            get
            {
                return Now;
            }
        }

        public string Time
        {
            get 
            {
                return Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

        }

        //指定年月日
        public DateTime AppointedDate(int year, int month, int day)
        {
            DateTime result = new DateTime(year, month, day);
            return result;
        }

        //获取月底
        public DateTime MonthEnd
        {
            get
            {
                DateTime statedate = new DateTime(Now.Year, Now.Month, Now.Day);

                DateTime result = new DateTime(statedate.Year, statedate.Month, DateTime.DaysInMonth(statedate.Year, statedate.Month));
                return result;
            }
        }



        //获取第20天
        public DateTime TwentiethDay
        {
            get
            {
                Month = DateTime.Now.Month + 1;

                Day = 20;

                DateTime result = new DateTime(Now.Year, Month, Day);

                return result;
            }
        }


        #region Interval Now属性多长时间发生一次变化，单位为毫秒
        //每个时间对象都有一个时钟分配对象
        private DispatcherTimer _timer = new DispatcherTimer();

        private static DependencyProperty IntervalProperty = DependencyProperty.Register(
            "Interval", typeof(int), typeof(SystemTime), new PropertyMetadata(new PropertyChangedCallback(OnIntervalChanged)));
        private static void OnIntervalChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SystemTime st = (SystemTime)obj;
            st.IntervalChanged();
        }
        private void IntervalChanged()
        {
            //设置时钟，当时钟到时，通知Now发生了变化
            _timer.Stop();
            _timer.Tick -= TimerChanged;
            _timer.Tick += new EventHandler(TimerChanged);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, Interval);
            _timer.Start();
        }
        private void TimerChanged(object o, EventArgs e)
        {
            //时钟到的是，通知Now属性发生变化了
            OnPropertyChanged("Now");
        }
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }
        #endregion

        #region ILoadable Members

        /// <summary>
        /// 开始加载数据
        /// </summary>
        public void Load()
        {
            Uri uri = new Uri(WebClientInfo.BaseAddress + "/" + Path);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    //获得与服务器时间之间的时间差，以后每次要数据，加上这个时间差
                    DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                    TimeSpan ts = new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - from1970.Ticks);
                    _timeDiff = long.Parse(a.Result) - (long)ts.TotalMilliseconds;
                    State = State.Loaded;
                }
                else
                {
                    State = State.LoadError;
                    Error = a.Error.GetMessage();
                }
                IsBusy = false;
                //通知加载完成
                OnCompleted(a);
                OnDataLoaded(a);
            };
            IsBusy = true;
            OnLoading();
            State = State.StartLoad;
            client.DownloadStringAsync(uri);
        }

        public string Name { get; set; }

        public event EventHandler Loading;
        public void OnLoading()
        {
            if (Loading != null)
            {
                Loading(this, null);
            }
        }

        public event AsyncCompletedEventHandler DataLoaded;
        public void OnDataLoaded(AsyncCompletedEventArgs args)
        {
            if (DataLoaded != null)
            {
                DataLoaded(this, args);
            }
        }

        #region WebClientInfo 用于去后台获取数据的基本地址描述，在xaml文件中进行配置
        public WebClientInfo WebClientInfo { get; set; }
        #endregion

        #region Path 获取属性的路径，一旦发生改变，将重新获取属性，但是如果指明属性路径改变时，不加载数据，则不会这么做。

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(SystemTime),
            new PropertyMetadata(new PropertyChangedCallback(OnPathChanged)));

        private static void OnPathChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SystemTime go = (SystemTime)dp;
            go.Load();
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        #endregion

        public event AsyncCompletedEventHandler Completed;
        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }

        #region State 工作状态
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(State), typeof(SystemTime),
            new PropertyMetadata(null));

        public State State
        {
            get { return (State)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        #endregion

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(SystemTime),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        #region IsBusy 是否忙
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(SystemTime),
            new PropertyMetadata(null));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}

