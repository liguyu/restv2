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
using Com.Aote.Behaviors;
using System.Collections.ObjectModel;
using Com.Aote.Utils;
using System.Collections.Generic;

namespace Com.Aote.ObjectTools
{
    //对象工作状态，加载数据与提交数据用同一个状态
    public enum State
    {
        Free,           //对象最开始所处状态
        StartLoad,      //对象开始加载数据
        Loaded,         //成功加载数据
        LoadError,      //加载数据出错
        Start,          //开始提交
        End,            //提交完成
        Error,          //提交错误
        Cancle,         //工作过程被终止
    }

    /// <summary>
    /// 代表一条错误信息，包括出错的异步对象名称以及错误信息。
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// 出错的异步对象名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 出错的异步对象的错误信息。
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 构造一条错误信息。
        /// </summary>
        /// <param name="name">出错的异步对象名称</param>
        /// <param name="error">错误信息</param>
        public ErrorInfo(string name, string error)
        {
            Name = name;
            Error = error;
        }
    }

    /// <summary>
    /// 代表一个异步操作对象，通常用于一些比较耗时的操作，比如卡操作，访问后台的操作等。
    /// 异步操作对象在开始操作前，要设置IsBusy属性为True，结束操作后，设置IsBusy属性为False。
    /// 异步对象操作结束后，要触发Completed事件，并将执行结果状态存放到Resutl中。
    /// Result中存放的有执行结果状态，以及一批错误信息。
    /// 之所以有一批错误信息，是因为象并发动作组这样的异步对象将拿到所有动作的错误信息列表。
    /// </summary>
    public interface IAsyncObject : IName
    {
        /// <summary>
        /// 异步对象是否正在工作的标记。
        /// 异步对象开始工作前，必须设置该标记为True，工作结束后，必须设置该标记为False。
        /// 一般情况下，子类实现该属性时，必须在Set方法中通知属性发生变化了，或者将该属性实现成依赖属性。
        /// </summary>
        bool IsBusy { get; set; }

        //工作状态，所处工作状态
        State State { get; set; }

        //错误信息
        string Error { get; set; }

        /// <summary>
        /// 工作完成时的通知事件，异步对象完成工作后，将触发该事件。
        /// 异步对象要从该事件的事件参数中获取执行结果，并将结果转存到新建的AsyncResult中。
        /// </summary>
        event AsyncCompletedEventHandler Completed;

        //异步完成事件的通知方法
        void OnCompleted(AsyncCompletedEventArgs e);
    }
}
