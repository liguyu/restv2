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
using System.Windows.Data;

namespace Com.Aote.Utils
{
    public static class BindingExtension
    {
        public static string NewToString(this Binding b)
        {
            return "{Source:" + b.Source + ", Path:" + b.Path.NewToString() + "}";
        }
    }
}
