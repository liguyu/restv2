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
using System.Linq;
using System.Collections;
using Com.Aote.ObjectTools;
using Com.Aote.Marks;
using System.Windows.Data;
using Com.Aote.Controls;
using Com.Aote.Reports;

namespace Com.Aote.Utils
{
    public static class FrameworkElementExtension
    {
        /// <summary>
        /// 扩充后的根据名称找对象的方法，如果名字=this, 返回自己；名字=data, 返回数据上下文。
        /// 其它，先在界面上找元素，没找到，在自己的资源里找，没找到，一直往上找父资源的，都没有找到，
        /// 在应用程序资源中找。
        /// </summary>
        /// <param name="ui"></param>
        public static object FindResource(this FrameworkElement ui, string name)
        {
            //如果是this，返回所附加到的对象
            if (name == "this")
            {
                return ui;
            }
            //是data，返回数据上下文
            if (name == "data")
            {
                return ui.DataContext;
            }
            //在界面上找元素
            object fui = ui.FindName(name);
            if (fui != null)
            {
                return fui;
            }
            //找资源
            object result = FindResourceInSelf(ui, name);
            if (result != null)
            {
                return result;
            }
            ui = ui.GetParent(); 
            while (ui != null)
            {
                //在界面上找元素
                fui = ui.FindName(name);
                if (fui != null)
                {
                    return fui;
                }
                result = FindResourceInSelf(ui, name);
                if (result != null)
                {
                    return result;
                }
                if (ui is CustomChildWindow)
                {
                    CustomChildWindow ccw = (CustomChildWindow)ui;
                    result = ccw.Parent.FindResource(name);
                    if (result != null)
                    {
                        return result;
                    }
                }
                ui = ui.GetParent();
            }
            //在界面上没找到，在应用程序中找
            if (Application.Current.Resources.Contains(name))
            {
                return Application.Current.Resources[name];
            }
            //在应用程序中没找到，在应用程序的主页面中找
            result = GetResourceFromFrame(name);
            if (result != null)
            {
                return result;
            }
            //if (ui is Table)
            //{
            //    return ((Table)ui).MainData;
            //}
            return null;
        }

        //在应用程序的主界面中找资源
        private static object GetResourceFromFrame(string name)
        {
            if (Application.Current.RootVisual == null || !(Application.Current.RootVisual is FrameworkElement))
            {
                return null;
            }
            FrameworkElement ui = (FrameworkElement)Application.Current.RootVisual;
            //框架里的资源字典，名称为resources
            ResourceLoad r = (ResourceLoad)ui.FindName("resources");
            if (r == null)
            {
                return null;
            }
            return (from p in r.Res where p.Name == name select p).FirstOrDefault();
        }

        private static FrameworkElement GetParent(this FrameworkElement ui)
        {
            DependencyObject o = ui.Parent;
            if (o != null && o is FrameworkElement)
            {
                return (FrameworkElement)o;
            }
            o = VisualTreeHelper.GetParent(ui);
            if (o != null && o is FrameworkElement)
            {
                return (FrameworkElement)o;
            }
            return null;
        }

        private static object FindResourceInSelf(FrameworkElement ui, string name)
        {
            if (ui is Panel)
            {
                //获取资源定义面板
                var r = (ResourceLoad)(from p in (ui as Panel).Children where p is ResourceLoad select p).FirstOrDefault();
                if (r != null)
                {
                    return (from p in r.Res where p.Name == name select p).FirstOrDefault(); 
                }
            }
            return null;
        }
    }
}
