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
using System.Windows.Printing;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
 

namespace Com.Aote.Controls
{
    /**
    * 多页打印对象
    */
    public class PrintPageObj : FrameworkElement, INotifyPropertyChanged
    {
        //打印区域
        private ObjectList items = new ObjectList();
        private GeneralObject go = new GeneralObject();

        #region PropertyChanged事件
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
        #endregion

        //区域中的列表明子
        public string listNameInArea;
        public string ListNameInArea
        {
            set
            {
                this.listNameInArea = value;
            }
            get
            {
                return this.listNameInArea;
            }
        }
        private UIElement area;
        public UIElement Area 
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                this.area = value;
                if (this.ListNameInArea != null)
                {
                    FrameworkElement fe = (FrameworkElement)this.FindName(this.ListNameInArea);
                    fe.GetType().GetProperty("ItemsSource").SetValue(fe, items, null);
                }
                else
                {
                    Area.GetType().GetProperty("ItemsSource").SetValue(Area, items, null);
                }
                
            }
            get
            {
                return this.area;
            }
        }


        private UIElement dataarea;
        public UIElement DataArea
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                this.dataarea = value;
                DataArea.GetType().GetProperty("DataContext").SetValue(DataArea, go, null);
            }
            get
            {
                return this.dataarea;
            }
        }

        //每页打印行数
        private int pageRow = 0;
        public int PageRow
        {
            set
            {
                this.pageRow = value;
            }
            get
            {
                return this.pageRow;
            }
        }

        //总页数
        public int Count { set; get; }

        //当前页数
        private int pageIndex = -1;
        public int PageIndex
        {
            set
            {
                pageIndex = value;
                OnPropertyChanged("PageIndex");
            }
            get
            {
                return this.pageIndex;
            }
        }

        
        //对应的List
        private BaseObjectList list;
        public BaseObjectList List
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                list = value;
            }
            get
            {
                return list;
            }
        }

        //是否翻页打印，用于列表不是翻页对象时，需要翻页打印
        public bool IsPrintPage { get; set; }

        //打印
        public void Print()
        {
            PageIndex = -1;
            Count = (List.Count % PageRow == 0) ? (List.Count / PageRow) : (List.Count / PageRow) + 1;
            if (Count == 0)
            {
                return;
            }
            if (IsPrintPage)
            {
                PagedObjectList pdl = new PagedObjectList();
                pdl.WebClientInfo = List.WebClientInfo;
                pdl.Path = List.Path;
                pdl.Count = List.Count;
                pdl.PageSize = PageRow;
                List = pdl;
            }
            int c = list.Count;
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (o, e) =>
            {
                PrintDocument pd1 = (PrintDocument)o;
                if (pd1.PrintedPageCount - 1 != PageIndex)
                {
                    return;
                }
                if (List is BasePagedList)
                {
                    PageIndex++;
                    BasePagedList pol = (BasePagedList)List;
                    pol.DataLoaded += (o1,e1) =>
                    {
                        //加载展示数据
                        items.CopyFrom(List, 0, this.pageRow);
                        //StackPanel panel = (Area as FrameworkElement).FindName("pageNoPanel") as StackPanel;
                        //// if (panel.Tag.Equals(""))
                        //// {
                        ////     return;
                        //// }
                        ////else 
                        //if (panel.Tag.Equals("showPageNo"))
                        //{
                        //    (panel.FindName("pageNo") as TextBlock).Text = "第" + (PageIndex + 1) + "页" + "共" + Count+"页";
                        //}
                            e.PageVisual = Area;
                            area.UpdateLayout();
                        
                        //打印完成，重置索引
                        if (PageIndex == Count-1)
                        {
                            //(panel.FindName("pageNo") as TextBlock).Text = "";
                            e.HasMorePages = false;
                        }
                        else
                        {
                            e.HasMorePages = true;
                        }
                    };
                    pol.PageIndex = PageIndex;
                }
                else
                {
                    //计算获取的数据开始，截止行
                    pageIndex = 0;
                    PageIndex++;
                    int startRow = (PageIndex - 1) * pageRow;
                    int endRow = (PageIndex * pageRow) - 1;
                    endRow = endRow > (List.Count - 1) ? (List.Count - 1) : (PageIndex * pageRow) - 1;
                    items.CopyFrom(List, startRow, endRow);
                    e.PageVisual = Area;
                    area.UpdateLayout();
                    //打印完成，重置索引
                    if (PageIndex == Count)
                    {
                        e.HasMorePages = false;
                        PageIndex = 0;
                    }
                    else
                    {
                        e.HasMorePages = true;
                    }
                }
            };
            pd.Print("");
       }



        //打印
        public void PrintD()
        {
            PageIndex = -1;
            Count = (List.Count % PageRow == 0) ? (List.Count / PageRow) : (List.Count / PageRow) + 1;
            if (Count == 0)
            {
                return;
            }
            int c = list.Count;
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (o, e) =>
            {
                PrintDocument pd1 = (PrintDocument)o;
                if (pd1.PrintedPageCount-1 != PageIndex)
                {
                    return;
                }
                if (List is PagedObjectList)
                {
                    PageIndex++;
                    PagedObjectList pol = (PagedObjectList)List;
                    pol.DataLoaded += (o1, e1) =>
                    {
                        //加载展示数据
                        go.CopyDataFrom(List[0]);
                        e.PageVisual = DataArea;
                        DataArea.UpdateLayout();
                        //打印完成，重置索引
                        if (PageIndex == Count-1)
                        {
                            e.HasMorePages = false;
                        }
                        else
                        {
                            e.HasMorePages = true;
                        }
                    };
                    pol.PageIndex = PageIndex;
                }
            };
            pd.Print("");
        }

    }
}
