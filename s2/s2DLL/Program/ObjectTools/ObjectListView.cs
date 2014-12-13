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
using System.Windows.Data;
using Com.Aote.Utils;

namespace Com.Aote.ObjectTools
{
    // CollecionViewSource无法放到我们所定义的资源下面，对其进行包装，以便可以采用我们的机制访问。
    public class ObjectListView : CollectionViewSource, IInitable, IName
    {
        #region GroupProperties 逗号分隔的属性分组字符串
        public static readonly DependencyProperty GroupPropertiesProperty =
           DependencyProperty.Register("GroupProperties", typeof(string), typeof(ObjectListView),
           new PropertyMetadata(new PropertyChangedCallback(OnGroupPropertiesChanged)));

        private static void OnGroupPropertiesChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            ObjectListView go = (ObjectListView)dp;
            go.GroupPropertiesChanged();
        }

        private void GroupPropertiesChanged()
        {
            GroupDescriptions.Clear();
            if(GroupProperties == null)
            {
                return;
            }
            foreach (string property in GroupProperties.Split(','))
            {
                GroupDescriptions.Add(new PropertyGroupDescription(property));
            }
        }

        public string GroupProperties
        {
            get { return (string)GetValue(GroupPropertiesProperty); }
            set { SetValue(GroupPropertiesProperty, value); }
        }
       
        #endregion

        #region IName Members
        public string Name { get; set; }
        #endregion

        #region IInitable Members
        private object UI;

        public static readonly DependencyProperty IsInitedProperty =
            DependencyProperty.Register("IsInited", typeof(bool), typeof(ObjectListView),
            new PropertyMetadata(false));

        public bool IsInited
        {
            get { return (bool)GetValue(IsInitedProperty); }
            set { SetValue(IsInitedProperty, value); }
        }

        public void Init(object ui)
        {
            UI = ui;
            OnLoaded();
            IsInited = true;
            OnInitFinished();
        }

        public object FindResource(string name)
        {
            if (name == "this")
            {
                return this;
            }
            return UI.FindResource(name);
        }

        public event RoutedEventHandler Loaded;
        private void OnLoaded()
        {
            if (Loaded != null)
            {
                Loaded(UI, new RoutedEventArgs());
            }
        }

        public event RoutedEventHandler InitFinished;
        private void OnInitFinished()
        {
            if (this.InitFinished != null)
            {
                InitFinished(this, null);
            }
        }
        
        #endregion
    }
}
