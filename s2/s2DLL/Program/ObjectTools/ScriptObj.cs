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
using System.Windows.Browser;
using System.Json;

namespace Com.Aote.ObjectTools
{
    //调用脚本对象
    public class ScriptObj : GeneralObject
    {
        //脚本方法名
        public string Method { get; set; }

        public bool isInvoke = false;
        public bool IsInvoke
        {
            get { return isInvoke; }
            set
            {
                isInvoke = value;
                if (isInvoke)
                {
                    Invoke();
                }
                
            }
        }



        //执行方法
        public void Invoke()
        {
            State = State.StartLoad;
            IsBusy = true;
            string str = Method;
            foreach (var item in this._customPropertyValues.Keys)
            {
                str = str.Replace("#" + item + "#", this._customPropertyValues[item] + "");
            }
             object o = HtmlPage.Window.Eval(str);
            if (o is string)
            {
                JsonObject item = JsonValue.Parse(o.ToString()) as JsonObject;
                FromJson(item);
                string tishi = this.GetPropertyValue("tishi")+"";
                string isValid = this.GetPropertyValue("isValid")+"";
                string result = this.GetPropertyValue("result")+"";
                if (isValid != null && isValid.Equals("False") || result != null && result.Equals("False"))
                {
                    Error = tishi;
                    State = State.Error;
                }
                else
                {
                    State = State.End;
                }
            }
            IsBusy = false;
            State = State.Loaded;
        }
    }
}
