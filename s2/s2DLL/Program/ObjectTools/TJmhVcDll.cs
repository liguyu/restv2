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
using System.Text;

namespace Com.Aote.ObjectTools
{
    //瑞森动态库
    public class TJmhVcDll
    {
        [DllImport("TJmh.dll", EntryPoint = "_OpenIC@12")]
        public static extern short OpenIC(short PortNo, short FacID, string CityCode);


        [DllImport("TJmh.dll", EntryPoint = "_InitCard@36")]
        public static extern bool InitCard(string CardNo, long GasV, long BuyTimes, short GasPrice, string CityCode, short FacID, short CardT, short UserInfo, short UserTypeID);

        [DllImport("TJmh.dll", EntryPoint = "_CloseIC@0")]
        public static extern bool CloseIC();

        [DllImport("TJmh.dll", EntryPoint = "_GetRemaingas@0")]
        public static extern long GetRemaingas();


        [DllImport("TJmh.dll", EntryPoint = "_FormatCard@0")]
        public static extern bool FormatCard();

    }
}
