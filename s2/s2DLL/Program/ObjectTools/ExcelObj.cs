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
using System.Runtime.InteropServices.Automation;
using System.Reflection;
using System.Threading;

namespace Com.Aote.ObjectTools
{
    public class ExcelObj : CustomTypeHelper,IAsyncObject
    {
        //DataGrid资源
        public DataGrid Source { get; set; }
        //导出的字段名,用逗号隔开，配置的顺序就是导出显示的顺序
        public string Bind { set; get; }

        private int rowIndex;
        private int coulmnIndex;

        int index;

        public void Export()
        {
            IsBusy = true;
            rowIndex = 1;
            coulmnIndex = 1;
            index = 0;
            try
            {
                dynamic excel = AutomationFactory.CreateObject("Excel.Application");
                excel.workbooks.Add();
                dynamic sheet = excel.ActiveSheet;
                //加载Excel表头数据
                for (int i = 0; i < Source.Columns.Count; ++i)
                {
                    dynamic headerCell = sheet.Cells[rowIndex, coulmnIndex + i];
                    //把当前Grid中表头信息赋值给Excel表头.
                    headerCell.Value = Source.Columns[i].Header;
                    headerCell.Font.Bold = true;//加粗
                    //headerCell.Interior.Color = 0xFF00;//设置背景颜色
                }
                char[] c = new char[] {','};
                string[] objs = Bind.Split(c);
                PagedObjectList pol = (PagedObjectList)Source.ItemsSource;
                pol.DataLoaded += (o, e) =>
                {
                    //加载展示数据
                    Load(objs, sheet, pol, excel);
                };
                //加载第一页
                pol.PageIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating excel: " + ex.Message);
            }
        }


        //加载展示数据
        private void Load(string[] objs, dynamic sheet, PagedObjectList pol, dynamic excel)
        {
            foreach (GeneralObject item in pol)
            {
                rowIndex++;
                index++;
                for (int i = 0; i < objs.Length; i++)
                {
                    dynamic cellFirstName = sheet.Cells[rowIndex, i + 1];
                    object value = item.GetPropertyValue(objs[i]);
                    //导出序号
                    if (objs[i].Equals("Index"))
                    {
                        value = index;
                    }
                    //设置单元格格式为文本
                    cellFirstName.NumberFormatLocal = "@";
                    cellFirstName.Value = value;
                    cellFirstName.Font.Color = 003399;
                }
            }
            //翻页
            int page = pol.Count / pol.PageSize;
            if (pol.Count % pol.PageSize > 0)
            {
                page++;
            }
            if (page - 1 > pol.PageIndex)
            {
                pol.PageIndex++;
            }
            else
            {
                excel.Visible = true;
                IsBusy = false;
                MessageBox.Show("导出成功Excel中!");
            }
        }

        /// <summary>
        /// 是否正忙于工作
        /// </summary>
        public bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public State State
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        public event System.ComponentModel.AsyncCompletedEventHandler Completed;

        public void OnCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            
        }

        public string Name
        {
            get;
            set;
        }
    }
}
