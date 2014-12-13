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
using Com.Aote.ObjectTools;
using System.Windows.Browser;

namespace Com.Aote.ObjectTools
{
    public class UtilAction : CustomTypeHelper, IName
    {

        //执行js
        public string Js { get; set; }
        public void DoJs()
        {
            string str = Js;
            foreach (var item in this._customPropertyValues.Keys)
            {
                str = str.Replace("#" + item + "#", this._customPropertyValues[item]+"");
            }
            HtmlPage.Window.Eval(str);
        }

        //计算日期差

        public string LargeTime 
        { 
            set; 
            get; 
        }
        public string SmallTime { set; get; }

        private int difference;
        public int Difference 
        {
            get 
            {
                return this.difference; 
            }
            set
            {
                this.difference = value;
                OnPropertyChanged("Difference");
            }
        }
        //计算差
        public void Calu() 
        {
            if (LargeTime != null && SmallTime != null)
            {
                DateTime largeTime = DateTime.Parse(LargeTime);
                DateTime smallTime = DateTime.Parse(SmallTime);
                TimeSpan s = new TimeSpan(largeTime.Ticks - smallTime.Ticks);
                this.Difference = s.Days;
            }
        }

        

        //是否计算
        private bool calculate = false;
        public bool Calculate
        {
            get { return this.calculate; }
            set
            {
                this.calculate = value;
                if (this.calculate)
                {
                    Calu();
                }
            }
        }

        //跳转页面
        public string Page { get; set; }
        public void Open()
        {
            Page p = (Page)Application.Current.RootVisual;
            Frame frame = (Frame)p.FindName("frame");
            Uri uri = new Uri(Page, UriKind.RelativeOrAbsolute);
            frame.Navigate(uri);
        }

        public string Name
        {
            get;
            set;
        }
    }
}
