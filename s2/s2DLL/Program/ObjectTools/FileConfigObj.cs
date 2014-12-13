using Com.Aote.Logs;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

/**
 *  文件配置对象
 * */
namespace Com.Aote.ObjectTools
{
    public class FileConfigObj : CustomTypeHelper, IAsyncObject
    {

        private static Log Log = Log.GetInstance("Com.Aote.ObjectTools.FileConfigObj");


        #region FilePath 文件路径
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    OnPropertyChanged("FilePath");
                }
            }
        }
        #endregion

        

        #region IsLoad 是否加载配置
        private bool isLoad;
        public bool IsLoad
        {
            get { return isLoad; }
            set
            {
                if (isLoad != value)
                {
                    isLoad = value;
                    if (isLoad && this.filePath !=null)
                    {
                        Load();
                        isLoad = false;
                    }
                }
            }
        }
        #endregion


        //装载配置文件
        public void Load()
        {
            try
            {
               //开始加载文件中配置,加载前清除已有属性
                this.State = State.StartLoad;
                this.IsBusy = true;
                StreamReader sr = new StreamReader(this.filePath);
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] str = s.Split('=');
                    string name = str[0];
                    string val = str[1];
                    this._customPropertyValues.Add(name, val);
                }
                this.IsBusy = false;
                this.State = State.End;
             }
            catch (Exception e)
            {
                Log.Debug("加载配置文件" + this.filePath + "异常!" + e.Message.ToString());
            }
        }

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        /// <summary>
        /// 异步对象工作状态。
        /// </summary>
        private State state;
        public State State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public string Error
        {
            get;
            set;
        }

        /// <summary>
        /// 工作完成事件，包括数据加载
        /// </summary>
        public event AsyncCompletedEventHandler Completed;
        public void OnCompleted(AsyncCompletedEventArgs args)
        {
            if (Completed != null)
            {
                Completed(this, args);
            }
        }

        public string Name { get; set; }

        
    }
}
