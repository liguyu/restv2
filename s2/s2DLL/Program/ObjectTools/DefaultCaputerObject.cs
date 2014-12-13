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
using System.Windows.Media.Imaging;
using FluxJpeg.Core;
using System.IO;
using FluxJpeg.Core.Encoder;
using System.ComponentModel;

namespace Com.Aote.ObjectTools
{
    public class DefaultCaputerObject :CustomTypeHelper, IAsyncObject
    {
        public string DefaultCaputerName { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string EntityName { get; set; }

        //执行拍照
        public void CaptureImageAnsync()
        {
            DefaultCaputerSource dcs = Application.Current.Resources[DefaultCaputerName] as DefaultCaputerSource;
            dcs.Video.CaptureImageAsync();
        }

        private string tempid = null;
        //保存照片
        public void SaveImage()
        {
            DefaultCaputerSource dcs = Application.Current.Resources[DefaultCaputerName] as DefaultCaputerSource;
            if (dcs.CutImage != null)
            {
                IsBusy = true;
                if (tempid == null)
                {
                    tempid = System.Guid.NewGuid().ToString();
                }
                byte[] by = Convert.FromBase64String(GetBase64Image(dcs.CutImage));
                Stream sr = new MemoryStream(by);
                WebClient webclient = new WebClient();
                Uri uri = new Uri(Path + "?FileName=" + FileName + "&BlobId=" + tempid + "&EntityName=" + EntityName);
                webclient.OpenWriteCompleted += new OpenWriteCompletedEventHandler(webclient_OpenWriteCompleted);
                webclient.Headers["Content-Type"] = "multipart/form-data";
                webclient.OpenWriteAsync(uri, "POST", sr);
                webclient.WriteStreamClosed += new WriteStreamClosedEventHandler(webclient_WriteStreamClosed);
            }
            else
            {
                MessageBox.Show("请拍照！");
            }
        }



        //将文件数据流发送到服务器上
        void webclient_OpenWriteCompleted(object sender, OpenWriteCompletedEventArgs e)
        {
            
            // e.UserState - 需要上传的流（客户端流）
            Stream clientStream = e.UserState as Stream;
            // e.Result - 目标地址的流（服务端流）
            Stream serverStream = e.Result;
            byte[] buffer = new byte[clientStream.Length / 1024];
            int readcount = 0;
            // clientStream.Read - 将需要上传的流读取到指定的字节数组中
            while ((readcount = clientStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // serverStream.Write - 将指定的字节数组写入到目标地址的流
                serverStream.Write(buffer, 0, readcount);
            }
            serverStream.Close();
            clientStream.Close();
        }

        void webclient_WriteStreamClosed(object sender, WriteStreamClosedEventArgs e)
        {
            //该值指示异步操作是否已被取消
            bool Cancelled = true;
            //上传文件成功;
            if (e.Error == null)
            {
                //设置实体id
                this.BlobId = tempid;
                State = State.Loaded;
            }
            else
            {
                Cancelled = false;
                State = State.LoadError;
                Error = e.Error.Message;
            }
            AsyncCompletedEventArgs args = new AsyncCompletedEventArgs(e.Error, Cancelled, State);
            OnCompleted(args);
            IsBusy = false;
        }

        #region IsInit 初始状态是指没有修改过的新对象，也就是调用new以后的状态
        public static readonly DependencyProperty IsInitProperty =
            DependencyProperty.Register("IsInit", typeof(bool), typeof(DefaultCaputerObject),
            new PropertyMetadata(new PropertyChangedCallback(OnIsInitChanged)));

        private static void OnIsInitChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            DefaultCaputerObject go = (DefaultCaputerObject)dp;
            //如果指明Path改变时，不加载数据，则只有当外界要求，加载数据时，才加载
            if (go.IsInit)
            {
                go.tempid = null;
                go.BlobId = null;
                DefaultCaputerSource dcs = Application.Current.Resources[go.DefaultCaputerName] as DefaultCaputerSource;
                dcs.CutImage = null;
            }
        }

        public bool IsInit
        {
            get { return (bool)GetValue(IsInitProperty); }
            set { SetValue(IsInitProperty, value); }
        }
        #endregion

        #region 实体id
        public static readonly DependencyProperty BlobIdProperty =
            DependencyProperty.Register("BlobId", typeof(string), typeof(DefaultCaputerObject), new PropertyMetadata(null));

        public string BlobId
        {
            get { return (string)GetValue(BlobIdProperty); }
            set { SetValue(BlobIdProperty, value); }
        }
        #endregion


        #region 文件名称
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(DefaultCaputerObject), new PropertyMetadata(null));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }
        #endregion

        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        private State state;
        public State State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #region Error 单条错误信息
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(string), typeof(DefaultCaputerObject),
            new PropertyMetadata(null));

        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
        #endregion

        public event System.ComponentModel.AsyncCompletedEventHandler Completed;

        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            
        }

        private string GetBase64Image(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bands = 3;
            byte[][,] raster = new byte[bands][,];

            for (int i = 0; i < bands; i++)
            {
                raster[i] = new byte[width, height];
            }

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int pixel = bitmap.Pixels[width * row + column];
                    raster[0][column, row] = (byte)(pixel >> 16);
                    raster[1][column, row] = (byte)(pixel >> 8);
                    raster[2][column, row] = (byte)pixel;
                }
            }

            ColorModel model = new ColorModel { colorspace = ColorSpace.RGB };
            FluxJpeg.Core.Image img = new FluxJpeg.Core.Image(model, raster);
            MemoryStream stream = new MemoryStream();
            JpegEncoder encoder = new JpegEncoder(img, 100, stream);
            encoder.Encode();

            stream.Seek(0, SeekOrigin.Begin);
            byte[] binaryData = new Byte[stream.Length];
            long bytesRead = stream.Read(binaryData, 0, (int)stream.Length);

            string base64String =
                    System.Convert.ToBase64String(binaryData,
                                                  0,
                                                  binaryData.Length);

            return base64String;

        }
    }
}
