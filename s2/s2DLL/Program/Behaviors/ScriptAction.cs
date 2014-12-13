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
using System.Collections.Generic;
using System.Reflection;
using System.Json;
using Com.Aote.ObjectTools;
using System.ComponentModel;
using System.Windows.Interactivity;
using Com.Aote.Utils;
using System.Windows.Markup;
using System.Linq;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 执行一段脚本的Action，脚本写法与事件处理写法一样。
    /// </summary>
    public class ScriptAction : DependencyObject, IInitable, IName
    {
        #region CanRun 是否可以执行，满足执行条件时，脚本开始执行
        public static readonly DependencyProperty CanRunProperty =
            DependencyProperty.Register("CanRun", typeof(bool), typeof(ScriptAction),
            new PropertyMetadata(new PropertyChangedCallback(OnCanRunChanged)));
        public bool CanRun
        {
            get { return (bool)GetValue(CanRunProperty); }
            set { SetValue(CanRunProperty, value); }
        }

        //如果可以执行，执行脚本
        private static void OnCanRunChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ScriptAction sa = (ScriptAction)o;
            //如果可以执行，开始执行，执行完成后，修改CanRun为false，以便下次条件满足时，执行
            if (sa.CanRun)
            {
                //执行Start过程
                sa.OnStart(new EventArgs());
                sa.CanRun = false;
            }
        }
        #endregion

        #region Start事件，当可以执行时，触发Start事件，执行脚本
        public event EventHandler Start;
        public void OnStart(EventArgs args)
        {
            if (Start != null)
            {
                Start(this, args);
            }
        }
        #endregion

        #region IInitable Members

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

        //是否进行初始化处理
        public bool IsInited { set; get; }
        virtual public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            this.IsInited = true;
            this.OnInitFinished();
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

        public object FindResource(string name)
        {
            if (name == "this")
                return this;
            return UI.FindResource(name);
        }
        #endregion

        public string Name { get; set; }
    }
}
