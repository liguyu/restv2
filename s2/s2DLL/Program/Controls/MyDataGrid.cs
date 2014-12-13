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
using System.Collections;
using System.Collections.Generic;

namespace Com.Aote.Controls
{
    public class MyDataGrid : DataGrid
    {
        public event EventHandler MySelectionChanged;
        public void OnMySelectionChanged()
        {
            if (MySelectionChanged != null)
            {
                MySelectionChanged(this, new EventArgs());
            }
        }

        public MyDataGrid()
            : base()
        {
            this.SelectionChanged += new SelectionChangedEventHandler(MyDataGrid_SelectionChanged);
        }

        List<object> list = new List<object>();
        void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyDataGrid dg = (MyDataGrid)sender;
            list = new List<object>();
            IEnumerator ie = dg.ItemsSource.GetEnumerator();
            while (ie.MoveNext())
            {
                list.Add(ie.Current);
                if (ie.Current == dg.SelectedItem)
                {
                    break;
                }
            }
            OnMySelectionChanged();
        }

        public IList MySelectedItems
        {
            get { return list; }
        }
    }
}
