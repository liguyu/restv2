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
using System.Runtime.InteropServices.Automation;
using System.Threading;

namespace Com.Aote.ObjectTools
{
    //POS机对象
    public class PosObj : IAsyncObject
    {
        dynamic pos = AutomationFactory.CreateObject("PDAPOSUPDOWN.PdaposupdownCtrl.1");

        public PosObj()
        {
            //PcPort = 0;
            //posUpload();
        }

        //串口
        public int PcPort { get; set; }
        //pos 上传
        public void posUpload()
        {
            ObjectList datalist = new ObjectList();
            GetEvent("pcPort").pcPort = PcPort;
            if (GetEvent("OpenComm").OpenComm() != 0)
            {
                MessageBox.Show("打开串口" + PcPort + "失败!");
                return;
            }
            GetEvent("ack").ack = "C";
            GetEvent("RespAckPos").RespAckPos();
            var ret = 0;
            while ((ret = GetEvent("Readdata").Readdata()) == 0)
            {
                var data = GetEvent("strdata").strdata;
                //如果数据内容是ok,传完了，发送ok过去后pos机控件开始清除数据
                if (data == "OK")
                {
                    datalist.Completed += (o, a) =>
                    {
                        if (a.Error == null)
                        {
                            GetEvent("ack").ack = "C";
                            GetEvent("RespAckPos").RespAckPos();
                        }
                    };
                    datalist.Save();
                    break;
                }
                else
                {
                    MessageBox.Show(data);
                    //解析,保存数据
                    GeneralObject go = parsePos(data);
                    datalist.Add(go);
                    //校验数据 ,正确 ，数据存储，否则，发送N
                    GetEvent("ack").ack = "C";
                    GetEvent("RespAckPos").RespAckPos();
                }
            }
            if (ret != 0)
            {
                MessageBox.Show("错误类型" + ret);
            }
            //关串口
            GetEvent("CloseComm").CloseComm();
        }

        //解析pos机数据
        private GeneralObject parsePos(string info)
        {
            //登陆用户
            //CommonObject user = (CommonObject)Application.Current.Resources["loginuser"];
            GeneralObject co = new GeneralObject();
            char[] ch = { '|' };
            string[] strs = info.Split(ch);
            //卡号
            co.SetValue("cardId", strs[0]); ;
            //操作员号
            co.SetValue("posOperate", strs[1]);
            //维修时间
            co.SetValue("repairsDate", parseDate(strs));
            //维修原因
            co.SetValue("repsirsReason", reason(strs));
            //维修结果
            //气量
            co.SetValue("gas", strs[6]);
            //  设备号
            co.SetValue("posCode", strs[7]);
            //表型
            co.SetValue("tabletype", tabletype(strs[8]));
            //操作员
            //co["opCode"] = user["userId"];
            ////操作地点
            ////CommonObject attributes = (CommonObject)user["attributes"];
            //co["opSpot"] = user["localport"];
            ////操作时间
            //DateTime dt = CalendarHelper.getServerDate();
            //co["opDate"] = dt.ToString("yyyy-MM-dd hh:mm:ss");
            return co;
        }

        private string parseDate(string[] strs)
        {
            string date = strs[2];
            string y = date.Substring(0, 4);
            string m = date.Substring(4, 2);
            string d = date.Substring(6, 2);
            string time = strs[3];
            string h = time.Substring(0, 2);
            string t = time.Substring(2, 2);
            string s = time.Substring(4, 2);
            return y + "-" + m + "-" + d + " " + h + ":" + t + ":" + s;
        }

        //表型
        private string tabletype(string type)
        {
            type = type.Replace(";", "");
            int i = Int32.Parse(type);
            string result = "";
            switch (i)
            {
                case (1):
                    result = "华捷加密(10)";
                    break;
                case (2):
                    result = "华捷普通(10)";
                    break;
                case (3):
                    result = "秦港";
                    break;
                case (4):
                    result = "秦川";
                    break;
                case (5):
                    result = "天庆";
                    break;
                case (6):
                    result = "致力老表";
                    break;
                case (7):
                    result = "致力新表";
                    break;
                case (8):
                    result = "赛福";
                    break;
                case (9):
                    result = "秦港工业";
                    break;
                case (11):
                    result = "华捷加密(12)";
                    break;
                case (12):
                    result = "华捷普通(12)";
                    break;
                case (13):
                    result = "天然气无线";
                    break;
                case (14):
                    result = "工业无线";
                    break;
                case (15):
                    result = "秦港民用无线";
                    break;
                case (16):
                    result = "秦港工业无线";
                    break;
                case (17):
                    result = "手抄表";
                    break;
                default:
                    break;
            }
            return result;
        }

        private string reason(string[] strs)
        {
            string Reason = "";
            int code = Int32.Parse(strs[4] + "");
            code = code - 23;
            switch (code)
            {
                case (0):
                    Reason = "卡坏气量已进表";
                    break;
                case (1):
                    Reason = "卡坏气量未进表";
                    break;
                case (2):
                    Reason = "卡号被改";
                    break;
                case (3):
                    Reason = "卡内容错误";
                    break;
                case (4):
                    Reason = "表内容错误";
                    break;
                case (5):
                    Reason = "表坏";
                    break;
                case (6):
                    Reason = "表参数错误";
                    break;
                case (7):
                    Reason = "表内气量丢失";
                    break;
                case (8):
                    Reason = "卡上无气量";
                    break;
                case (9):
                    Reason = "其他气量错误";
                    break;
                case (10):
                    Reason = "其他原因";
                    break;
                default:
                    break;
            }
            return Reason;
        }

        public dynamic GetEvent(string EventName)
        {
            AutomationFactory.GetEvent(pos, EventName);
            return pos;
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
