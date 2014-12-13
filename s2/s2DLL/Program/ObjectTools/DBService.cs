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
using System.Json;
using System.Collections.Generic;
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    //提供数据库服务的类型
    public class DBService
    {
        //根据地址，缓存的数据库服务
        private static Dictionary<string, DBService> services = new Dictionary<string, DBService>();

        //根据基本地址，获取一个数据库服务对象
        public static DBService Get(string uri)
        {
            if (!services.ContainsKey(uri))
            {
                services[uri] = new DBService() { Uri = uri };
            }
            return services[uri];
        }

        //地址信息
        public string Uri { get; set; }

        //把函数转换出来的Json串交给后台数据库服务
        public void Invoke(IAsyncObject obj, Func<JsonValue> method)
        {
            //传递到后台执行
            Uri uri = new Uri(Uri);
            WebClient client = new WebClient();
            client.UploadStringCompleted += (o, e) =>
            {
                obj.IsBusy = false;
                //通知数据提交过程完成
                if (e.Error != null)
                {
                    obj.State = State.Error;
                    obj.Error = e.Error.GetMessage();
                }
                else
                {
                    //返回数据重新赋值给对象
                    JsonObject resultJson =  (JsonObject)JsonValue.Parse(e.Result);
                    if (resultJson.ContainsKey(obj.Name))
                    {
                        (obj as IFromJson).FromJson((JsonObject)resultJson[obj.Name]);
                    }
                    obj.State = State.End;
                }
                obj.OnCompleted(e);
            };
            JsonArray array = new JsonArray();
            JsonValue json = method();
            if (json is JsonObject)
            {
                //把执行批处理对象的名字添加进去
                json["name"] = obj.Name;
                array.Add(json);
            }
            else
            {
                array = (JsonArray) json;
            }
            obj.IsBusy = true;
            obj.State = State.Start;
            client.UploadStringAsync(uri, array.ToString());
        }
    }
}
