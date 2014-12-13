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

namespace Com.Aote.ObjectTools
{
    //对象可以从Json串赋值
    public interface IFromJson
    {
        //从Json串赋值
        void FromJson(JsonObject json);
    }
}
