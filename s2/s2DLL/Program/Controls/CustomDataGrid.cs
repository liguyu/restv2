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

namespace Com.Aote.Controls
{
    public class CustomDataGrid :DataGrid
    {
        public CustomDataGrid()
        {
            DefaultStyleKey = typeof(DataGrid);
        }


        #region InputIndex  需要输入的跳转项
        public static DependencyProperty InputIndexProperty = DependencyProperty.RegisterAttached(
           "InputIndex", typeof(string), typeof(CustomDataGrid), new PropertyMetadata(null));
        public static string GetInputIndex(FrameworkElement ui)
        {
            return (string)ui.GetValue(InputIndexProperty);
        }
        public static void SetInputIndex(FrameworkElement ui, string value)
        {
            ui.SetValue(InputIndexProperty, value);
        }
        #endregion


        protected override void OnKeyDown(KeyEventArgs e)
        {
            //配置单元格索引
           string inputIndex =  CustomDataGrid.GetInputIndex(this);
            //是否跳行
            bool isNextRow = true;
            //当前索引行
            int nextIndex = this.CurrentColumn.DisplayIndex;
            if (e.Key.Equals(Key.Tab) || e.Key.Equals(Key.Enter))
            {
                e.Handled = true;
                int currentRow = this.SelectedIndex;
                BaseObjectList ol = (BaseObjectList)this.ItemsSource;
                if (currentRow < ol.Size - 1)
                {
                    e.Handled = true;
                    if (ol[currentRow + 1] != null)
                    {
                        //默认跳转
                        if (inputIndex == null)
                        {
                            GeneralObject go = ol[currentRow + 1];
                            this.SelectedIndex = currentRow + 1;
                            DataGridColumn fe = this.Columns[this.CurrentColumn.DisplayIndex];
                            this.CurrentColumn = fe;
                            this.ScrollIntoView(go, fe);
                            FrameworkElement c = (FrameworkElement)this.CurrentColumn.GetCellContent(go);
                            c.GetType().GetMethod("Focus").Invoke(c, null);
                        }
                        //计算是否跳转下一行何列位置
                        else
                        {
                          
                           string[] ins =  inputIndex.Split(new char[] { '|' });
                           for (int i = 0; i < ins.Length; i++)
                           {
                               int w = int.Parse(ins[i]);
                               if (w == this.CurrentColumn.DisplayIndex)
                               {
                                   //当期索引== 配置的结束索引，下一行，第一个索引
                                   if (i == ins.Length-1)
                                   {
                                       isNextRow = true;
                                       nextIndex = int.Parse(ins[0]);
                                       break;
                                   }
                                   //当期那索引==配置的索引未结束,下一个索引
                                   else
                                   {
                                       isNextRow = false;
                                       nextIndex = int.Parse(ins[i+1]);
                                       break;
                                   }
                               }
                           }
                           GeneralObject go = ol[currentRow];
                           if (isNextRow)
                           {
                               go  = ol[currentRow + 1];
                               this.SelectedIndex = currentRow + 1;
                           }
                           DataGridColumn fe = this.Columns[nextIndex];
                           this.CurrentColumn = fe;
                           this.ScrollIntoView(go, fe);
                           FrameworkElement c = (FrameworkElement)this.CurrentColumn.GetCellContent(go);
                           c.GetType().GetMethod("Focus").Invoke(c, null);
                       
                        }
                    
                    }
                }
            }
            else
            {
                base.OnKeyDown(e);
            }

        }


    }
}
