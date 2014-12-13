using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Json;
using System.Reflection;
using System.Windows.Data;
using System.Collections;
using System.Net;
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 不带翻页的对象列表，一次性把所有对象全部加载进来，一般情况下，尽可能使用这个对象，特别是在进行编辑的情况下
    /// 一定不要使用可以翻页的对象。可以翻页的列表对象主要用于大数据量查询的情况。
    /// </summary>
    public class ObjectList : BaseObjectList
    {
        #region Count 总共数据个数
        override public int Count
        {
            get { return objects.Count; }
            set { throw new NotImplementedException(); }
        }
        #endregion

        #region Load 加载数据
        /// <summary>
        /// 加载数据的过程，一次性把所有数据加载过来，由于%在路径中不能使用，所以把%转换为不常用的'~'符号。
        /// 后台再将'~'转换为'%'。
        /// </summary>
        override public void Load()
        {
            if (Path == null)
            {
                return;
            }
            string uuid = System.Guid.NewGuid().ToString();
            Uri uri = new Uri(WebClientInfo.BaseAddress + "/" + Path.Replace("%", "%25").Replace("#", "%23").Replace("^", "<") + "?uuid=" + uuid);
            WebClient client = new WebClient();
            client.OpenReadCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    //更新数据
                    JsonArray items = JsonValue.Load(a.Result) as JsonArray;
                    FromJson(items);
                    State = State.Loaded;
                }
                else
                {
                    State = State.LoadError;
                    Error = a.Error.GetMessage();
                }
                IsBusy = false;
                //通知数据加载完成
                OnDataLoaded(a);
                OnCompleted(a);
                //通知总数变化
                OnPropertyChanged("Count");
            };
            //通知开始加载了
            IsBusy = true;
            OnLoading();
            State = State.StartLoad;
            client.OpenReadAsync(uri);
        }
        #endregion

    }
}
