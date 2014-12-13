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
    //秦川
    public class QCdllVcDll
    {
        /// <summary>
        /// 打开串口并核对密码
        /// </summary>
        /// <returns>0--密码正确 1—串口错误 2—没有插卡 3—卡坏 4—密码错误 5-卡型错误</returns>
        [DllImport("QCdll.dll", EntryPoint = "openport")]
        public static extern int openport();

        /// <summary>
        /// 制作超级卡
        /// </summary>
        /// <param name="val"></param>
        /// <returns> -1—错误     0—正确</returns>
        [DllImport("QCdll.dll", EntryPoint = "suppercard")]
        public static extern int suppercard(int val);


        /// <summary>
        /// 制作初始化卡（清零卡）
        /// </summary>
        /// <returns>-1—错误     0—正确</returns>
        [DllImport("QCdll.dll", EntryPoint = "clearcard")]
        public static extern int clearcard();


        /// <summary>
        /// 返回用户卡内的气量
        /// </summary>
        /// <returns>-1—错误</returns>
        [DllImport("QCdll.dll", EntryPoint = "getgas")]
        public static extern long getgas();


        /// <summary>
        /// 返回卡内的用户卡号
        /// </summary>
        /// <returns>-1—读卡错误  0—没有卡号</returns>
        [DllImport("QCdll.dll", EntryPoint = "getcardno")]
        public static extern long getcardno();


        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        [DllImport("QCdll.dll", EntryPoint = "closeport")]
        public static extern void closeport();
    }
}
