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
using System.Windows.Threading;
using System.Json;
using System.Collections.Generic;
using Com.Aote.Utils;
using System.Windows.Browser;

namespace Com.Aote.ObjectTools
{
    public class Phone : CustomTypeHelper, IAsyncObject
    {
        #region 单例
        public static Phone instance;
        public static Phone GetInstance()
        {
            if (instance == null)
            {
                instance = new Phone();
            }
            return instance;
        }
        #endregion


        #region EntityType 对象类型，对应后台Hibernate类
        private string entityType;

        /// <summary>
        /// 对象类型，对应后台Hibernate类，设置时，从类型信息表中获取类型，设置本对象类型为获取到的类型。
        /// </summary>
        public string EntityType
        {
            get { return this.entityType; }
            set
            {
                this.entityType = value;
                //设置类型
                SetCustomType(CustomTypes.GetInstance().GetType(value));
            }
        }
        #endregion

        public Phone()
        {
            
         }

         

        private bool init = false;
        public bool IsInit
        {
            get { return this.init; }
            set
            {
                this.init = value;
                if (this.init)
                {
                    Init();
                }
            }
        }


        //是否记录
        private bool isRecordDB = false;
        public bool IsRecordDB
        {
            get { return this.isRecordDB; }
            set
            {
                this.isRecordDB = value;
            }
        }
        //保存的记录id，自己产生
        public string recordId { set; get; }
      //登陆时间
        public DateTime loginTime { set; get; }
       


        /**
         * 某个通用对象，电话对象属性变化时会反映到该对象
         **/
        public GeneralObject toGo { set; get; }


        /**
         * 电话初始化动作
         * */
        private void Init()
        {
            //装载电话配置文件
            //this.ConfigFile = new XmlFile("Phone.xml");
            //判断是否进行话务工作
            if (!isAnswer())
            {
                return;
            }
            //加载状态对照表
            //LoadPhoneStates();
            //后台语音服务器地址
            //this.TelServiceUrl = this.ConfigFile.getProperty("TelService", "uri");
            //LoginUser user = (LoginUser)Tools.Session["loginuser"];
            //this.LineNum = (string)user["linenum"];
            //this.GongHao = (string)user["gonghao"];
           
            //启动工作线程
            DispatcherTimer work = new DispatcherTimer();
            work.Interval = TimeSpan.FromSeconds(1);
            work.Tick += new EventHandler(work_Tick);
            work.Start();
            if (this.isRecordDB)
            {
                SystemTime t = new SystemTime();
                loginTime = t.Now;
                this.recordId = System.Guid.NewGuid().ToString();
                //启动保存置忙时间线程
                DispatcherTimer save = new DispatcherTimer();
                save.Interval = TimeSpan.FromSeconds(30);
                save.Tick += new EventHandler(Save_Tick);
                save.Start();
            }
        }


        /**
         * 退出
         */
        void Save_Tick(object sender, EventArgs e1)
        {
            //记录数据
            //传递到后台执行
            Uri uri = new Uri(WebClientInfo.BaseAddress);
            WebClient client = new WebClient();
            client.UploadStringCompleted += (o, e) =>
            {
                this.IsBusy = false;
                //通知数据提交过程完成
                if (e.Error != null)
                {
                    this.State = State.Error;
                    this.Error = e.Error.GetMessage();

                }
               
                this.OnCompleted(e);
            };
            JsonArray array = new JsonArray();
            JsonValue json = SaveToJson();
            if (json is JsonObject)
            {
                //把执行批处理对象的名字添加进去
                json["name"] = this.Name;
                array.Add(json);
            }
            else
            {
                array = (JsonArray)json;
            }
            this.IsBusy = true;
            this.State = State.Start;
            client.UploadStringAsync(uri, array.ToString());
        }


        #region WebClientInfo 用于去后台获取数据的基本地址描述，在xaml文件中进行配置
        public WebClientInfo WebClientInfo { get; set; }
        #endregion
 
 

        #region SaveToJson 把对象要保持的状态转换成Json指令，由批处理一起出来
        /// <summary>
        /// 保存对象，返回保存对象的Json格式的指令，不执行实际的后台保存工作。统一由BatchExcuteAction
        /// 把要执行的数据库操作数据发送给后台服务。
        /// </summary>
        /// <returns>json格式的保存对象的操作数据</returns>
        /// 
        
        public JsonObject SaveToJson()
        {
            JsonObject result = new JsonObject();
             result["operator"] = "save";
            result["entity"] = "t_opbusyrecord";
            result["data"] = ToJson();
            return result;
        }

        #endregion


        #region ToJson 把数据转换成Json串
        public JsonObject ToJson()
        {
            SystemTime t = new SystemTime();
            //对象转化成json
            JsonObject json = new JsonObject();
            //放置实体类型
            json["EntityType"] = "t_opbusyrecord";
            Dictionary<string, object> attrs = new  Dictionary<string, object>();
            attrs["logintime"] = loginTime.ToString("yyyyMMdd HH:mm:ss");
            attrs["exittime"] = t.Now.ToString("yyyyMMdd HH:mm:ss");
            attrs["busytime"] = busySec;
            attrs["gonghao"] = GongHao;
            attrs["id"] = this.recordId;
            attrs["recordtime"] = t.Now.ToString("yyyyMMdd HH:mm:ss");
            foreach (KeyValuePair<string, object> kvp in attrs)
            {
                
                //整数类型，可以为空
                 if (kvp.Value is int)
                    json[kvp.Key] = (int)kvp.Value;
                //double类型，可以为空
                else if (kvp.Value is double)
                    json[kvp.Key] = (double)kvp.Value;
                //decimal
                else if (kvp.Value is decimal)
                    json[kvp.Key] = (decimal)kvp.Value;
                //bool型
                else if (kvp.Value is bool)
                    json[kvp.Key] = (bool)kvp.Value;
                //字符串
                else if (kvp.Value is string)
                    json[kvp.Key] = kvp.Value as string;
                //日期
                else if (kvp.Value is DateTime)
                {
                    //DateTime from1970 = new DateTime(1970, 1, 1,0,0,0);
                    DateTime from1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                    DateTime valDate = (DateTime)kvp.Value;
                    TimeSpan ts = valDate.ToUniversalTime() - from1970;
                    json[kvp.Key] = (Int64)(ts.TotalMilliseconds + 0.5);
                }
                //列表数据
                else if (kvp.Value is BaseObjectList)
                    json[kvp.Key] = (kvp.Value as BaseObjectList).ToJson();
                else
                    throw new Exception("不认识的字段类型, " + kvp.Value.GetType());
            }
            return json;
        }
        #endregion

        /**
         * 判断登陆人是否处理电话业务
         **/
        public string LoginRoles { get; set; }
        public string RoleNames { get; set; }
        private bool isAnswer()
        {
            //LoginUser user = (LoginUser)Tools.Session["loginuser"];
            //string loginRoles = (string)user["rolenames"];
            //string roleNames = this.ConfigFile.getProperty("Role", "names");
            char[] ch = { ',' };
            string[] strs = RoleNames.Split(ch);
            foreach (string s in strs)
            {
                if (LoginRoles.IndexOf(s) != -1)
                {
                    return true;
                }
            }
            return false;

        }


        /**
         * 主工作方法定时得到通道信息
         **/
        void work_Tick(object sender, EventArgs e)
        {
            string address = this.TelServiceUrl + "/GetLineInfo?lineNum=" + this.LineNum + "&gonghao=" + this.GongHao;
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    //更新数据
                    JsonObject items = JsonValue.Parse(a.Result) as JsonObject;
                    items["PhoneState"] = items["State"];
                    items.Remove("State");
                    GeneralObject go = new GeneralObject();
                    go.FromJson(items);
                    if (go == null)
                    {
                        this.PhoneState = GetPropertyValue("14") + "";
                        return;
                    }
                    //设置电话信息
                    string stateIndex = go.GetPropertyValue("PhoneState") + "";
                    //如果状态是接听,并且电话号码和录音号与语音服务部相同，设置
                    this.PhoneState = GetPropertyValue(stateIndex) + "";
                    //如果电话号码相同，不通知
                    string oldNumber = this.CallNumber;
                    if (oldNumber == null)
                    {
                        oldNumber = "";
                    }
                    string newNumber = go.GetPropertyValue("CallerPhone") + "";
                    if (newNumber != null && !newNumber.Equals("") && !oldNumber.Equals(newNumber))
                    {
                        this.CallNumber = newNumber;
                    }
                    //如果记录号不相同，设置
                    string oldRecordFile = this.RecordFile;
                    if (oldRecordFile == null)
                    {
                        oldRecordFile = "";
                    }
                    string newRecordFile = go.GetPropertyValue("RecordFile") + "";
                    if (newRecordFile != null && !newRecordFile.Equals("") && !oldRecordFile.Equals(newRecordFile))
                    {
                        this.RecordFile = newRecordFile;
                    }
                    State = State.Loaded;
                }
                else
                {
                    this.PhoneState = GetPropertyValue("14") + "";
                    State = State.LoadError;
                    Error = a.Error.Message;
                }
            };
            //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadStringAsync(uri, "");
        }

        //void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        JsonArray items = JsonValue.Load(e.Result) as JsonArray;
        //        FromJson(items);
        //        GeneralObject go = (GeneralObject)JsonHelper.JsonToObject(e.Result);
        //        if (go == null)
        //        {
        //            this.PhoneState = this.states["14"];
        //            return;
        //        }
        //        //设置电话信息
        //        string stateIndex = go.attrs["State"] + "";
        //        //如果状态是接听,并且电话号码和录音号与语音服务部相同，设置
        //        this.PhoneState = this.states[stateIndex];
        //        //如果电话号码相同，不通知
        //        string oldNumber = this.CallNumber;
        //        if (oldNumber == null)
        //        {
        //            oldNumber = "";
        //        }
        //        string newNumber = (string)go.attrs["CallerPhone"];
        //        if (newNumber != null && !newNumber.Equals("") && !oldNumber.Equals(newNumber))
        //        {
        //            this.CallNumber = newNumber;
        //        }
        //        //如果记录号不相同，设置
        //        string oldRecordFile = this.RecordFile;
        //        if (oldRecordFile == null)
        //        {
        //            oldRecordFile = "";
        //        }
        //        string newRecordFile = (string)go.attrs["RecordFile"];
        //        if (newRecordFile != null && !newRecordFile.Equals("") && !oldRecordFile.Equals(newRecordFile))
        //        {
        //            this.RecordFile = newRecordFile;
        //        }
        //    }
        //    else
        //    {
        //        this.PhoneState = this.states["14"];
        //    }
        //}



        //电话配置文件
        //public XmlFile ConfigFile { set; get; }

        //语音服务器地址
        public string TelServiceUrl { set; get; }

        //使用的语音通道号
        public string LineNum { set; get; }

        //工号
        public string GongHao { set; get; }




        /**
         * 从配置文件装载状态对照表
         **/
        public void LoadPhoneStates()
        {
            //List<XElement> elems = this.ConfigFile.getElementes("States", "state");
            //foreach (XElement elem in elems)
            //{
            //    string key = elem.Attribute("key").Value;
            //    string state = elem.Attribute("show").Value;
            //    states[key] = state;
            //}
        }


        //状态
        private string phoneState;
        public string PhoneState
        {
            set
            {
                //统计置忙时间
                string oldState = this.phoneState;
                this.phoneState = value;
                //this.toGo["PhoneState"] = this.phoneState;
                OnPropertyChanged("PhoneState");
                CountBusyTime(oldState, phoneState);
            }
            get
            {
                return this.phoneState;
            }
        }
       

        //置忙计时
        public DateTime busyStart { set; get; }
        public DateTime busyEnd { set; get; }
        public double busySec { set; get; }

        //统计置忙时间
        public void CountBusyTime(string oldState, string newState)
        {
            if (oldState == null || newState == null)
            {
                return;
            }
          
            //如果新状态是忙碌
            if ( !oldState.Equals("忙碌") && newState.Equals("忙碌"))
            {
                busyStart = System.DateTime.Now;
                return;
            }
            else if (oldState.Equals("忙碌") && !newState.Equals("忙碌"))
            {
               return;
            }
           // 如果现在是忙碌状态
            else if (oldState.Equals("忙碌"))
            {
                busyEnd = System.DateTime.Now;
                TimeSpan ts = busyEnd.Subtract(busyStart).Duration();
                busySec += ts.TotalSeconds;
                busyStart = System.DateTime.Now;
            }
           
           
        }

        //主叫号码
        private string callNumber;
        public string CallNumber
        {
            set
            {
                this.callNumber = value;
                //this.toGo["CallNumber"] = this.callNumber;
                OnPropertyChanged("CallNumber");
            }
            get
            {
                return this.callNumber;
            }
        }

        //录音文件
        private string recordFile;
        public string RecordFile
        {
            set
            {
                this.recordFile = value;
                //this.toGo["RecordFile"] = this.recordFile;
                OnPropertyChanged("RecordFile");
            }
            get
            {
                return this.recordFile;
            }
        }


        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string name)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(name));
        //    }
        //}


        #region 附加属性:Field,取Field指定的电话对象属性
        public static DependencyProperty PhoneAttresProperty = DependencyProperty.RegisterAttached(
           "PhoneAttres", typeof(string), typeof(Phone), new PropertyMetadata(new PropertyChangedCallback(OnFieldChanged)));
        public static string GetPhoneAttres(FrameworkElement ui)
        {
            return (string)ui.GetValue(PhoneAttresProperty);
        }
        public static void SetPhoneAttres(FrameworkElement ui, object value)
        {
            ui.SetValue(PhoneAttresProperty, value);
        }
        public static void OnFieldChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement fe = (FrameworkElement)obj;
            Phone p = Phone.GetInstance();
            p.toGo = (GeneralObject)fe.DataContext;
        }


        #endregion


        /**
         * 接听
         **/
        public void Answer()
        {
            string address = this.TelServiceUrl + "/Receiver?lineNum=" + this.LineNum;
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    if (a.Result.IndexOf("sucessful") == -1)
                    {
                        MessageBox.Show("接听操作失败!");
                    }
                }
            };
            client.DownloadStringAsync(uri, "");
        }
        //外拨
        public void CallPhone()
        {
            string address = this.TelServiceUrl + "/CallPhone?lineNum=" + this.LineNum + "&phone="+GetPropertyValue("CallPhone");
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    if (a.Result.IndexOf("请先摘机") != -1)
                    {
                        MessageBox.Show("请先摘机，然后拨号!");
                    }
                }
            };
            client.DownloadStringAsync(uri, "");
        }

        /**
         * 挂断
         **/
        public void HandUp()
        {
            string address = this.TelServiceUrl + "/HandUp?lineNum=" + this.LineNum;
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null || a.Result == null || !a.Result.Equals("sucessful") || !a.Result.Equals("nocall"))
                {
                    MessageBox.Show("挂断操作失败!");
                }
            };
            //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(HandUp_DownloadStringCompleted);
            client.DownloadStringAsync(uri, "");
        }
        //置忙
        public void SetBusy()
        {
            string address = this.TelServiceUrl + "/SetBusy?lineNum=" + this.LineNum;
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error != null)
                {
                    MessageBox.Show("置忙操作失败!");
                }
            };
            client.DownloadStringAsync(uri, "");
        }

        private bool confirmHandUp = false;
        public bool Confirm
        {
            get { return this.confirmHandUp; }
            set
            {
                this.confirmHandUp = value;
                if (this.confirmHandUp)
                {
                    ConfirmHandUp();
                }
            }
        }

        /**
          * 提交挂断
          **/
        public void ConfirmHandUp()
        {

            string address = this.TelServiceUrl + "/ConfirmHandup?lineNum=" + this.LineNum;
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    this.PhoneState = GetPropertyValue("0") + "";
                    State = State.Loaded;
                    this.CallNumber = "未来电";
                }
                else
                {
                    this.PhoneState = GetPropertyValue("14")+"";
                    State = State.LoadError;
                    Error = a.Error.Message;
                }
            };
            client.DownloadStringAsync(uri, "");
        }

        //播放外音
        public void startwav()
        {
            string address = this.TelServiceUrl + "/startwav?lineNum=" + this.LineNum + "&filename=" + GetPropertyValue("filename");
            Uri uri = new Uri(address);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (o, a) =>
            {
                if (a.Error == null)
                {
                    if (a.Result.IndexOf("请先摘机") != -1)
                    {
                        MessageBox.Show("请先摘机，然后拨号!");
                    }
                }
            };
            client.DownloadStringAsync(uri, "");
        }

        public bool IsBusy
        {
            get;
            set;
        }

        public State State
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        public event System.ComponentModel.AsyncCompletedEventHandler Completed;

        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {

        }

        public string Name
        {
            get;
            set;
        }
    }
}
