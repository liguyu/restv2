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
using System.ComponentModel;
using Com.Aote.Logs;

namespace Com.Aote.Marks
{
    /// <summary>
    /// Bingding的辅助类，每个绑定都绑到这个对象的Value属性上。
    /// </summary>
    public class BindingSlave : DependencyObject, INotifyPropertyChanged
    {
        private static Log Log = Log.GetInstance("Com.Aote.Bindings.BindingSlave");

        #region Result 所对应的编译结果，专用于表达式解析
        public Delegate Result { get; set; }
        #endregion

        #region Value 辅助类的值，绑定将绑到这个值上

        /// <summary>
        /// 依赖属性，以便绑定可以绑到这个属性上。
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(BindingSlave),
                new PropertyMetadata(null, OnValueChanged));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// 当属性发生变化时，发送Value改变事件，以便多绑定可以监听这个属性的变化。
        /// </summary>
        /// <param name="depObj">BindingSlave本身</param>
        /// <param name="e">属性变化参数</param>
        private static void OnValueChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BindingSlave slave = depObj as BindingSlave;
            Log.Debug("绑定slave值变了，值为:" + (slave.Value == null ? "null" : slave.Value));
            slave.OnPropertyChanged("Value");
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
