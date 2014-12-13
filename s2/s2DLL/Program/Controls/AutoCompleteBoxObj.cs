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

    public class AutoCompleteBoxObj:AutoCompleteBox
    {
        private bool isPopulateComplete = false;
        public bool IsPopulateComplete
        {
            get { return this.isPopulateComplete; }
            set 
            {
                //如果是真的时候，提示完成
                if ((bool)value)
                {
                    ObjectList ol = new ObjectList();
                    foreach (GeneralObject go in Source)
                    {
                        if (Transform != null)
                        {
                            string name = go.GetPropertyValue(Transform) + "";
                            go.SetPropertyValue(this.ValueMemberPath, name, true);
                        }
                        ol.Add(go);   
                    }
                    this.ItemsSource = ol;
                    this.PopulateComplete();
                }
                this.isPopulateComplete = value;
            }
        }
        //数据源
        public BaseObjectList Source
        {
            get;
            set;
        }

        //把列表中的字段名，转化成绑定的字段名
        public string Transform
        {
            get;
            set;
        }

    }
}
