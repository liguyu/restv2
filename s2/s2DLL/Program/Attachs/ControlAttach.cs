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
using System.Reflection;
using System.Windows.Data;
using Com.Aote.Logs;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Com.Aote.ObjectTools;
using System.Collections;
using System.Linq;
using System.Windows.Navigation;
using System.Collections.Generic;

namespace Com.Aote.Attachs
{
    /// <summary>
    /// 控件附加一些特点，比如回车更新绑定等，主要给文本输入框进行附加
    /// </summary>
    public class ControlAttach
    {
        #region UpdateOnEnter 回车时更新绑定，值为要更新的属性名，一般情况下，为Text
        public static DependencyProperty UpdateOnEnterProperty = DependencyProperty.RegisterAttached(
           "UpdateOnEnter", typeof(string), typeof(TextBox), new PropertyMetadata(new PropertyChangedCallback(OnUpdateOnEnterChanged)));
        public static string GetUpdateOnEnter(FrameworkElement ui)
        {
            return (string)ui.GetValue(UpdateOnEnterProperty);
        }
        public static void SetUpdateOnEnter(FrameworkElement ui, string value)
        {
            ui.SetValue(UpdateOnEnterProperty, value);
        }

        /// <summary>
        /// 回车时更新绑定，值为要更新的属性名，一般情况下，为Text
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnUpdateOnEnterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = obj as FrameworkElement;
            //监听回车事件，回车时更新值
            ui.KeyDown += (o, e) =>
            {
                //如果不是回车键，不管
                if (e.Key == Key.Enter)
                {
                    //获得ui的属性binding
                    FieldInfo field = ui.GetType().GetField(args.NewValue + "Property");
                    DependencyProperty dp = (DependencyProperty)field.GetValue(null);
                    BindingExpression be = ui.GetBindingExpression(dp);
                    //让其更新
                    be.UpdateSource();
                }
            };
        }

        #endregion

        #region UpdateOnTextChanged 按下字符时更新绑定，值为要更新的属性名，一般情况下，为Text
        public static DependencyProperty UpdateOnTextChangedProperty = DependencyProperty.RegisterAttached(
           "UpdateOnTextChanged", typeof(string), typeof(TextBox), new PropertyMetadata(new PropertyChangedCallback(OnUpdateOnTextChanged)));
        public static string GetUpdateOnTextChanged(FrameworkElement ui)
        {
            return (string)ui.GetValue(UpdateOnTextChangedProperty);
        }
        public static void SetUpdateOnTextChanged(FrameworkElement ui, string value)
        {
            ui.SetValue(UpdateOnTextChangedProperty, value);
        }

        /// <summary>
        /// 回车时更新绑定，值为要更新的属性名，一般情况下，为Text
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnUpdateOnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TextBox ui = obj as TextBox;
            //监听字符按下事件，字符按下时更新值
            ui.TextChanged += (o, e) =>
            {
                //获得ui的属性binding
                FieldInfo field = ui.GetType().GetField(args.NewValue + "Property");
                DependencyProperty dp = (DependencyProperty)field.GetValue(null);
                BindingExpression be = ui.GetBindingExpression(dp);
                //让其更新
                be.UpdateSource();
            };
        }

        #endregion
        #region FocusToOnEnter 回车时光标转移，值为要转移到的元素名
        public static DependencyProperty FocusToOnEnterProperty = DependencyProperty.RegisterAttached(
           "FocusToOnEnter", typeof(string), typeof(Control), new PropertyMetadata(new PropertyChangedCallback(OnFocusToOnEnterChanged)));
        public static string GetFocusToOnEnter(FrameworkElement ui)
        {
            return (string)ui.GetValue(FocusToOnEnterProperty);
        }
        public static void SetFocusToOnEnter(FrameworkElement ui, string value)
        {
            ui.SetValue(FocusToOnEnterProperty, value);
        }

        /// <summary>
        /// 回车时光标转移，值为要转移到的元素名
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnFocusToOnEnterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = obj as FrameworkElement;
            //监听回车事件，回车时光标转移
            ui.KeyDown += (o, e) =>
            {
                //如果不是回车键，不管
                if (e.Key == Key.Enter)
                {
                    FrameworkElement c = (FrameworkElement)ui.FindName(args.NewValue + "");
                    //因为focus具体的继承层次不明确，所以用反射调用
                    c.GetType().GetMethod("Focus").Invoke(c, null);
                }
            };
        }

        #endregion    

        #region Focus 界面开始时，默认光标位置，值为默认光标所在元素名
        public static DependencyProperty FocusProperty = DependencyProperty.RegisterAttached(
           "Focus", typeof(string), typeof(Panel), new PropertyMetadata(new PropertyChangedCallback(OnFocusChanged)));
        public static string GetFocus(FrameworkElement ui)
        {
            return (string)ui.GetValue(FocusProperty);
        }
        public static void SetFocus(FrameworkElement ui, string value)
        {
            ui.SetValue(FocusProperty, value);
        }

        /// <summary>
        /// 界面开始时，默认光标位置，值为默认光标所在元素名
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnFocusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = obj as FrameworkElement;
            if (args.NewValue != null)
            {
                //如果获得焦点的控件不可用，在可用时获得焦点
                Control con = (Control)ui.FindName((string)args.NewValue);
                if (!con.IsEnabled)
                {
                    con.IsEnabledChanged += new DependencyPropertyChangedEventHandler(EnableHandle);
                }
                else
                {
                    con.Focus();
                }
            }
        }

        private static void EnableHandle(object o, DependencyPropertyChangedEventArgs e)
        {
            Control c = (Control)o;
            c.IsEnabledChanged -= EnableHandle;
            c.Focus();
        }

        #endregion    

        #region Source 当Source发生改变时，根据Source指定的Uri加载页面，作为控件内容
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", typeof(string), typeof(ContentControl), new PropertyMetadata(OnSourceChanged));
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContentControl c = (ContentControl)d;
            if (e.NewValue == null)
            {
                return;
            }
            PageResourceContentLoader load = new PageResourceContentLoader();
            load.BeginLoad(new Uri(e.NewValue + ".xaml", UriKind.Relative), null, new AsyncCallback(r =>
            {
                LoadResult ui = load.EndLoad(r);        
                c.Content = ui.LoadedContent;
            }), 1);
        }

        public static string GetSource(DependencyObject d)
        {
            return (string)d.GetValue(SourceProperty);
        }
        public static void SetSource(DependencyObject d, string value)
        {
            d.SetValue(SourceProperty, value);
        }
        #endregion

        #region DefaultButton 默认button，附加到输入框上，在输入框上按回车后，触发button执行
        public static readonly DependencyProperty DefaultButtonProperty =
               DependencyProperty.RegisterAttached("DefaultButton", typeof(string), typeof(ControlAttach), new PropertyMetadata(OnDefaultButtonChanged));

        public static string GetDefaultButton(DependencyObject d)
        {
            return (string)d.GetValue(DefaultButtonProperty);
        }
        public static void SetDefaultButton(DependencyObject d, string value)
        {
            d.SetValue(DefaultButtonProperty, value);
        }

        private static void OnDefaultButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement tb = (FrameworkElement)d;
            if (tb != null)
            {
                tb.KeyDown -= EnterProc;
                tb.KeyDown += EnterProc;
            }
        }

        private static KeyEventHandler EnterProc = (o, e) => 
        {
            FrameworkElement tb = (FrameworkElement)o;
            if (e.Key == Key.Enter)
            {
                string name = ControlAttach.GetDefaultButton(tb);
                Button button = (Button)tb.FindName(name);
                if (button != null && button.IsEnabled)
                {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer((Button)button);
                    IInvokeProvider ip = (IInvokeProvider)peer;
                    ip.Invoke();
                }
            }
        };
        #endregion

        #region LostFocus 焦点离开面板时，要执行的动作

        public static DependencyProperty LostFocusProperty = DependencyProperty.RegisterAttached(
           "LostFocus", typeof(Delegate), typeof(Panel), new PropertyMetadata(new PropertyChangedCallback(OnLostFocusChanged)));
        public static Delegate GetLostFocus(FrameworkElement ui)
        {
            return (Delegate)ui.GetValue(LostFocusProperty);
        }
        public static void SetLostFocus(FrameworkElement ui, Delegate value)
        {
            ui.SetValue(LostFocusProperty, value);
        }

        private static void OnLostFocusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //监听附加对象的焦点丢失事件
            FrameworkElement ui = obj as FrameworkElement;
            ui.LostFocus += (o, e) => 
            {
                FrameworkElement focus = FocusManager.GetFocusedElement() as FrameworkElement;
                //判断focus是否在附加对象内部
                while (focus != null && focus != ui)
                {
                    object nFocus = VisualTreeHelper.GetParent(focus);
                    if (VisualTreeHelper.GetParent(focus) != null)
                    {
                        focus = (FrameworkElement)nFocus;
                    }
                    else
                    {
                        //VisualTree找不到，用Parent尝试
                        focus = (FrameworkElement)focus.Parent;
                    }
                }
                //获得拥有焦点的对象不在附加对象内部，调用附加的代理
                if (focus == null)
                {
                    (args.NewValue as Delegate).DynamicInvoke(null);
                }
            };
        }

        #endregion

        #region InitResource 设置为真是，在界面Loaded以后，开始执行界面资源的初始化工作
        public static DependencyProperty InitResourceProperty = DependencyProperty.RegisterAttached(
           "InitResource", typeof(bool), typeof(Panel), new PropertyMetadata(new PropertyChangedCallback(OnInitResourceChanged)));
        public static bool GetInitResource(FrameworkElement ui)
        {
            return (bool)ui.GetValue(InitResourceProperty);
        }
        public static void SetInitResource(FrameworkElement ui, bool value)
        {
            ui.SetValue(InitResourceProperty, value);
        }

        private static void OnInitResourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement ui = obj as FrameworkElement;
            if (args.NewValue != null && (bool)args.NewValue)
            {
                ui.Loaded += new RoutedEventHandler(LoadHandle);
            }
        }

        private static void LoadHandle(object o, RoutedEventArgs e)
        {
            FrameworkElement ui = (FrameworkElement)o;
            ui.Loaded -= LoadHandle;
            foreach (IInitable obj in (from p in ui.Resources where p.Value is IInitable select p.Value))
            {
                obj.Init(ui);
            }
        }
        #endregion

        #region ShowIndex 要显示的子，用于显示一批叠加在一起的界面中的一个
        public static DependencyProperty ShowIndexProperty = DependencyProperty.RegisterAttached(
           "ShowIndex", typeof(int), typeof(Panel), new PropertyMetadata(new PropertyChangedCallback(OnShowIndexChanged)));
        public static int GetShowIndex(FrameworkElement ui)
        {
            return (int)ui.GetValue(ShowIndexProperty);
        }
        public static void SetShowIndex(FrameworkElement ui, int value)
        {
            ui.SetValue(ShowIndexProperty, value);
        }

        private static void OnShowIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Panel ui = obj as Panel;
            // 显示指定页面
            int i = 0;
            foreach (FrameworkElement fe in ui.Children)
            {
                if (i.Equals(args.NewValue))
                {
                    fe.Visibility = Visibility.Visible;
                }
                else
                {
                    fe.Visibility = Visibility.Collapsed;
                }
                i++;
            }
        }
        #endregion

        #region ContentTemplate DataGrid的RowGroup内容的模板，内部使用
        public static DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached(
           "ContentTemplate", typeof(DataTemplate), typeof(DataGridRowGroupHeader), null);
        public static DataTemplate GetContentTemplate(DataGridRowGroupHeader ui)
        {
            return (DataTemplate)ui.GetValue(ContentTemplateProperty);
        }
        public static void SetContentTemplate(DataGridRowGroupHeader ui, DataTemplate value)
        {
            ui.SetValue(ContentTemplateProperty, value);
        }
        #endregion

        #region GroupRowTemplate DataGrid的GroupRow的数据模板
        public static DependencyProperty GroupRowTemplateProperty = DependencyProperty.RegisterAttached(
           "GroupRowTemplate", typeof(DataTemplate), typeof(DataGrid), new PropertyMetadata(new PropertyChangedCallback(OnGroupRowTemplateChanged)));
        public static DataTemplate GetGroupRowTemplate(DataGrid ui)
        {
            return (DataTemplate)ui.GetValue(GroupRowTemplateProperty);
        }
        public static void SetGroupRowTemplate(DataGrid ui, DataTemplate value)
        {
            ui.SetValue(GroupRowTemplateProperty, value);
        }

        private static void OnGroupRowTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataGrid ui = obj as DataGrid;
            DataTemplate dt = args.NewValue as DataTemplate;
            if (dt == null)
            {
                return;
            }
            ui.LoadingRowGroup += (o, e) =>
            {
                SetContentTemplate(e.RowGroupHeader, dt);
            };
        }
        #endregion

        #region PageName 用于Panel，特别是Grid中，用于显示指定名称的页面，如果页面已经打开，把页面移动到最上面。
        public static readonly DependencyProperty PageNameProperty = DependencyProperty.RegisterAttached(
            "PageName", typeof(string), typeof(Panel), new PropertyMetadata(OnPageNameChanged));
        private static void OnPageNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panel c = (Panel)d;
            //获取页面缓存
            List<NameAndFrame> pages = GetPanelPages(c);
            //旧页面变为不可见
            if (e.OldValue != null)
            {
                NameAndFrame oldNaf = GetChildren((string)e.OldValue, pages);
                if (oldNaf != null)
                {
                    oldNaf.frame.Visibility = Visibility.Collapsed;
                }
            }
            if (e.NewValue == null)
            {
                return;
            }
            string newName = (string)e.NewValue;
            //如果不包含页面，说明页面没有打开，打开页面
            NameAndFrame newui = GetChildren(newName, pages);
            if (newui == null)
            {
                PageResourceContentLoader load = new PageResourceContentLoader();
                load.BeginLoad(new Uri(e.NewValue + ".xaml", UriKind.Relative), null, new AsyncCallback(r =>
                {
                    LoadResult ui = load.EndLoad(r);
                    FrameworkElement page = (FrameworkElement)ui.LoadedContent; 
                    c.Children.Add(page);
                    //添加到页面缓存中
                    NameAndFrame newpage = new NameAndFrame(newName, page);
                    pages.Add(newpage);
                }), 1);
            }
            else
            {
                //修改页面为可见
                newui.frame.Visibility = Visibility.Visible;
                //将新的放到栈顶
                pages.Remove(newui);
                pages.Add(newui);
            }
        }

        //存放所有Panel与页面缓存列表
        class NameAndFrame
        {
            public NameAndFrame(string name, FrameworkElement frame)
            {
                this.name = name;
                this.frame = frame;
            }
            public string name;
            public FrameworkElement frame;
        }
        private static Dictionary<Panel, List<NameAndFrame>> _pages = new Dictionary<Panel, List<NameAndFrame>>();
        //获取存放已打开页面的数据字典
        private static List<NameAndFrame> GetPanelPages(Panel panel)
        {
            if (!_pages.ContainsKey(panel))
            {
                _pages[panel] = new List<NameAndFrame>();
            }
            return _pages[panel];
        }

        //根据名称找名称界面对
        private static NameAndFrame GetChildren(string name, List<NameAndFrame> frames)
        {
            foreach (NameAndFrame naf in frames)
            {
                if (naf.name == name)
                {
                    return naf;
                }
            }
            return null;
        }

        //移走容器最上面的页面
        public static void RemoveTop(Panel p)
        {
            //获取页面缓存
            List<NameAndFrame> pages = GetPanelPages(p);
            p.Children.Remove(pages[pages.Count - 1].frame);
            pages.RemoveAt(pages.Count - 1);
        }

        //获得容器最上面页面的名称
        public static string GetTop(Panel p)
        {
            //获取页面缓存
            List<NameAndFrame> pages = GetPanelPages(p);
            if (pages.Count == 0)
            {
                return null;
            }
            NameAndFrame naf = pages[pages.Count - 1];
            return naf.name;
        }

        public static string GetPageName(DependencyObject d)
        {
            return (string)d.GetValue(PageNameProperty);
        }
        public static void SetPageName(DependencyObject d, string value)
        {
            d.SetValue(PageNameProperty, value);
        }
        #endregion

        #region IsButtonDown 鼠标是否按下
        public static DependencyProperty IsButtonDownProperty = DependencyProperty.RegisterAttached(
           "IsButtonDown", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(false));
        public static bool GetIsButtonDown(FrameworkElement ui)
        {
            return (bool)ui.GetValue(FixedDragProperty);
        }
        public static void SetIsButtonDown(FrameworkElement ui, bool value)
        {
            ui.SetValue(FixedDragProperty, value);
        }
        #endregion

        #region FixedDrag 修改DragDropTarget的bug
        public static DependencyProperty FixedDragProperty = DependencyProperty.RegisterAttached(
           "FixedDrag", typeof(bool), typeof(TreeViewDragDropTarget), 
           new PropertyMetadata(new PropertyChangedCallback(OnFixedDragChanged)));
        public static bool GetFixedDrag(TreeViewDragDropTarget ui)
        {
            return (bool)ui.GetValue(FixedDragProperty);
        }
        public static void SetFixedDrag(TreeViewDragDropTarget ui, bool value)
        {
            ui.SetValue(FixedDragProperty, value);
        }

        private static void OnFixedDragChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TreeViewDragDropTarget tree = (TreeViewDragDropTarget)obj;
            //监听开始拖动事件，如果鼠标按下，才真正拖动
            tree.ItemDragStarting += (o, e) =>
            {
                if (!GetIsButtonDown(tree))
                {
                    e.Cancel = true;
                    e.Handled = true;
                }
            };
            //在鼠标按下及松开时，管理鼠标是否按下状态
            tree.AddHandler(TreeView.MouseLeftButtonDownEvent, 
                new MouseButtonEventHandler((o, e) => 
                { 
                    SetIsButtonDown(tree, true); 
                }), true);
            tree.AddHandler(TreeView.MouseLeftButtonUpEvent,
                new MouseButtonEventHandler((o, e) =>
                {
                    SetIsButtonDown(tree, false);
                }), true);
        }

        #endregion
    }
}
