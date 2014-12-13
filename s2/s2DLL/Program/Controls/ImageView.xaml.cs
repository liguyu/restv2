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
using Com.Aote.ObjectTools;

namespace Com.Aote.Controls
{
    //图片显示窗口
    public partial class ImageView : UserControl, IName
    {
        #region Path 图片路径
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageView), null);

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion

        public ImageView()
        {
            InitializeComponent();
        }

        public void Show()
        {
            //image位置还原
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            image.Width = double.NaN;
            //变为可见
            this.Visibility = Visibility.Visible;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //关闭窗口
            this.Visibility = Visibility.Collapsed;
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                image.Width = image.ActualWidth * (e.Delta / 120 * 0.9);
            }
            if (e.Delta < 0)
            {
                image.Width = image.ActualWidth / (-e.Delta / 120 * 0.9);
            }
        }

        private bool isDrag = false;
        private Point startPoint;
        private Point oldPos;

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            //在拖动，将image位置移动
            if (isDrag)
            {
                Point pos = e.GetPosition(LayoutRoot);
                Canvas.SetLeft(image, oldPos.X + pos.X - startPoint.X);
                Canvas.SetTop(image, oldPos.Y + pos.Y - startPoint.Y);
            }
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(LayoutRoot);
            isDrag = true;
            oldPos = new Point(Canvas.GetLeft(image), Canvas.GetTop(image));
            image.CaptureMouse();
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrag = false;
            image.ReleaseMouseCapture();
        }
    }
}

