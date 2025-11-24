using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TIKGenerator.Views.Components.InputField
{
    public enum InputFieldType
    {
        Text,
        Number
    }

    public partial class InputField : UserControl
    {
        public InputFieldType InputType
        {
            get => (InputFieldType)GetValue(InputTypeProperty);
            set => SetValue(InputTypeProperty, value);
        }
        public static readonly DependencyProperty InputTypeProperty =
            DependencyProperty.Register(nameof(InputType), typeof(InputFieldType), typeof(InputField), new PropertyMetadata(InputFieldType.Text));

        public InputField()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(InputField));

        public string TextValue
        {
            get => (string)GetValue(TextValueProperty);
            set => SetValue(TextValueProperty, value);
        }
        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register(nameof(TextValue), typeof(string), typeof(InputField),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextValueChanged));

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(InputField));

        public double? NumberValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TextValue))
                    return null;

                if (double.TryParse(TextValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d;

                return null;
            }
        }

        private static void OnTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (InputField)d;
            ctrl.Validate();
        }

        public Func<double?, string> Rule { get; set; }

        public void Validate()
        {
            if (Rule != null)
                Error = Rule(NumberValue);
            else
                Error = null;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (InputType == InputFieldType.Number)
            {
                Regex regex = new Regex("[^0-9,]");
                e.Handled = regex.IsMatch(e.Text);
            }
            else
            {
                e.Handled = false;
            }
        }
    }

}
