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
using Com.Aote.Logs;
using Com.Aote.ObjectTools;
using Com.Aote.Utils;


namespace Com.Aote.Controls
{
    /**
     * 自定义子窗口
     */
    public class CustomChildWindow : ChildWindow
    {
  
        #region Result属性,子窗口返回结果

        public static readonly DependencyProperty ReturnValueProperty =
            DependencyProperty.Register("ReturnValue", typeof(object), typeof(CustomChildWindow),
            new PropertyMetadata(null));

 
        public object ReturnValue
        {
            get { return GetValue(ReturnValueProperty); }
            set { SetValue(ReturnValueProperty, value); }
        }
        #endregion

        #region Result属性,子窗口打开时参数

        public static readonly DependencyProperty ParamValueProperty =
            DependencyProperty.Register("ParamValue", typeof(object), typeof(CustomChildWindow),
            new PropertyMetadata(null));


        public object ParamValue
        {
            get { return GetValue(ParamValueProperty); }
            set { SetValue(ParamValueProperty, value); }
        }
        #endregion

        public object Parent { get; set; }

       
    }
}
