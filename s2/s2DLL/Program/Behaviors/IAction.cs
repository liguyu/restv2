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

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 代表一个可以执行动作的对象，所有动作对象都有一个Invoke方法，调用Invoke方法，动作开始工作。
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// 所有子类的工作过程在这个方法里实现。外界调用这个方法，让动作开始执行。
        /// </summary>
        void Invoke();
    }
}
