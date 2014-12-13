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
using System.Collections.Generic;
using Com.Aote.ObjectTools;
using System.Collections;

namespace Com.Aote.Reports
{
    /// <summary>
    /// 一种头部，左部都可能有变化数据的复杂报表
    /// </summary>
    public class HeadLeftReport : Control
    {
        #region TableHeaderTemplate 表头变化部分模板
        public DataTemplate TableHeaderTemplate
        {
            get { return (DataTemplate)GetValue(TableHeaderTemplateProperty); }
            set { SetValue(TableHeaderTemplateProperty, value); }
        }

        private static void OnTableHeaderTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            control.UpdateHeader();
        }

        public static readonly DependencyProperty TableHeaderTemplateProperty =
            DependencyProperty.Register("TableHeaderTemplate", typeof(DataTemplate), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableHeaderTemplateChanged)));
        #endregion

        #region TailTemplate 表尾模板
        public DataTemplate TailTemplate
        {
            get { return (DataTemplate)GetValue(TailTemplateProperty); }
            set { SetValue(TailTemplateProperty, value); }
        }

        private static void OnTailTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            control.UpdateTail();
        }

        public static readonly DependencyProperty TailTemplateProperty =
            DependencyProperty.Register("TailTemplate", typeof(DataTemplate), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTailTemplateChanged)));
       
        #endregion

        #region TableHeaderItems 表头变化部分数据
        public ICollection TableHeaderItems
        {
            get { return (ICollection)GetValue(TableHeaderItemsProperty); }
            set { SetValue(TableHeaderItemsProperty, value); }
        }

        private static void OnTableHeaderItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            ICollection value = (ICollection)e.NewValue;
            //先用现有数据更新一次表头，当表头数据加载完成后，用新的表头数据再次更新表头
            if (value.Count != 0)
            {
                control.UpdateHeader();
            }
            if(value is ILoadable)
            {
                ILoadable obj = (ILoadable)value;
                obj.DataLoaded += (o1, e1) =>
                {
                    control.UpdateHeader();
                };
            }
        }

        public static readonly DependencyProperty TableHeaderItemsProperty =
            DependencyProperty.Register("TableHeaderItems", typeof(ICollection), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableHeaderItemsChanged)));
        #endregion

        #region TableLeftTemplate 表左部变化部分模板
        public DataTemplate TableLeftTemplate
        {
            get { return (DataTemplate)GetValue(TableLeftTemplateProperty); }
            set { SetValue(TableLeftTemplateProperty, value); }
        }

        private static void OnTableLeftTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            control.UpdateLeft();
        }

        public static readonly DependencyProperty TableLeftTemplateProperty =
            DependencyProperty.Register("TableLeftTemplate", typeof(DataTemplate), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableLeftTemplateChanged)));
        #endregion

        #region TableLeftItems 表左部变化部分数据
        public ICollection TableLeftItems
        {
            get { return (ICollection)GetValue(TableLeftItemsProperty); }
            set { SetValue(TableLeftItemsProperty, value); }
        }

        private static void OnTableLeftItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            ICollection value = (ICollection)e.NewValue;
            //先用现有数据更新一次表头，当表头数据加载完成后，用新的表头数据再次更新表头
            if (value.Count != 0)
            {
                control.UpdateLeft();
            }
            if (value is ILoadable)
            {
                ILoadable obj = (ILoadable)value;
                obj.DataLoaded += (o1, e1) =>
                {
                    control.UpdateLeft();
                };
            }
        }

        public static readonly DependencyProperty TableLeftItemsProperty =
            DependencyProperty.Register("TableLeftItems", typeof(ICollection), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableLeftItemsChanged)));
        #endregion

        #region TableBodyTemplate 表体单个单元格模板
        public DataTemplate TableBodyTemplate
        {
            get { return (DataTemplate)GetValue(TableBodyTemplateProperty); }
            set { SetValue(TableBodyTemplateProperty, value); }
        }

        private static void OnTableBodyTemplateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            control.UpdateTemplate();
        }

        public static readonly DependencyProperty TableBodyTemplateProperty =
            DependencyProperty.Register("TableBodyTemplate", typeof(DataTemplate), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableBodyTemplateChanged)));
        #endregion

        #region TableBodyItems 表体数据源
        public IEnumerable<object> TableBodyItems
        {
            get { return (IEnumerable<object>)GetValue(TableBodyItemsProperty); }
            set { SetValue(TableBodyItemsProperty, value); }
        }

        private static void OnTableBodyItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            HeadLeftReport control = (HeadLeftReport)o;
            control.UpdateTemplate();
            //数据加载完成后，要重绘报表
            if (e.NewValue is ILoadable)
            {
                ILoadable obj = (ILoadable)e.NewValue;
                obj.DataLoaded += (o1, e1) =>
                {
                    control.UpdateTableBody();
                    control.UpdateTableTail();
                };
            }
        }

        public static readonly DependencyProperty TableBodyItemsProperty =
            DependencyProperty.Register("TableBodyItems", typeof(IEnumerable<object>), typeof(HeadLeftReport),
            new PropertyMetadata(new PropertyChangedCallback(OnTableBodyItemsChanged)));
        #endregion

        //总列数
        int _col;
        //总行数
        int _row;

        //头部的行数
        int _headRow;
        //左部的列数
        int _leftCol;

        //尾部的行数
        int _tailRow;

        //表头元素列表
        private List<FrameworkElement> _headerElements;

        //表左部元素列表
        private List<FrameworkElement> _leftElements;

        //表体单元格，重新生成表体时，这些单元格要从表体清除
        private List<FrameworkElement> _bodyElements = new List<FrameworkElement>();

        //表尾单元格
        private List<FrameworkElement> _tailElements = new List<FrameworkElement>();

        //最后产生的Grid
        private Grid grid;

        //处理表头部分
        public void UpdateHeader()
        {
            if (TableHeaderItems == null || TableHeaderTemplate == null || TableHeaderItems.Count == 0) return;

            //变化部分当前列号
            _col = 0;
            //存放表头数据
            _headerElements = new List<FrameworkElement>();
            foreach (object o in TableHeaderItems)
            {
                Grid g = (Grid)TableHeaderTemplate.LoadContent();
                g.DataContext = o;
                //获取某一项表头变化部分，修改列后存起来
                foreach (FrameworkElement child in g.Children)
                {
                    int c = Grid.GetColumn(child);
                    Grid.SetColumn(child, c + _col);
                    //放置到表头元素表中
                    _headerElements.Add(child);
                }
                //情况所有列表内容
                g.Children.Clear();
                //重新计算总列号
                _col += g.ColumnDefinitions.Count;
                //登记头部行数
                _headRow = g.ColumnDefinitions.Count;
            }
            //绘制报表
            UpdateTemplate();
        }

        //处理表尾部分
        public void UpdateTail()
        {
            Grid g = (Grid)TailTemplate.LoadContent();
            _tailRow = g.RowDefinitions.Count;
            UpdateTemplate();
        }

        //处理表左边
        public void UpdateLeft()
        {
            if (TableLeftItems == null || TableLeftTemplate == null || TableLeftItems.Count == 0) return;

            //变化部分当前行
            _row = 0;
            //存放左边元素
            _leftElements = new List<FrameworkElement>();
            foreach (object o in TableLeftItems)
            {
                Grid g = (Grid)TableLeftTemplate.LoadContent();
                g.DataContext = o;
                //获取某一项变化部分，修改列后存起来
                foreach (FrameworkElement child in g.Children)
                {
                    int c = Grid.GetRow(child);
                    Grid.SetRow(child, c + _row);
                    //放置到表头元素表中
                    _leftElements.Add(child);
                }
                //清空所有列表内容
                g.Children.Clear();
                //重新计算总行号
                _row += g.RowDefinitions.Count;
                //登记左部列数
                _leftCol = g.RowDefinitions.Count;
            }
            //绘制报表
            UpdateTemplate();
        }

        //绘制报表
        private void UpdateTemplate()
        {
            //模板，表头，表左部，表体都必须准备好，才做
            if (Template == null || TailTemplate == null || 
                _headerElements == null || _leftElements == null ||
                TableBodyTemplate == null || TableHeaderItems == null) return;
            //把旧的Grid清空，以便子可以重新放入grid中
            if (grid != null)
            {
                grid.Children.Clear();
            }
            //往Template的Cavans部分插入表格
            ContentControl content = (ContentControl)GetTemplateChild("body");
            grid = new Grid();
            content.Content = grid;
            //设置列数，加上左部的列数
            for (int i = 0; i < _col + _leftCol; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(cd);
            }
            //设置行数，加上头部的行以及表尾的行
            for (int i = 0; i < _row + _headRow + _tailRow; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                grid.RowDefinitions.Add(rd);
            }
            //把表头数据插入表格中，列号要加上左部所占列
            foreach (FrameworkElement ui in _headerElements)
            {
                //用边框把内容框起来
                Border b = new Border();
                b.Child = ui;
                b.BorderThickness = new Thickness(1, 1, 1, 1);
                b.BorderBrush = new SolidColorBrush(Colors.Black);
                Grid.SetRow(b, Grid.GetRow(ui));
                Grid.SetColumn(b, Grid.GetColumn(ui) + _leftCol);
                Grid.SetRowSpan(b, Grid.GetRowSpan(ui));
                Grid.SetColumnSpan(b, Grid.GetColumnSpan(ui));
                grid.Children.Add(b);
            }
            //把左部元素插入表格中，行号要加上头部所占行
            foreach (FrameworkElement ui in _leftElements)
            {
                //用边框把内容框起来
                Border b = new Border();
                b.Child = ui;
                b.BorderThickness = new Thickness(1, 1, 1, 1);
                b.BorderBrush = new SolidColorBrush(Colors.Black);
                Grid.SetRow(b, Grid.GetRow(ui) + _headRow);
                Grid.SetColumn(b, Grid.GetColumn(ui));
                Grid.SetRowSpan(b, Grid.GetRowSpan(ui));
                Grid.SetColumnSpan(b, Grid.GetColumnSpan(ui));
                grid.Children.Add(b);
            }
            UpdateTableBody();
            UpdateTableTail();
        }

        //生成表体部分
        private void UpdateTableBody()
        {
            //把原来产生的单元格清除掉
            foreach (FrameworkElement ui in _bodyElements)
            {
                grid.Children.Remove(ui);
            }
            _bodyElements.Clear();

            int i1 = 0;
            //把单元格插入表格中，行号及列号添加头及左部所占单元格
            foreach (object header in TableHeaderItems)
            {
                int j = 0;
                foreach (object left in TableLeftItems)
                {
                    Grid ui = (Grid)TableBodyTemplate.LoadContent();
                    //每个单元格的数据上下文一样，插入特殊的数据实体
                    DataObjectsView view = new DataObjectsView() { Row = left, Col = header, Objects = TableBodyItems };
                    //表体模板由多个单元格组成
                    List<UIElement> cells = new List<UIElement>(ui.Children);
                    ui.Children.Clear();
                    foreach (FrameworkElement cell in cells)
                    {
                        cell.DataContext = view;
                        //用边框把内容框起来
                        Border b = new Border();
                        b.Child = cell;
                        b.BorderThickness = new Thickness(1, 1, 1, 1);
                        b.BorderBrush = new SolidColorBrush(Colors.Black);
                        //单元格所在行为 整个头部所占行 + 单元格行号 * 每个单元格行总数E:\workspace3.5\restV2\s2\s2\App.xaml
                        Grid.SetRow(b, Grid.GetRow(cell) + _headRow + j * ui.RowDefinitions.Count);
                        //单元格所占列为 整个左边所占列 + 单元格列号 * 每个单元格列总数
                        Grid.SetColumn(b, Grid.GetColumn(cell) + _leftCol + i1 * ui.ColumnDefinitions.Count);
                        Grid.SetRowSpan(b, Grid.GetRowSpan(ui));
                        Grid.SetColumnSpan(b, Grid.GetColumnSpan(ui));
                        grid.Children.Add(b);
                        _bodyElements.Add(b);
                    }
                    j++;
                }
                i1++;
            }
        }

        //生成表尾部分
        private void UpdateTableTail()
        {
            //把原来产生的单元格清除掉
            foreach (FrameworkElement ui in _tailElements)
            {
                grid.Children.Remove(ui);
            }
            _tailElements.Clear();

            int i1 = 0;
            //把单元格插入表格中，行号及列号添加头及左部所占单元格
            foreach (object header in TableHeaderItems)
            {
                Grid ui = (Grid)TailTemplate.LoadContent();
                //每个单元格的数据上下文一样，插入特殊的数据实体
                DataObjectsView view = new DataObjectsView() { Col = header, Objects = TableBodyItems };
                //表体模板由多个单元格组成
                List<UIElement> cells = new List<UIElement>(ui.Children);
                ui.Children.Clear();
                foreach (FrameworkElement cell in cells)
                {
                    cell.DataContext = view;
                    //用边框把内容框起来
                    Border b = new Border();
                    b.Child = cell;
                    b.BorderThickness = new Thickness(1, 1, 1, 1);
                    b.BorderBrush = new SolidColorBrush(Colors.Black);
                    //单元格所在行为 整个头部所占行 + 表左部所占行
                    Grid.SetRow(b, Grid.GetRow(cell) + _headRow + _row);
                    //单元格所占列为 整个左边所占列 + 单元格列号 * 每个单元格列总数
                    Grid.SetColumn(b, Grid.GetColumn(cell) + _leftCol + i1 * ui.ColumnDefinitions.Count);
                    Grid.SetRowSpan(b, Grid.GetRowSpan(ui));
                    Grid.SetColumnSpan(b, Grid.GetColumnSpan(ui));
                    grid.Children.Add(b);
                    _bodyElements.Add(b);
                }
                i1++;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateTemplate();
        }
    }
}
