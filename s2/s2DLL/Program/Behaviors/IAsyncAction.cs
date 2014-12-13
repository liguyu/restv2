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
using System.ComponentModel;
using Com.Aote.ObjectTools;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 代表一个异步动作，异步动作既是一个动作（继承了IAciont），又是一个异步对象（继承了IAsyncObject）
    /// </summary>
    public interface IAsyncAction : IAction, IAsyncObject
    {
    }
}
