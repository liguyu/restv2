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

namespace Com.Aote.ObjectTools
{
    //日期对象，获得选择的日期属性
    public class DateObject : CustomTypeHelper, IAsyncObject
    {
        #region Year
        private int year;
        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
                OnPropertyChanged("Year");
            }
        }
        #endregion

        #region Month
        private int month;
        public int Month 
        {
            get
            {
                return month;
            }
            set
            {
                month = value;
                OnPropertyChanged("Month");
            }
        }
        #endregion

        #region Day
        private int day;
        public int Day
        {
            get
            {
                return day;
            }
            set
            {
                day = value;
                OnPropertyChanged("Day");
            }
        }
        #endregion

        #region TMonth
        private string tmonth;
        public string TMonth
        {
            get
            {
                return tmonth;
            }
            set
            {
                if (Int32.Parse(value) % 2 == 0)
                {
                    tmonth = "2,4,6,8,10,12";
                }
                else
                {
                    tmonth  = "1,3,5,7,9,11";
                }
                OnPropertyChanged("TMonth");
            }
        }
        #endregion


        #region CurrentDate 当前日期

        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(DateTime), typeof(DateObject),
            new PropertyMetadata(new PropertyChangedCallback(OnCurrentDateChanged)));

        private static void OnCurrentDateChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            DateObject go = (DateObject)dp;
            DateTime dt = (DateTime)args.NewValue;
            go.Month = dt.Month;
            go.TMonth = dt.Month+"";
            go.Year = dt.Year;
            go.Day = dt.Day;
        }

        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }
        #endregion

        public bool IsBusy
        {
            get;
            set;
        }

        public State State
        {
            get;
            set;
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

        public string Name
        {
            get;
            set;
        }
    }
}
