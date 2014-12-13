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
using System.Windows.Threading;

namespace Com.Aote.ObjectTools
{
    public class Timer : CustomTypeHelper, IAsyncObject
    {

        DispatcherTimer timer = new DispatcherTimer();

        public event RoutedEventHandler TimeEvent;

        public string Name
        {
            get;
            set;
        }

        //执行事件时间间隔,单位为毫秒
        private int interval;
        public int Interval 
        {
            get { return interval; }
            set
            {
                State = State.Start;
                this.interval = value;
                //间隔执行
                this.timeInterval();
            }
        }

        private void timeInterval()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, Interval);
            timer.Tick -= timer_Tick;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (TimeEvent != null)
            {
                TimeEvent(this, new RoutedEventArgs());
            }
            State = State.Loaded;
        }


        public bool IsBusy
        {
            get;
            set;
        }

        private State state;
        public State State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
                OnPropertyChanged("State");
            }
        }

        public string Error
        {
            get;
            set;
        }

        public event AsyncCompletedEventHandler Completed;

        public void OnCompleted(AsyncCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }
    }
}
