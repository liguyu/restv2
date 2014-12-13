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
using System.Xaml;
using System.Windows.Data;
using System.Windows.Markup;
using Com.Aote.ObjectTools;

namespace Com.Aote.Marks
{
    //新的binding标记，默认值转换成大家常用的类型
    public class Bind : FrameworkElement, IMarkupExtension<object>
    {
        public object Source { get; set; }
        public string Path { get; set; }
        public object parameter;
        public object value;
     

        private BindingMode mode = BindingMode.TwoWay;
        public BindingMode Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                }
            }
        }

        #region UpdateOnChange 是否文本改变时，触发绑定
        private bool change = false;
        public bool UpdateOnChange
        {
            get { return change; }
            set
            {
                if (change != value)
                {
                    change = value;
                }
            }
        }
        #endregion

        //string  过滤字符串，在该字符串里的字符全部过滤掉
        #region
        private  string filter;
        public string Filter
        {

            get { return filter; }
            set
            {
                if (filter != value)
                {
                    filter = value;
                }
            }
        }
        #endregion



        #region IMarkupExtension<object> Members

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var target =
               (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            //没有source，默认为数据上下文
            if (Source == null && target is FrameworkElement)
            {
                Source = (target as FrameworkElement).DataContext;
            }
            Binding b = new Binding();
            b.Source = Source;
            if (Path != null) b.Path = new PropertyPath(Path);
            //模式默认为双向
            b.Mode = Mode;
            //默认转换器为空串转空值转换器
            b.Converter = new EmptyStringConverter();
            b.ConverterParameter = filter;
            b.ValidatesOnExceptions = true;
            b.ValidatesOnNotifyDataErrors = true;
            b.ValidatesOnDataErrors = true;
            b.NotifyOnValidationError = true;

            //如果目标对象是TextBox，且设置了，当TextBox的Text发生改变时，触发绑定
            if ((target.TargetObject is TextBox) && change)
            {
                TextBox txt = target.TargetObject as TextBox;
                txt.TextChanged += (o, e) =>
                {                
                    TextBox text = o as TextBox;
                    var bindingExpression = text.GetBindingExpression(TextBox.TextProperty);
                    if (filter != null && text.Text != null)
                    {
                        string result = (string)text.Text;
                        //用每一个过滤字符替换value中的内容为空串
                        foreach (char ch in filter)
                        {
                            result = result.Replace("" + ch, "");
                        }
                        text.Text = result;
                    }
                    if (bindingExpression != null)
                    {
                        bindingExpression.UpdateSource();
                    }
                };

            }
            //如果目标对象是PasswordBox，且设置了，当PasswordBox的Password发生改变时，触发绑定
            if ((target.TargetObject is PasswordBox) && change)
            {
                PasswordBox txt = target.TargetObject as PasswordBox;
                txt.PasswordChanged += (o, e) =>
                {
                    PasswordBox text = o as PasswordBox;
                    var bindingExpression = text.GetBindingExpression(PasswordBox.PasswordProperty);
                    if (filter != null && text.Password != null)
                    {
                        string result = (string)text.Password;
                        //用每一个过滤字符替换value中的内容为空串
                        foreach (char ch in filter)
                        {
                            result = result.Replace("" + ch, "");
                        }
                        text.Password = result;
                    }
                    if (bindingExpression != null)
                    {
                        bindingExpression.UpdateSource();
                    }
                };

            }

            //注册错误通知事件,绑定发生错误(例如类型错误)，通知对象
            FrameworkElement fe = (FrameworkElement)target.TargetObject;
            fe.BindingValidationError += (o, e) =>
            {
                if (e.Action.Equals(System.Windows.Controls.ValidationErrorEventAction.Added))
                {

                    CustomTypeHelper ct = (CustomTypeHelper)fe.DataContext;
                    if (ct != null)
                    {
                        ct.OnError(this.Path, e.Error.ErrorContent.ToString());
                    }

                }

            };
            return b;
        }

        #endregion
    }
}
