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

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 可初始化的对象，这类对象可以在界面加载完成后，执行自己的初始化过程
    /// </summary>
    public interface IInitable
    {
        //是否初始化过
        bool IsInited { set; get; }
        
        //初始化，ui参数可以让对象获得周围环境，有可能是界面元素，也有可能是应用程序
        void Init(object ui);

        //找资源
        object FindResource(string name);

        //可出示化对象必须通过Loaded事件触发本身的初始化过程
        event RoutedEventHandler Loaded;

        //初始化完成事件
        event RoutedEventHandler InitFinished;
    }
}
