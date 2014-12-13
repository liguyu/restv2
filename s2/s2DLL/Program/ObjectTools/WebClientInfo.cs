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
using System.Windows.Browser;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 提供WebClient调用时的基础地址信息，可以引用资源，不可以绑定。
    /// </summary>
    public class WebClientInfo
    {
        /// <summary>
        /// 基础地址信息
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// 是否自动获得ip
        /// </summary>
        private bool auto;
        public bool AutoIp 
        {
            get { return auto; }
            set 
            {
                auto = value;
                //true
                if (auto)
                {
                    //服务器地址
                    string host = HtmlPage.Document.DocumentUri.Host;
                    //string ip = "http://" + host;
                    BaseAddress = BaseAddress.Replace("#IP#", host); ;
                }
            }
        }
    }
}