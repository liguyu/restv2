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
    /// 串行执行的异步动作组，
    /// 在串行执行过程中，有一个异步动作执行失败，整个执行序列是否继续进行可以进行配置，默认为false，不继续
    /// 目前还没有实现已经执行的动作序列回滚，将来要实现已经执行的动作序列回滚。
    /// </summary>
    public class SyncActionFactory : ActionFactory
    {
        /// <summary>
        /// 发生错误时，是否继续，默认为false，不继续，也可以配置成继续。
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// 代表当前已经执行到那个动作了。
        /// 当串行动作组收到某个动作执行完后的事件时，根据这个变量指定的内容确定继续从那个动作开始执行。
        /// </summary>
        private int index = 0;

        /// <summary>
        /// 动作组的总体执行过程在这个方法中实现。
        /// 这个方法设置完IsBusy为True后，调用内部执行过程完成实际动作序列的执行。
        /// </summary>
        public override void Invoke()
        {
            index = 0;
            IsBusy = true;
            State = State.Start;
            Excute();
        }

        #region CanSave 是否保存对象属性到数据库
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.Register("CanSave", typeof(bool), typeof(SyncActionFactory),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSaveChanged)));

        private static void OnCanSaveChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SyncActionFactory bea = (SyncActionFactory)dp;
            if (bea.CanSave)
            {
                bea.Invoke();
            }
            bea.CanSave = false;
        }

        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set
            {
                SetValue(CanSaveProperty, value);
            }
        }
        #endregion


        /// <summary>
        /// 子动作完成后，首先检查动作完成状态，如果有错误，终止执行，通知外界发生了什么错误。
        /// 如果没有错误，则继续执行下面的动作。
        /// </summary>
        /// <param name="action">完成工作的动作</param>
        /// <param name="args">子动作完成时的参数</param>
        protected override void OnActionCompleted(IAsyncAction action, AsyncCompletedEventArgs args)
        {
            //如果失败，且不能继续执行，直接通知失败，否则，继续下一个动作
            if (args.Error != null && !ContinueOnError)
            {
                //通知前，还原index
                index = 0;
                IsBusy = false;
                State = State.Error;
                Error = args.Error.GetMessage();
                OnCompleted(args);
            }
            else
            {
                Excute();
            }
        }

        /// <summary>
        /// 实际执行过程，某个子动作成功执行后，将调用该方法继续执行后面的动作
        /// </summary>
        private void Excute()
        {
            while (index < Count)
            {
                BaseAsyncAction action = (BaseAsyncAction)this[index++];
                if (action.CanSave)
                {
                    action.Invoke();
                    //如果是异步动作，退出，等待异步动作完成
                    if (action is IAsyncAction)
                    {
                        return;
                    }
                }
            }
            //所有动作执行完毕，index还原，并通知完成
            index = 0;
            IsBusy = false;
            State = State.End;
            OnCompleted(new AsyncCompletedEventArgs(null, false, null));
        }
    }
}
