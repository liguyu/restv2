using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.Controls
{
    public partial class ShowMessage : CustomChildWindow
    {
        public ShowMessage()
        {
            InitializeComponent();
        }

        #region Message 窗口弹出提示信息
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ShowMessage),
            new PropertyMetadata(new PropertyChangedCallback(OnMessageChanged)));

        private static void OnMessageChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            ShowMessage sm = (ShowMessage)dp;
            TextBlock tb = sm.FindName("MessageText") as TextBlock;
            tb.Text = args.NewValue.ToString();
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        #endregion

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReturnValue = true;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReturnValue = false;
            this.DialogResult = false;
        }
    }
}

