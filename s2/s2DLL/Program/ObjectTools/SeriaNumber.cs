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
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    //编号产生器，获取编号值时，产生需服务器端计算的表达式，服务端把这部分内容计算后，回给客户端
    public class SeriaNumber : IInitable, IName
    {
        //编号key，每个key从1开始产生编号
        public string Key { get; set; }

        //是否进行初始化处理
        public bool IsInited { set; get; }


        //编号值，结果格式为#SeriaNumber?Key#，这种格式后台服务可以解析。
        public string Value 
        {
            get {
                if (this.Length == null || this.Length.Equals(""))
                {
                    return "#SeriaNumber?name=" + Key + "#";
                }
                else
                {
                    return "#SerialNumber?name=" + Key + "&length=" + this.Length + "#";
                }
               
            } 
        }

        //编号长度
        public string Length { get; set; }

        #region IInitable Members

        public string Name { get; set; }

     
        //提供环境信息的对象，可以是应用程序或者界面元素之一
        private object UI;

        //Loaded事件，触发这个事件通知配置等对象开始工作
        public event RoutedEventHandler Loaded;
        private void OnLoaded()
        {
            if (Loaded != null)
            {
                Loaded(UI, new RoutedEventArgs());
            }
        }

        virtual public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            this.IsInited = true;
            this.OnInitFinished();
        }

        public object FindResource(string name)
        {
            if (name == "this")
                return this;
            return UI.FindResource(name);
        }

        //初始化完成事件
        public event RoutedEventHandler InitFinished;
        public void OnInitFinished()
        {
            if (this.InitFinished != null)
            {
                InitFinished(this, null);
            }
        }
        
        #endregion


    }
}
