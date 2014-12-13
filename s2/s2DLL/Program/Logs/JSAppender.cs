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

namespace Com.Aote.Logs
{
    public class JSAppender : IAppender
    {
        public void ShowMessage(string msg)
        {
            ScriptObject hello = HtmlPage.Window.GetProperty("showMessage") as ScriptObject;
            Object[] message = new Object[1];
            message[0] = msg;
            hello.InvokeSelf(message);
        }
    }
}
