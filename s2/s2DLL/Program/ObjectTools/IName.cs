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
    //放到资源管理中的对象必须有名字
    public interface IName
    {
        string Name { get; set; }
    }
}
