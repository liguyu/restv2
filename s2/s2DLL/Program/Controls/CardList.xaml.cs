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
using System.ComponentModel;

namespace Com.Aote.Controls
{
    public partial class CardList : Canvas, INotifyPropertyChanged
    {
        public CardList()
        {
            InitializeComponent();
            this.MouseEnter -= CardList_MouseEnter;
            this.MouseEnter += new MouseEventHandler(CardList_MouseEnter);
            this.MouseLeave -= CardList_MouseLeave;
            this.MouseLeave += new MouseEventHandler(CardList_MouseLeave);
        }

        void CardList_MouseEnter(object sender, MouseEventArgs e)
        {
            OneRow = false;
        }

        void CardList_MouseLeave(object sender, MouseEventArgs e)
        {
            OneRow = true;
        }

        #region ItemsSource 数据源
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CardList),
            new PropertyMetadata(new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardList control = (CardList)o;
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
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CardList),
            new PropertyMetadata(new PropertyChangedCallback(OnItemTemplateChanged)));

        private static void OnItemTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardList control = (CardList)o;
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
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CardList), null);

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        #endregion

        #region 布局行列数
        private int cols;
        public int Cols
        {
            set
            {
                cols = value;
                if (cols != null && cols > 0 && Rows > 0)
                {
                    this.UpdateItems();
                }
            }
            get
            {
                return this.cols;
            }
        }

        private int rows;
        public int Rows
        {
            set
            {
                rows = value;
                if (rows != null && rows > 0 && Cols > 0)
                {
                    this.UpdateItems();
                }
            }
            get
            {
                return this.rows;
            }
        }
        #endregion

        #region OneRow 显示一行缩略图标
        public static readonly DependencyProperty OneRowProperty =
            DependencyProperty.Register("OneRow", typeof(bool), typeof(CardList), null);

        public bool OneRow
        {
            get { return (bool)GetValue(OneRowProperty); }
            set
            {
                SetValue(OneRowProperty, value);
                ShowItems();
            }
        }
        #endregion

        //元素与数据项之间的对应关系
        private Dictionary<FrameworkElement, object> items = new Dictionary<FrameworkElement, object>();

        //更新数据项
        public void UpdateItems()
        {
            if (ItemsSource == null || ItemTemplate == null || this.rows <= 0 || this.cols <= 0) return;

            // 创建透明背景色
            SolidColorBrush scb = new SolidColorBrush(Color.FromArgb(byte.Parse("13"), byte.Parse("255"), byte.Parse("255"), byte.Parse("255")));
            this.Background = scb;

            //清空子元素
            this.Children.Clear();
            items.Clear();
            
            //设置实际内容
            int i = 0;
            int rowIndex = 0;
            int colIndex = 0;
            foreach (object o in ItemsSource)
            {
                FrameworkElement g = (FrameworkElement)ItemTemplate.LoadContent();
                g.DataContext = o;
                double left = colIndex * g.Width;
                double top = rowIndex * g.Height;
                Console.WriteLine("num:" + i + ",left:" + left + ",top:" + top);
                Canvas.SetLeft(g, left);
                Canvas.SetTop(g, top);
                this.Children.Add(g);
                items[g] = o;
                g.MouseLeftButtonUp -= g_MouseLeftButtonUp;
                g.MouseLeftButtonUp += new MouseButtonEventHandler(g_MouseLeftButtonUp);
                colIndex++;
                //一行完成，设置行数，列数重新计数
                if (colIndex == this.cols)
                {
                    colIndex = 0;
                    rowIndex++;
                }
                i++;
            }
            ShowItems();
        }

        //显示数据项
        public void ShowItems()
        {
            if (ItemsSource == null || ItemTemplate == null || this.rows <= 0 || this.cols <= 0) return;

            // 设置canvens宽高
            FrameworkElement template = (FrameworkElement)ItemTemplate.LoadContent();
            this.Width = this.cols * template.Width;
            if (OneRow)
            {
                this.Height = 1 * template.Height;
            }
            else
            {
                this.Height = this.rows * template.Height;
            }

            int colIndex = 0;
            foreach (FrameworkElement fe in items.Keys)
            {
                if (OneRow && colIndex >= this.cols)
                {
                    fe.Visibility = Visibility.Collapsed;
                }
                else
                {
                    fe.Visibility = Visibility.Visible;
                }
                colIndex++;
            }
        }

        void g_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedItem = null;
            OnPropertyChanged("SelectedItem");
            SelectedItem = items[(FrameworkElement)sender];
            OnPropertyChanged("SelectedItem");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
