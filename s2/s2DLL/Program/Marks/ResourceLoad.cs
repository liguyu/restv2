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
using System.Windows.Markup;
using System.Collections.Generic;
using Com.Aote.ObjectTools;
using System.Linq;

namespace Com.Aote.Marks
{
    //是一个包含资源的界面对象
    [ContentProperty("Res")]
    public class ResourceLoad : FrameworkElement
    {
        //资源配置过程，每个资源必须有名字，名字作为资源的关键字
        private List<IName> res = new List<IName>();
        public List<IName> Res 
        {
            get { return res; }
        }

        public ResourceLoad()
        {
            Loaded += new RoutedEventHandler(ResourceLoadHandle);
        }

        private void ResourceLoadHandle(object o, RoutedEventArgs e)
        {
            Loaded -= ResourceLoadHandle;
            foreach (IInitable obj in (from p in res where p is IInitable select p))
            {
                obj.Init(this.Parent);
            }
        }
    }
}
