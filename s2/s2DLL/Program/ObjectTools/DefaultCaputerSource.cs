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
using System.IO;
using FluxJpeg.Core;
using FluxJpeg.Core.Encoder;

namespace Com.Aote.ObjectTools
{
    public class DefaultCaputerSource : CustomTypeHelper
    {
        public DefaultCaputerSource()
        {
            if (CaptureDeviceConfiguration.RequestDeviceAccess())
            {
                //启动摄像头
                video.Start();
            }
            else
            {
                MessageBox.Show("设备启动错误！");
            }
            
            
            video.CaptureImageCompleted += (o, e) =>
            {
                CutImage = e.Result;
            };
        }

        CaptureSource video = new CaptureSource();
       

        public CaptureSource Video
        {
            get { return video; }
        }

        WriteableBitmap wbp;
        public WriteableBitmap CutImage 
        {
            get { return wbp; }
            set
            {
                wbp = value;
                OnPropertyChanged("CutImage");
            }
        }

        


    }

}
