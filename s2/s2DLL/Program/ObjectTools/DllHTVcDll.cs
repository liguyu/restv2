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
using System.Runtime.InteropServices;

namespace Com.Aote.ObjectTools
{
    public class DllHTVcDll
    {
        [DllImport("DllHT.dll", EntryPoint = "_ReadCard_IC@0")]
        public static extern string ReadCard_IC(short HTport, long HTbaud);


        [DllImport("DllHT.dll", EntryPoint = "ReadCard_Gas")]
        public static extern string ReadCard_Gas(short HTport, long HTbaud);

        [DllImport("DllHT.dll", EntryPoint = "0x60030008")]
        public static extern bool NewCard(short HTport, long HTbaud, short Meter, double Gas, string IC, double Alert);

    }
}
