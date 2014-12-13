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
using System.Windows.Automation.Peers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

namespace Com.Aote.ObjectTools
{
    public class DataGridSelectionAdapter : DataGrid, ISelectionAdapter
    {

        private bool IgnoreAnySelection { get; set; }

        private bool IgnoringSelectionChanged { get; set; }

        public new event SelectionChangedEventHandler SelectionChanged;

        public event RoutedEventHandler Commit;

        public event RoutedEventHandler Cancel;



        public DataGridSelectionAdapter()
        {

            base.SelectionChanged += OnSelectionChanged;

            MouseLeftButtonUp += OnSelectorMouseLeftButtonUp;

        }



        public new object SelectedItem
        {

            get
            {

                return base.SelectedItem;

            }

            set
            {

                IgnoringSelectionChanged = true;

                base.SelectedItem = value;

                IgnoringSelectionChanged = false;

            }

        }



        private void OnSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            IgnoreAnySelection = false;



            OnSelectionChanged(this, null);

            OnCommit(this, new RoutedEventArgs());

        }



        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (IgnoringSelectionChanged)
            {

                return;

            }



            if (IgnoreAnySelection)
            {

                return;

            }



            SelectionChangedEventHandler handler = this.SelectionChanged;

            if (handler != null)
            {

                handler(sender, e);

            }

        }



        public new IEnumerable ItemsSource
        {

            get { return base.ItemsSource; }



            set
            {

                if (base.ItemsSource != null)
                {

                    INotifyCollectionChanged notify = base.ItemsSource as INotifyCollectionChanged;

                    if (notify != null)
                    {

                        notify.CollectionChanged -= OnCollectionChanged;

                    }

                }



                base.ItemsSource = value;



                if (base.ItemsSource != null)
                {

                    INotifyCollectionChanged notify = base.ItemsSource as INotifyCollectionChanged;

                    if (notify != null)
                    {

                        notify.CollectionChanged += OnCollectionChanged;

                    }

                }

            }

        }



        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            IgnoreAnySelection = true;

        }



        private ObservableCollection<object> Items
        {

            get { return ItemsSource as ObservableCollection<object>; }

        }



        private void SelectedIndexIncrement()
        {

            SelectedIndex = SelectedIndex + 1 >= Items.Count ? -1 : SelectedIndex + 1;

            ScrollIntoView(SelectedItem, this.Columns[0]);

        }



        private void SelectedIndexDecrement()
        {

            int index = SelectedIndex;

            if (index >= 0)
            {

                SelectedIndex--;

            }

            else if (index == -1)
            {

                SelectedIndex = Items.Count - 1;

            }



            ScrollIntoView(SelectedItem, this.Columns[0]);

        }



        public void HandleKeyDown(KeyEventArgs e)
        {

            switch (e.Key)
            {

                case Key.Enter:

                    OnCommit(this, e);

                    e.Handled = true;

                    break;



                case Key.Up:

                    IgnoreAnySelection = false;

                    SelectedIndexDecrement();

                    e.Handled = true;

                    break;



                case Key.Down:

                    if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                    {

                        IgnoreAnySelection = false;

                        SelectedIndexIncrement();

                        e.Handled = true;

                    }

                    break;



                case Key.Escape:

                    OnCancel(this, e);

                    e.Handled = true;

                    break;



                default:

                    break;

            }

        }



        private void OnCommit(object sender, RoutedEventArgs e)
        {

            RoutedEventHandler handler = Commit;

            if (handler != null)
            {

                handler(sender, e);

            }



            AfterAdapterAction();

        }



        private void OnCancel(object sender, RoutedEventArgs e)
        {

            RoutedEventHandler handler = Cancel;

            if (handler != null)
            {

                handler(sender, e);

            }



            AfterAdapterAction();

        }



        private void AfterAdapterAction()
        {

            IgnoringSelectionChanged = true;

            SelectedItem = null;

            SelectedIndex = -1;

            IgnoringSelectionChanged = false;



            // Reset, to ignore any future changes

            IgnoreAnySelection = true;

        }



        public AutomationPeer CreateAutomationPeer()
        {

            return new DataGridAutomationPeer(this);

        }

    }


}
