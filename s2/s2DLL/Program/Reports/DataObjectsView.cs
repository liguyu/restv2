using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using Com.Aote.Marks;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Com.Aote.Reports
{
    /// <summary>
    /// 根据表头和表体变化的数据单元的视图，在报表产生过程中，由程序根据表头、表左部、表体三个数据
    /// 集合生成。
    /// </summary>
    public class DataObjectsView
    {
        //全部数据，单元格所需数据源
        public IEnumerable<object> Objects { get; set; }

        //表左部当前行数据
        public object Row { get; set; }

        //表头的当前列数据
        public object Col { get; set; }

        //利用索引属性的字符串特性，找对象，path可以是一个表达式
        public object this[string path]
        {
            get
            {
                //采用程序解析方式对表达式进行解析，产生判断对象满足条件的代理，
                //利用linq查找对象，对象需要满足的条件由代理判断
                Program prog = new Program(path, null, false);
                Delegate d = prog.Parse(prog.Exp);
                object result = (from p in Objects where IsTrue(d, prog.exp, p) select p).FirstOrDefault();
                return result;
            }
        }

        private bool IsTrue(Delegate d, Expression exp, object p)
        {
            bool result = (bool)d.DynamicInvoke(new object[] { p, Row, Col });
            return result;
        }
    }
}
