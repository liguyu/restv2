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
using System.Windows.Data;
using System.ComponentModel;

namespace Com.Aote.Controls
{
    [TemplatePart(Name = "TextBoxInput", Type = typeof(TextBox))]

    public class EditComboBox : ComboBox
    {
        public TextBox _TextBoxInput = new TextBox();

        private EditComboBox editComboBox;

        public EditComboBox()
        {
            DefaultStyleKey = typeof(EditComboBox);
            editComboBox = this;
        }

        #region Value 附加属性值，用于和对象属性值绑定
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(EditComboBox),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        private static void OnValueChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            string str = "";
            if (args.NewValue != null)
            {
                str = args.NewValue.ToString();
            }
            EditComboBox ec = dp as EditComboBox;
            ec.Value = str;
            ec._TextBoxInput.Text = str;
        }
        #endregion

        #region 关联文本框和下拉框之间的值
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _TextBoxInput = (TextBox)GetTemplateChild("TextBoxInput");
            //选中项变化
            editComboBox.SelectionChanged += (o, e) =>
            {
                if (editComboBox.SelectedItem == null)
                {
                    editComboBox._TextBoxInput.Text = "";
                }
                else if (editComboBox.SelectedItem is GeneralObject)
                {
                    editComboBox._TextBoxInput.Text = editComboBox.SelectedValue + "";
                }
                else
                {
                    editComboBox._TextBoxInput.Text = (editComboBox.SelectedItem as ComboBoxItem).Content + "";
                }
            };
            //屏蔽Enter键
            editComboBox._TextBoxInput.KeyDown += (o, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            };
            //文本内容变化
            editComboBox._TextBoxInput.TextChanged += (o, e) =>
            {
                editComboBox.Value = editComboBox._TextBoxInput.Text;
            };

            editComboBox._TextBoxInput.AddHandler(TextBox.MouseLeftButtonDownEvent,
                new MouseButtonEventHandler(_TextBoxInput_MouseLeftButtonDown), true);

            if (editComboBox.SelectedItem == null)
            {
                editComboBox._TextBoxInput.Text = "";
            }
            else if (editComboBox.SelectedItem is GeneralObject)
            {
                editComboBox._TextBoxInput.Text = editComboBox.SelectedValue + "";
            }
            else
            {
                editComboBox._TextBoxInput.Text = (editComboBox.SelectedItem as ComboBoxItem).Content + "";
            }
        }

        void _TextBoxInput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            editComboBox._TextBoxInput.SelectAll();
        }
#endregion
    }
}
