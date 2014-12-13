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
using System.Collections.Specialized;

namespace Com.Aote.Attachs
{
    /// <summary>
    /// 给TabControl附加属性，以便其可以绑定到数据源。原来的TabControl不支持绑定到数据源。
    /// </summary>
    public class TabControlAttach
    {
        #region ItemsSource 扩展ItemsSource，原来的不用了，因为原来的无法进行绑定

        /// <summary>
        /// ItemsSource附加属性，附加该属性后，TabControl就可以支持数据源绑定了。
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
            "ItemsSource", typeof(IList), typeof(TabControl), new PropertyMetadata(OnItemsSourceChanged));
 
        /// <summary>
        /// 当源发生改变时，获得源中的数据，产生TabItem，设置TabItem的DataContent为源中的数据。
        /// TabItem的Header为Header模板转换结果，TabItem的Content为内容模板转换结果。
        /// 要注意的是，源是一个集合，所以必须监听集合中数据的变化，而不能只看集合本身。
        /// </summary>
        /// <param name="d">附加到的TabControl</param>
        /// <param name="e">属性变化事件</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as TabControl;
            var items = e.NewValue as IEnumerable;
            if (items != null)
            {
                //开始设置时获取所有数据
                source.Items.Clear();
                AddItems(source, items);
                //监听集合变化，在集合发生变化时，获取变化了的数据
                INotifyCollectionChanged c = (INotifyCollectionChanged)items;
                c.CollectionChanged += (o, a) =>
                {
                    //把所有新增项添加到TabControl中
                    if (a.NewItems != null)
                    {
                        AddItems(source, a.NewItems);
                    }
                    //删除所有移除的项目
                    if (a.OldItems != null)
                    {
                        DeleteItems(source, a.OldStartingIndex);
                    }
                };
            }
        }
        
        /// <summary>
        /// 根据给定的列表数据，往TabControl中添加项目
        /// </summary>
        /// <param name="source">TabControl</param>
        /// <param name="items">列表数据</param>
        private static void AddItems(TabControl source, IEnumerable items)
        {
            var headerTemplate = GetHeaderTemplate(source);
            var contentTemplate = GetContentTempalte(source);
            foreach (var item in items)
            {
                var tabItem = new TabItem
                {
                    DataContext = item,
                    //头与内容都是经过模板转换后的结果
                    Header = headerTemplate.LoadContent(),
                    Content = contentTemplate.LoadContent(),
                };
                source.Items.Add(tabItem);
            }
        }

        /// <summary>
        /// 根据给定的列表数据，从TabControl的Items中删除数据
        /// </summary>
        /// <param name="tab">TabControl</param>
        /// <param name="items">要删除的数据</param>
        private static void DeleteItems(TabControl tab, int index)
        {
            tab.Items.RemoveAt(index);
        }

        public static IList GetItemsSource(DependencyObject d)
        {
            return (IList)d.GetValue(ItemsSourceProperty);
        }
        public static void SetItemsSource(DependencyObject d, IList value)
        {
            d.SetValue(ItemsSourceProperty, value);
        }
        #endregion

        #region HeaderTemplate TabItem的header模板

        /// <summary>
        /// 头模板，TabControl的数据源将利用该模板显示TabControl的头
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(TabControl),
            new PropertyMetadata(OnHeaderTemplateChanged));

        /// <summary>
        /// 当头模板改变时，先将TabControl中已有项的头模板换掉
        /// </summary>
        /// <param name="d">TabControl</param>
        /// <param name="e">属性改变事件参数</param>
        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as TabControl;
            var template = e.NewValue as DataTemplate;

            foreach (TabItem item in source.Items)
            {
                item.HeaderTemplate = template;
            }
        }

        public static DataTemplate GetHeaderTemplate(DependencyObject d)
        {
            return (DataTemplate)d.GetValue(HeaderTemplateProperty);
        }
        public static void SetHeaderTemplate(DependencyObject d, DataTemplate value)
        {
            d.SetValue(HeaderTemplateProperty, value);
        }
        #endregion

        #region ContentTemplate 内容模板，用于确定显示内容

        /// <summary>
        /// 内容模板，用于确定显示内容。
        /// </summary>
        public static readonly DependencyProperty ContentTempalteProperty =
            DependencyProperty.Register("ContentTempalte", typeof(DataTemplate), typeof(TabControl),
            new PropertyMetadata(OnContentTempalteChanged));

        /// <summary>
        /// 内容模板发生变化是，把所有已有项的内容模板换成新模板
        /// </summary>
        /// <param name="d">TabControl</param>
        /// <param name="e">包含新旧属性</param>
        private static void OnContentTempalteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as TabControl;
            var template = e.NewValue as DataTemplate;

            foreach (TabItem item in source.Items)
            {
                item.ContentTemplate = template;
            }
        }

        public static DataTemplate GetContentTempalte(DependencyObject d)
        {
            return (DataTemplate)d.GetValue(ContentTempalteProperty);
        }
        public static void SetContentTempalte(DependencyObject d, DataTemplate value)
        {
            d.SetValue(ContentTempalteProperty, value);
        }
        #endregion
    }
}


