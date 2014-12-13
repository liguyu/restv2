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
using Com.Aote.Utils;

namespace Com.Aote.Behaviors
{
    /// <summary>
    /// 异步调用对象方法的动作，被调用的对象必须是一个异步对象，被调用的方法必须是一个异步执行方法。
    /// 执行完被调用的方法后，必须返回异步完成事件，并处理好异步执行结果。
    /// </summary>
    public class CallMethodAction : BaseAsyncAction
    {
        /// <summary>
        /// 要执行异步过程的对象，可以用StaticResource引用，不能绑定。
        /// </summary>
        public IAsyncObject TargetObject { get; set; }

        /// <summary>
        /// 要调用的方法名称。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 调用目标对象的方法，开始工作。调用目标对象方法前，监听目标对象的工作完成事件。
        /// 目标对象工作完成后，要删除掉该监听器，以保证只在这个动作调用目标对象方法工作期间进行监听。
        /// </summary>
        public override void Invoke()
        {
            //调用前监听对象异步工作完成事件
            TargetObject.Completed += TargetObjectCompleted;
            //调用对象方法
            IsBusy = true;
            State = State.Start;
            TargetObject.GetType().GetMethod(MethodName).Invoke(TargetObject, null);
        }

        /// <summary>
        /// 目标对象工作完成后的处理过程。首先删除监听，然后通知工作完成。
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TargetObjectCompleted(object o, AsyncCompletedEventArgs e)
        {
            //删除本事件处理器
            TargetObject.Completed -= TargetObjectCompleted;
            IsBusy = false;
            if (e.Error != null)
            {
                State = State.Error;
                Error = e.Error.GetMessage();
            }
            else
            {
                State = State.End;
            }
            //通知完成结果
            OnCompleted(e);
        }
    }
}
