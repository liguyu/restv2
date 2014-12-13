using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.Attachs
{
    public class VideoAttach
    {
        #region VideoSource附加到VideoBrush

        public static DependencyProperty VideoSourceProperty =
      DependencyProperty.RegisterAttached("VideoSource", typeof(CaptureSource), typeof(VideoBrush), new PropertyMetadata(VideoSouceChanged));

        public static CaptureSource GetVideoSource(DependencyObject d)
        {
            return (CaptureSource)d.GetValue(VideoSourceProperty);
        }
        public static void SetVideoSource(DependencyObject d, CaptureSource value)
        {
            d.SetValue(VideoSourceProperty, value);
        }
        private static void VideoSouceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CaptureSource video = (CaptureSource)e.NewValue;
            VideoBrush vb = (VideoBrush)d;
            
            vb.SetSource(video);
 
        }
        #endregion


        #region VideoDevice附加到capturesource

        public static DependencyProperty VideoDeviceProperty =
      DependencyProperty.RegisterAttached("VideoDevice", typeof(bool), typeof(CaptureSource), new PropertyMetadata(CaptureSourceChanged));

        public static bool GetCaptureSource(DependencyObject d)
        {
            return (bool)d.GetValue(VideoDeviceProperty);
        }
        public static void SetCaptureSource(DependencyObject d, bool value)
        {
            d.SetValue(VideoDeviceProperty, value);
        }
        private static void CaptureSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

          
            CaptureSource cs = (CaptureSource)d;
            MessageBox.Show("test");
            cs.VideoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();
           
           
         
         }
        #endregion
    }
}
