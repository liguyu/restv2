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
using System.Windows.Navigation;

namespace Com.Aote.Controls
{
    public class MyFrame : Grid
    {
        #region Source 当Source发生改变时，根据Source指定的Uri加载页面
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", typeof(string), typeof(MyFrame), new PropertyMetadata(OnSourceChanged));
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            MyFrame c = (MyFrame)d;
            PageResourceContentLoader load = new PageResourceContentLoader();
            load.BeginLoad(new Uri(e.NewValue.ToString(), UriKind.Relative), null, new AsyncCallback(r =>
            {
                LoadResult ui = load.EndLoad(r);
                c.Children.Clear();
                c.Children.Add((UIElement)ui.LoadedContent);
            }), 1);
        }

        public string Source
        {
            get
            {
                return (string)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }
        #endregion
    }
}
