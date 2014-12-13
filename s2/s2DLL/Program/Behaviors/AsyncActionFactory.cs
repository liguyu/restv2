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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Com.Aote.ObjectTools;

namespace Com.Aote.Behaviors
{
    //所有动作并发执行
    /// <summary>
    /// 代表一个并发异步动作组，组里的所有异步动作是并发执行的。
    /// 异步动作组在开始工作时，让所有动作开始执行，并登记这些动作的执行结果为空。
    /// 当某一个动作执行完成后，检查执行结果表，如果还有没有完成的动作，继续等待。
    /// 如果所有动作都完成了，根据执行结果表确定动作组的执行结果。
    /// 如果其中有一个动作执行失败，则动作组执行失败。只有所有动作都执行成功，动作组才算执行成功。
    /// </summary>
    public class AsyncActionFactory : ActionFactory
    {
        /// <summary>
        /// 异步动作执行状态表，登记所有异步动作的执行状态。
        /// key为异步动作，value为异步动作完成时的参数，如果为空，说明异步动作还没有完成
        /// </summary>
        private Dictionary<IAsyncAction, AsyncCompletedEventArgs> asyncActions = new Dictionary<IAsyncAction, AsyncCompletedEventArgs>();

        #region Errors 错误对象列表，只有并发动作组才有错误对象列表
        List<ErrorInfo> _errors = new List<ErrorInfo>();
        List<ErrorInfo> Errors 
        {
            get { return _errors; }
        }
        #endregion

        /// <summary>
        /// 开始执行并发动作组，执行前将异步动作状态表清空，表明所有动作都没有完成。
        /// 并且设置IsBusy为True，表明并发异步动作组开始工作了。
        /// </summary>
        override public void Invoke()
        {
            IsBusy = true;
            State = State.Start;

            //初始化异步动作状态表，只处理异步动作，非异步动作不管
            foreach(IAsyncAction action in (from action in this where action is IAsyncAction select action))
            {
                asyncActions[action] = null;
            }

            //直接调用所有动作，让其开始执行，然后再等待所有异步动作完成
            for (int i = 0; i < Count; i++)
            {
                IAction action = this[i];
                action.Invoke();
            }
        }

        /// <summary>
        /// 某个子异步动作完成后的，检查异步动作状态表，还有未完成的，继续等待。
        /// 如果所有子动作都完成了，看有没有执行失败的动作，只有所有动作都执行成功，整个动作组才算执行成功。
        /// </summary>
        /// <param name="action">完成工作的子异步动作</param>
        /// <param name="args">子异步动作完成工作时的参数</param>
        protected override void OnActionCompleted(IAsyncAction action, AsyncCompletedEventArgs args)
        {
            //注册异步动作状态
            this.asyncActions[action] = args;
            //如果还有动作没有完成，直接返回
            if ((from a in this.asyncActions.Values where a == null select a).Count() != 0)
            {
                return;
            }
            IsBusy = false;
            //如果有失败动作，把所有失败信息合并到一起，通知失败了
            var errors = from a in this.asyncActions.Values where a.Error != null select a;
            if (errors.Count() != 0)
            {
                State = State.Error;
                //把所有错误结果填入结果中
                foreach(IAsyncAction key in errors)
                {
                    //把子的出错信息复制过来，子不能是异步动作组
                    Errors.Add(new ErrorInfo(key.Name, key.Error));
                }
                OnCompleted(errors.First());
            }
            //否则，通知成功
            else
            {
                State = State.End;
                OnCompleted(new AsyncCompletedEventArgs(null, false, null));
            }
        }
    }
}
