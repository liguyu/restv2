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
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Json;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using System.Windows.Browser;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 查询条件对象，有一个Condition属性，表示最后产生的字符串形式的条件结果，该字符串是按照HQL语句
    /// 要求的形式制作的，为了书写方便，在配置中，小于号用"^"代替了，把条件给最终结果时，注意转换。
    /// 在查询时，如果条件没有发生变化，采用添加空格的方式，迫使条件发生变化，以便每次点击查询按钮都能起作用。
    /// 为了方便配置，如果没有输入查询条件，则条件设置成 1=1 当成条件为真进行处理。
    /// </summary>
    public class SearchObject : CustomTypeHelper, IName
    {
        public string Name { get; set; }

        #region CanSearch 是否查询
        public static readonly DependencyProperty CanSearchProperty =
            DependencyProperty.Register("CanSearch", typeof(bool), typeof(SearchObject),
            new PropertyMetadata(new PropertyChangedCallback(OnCanSearchChanged)));

        private static void OnCanSearchChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            SearchObject go = (SearchObject)dp;
            if (go.CanSearch)
            {
                go.Search();
            }
            go.CanSearch = false;
        }

        public bool CanSearch
        {
            get { return (bool)GetValue(CanSearchProperty); }
            set { SetValue(CanSearchProperty, value); }
        }
        #endregion

        #region 查询条件
        /// <summary>
        /// 最终查询条件，要根据条件进行查询的对象的Path要绑到查询对象的Condition属性上。
        /// </summary>
        private string condition;
        public string Condition 
        {
            get { return condition; }
            set
            {
                if (this.condition != value)
                {
                    this.condition = value;
                    OnPropertyChanged("Condition");
                }
            }
        }
        #endregion


        #region 是否为Or查询条件
        /// <summary>
        /// 最终查询条件，要根据条件进行查询的对象的Path要绑到查询对象的Condition属性上。
        /// </summary>
        private string sign = null;
        public string Sign
        {
            get { return sign; }
            set
            {
                if (this.sign != value)
                {
                    this.sign = value;
                    OnPropertyChanged("Sign");
                }
            }
        }
        #endregion

        /// <summary>
        /// 产生查询条件，把输入的内容，根据配置转换成完整的HQL的条件格式。各输入内容用“and”连接。
        /// 如果条件没有发生变化，通过添加空格的方式迫使其变化。
        /// 如果没有条件，设置条件为 1=1 当做条件为真处理。
        /// </summary>
        public void Search()
        {
            string result = "";
            foreach (KeyValuePair<string, object> kvp in _customPropertyValues)
            {
                //空串不当条件
                if (kvp.Value != null && kvp.Value.ToString().Trim() != "")
                {
                    if (result != "" && sign !=null)
                    {
                        result += "  " + sign + "  ";
                    }
                    else if (result != "")
                    {
                        result += " and ";
                    }
                    result += GetCondition(kvp.Key, kvp.Value);
                    foreach (KeyValuePair<string, object> kvp1 in _customPropertyValues)
                    {
                        result = result.Replace("#" + kvp1.Key + "#", kvp1.Value + "");
                    }
                }
            }
            //如果没有条件，设置条件为1=1，当做条件为真处理
            if (result == "")
            {
                result = "1=1";
            }
            if (sign!=null)
            {
                result ="("+ result+ ")";
            }
            //如果没有发生变化，通过添加空格迫使其发生变化
            if (Condition == result)
            {
                result = result + " ";
            }
            Condition = result;
        }

        /// <summary>
        /// 获取某个输入字段转换出来的条件，如果没有设置，则默认为字段名=输入内容
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns>转换出来的单个条件</returns>
        private string GetCondition(string name, object value)
        {
            //如果没有配置，返回字段名=输入条件
            var p = (from ps in PropertySetters where ps.PropertyName == name && ps.Operator != null select ps).First();
            if(p == null)
            {
                //如果value不是字符串，就不加引号
                if (value is string)

                {
                    value = "'" + value + "'";
                }
                return name + "=" + value;
            }
            //替换操作中的this以及小于号
            string oper = p.Operator.Replace("this", value.ToString()).Replace("^", "<");
            return oper;
        }
    }
}
