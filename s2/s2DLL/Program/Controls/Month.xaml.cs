using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Com.Aote.Controls
{
	public partial class Month : UserControl
    {
        #region SelectedDate 最后选择的结果。

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(Month),
            new PropertyMetadata(new PropertyChangedCallback(OnSelectedDateChanged)));

        private static void OnSelectedDateChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            Month m = (Month)dp;
            m.SelectedDateChanged();
        }

        private void SelectedDateChanged()
        {
            if (SelectedDate == null)
            {
                //设置文本框内容为空
                text.Text = "";
                //选择项的年月为空
                yearlist.SelectedItem = null;
                monthlist.SelectedItem = null;
            }
            else
            {
                DateTime dt = SelectedDate.Value;
                //设置文本显示框内容
                text.Text = dt.ToString(StringFormat);
                //设置年月选择项
                yearlist.SelectedItem = dt.Year;
                monthlist.SelectedItem = dt.Month;
            }
        }

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }
        #endregion

        #region StringFormat 日期格式
        public string StringFormat { get; set; }
        #endregion

        public Month()
		{
			// Required to initialize variables
			InitializeComponent();
            // 给月份付选择项
            List<int> months = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                months.Add(i);
            }
            monthlist.ItemsSource = months;
            // 给年付选择项
            List<int> years = new List<int>();
            for (int i = 2001; i <= 2040; i++)
            {
                years.Add(i);
            }
            yearlist.ItemsSource = years;
		}

		private void monthlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
            if (monthlist.SelectedItem == null)
            {
                return;
            }
            //关闭弹出框
            toggle.IsChecked = false;
            //由选择项产生最后的结果
            SelectedDate = new DateTime((int)yearlist.SelectedItem, (int)monthlist.SelectedItem, 1);
        }

		private void yearlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			//月份选择变空
            monthlist.SelectedItem = null;
		}

		private void clear_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            //清除选择内容
            SelectedDate = null;
		}
	}
}