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

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 可加载数据的对象，这类对象将通过Load方法加载数据。
    /// 加载数据前，触发Loading事件，加载完数据后，触发Loaded事件。
    /// 一般情况下，可加载数据对象同时也是异步对象，将加载数据的过程当做异步操作。
    /// 这种情况下，在加载数据前，要将IsBusy设置为True，加载数据后，将其设置成False，并触发异步操作完成事件。
    /// 在触发异步操作完成事件时，要记得将操作结果给Result属性。
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// 开始加载数据事件，所有子类在加载数据前，必须触发该事件。
        /// </summary>
        event EventHandler Loading;

        /// <summary>
        /// 数据加载结束事件，所有子类在加载数据结束后，必须触发该事件。
        /// </summary>
        event AsyncCompletedEventHandler DataLoaded;

        //加载数据的过程状态，与工作用同一个状态表示
        State State { get; set; }
 
        //加载数据的路径，一般情况下，路径变化后，自动加载
        string Path { get; set; }

        //加载数据的基础地址
        WebClientInfo WebClientInfo { get; set; }

        /// <summary>
        /// 开始加载数据
        /// </summary>
        void Load();
    }
}
