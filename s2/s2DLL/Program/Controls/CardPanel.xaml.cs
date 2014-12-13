using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;

namespace Com.Aote.Controls
{
    //存放坐标变换参数，缩放比例只有一个，即按等比例缩放
    public class TransformData : DependencyObject
    {
        #region StartScale
        public static readonly DependencyProperty StartScaleProperty =
            DependencyProperty.Register("StartScale", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double StartScale
        {
            get { return (double)GetValue(StartScaleProperty); }
            set { SetValue(StartScaleProperty, value); }
        }
        #endregion

        #region EndScale
        public static readonly DependencyProperty EndScaleProperty =
            DependencyProperty.Register("EndScale", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double EndScale
        {
            get { return (double)GetValue(EndScaleProperty); }
            set { SetValue(EndScaleProperty, value); }
        }
        #endregion

        #region StartTranslateX
        public static readonly DependencyProperty StartTranslateXProperty =
            DependencyProperty.Register("StartTranslateX", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double StartTranslateX
        {
            get { return (double)GetValue(StartTranslateXProperty); }
            set { SetValue(StartTranslateXProperty, value); }
        }
        #endregion

        #region EndTranslateX
        public static readonly DependencyProperty EndTranslateXProperty =
            DependencyProperty.Register("EndTranslateX", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double EndTranslateX
        {
            get { return (double)GetValue(EndTranslateXProperty); }
            set { SetValue(EndTranslateXProperty, value); }
        }
        #endregion

        #region StartTranslateY
        public static readonly DependencyProperty StartTranslateYProperty =
            DependencyProperty.Register("StartTranslateY", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double StartTranslateY
        {
            get { return (double)GetValue(StartTranslateYProperty); }
            set { SetValue(StartTranslateYProperty, value); }
        }
        #endregion

        #region EndTranslateY
        public static readonly DependencyProperty EndTranslateYProperty =
            DependencyProperty.Register("EndTranslateY", typeof(double), typeof(TransformData),
            new PropertyMetadata(null));
        public double EndTranslateY
        {
            get { return (double)GetValue(EndTranslateYProperty); }
            set { SetValue(EndTranslateYProperty, value); }
        }
        #endregion
    }

    public partial class CardPanel : UserControl
    {
        public CardPanel()
        {
            InitializeComponent();
            //设置初始坐标数据
            TransformData.StartScale = 1;
            TransformData.EndScale = 1;
            TransformData.StartTranslateX = 0;
            TransformData.StartTranslateY = 0;
            //当控件大小变化后，让主布局充满
            this.LayoutUpdated += (o, e) =>
            {
                Layout.Width = this.ActualWidth;
                Layout.Height = this.ActualHeight;
                //重新排列子页面
                LayoutPages();
            };
            this.MouseWheel += new MouseWheelEventHandler(CardPanel_MouseWheel);
        }

        public TransformData TransformData
        {
            get { return (TransformData)Resources["TransformData"]; }
        }

        #region ItemsSource 数据源
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CardPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardPanel control = (CardPanel)o;
            IEnumerable value = (IEnumerable)e.NewValue;
            //先用现有数据更新一次表头，当表头数据加载完成后，用新的表头数据再次更新表头
            if (value != null)
            {
                control.UpdateItems();
                if (value is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)value).CollectionChanged += (o1, e1) =>
                    {
                        control.UpdateItems();
                    };
                }
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        #endregion

        #region ItemTemplate 数据项模板
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CardPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnItemTemplateChanged)));

        private static void OnItemTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardPanel control = (CardPanel)o;
            control.UpdateItems();
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        #endregion

        #region SelectedItem 当前选中项
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CardPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnSelectedItemChanged)));

        private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardPanel control = (CardPanel)o;
            if (e.NewValue == null) return;
            control.Zoom((FrameworkElement)control.objects[e.NewValue]);
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        #endregion

        #region 内部页面的宽 PageWidth
        public static readonly DependencyProperty PageWidthProperty =
            DependencyProperty.Register("PageWidth", typeof(double), typeof(CardPanel),
            new PropertyMetadata(920.0));

        public double PageWidth
        {
            get { return (double)GetValue(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }
        #endregion

        #region 内部页面的高 PageHeight
        public static readonly DependencyProperty PageHeightProperty =
            DependencyProperty.Register("PageHeight", typeof(double), typeof(CardPanel),
            new PropertyMetadata(610.0));

        public double PageHeight
        {
            get { return (double)GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }
        #endregion

        #region 内部页面之间的横向间距 PagePaddingX
        public static readonly DependencyProperty PagePaddingXProperty =
            DependencyProperty.Register("PagePaddingX", typeof(double), typeof(CardPanel),
            new PropertyMetadata(5.0));

        public double PagePaddingX
        {
            get { return (double)GetValue(PagePaddingXProperty); }
            set { SetValue(PagePaddingXProperty, value); }
        }
        #endregion

        #region 内部页面之间的纵向间距 PagePaddingY
        public static readonly DependencyProperty PagePaddingYProperty =
            DependencyProperty.Register("PagePaddingY", typeof(double), typeof(CardPanel),
            new PropertyMetadata(5.0));

        public double PagePaddingY
        {
            get { return (double)GetValue(PagePaddingYProperty); }
            set { SetValue(PagePaddingYProperty, value); }
        }
        #endregion

        #region 布局行数 rows
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(CardPanel),
            new PropertyMetadata(null));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }
        #endregion

        #region 布局列数
        public static readonly DependencyProperty ColsProperty =
            DependencyProperty.Register("Cols", typeof(int), typeof(CardPanel),
            new PropertyMetadata(null));

        public int Cols
        {
            get { return (int)GetValue(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }
        #endregion
        private bool zoomed = false;
        private UIElement last;

        private bool isDrag = false;
        private bool isButtonDown = false;
        private Point startPoint;

        //元素与数据项之间的对应关系
        private Dictionary<FrameworkElement, object> items = new Dictionary<FrameworkElement, object>();
        private Dictionary<object, FrameworkElement> objects = new Dictionary<object, FrameworkElement>();

        //更新数据项
        private void UpdateItems()
        {
            if (ItemsSource == null || ItemTemplate == null) return;

            Canvas.Children.Clear();
            Canvas.MouseRightButtonDown += new MouseButtonEventHandler(Canvas_MouseRightButtonDown);
            items.Clear();
            objects.Clear();
            foreach (object o in ItemsSource)
            {
                FrameworkElement g = (FrameworkElement)ItemTemplate.LoadContent();
                g.DataContext = o;
                //用viewbox将子页面包起来，以便自动缩小，viewbox的大小在排列后产生
                Viewbox v = new Viewbox();
                v.Child = g;
                //把viewbox当做子保存起来
                Canvas.Children.Add(v);
                items[v] = o;
                objects[o] = v;
                v.MouseLeftButtonUp += new MouseButtonEventHandler(g_MouseLeftButtonUp);
                v.MouseLeftButtonDown += new MouseButtonEventHandler(g_MouseLeftButtonDown);
                v.MouseRightButtonUp += new MouseButtonEventHandler(v_MouseRightButtonUp);
                v.MouseMove += new MouseEventHandler(g_MouseMove);
            }
        }

        void v_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Viewbox v = (Viewbox)sender;
            FrameworkElement ui = (FrameworkElement)sender;
            ui.ReleaseMouseCapture();
            isButtonDown = false;
            if (isDrag)
            {
                isDrag = false;
                return;
            }
            isDrag = false;
            //不在放大状态，转到放大状态
            if (!zoomed || sender != last)
            {
                //Zoom(ui);
                ////设置其他没有选中不可见
                //foreach (Viewbox item in items.Keys)
                //{
                //    if (!item.Equals(v))
                //    {
                //        item.Visibility = Visibility.Collapsed;
                //    }
                //    else
                //    {
                //        item.Visibility = Visibility.Visible;
                //    }
                //}
            }
            else
            {
                //缩小的时候，设置所有可见
                foreach (Viewbox item in items.Keys)
                {
                    item.Visibility = Visibility.Visible;
                }
                //还原
                TransformData.EndScale = 1;
                TransformData.EndTranslateX = 0;
                TransformData.EndTranslateY = 0;
                BeginStoryboard();
                zoomed = false;
            }
            last = (UIElement)sender;
        }

        void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        //排列数据项页面
        private void LayoutPages()
        {
            if (Canvas.Children.Count == 0)
            {
                return;
            }
            //计算横向及纵向比例关系
            double radio = this.ActualWidth / this.ActualHeight;

            //每行放cols列算出行数，根据容器大小算出每个的高度乘以行数，如果超出容器高度则cols加1
            int rows = 0;
            int cols = 0;
            for (cols = 1; cols <= Canvas.Children.Count; cols++)
            {
                rows = Canvas.Children.Count / cols;
                if (Canvas.Children.Count % cols > 0)
                {
                    rows++;
                }
                double nheight = (this.ActualWidth / cols) / radio;
                if (nheight * rows <= this.ActualHeight)
                {
                    break;
                }
            }
            this.Rows = rows;
            this.Cols = cols;
            double height = this.ActualHeight / rows;
            double width = this.ActualWidth / cols;
            double scalx = width / (PageWidth + PagePaddingX);
            double scaly = height / (PageHeight + PagePaddingY);
            double scal = Math.Min(scalx, scaly);
            width = (PageWidth + PagePaddingX) * scal;
            height = (PageHeight + PagePaddingY) * scal;
            //一个个处理，够一行后，移动到下一行
            int i = 0;
            int row = 0;
            int col = 0;
            //跳转大页面的时候，这里报错，所有注释了
            try
            {
                foreach (FrameworkElement ui in Canvas.Children)
                {
                    //设置实际宽高，根据实际宽高，计算位置
                    ui.Width = width - PagePaddingX;
                    ui.Height = height - PagePaddingY;
                    Canvas.SetLeft(ui, col * width);
                    Canvas.SetTop(ui, row * height);
                    col++;
                    //超出一行，到下一行
                    if (col == cols)
                    {
                        col = 0;
                        row++;
                    }
                    i++;
                }
            }
            catch (Exception)
            {
                
            }
            
        }

        void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement ui = (UIElement)sender;
            startPoint = e.GetPosition(ui);
            isButtonDown = true;
            ui.CaptureMouse();
        }

        void g_MouseMove(object sender, MouseEventArgs e)
        {
            //在拖动，Canvas移动
            if (isButtonDown)
            {
                isDrag = true;
                FrameworkElement ui = (FrameworkElement)sender;
                Point pos = e.GetPosition(ui);
                TransformData.EndTranslateX = TransformData.EndTranslateX + (pos.X - startPoint.X) * TransformData.EndScale;
                TransformData.EndTranslateY = TransformData.EndTranslateY + (pos.Y - startPoint.Y) * TransformData.EndScale;
                startPoint = pos;
                BeginStoryboard();
                zoomed = false;
            }
        }

        void Zoom(FrameworkElement ui)
        {
            //设置其他没有选中不可见
            foreach (Viewbox item in items.Keys)
            {
                if (!item.Equals(ui))
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    item.Visibility = Visibility.Visible;
                }
            }
            double left = Canvas.GetLeft(ui);
            double top = Canvas.GetTop(ui);
            //取缩放比例中最小值
            double sx = Canvas.Width / ui.Width;
            double sy = Canvas.Height / ui.Height;
            TransformData.EndScale = Math.Min(sx, sy);
            TransformData.EndTranslateX = -(left * TransformData.EndScale);
            TransformData.EndTranslateY = -(top * TransformData.EndScale);
            BeginStoryboard();
            zoomed = true;
        }

        void g_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Viewbox v = (Viewbox)sender;
            FrameworkElement ui = (FrameworkElement)sender;
            ui.ReleaseMouseCapture();
            isButtonDown = false;
            if (isDrag)
            {
                isDrag = false;
                return;
            }
            isDrag = false;
            //不在放大状态，转到放大状态
            if (!zoomed || sender != last)
            {
                Zoom(ui);
            }
            else
            {
                ////缩小的时候，设置所有可见
                //foreach (Viewbox item in items.Keys)
                //{
                //    item.Visibility = Visibility.Visible;
                //}
                ////还原
                //TransformData.EndScale = 1;
                //TransformData.EndTranslateX = 0;
                //TransformData.EndTranslateY = 0;
                //BeginStoryboard();
                //zoomed = false;
            }
            last = (UIElement)sender;
        }

        void CardPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                TransformData.EndScale *= 0.9;
                TransformData.EndTranslateX += Canvas.Width * 0.1;
                TransformData.EndTranslateY += Canvas.Height * 0.1;
                BeginStoryboard();
            }
            if (e.Delta < 0)
            {
                TransformData.EndScale /= 0.9;
                TransformData.EndTranslateX -= Canvas.Width * 0.1;
                TransformData.EndTranslateY -= Canvas.Height * 0.1;
                BeginStoryboard();
            }
            zoomed = false;
        }

        private void BeginStoryboard()
        {
            Storyboard s = (Storyboard)Resources["Storyboard"];
            s.Begin();
            s.Completed += (o2, e2) =>
            {
                TransformData.StartScale = TransformData.EndScale;
                TransformData.StartTranslateX = TransformData.EndTranslateX;
                TransformData.StartTranslateY = TransformData.EndTranslateY;
            };
        }

    }
}
