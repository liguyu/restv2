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
using Com.Aote.Logs;
using Com.Aote.ObjectTools;
using System.Collections;
using System.Reflection;
using System.Windows.Data;
using Com.Aote.Utils;
using System.Linq;


namespace Com.Aote.Attachs
{

    /// <summary>
    /// 给数据对象附加
    /// </summary>
    public class Data
    {
        private static Log Log = Log.GetInstance("Com.Aote.Attachs.Data");


        #region Items 给元素附加列表数据源
        public static DependencyProperty ItemsProperty = DependencyProperty.RegisterAttached(
           "Items", typeof(BaseObjectList), typeof(FrameworkElement), new PropertyMetadata(new PropertyChangedCallback(OnItemsChanged)));
        public static BaseObjectList GetItems(FrameworkElement ui)
        {
            return (BaseObjectList)ui.GetValue(ItemsProperty);
        }
        public static void SetItems(FrameworkElement ui, BaseObjectList value)
        {
            ui.SetValue(ItemsProperty, value);
        }

        //界面元素laod时，让对象进行初始化
        private static void OnItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = (FrameworkElement)obj;
            obj.GetType().GetProperty("ItemsSource").SetValue(obj, (IEnumerable)args.NewValue, null);
            ui.Loaded += new RoutedEventHandler(Items_Loaded);
        }

        static void Items_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement ui = (FrameworkElement)sender;
            ui.Loaded -= Items_Loaded;
            BaseObjectList bol = (BaseObjectList)GetItems(ui);
            bol.Init(ui);
        }

        #endregion

        #region Context 给元素附加通用对象
        public static DependencyProperty ContextProperty = DependencyProperty.RegisterAttached(
           "Context", typeof(CustomTypeHelper), typeof(FrameworkElement), new PropertyMetadata(new PropertyChangedCallback(OnContextChanged)));
        public static CustomTypeHelper GetContext(FrameworkElement ui)
        {
            return (CustomTypeHelper)ui.GetValue(ContextProperty);
        }
        public static void SetContext(FrameworkElement ui, CustomTypeHelper value)
        {
            ui.SetValue(ContextProperty, value);
        }

        //界面元素laod时，让对象进行初始化
        private static void OnContextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = (FrameworkElement)obj;
            ui.DataContext = args.NewValue;
            ui.Loaded += new RoutedEventHandler(Context_Loaded);
        }

        static void Context_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement ui = (FrameworkElement)sender;
            ui.Loaded -= Context_Loaded;
            CustomTypeHelper go = (CustomTypeHelper)GetContext(ui);
            go.Init(ui);
        }

        #endregion

        #region Prop 给元素附加属性配置
        public static DependencyProperty PropProperty = DependencyProperty.RegisterAttached(
           "Prop", typeof(PropertySetter), typeof(FrameworkElement), new PropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
        public static PropertySetter GetProp(FrameworkElement ui)
        {
            return (PropertySetter)ui.GetValue(PropProperty);
        }
        public static void SetProp(FrameworkElement ui, PropertySetter value)
        {
            ui.SetValue(PropProperty, value);
        }

        //先给元素的GeneralObject放入该属性，待元素加载完成后，调用属性初始化
        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = (FrameworkElement)obj;
            //DataContext存在，直接放入DataContext中
            UIDataContextChanged(ui);
            //否则，当DataContext有值时，再放入
            ui.DataContextChanged += (o, e) =>
            {
                UIDataContextChanged(ui);
            };
            //加载完成后，进行属性初始化
            ui.Loaded += new RoutedEventHandler(ui_Loaded);
        }

        private static void ui_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement ui = (FrameworkElement)sender;
            ui.Loaded -= ui_Loaded;
            PropertySetter ps = (PropertySetter)GetProp(ui);
            if (!ps.IsInited)
            {
                ps.Init(ui);
            }
        }

        //把属性设置放入对象中
        private static void UIDataContextChanged(FrameworkElement ui)
        {
            PropertySetter ps = (PropertySetter)GetProp(ui);
            if (ui.DataContext != null)
            {
                //把属性设置放入对象中
                CustomTypeHelper go = (CustomTypeHelper)ui.DataContext;
                go.Add(ps);
            }
            //设置PropertySetter自己的DataContext为UI的DataContext
            ps.DataContext = ui.DataContext;            
        }

        #endregion

        #region EmptyRowType 用于DataGrid等有ItemsSource的对象，声明界面对象对应的数据源的空行实体类型，以便进行表格式编辑。
        public static readonly DependencyProperty EmptyRowTypeProperty = DependencyProperty.RegisterAttached(
            "EmptyRowType", typeof(string), typeof(ItemsControl), new PropertyMetadata(OnEmptyRowTypeChanged));
        private static void OnEmptyRowTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl c = (ItemsControl)d;
            //不要空行，直接返回
            if (e.NewValue == null)
            {
                return;
            }
            BaseObjectList list = (BaseObjectList)c.ItemsSource;
            //如果还没有设置ItemsSource，监听ItemsSource改变事件，在ItemsSource有值时进行设置
            if (list == null)
            {
                c.Watch("ItemsSource", OnItemSourceChanged);
                return;
            }
            //设置过有空行，不重复设置
            if (list.HasEmptyRow)
            {
                return;
            }
            list.EntityType = (string)e.NewValue;
            list.HasEmptyRow = true;
        }

        //监听ItemsSource发生的变换
        private static void OnItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl c = (ItemsControl)d;
            if (c.ItemsSource == null)
            {
                return;
            }
            BaseObjectList list = (BaseObjectList)c.ItemsSource;
            //设置过有空行，不重复设置
            if (list.HasEmptyRow)
            {
                return;
            }
            list.EntityType = GetEmptyRowType(c);
            list.HasEmptyRow = true;
        }

        public static string GetEmptyRowType(DependencyObject d)
        {
            return (string)d.GetValue(EmptyRowTypeProperty);
        }
        public static void SetEmptyRowType(DependencyObject d, string value)
        {
            d.SetValue(EmptyRowTypeProperty, value);
        }
        #endregion

    }
}
