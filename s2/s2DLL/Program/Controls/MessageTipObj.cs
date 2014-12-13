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


namespace Com.Aote.Controls
{

   
    /**
     * 信息提示对象
     **/
    public class MessageTipObj : FrameworkElement
    {


        /// <summary>
        /// OK事件，点击确定发出
        /// </summary>
        public event EventHandler OK;
        public void OnOK()
        {
            if (OK != null)
            {
                OK(this,null);
            }
        }

        /// <summary>
        /// Cancle事件，点击取消发出
        /// </summary>
        public event EventHandler Cancel;
        public void OnCancel()
        {
            if (Cancel != null)
            {
                Cancel(this,null);
            }
        }


        public MessageType Type { get; set; }
        


        //提示信息
        public static readonly DependencyProperty TipProperty =
           DependencyProperty.Register("Tip", typeof(string), typeof(MessageTipObj),
           new PropertyMetadata(null));

        public string Tip
        {
            get { return (string)GetValue(TipProperty); }
            set { SetValue(TipProperty, value); }
        }

        

        //显示方法
        public void Show()
        {
            if (Type == null || Type == MessageType.MessageBoxResult)
            {
                MessageBoxResult mbr = MessageBox.Show(Tip, "提示", MessageBoxButton.OKCancel);
                if (mbr == MessageBoxResult.OK)
                {
                    this.OnOK();
                }
                else
                {
                    this.OnCancel();
                }
            }
            else if (Type == MessageType.MessageBox)
            {
                MessageBox.Show(Tip);
            }
        }

        //状态控制弹出提示框
        public static readonly DependencyProperty IsShowProperty =
           DependencyProperty.Register("IsShow", typeof(bool), typeof(MessageTipObj),
           new PropertyMetadata(new PropertyChangedCallback(OnIsShowPropertyChanged)));

        private static void OnIsShowPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            MessageTipObj go = (MessageTipObj)dp;
            //如果指明Path改变时，不加载数据，则只有当外界要求，加载数据时，才加载
            if (go.IsShow)
            {
                go.Show();
                go.IsShow = false;
            }
        }
        public bool IsShow
        {
            get { return (bool)GetValue(IsShowProperty); }
            set { SetValue(IsShowProperty, value); }
        }


        public enum MessageType
        {
            MessageBoxResult,
            MessageBox
        }
    }
}
