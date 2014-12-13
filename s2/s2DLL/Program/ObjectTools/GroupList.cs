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
using System.Collections.Generic;

namespace Com.Aote.ObjectTools
{
    /// <summary>
    /// 分组列表，可进行多级分组，根据当前分组情况去后台分页加载数据。可支持流式数据获取。
    /// </summary>
    public class GroupList : PagedList
    {
        #region Sums 汇总字段
        private string sums;
        public string Sums
        {
            get
            {
                return this.sums;
            }
            set
            {
                if (this.sums == value)
                {
                    return;
                }
                this.sums = value;
            }
        }
        #endregion

        #region Groups 多级分组表达式，分组之间用“,”分隔
        private string[] groupNames;

        private string groups;
        public string Groups
        {
            get
            {
                return this.groups;
            }
            set
            {
                if (this.groups == value)
                {
                    return;
                }
                this.groups = value;
                this.groupNames = this.groups.Split(',');
            }
        }
        #endregion

        #region Level 当前展开的级别
        private int level;
        public int Level
        {
            get
            {
                return this.level;
            }
            set
            {
                if (this.level == value)
                {
                    return;
                }
                this.level = value;
            }
        }
        #endregion

        #region PageHQL 获取一页数据的HQL语句
        private List<string> selectedValues = new List<string>();

        public override string PageHQL
        {
            get
            {
                if (this.HQL == null)
                {
                    return null;
                }

                //根据当前级别，产生select部分，包含汇总
                string select = "";
                if (this.level != this.groupNames.Length)
                {
                    select = select + this.groupNames[this.level];
                }
                select = select + "," + this.Sums;
                //根据当前级别，产生group by部分
                string groupby = "";
                if (this.level != this.groupNames.Length)
                {
                    groupby = " group by " + this.groupNames[this.level];
                }
                //根据当前级别，以及前面各级别的选择内容，产生where部分
                string where = "";
                for (int i = 0; i < selectedValues.Count; i++)
                {
                    if (where == "")
                    {
                        where = " where " + this.groupNames[i] + "='" + this.selectedValues[i] + "'";
                    }
                    else
                    {
                        where = where + " and " + this.groupNames[i] + "='" + this.selectedValues[i] + "'";
                    }
                }
                //根据当前级别，组织HQL语句
                string hql = "select " + select + " from (" + this.HQL + ") p " + where + groupby;
                return hql;
            }
        }
        #endregion

        #region CountHQL 产生计算总数的hql语句
        public override string CountHQL
        {
            get
            {
                return PageHQL;
            }
        }
        #endregion

        #region ReturnNames 分组语句执行后，返回的字段名称
        public override string ReturnNames
        {
            get
            {
                return Names;
            }
        }
        #endregion

        #region OpenOrClose 对象原来打开，则关闭。原来关闭，则打开
        private List<GeneralObject> opened = new List<GeneralObject>();
        public void OpenOrClose_(GeneralObject go, string name)
        {
            // 在打开列表里，为打开状态，否则，为关闭状态
            int index = opened.IndexOf(go);
            // 没打开
            if (index == -1)
            {
                // 把该项内容加载到打开列表里
                opened.Add(go);
                // 把当前选中项的属性值，放到selectedValue中，以便产生SQL语句时用
                string value = go.GetPropertyValue(name).ToString();
                this.selectedValues.Add(value);
                // 改变级别
                this.Level = opened.Count;
                // 加载数据，加载以后，要把前几个级别的数据添加进去
                this.DataLoaded += new System.ComponentModel.AsyncCompletedEventHandler(GroupList_DataLoaded);
                this.Load();
            }
        }

        //加载以后，要把前几个级别的数据添加进去
        void GroupList_DataLoaded(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DataLoaded -= GroupList_DataLoaded;
            for (int i = opened.Count - 1; i >= 0; i--)
            {
                this.Insert(0, opened[i]);
            }
        }
        #endregion
    }
}
