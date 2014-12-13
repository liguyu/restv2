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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Text;

namespace s2DLL.Program.Controls
{
    public class MyDataGrid1 : DataGrid
    {
        private const string HeaderCheckBoxName = "select_column_checkbox";

        private Dictionary<object, MarkObject> _markObjects;
        private DataGridTemplateColumn _selectColumn;
        private CheckBox _selectCheckBox;

        public MyDataGrid1()
        {
            _markObjects = new Dictionary<object, MarkObject>();

            _selectColumn = new DataGridTemplateColumn();
            _selectColumn.HeaderStyle = GetHeaderStyle();
            _selectColumn.CellTemplate = GetCellTemplate();
            this.Columns.Insert(0, _selectColumn);
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        public void SelectAll()
        {
            if (_selectCheckBox != null)
                _selectCheckBox.IsChecked = true;
            SetAllSelectedStates(true);
        }

        public void UnselectAll()
        {
            if (_selectCheckBox != null)
                _selectCheckBox.IsChecked = false;
            SetAllSelectedStates(false);
        }

        public List<T> GetSelectedItems<T>()
        {
            List<T> result = new List<T>();
            if (ItemsSource != null)
            {
                var enu = ItemsSource.GetEnumerator();
                while (enu.MoveNext())
                {
                    if (GetMarkObject(enu.Current).Selected)
                        result.Add((T)enu.Current);
                }
            }
            return result;
        }

        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            base.OnLoadingRow(e);

            object dataContext = e.Row.DataContext;
            FrameworkElement element = _selectColumn.GetCellContent(e.Row);
            element.DataContext = GetMarkObject(dataContext);
        }

        private Style GetHeaderStyle()
        {
            Style style = new System.Windows.Style();
            style.TargetType = typeof(ContentControl);

            StringBuilder tmp = new StringBuilder();
            tmp.Append("<DataTemplate ");
            tmp.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            tmp.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >");
            tmp.Append(string.Format("<CheckBox  Content='Select All' x:Name='{0}' VerticalAlignment='Center' HorizontalAlignment='Center' />", HeaderCheckBoxName));
            tmp.Append("</DataTemplate>");
            DataTemplate contentTemplate = XamlReader.Load(tmp.ToString()) as DataTemplate;

            style.Setters.Add(new Setter(ContentControl.ContentTemplateProperty, contentTemplate));
            return style;
        }

        private DataTemplate GetCellTemplate()
        {
            StringBuilder tmp = new StringBuilder();
            tmp.Append("<DataTemplate ");
            tmp.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            tmp.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' >");
            tmp.Append("<CheckBox IsChecked='{Binding Selected,Mode=TwoWay}' VerticalAlignment='Center' HorizontalAlignment='Center' />");
            tmp.Append("</DataTemplate>");
            return XamlReader.Load(tmp.ToString()) as DataTemplate;
        }

        private MarkObject GetMarkObject(Object obj)
        {
            if (_markObjects.ContainsKey(obj) == false)
            {
                MarkObject markObject;
                markObject = new MarkObject();
                _markObjects.Add(obj, markObject);
            }

            return _markObjects[obj];
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _selectCheckBox = this.GetChild<CheckBox>(HeaderCheckBoxName);
            if (_selectCheckBox == null)
                return;

            _selectCheckBox.Checked += (sender2, e2) => SetAllSelectedStates(true);
            _selectCheckBox.Unchecked += (sender2, e2) => SetAllSelectedStates(false);
        }

        private void SetAllSelectedStates(bool value)
        {
            if (ItemsSource == null)
                return;

            var enu = ItemsSource.GetEnumerator();
            while (enu.MoveNext())
            {
                GetMarkObject(enu.Current).Selected = value;
            }
        }
    }
}
