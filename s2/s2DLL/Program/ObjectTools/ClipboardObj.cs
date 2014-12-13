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

namespace Com.Aote.ObjectTools
{
    //剪切板对象
    public class ClipboardObj : IName
    {
        public string Text
        {
            set{
                Clipboard.SetText(value);
            }
            get{
                return Clipboard.GetText();
            }
        }

        public string Name { get; set; }
    }
}
