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
using Com.Aote.ObjectTools;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace Com.Aote.Attachs
{
    public class DataGridAttach
    {
        #region UpdateOnEnter 回车时更新绑定，值为要更新的属性名，一般情况下，为Text
        public static DependencyProperty FoceToCellOnEnterProperty = DependencyProperty.RegisterAttached(
           "FoceToCellOnEnter", typeof(int), typeof(DataGrid), new PropertyMetadata(new PropertyChangedCallback(OnFoceToCellOnEnterChanged)));
        public static int GetFoceToCellOnEnter(FrameworkElement ui)
        {
            return (int)ui.GetValue(FoceToCellOnEnterProperty);
        }
        public static void SetFoceToCellOnEnter(FrameworkElement ui, int value)
        {
            ui.SetValue(FoceToCellOnEnterProperty, value);
        }

        /// <summary>
        /// 回车时更新绑定，值为要更新的属性名，一般情况下，为Text
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnFoceToCellOnEnterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataGrid dg = (DataGrid)obj;
            dg.KeyDown += new KeyEventHandler(dg_KeyDown);
           
        }

        static void dg_KeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (e.Key.Equals(Key.Tab))
            {
                e.Handled = true;
                int currentRow = dg.SelectedIndex;
                ObjectList ol = (ObjectList)dg.ItemsSource;
                if(currentRow < ol.Count -1)
                 {
                    GeneralObject go = ol[currentRow + 1];
                    dg.SelectedIndex = currentRow + 1;
                    int colIndex = GetFoceToCellOnEnter(dg);
                    DataGridColumn fe = dg.Columns[colIndex];
                    dg.CurrentColumn = fe;
                    dg.ScrollIntoView(go, fe);
                    FrameworkElement c = (FrameworkElement)dg.CurrentColumn.GetCellContent(go);
                    c.GetType().GetMethod("Focus").Invoke(c, null);
                }

            }
        }

        

        #endregion

        #region ServerSort 是否执行后台排序，默认为False
        public static DependencyProperty ServerSortProperty = DependencyProperty.RegisterAttached(
           "ServerSort", typeof(bool), typeof(DataGrid), new PropertyMetadata(new PropertyChangedCallback(OnServerSortChanged)));
        public static bool GetServerSort(FrameworkElement ui)
        {
            return (bool)ui.GetValue(ServerSortProperty);
        }
        public static void SetServerSort(FrameworkElement ui, bool value)
        {
            ui.SetValue(ServerSortProperty, value);
        }

        /// <summary>
        /// 是否执行后台排序，为True时执行
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnServerSortChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == false)
            {
                return;
            }
            DataGrid dg = (DataGrid)obj;
            dg.MouseLeftButtonDown -= dg_MouseLeftButtonDown;
            dg.MouseLeftButtonDown += new MouseButtonEventHandler(dg_MouseLeftButtonDown);
        }

        static void dg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            //获取点击列标题
            var u = from element in VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), grid)
                    where element is DataGridColumnHeader
                    select element;
            if (u.Count() == 1)
            {
                e.Handled = true;
                DataGridColumnHeader header = (DataGridColumnHeader)u.Single();
                string headername = header.Content.ToString();
                //将获得的标题名传给模型处理
                BaseObjectList list = (BaseObjectList)grid.ItemsSource;
                list.ChangeSortName(headername);
            }
            else
            {
                e.Handled = false;
            }
        }
        #endregion
    }
}
