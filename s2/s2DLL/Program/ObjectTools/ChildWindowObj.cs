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
using Com.Aote.Utils;
using Com.Aote.Logs;
using System.Windows.Navigation;
using Com.Aote.Controls;

namespace Com.Aote.ObjectTools
{
    /**
     * 子窗口对象，作为中转对象，用于打开子串口，获取子窗口结果
     */
    public class ChildWindowObj : DependencyObject, IInitable, IName
    {

        private static Log Log = Log.GetInstance("Com.Aote.Controls.ChildWindowObj");

         //提供环境信息的对象，可以是应用程序或者界面元素之一
        private object UI;

        public string Name { get; set; }

        //子窗口页面名称
        public string CWName { set; get; }

        public object ParamObj { set; get; }
        
        #region Open属性，当Open属性为true,打开自己

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ChildWindowObj),
            new PropertyMetadata(new PropertyChangedCallback(OnIsOpenChanged)));

        public static void OnIsOpenChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            object newVal = args.NewValue;
            //IsOpen为True,并且有子窗口名称，创建子窗口并显示
            if (newVal != null && (bool)newVal)
            {
                ChildWindowObj cwo = (ChildWindowObj)dp;
                if (cwo.CWName == null)
                {
                    throw new Exception("子窗口名称为空");
                }
                PageResourceContentLoader load = new PageResourceContentLoader();
                load.BeginLoad(new Uri(cwo.CWName + ".xaml", UriKind.Relative), null, new AsyncCallback(r =>
                {
                    LoadResult ui = load.EndLoad(r);
                    CustomChildWindow showWin = (CustomChildWindow)ui.LoadedContent;
                    if (cwo.ParamObj != null)
                    {
                        showWin.ParamValue = cwo.ParamObj;
                    }
                    showWin.Show();
                    showWin.Parent = dp;
                    //注册关闭事件获取返回值
                    showWin.Closed += (o, e) =>
                        {
                            CustomChildWindow cw = (CustomChildWindow)o;
                            if (cw.ReturnValue != null)
                            {
                                cw.ReturnValue.ToString();
                                cwo.Result = cw.ReturnValue;
                                cwo.OnCompleted();
                            }
                        };
                }), 1);
                cwo.IsOpen = false;
            }

        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion


        #region Result属性,子窗口返回结果

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(object), typeof(ChildWindowObj),
            new PropertyMetadata(null));



        public object Result
        {
            get { return GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }
        #endregion


         #region IInitable Members
        public bool IsInited { set; get; }

        public void Init(object ui)
        {
            this.UI = ui;
            this.IsInited = true;
            this.OnInitFinished();
        }

        public object FindResource(string name)
        {
            if (name == "this")
                return this;
            return UI.FindResource(name);
        }

        public event RoutedEventHandler InitFinished;
        public void OnInitFinished()
        {
            if (this.InitFinished != null)
            {
                InitFinished(this, null);
            }
        }

        //Loaded事件，触发这个事件通知配置等对象开始工作
        public event RoutedEventHandler Loaded;
        private void OnLoaded()
        {
            if (Loaded != null)
            {
                Loaded(UI, new RoutedEventArgs());
            }
        }

        #endregion

        #region Completed 完成事件带回返回值
        /// <summary>
        /// 完成事件带回返回值
        /// </summary>
        public event EventHandler Completed;
        public void OnCompleted()
        {
            if (Completed != null)
            {
                Completed(this, null);
            }
        }
        #endregion





         
    }
}
