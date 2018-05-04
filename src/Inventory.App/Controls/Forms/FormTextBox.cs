﻿using System;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Inventory.Controls
{
    public interface IFormControl
    {
        event EventHandler<TextVisualState> VisualStateChanged;

        TextEditMode2 Mode { get; }
        TextVisualState VisualState { get; }

        bool IsEnabled { get; }

        bool Focus(FocusState value);

        void SetVisualState(TextVisualState visualState);
    }

    public enum TextDataType
    {
        String,
        Integer,
        Decimal,
        Double
    }

    public enum TextEditMode2
    {
        Auto,
        ReadWrite
    }

    public enum TextVisualState
    {
        Idle,
        Ready,
        Focused
    }

    public class FormTextBox : TextBox, IFormControl
    {
        public event EventHandler<TextVisualState> VisualStateChanged;

        private Border _borderElement = null;
        private Control _contentElement = null;
        private Border _displayContent = null;

        private bool _isInitialized = false;

        public FormTextBox()
        {
            DefaultStyleKey = typeof(FormTextBox);
            RegisterPropertyChangedCallback(TextProperty, OnTextChanged);
            BeforeTextChanging += OnBeforeTextChanging;
        }

        public TextVisualState VisualState { get; private set; }

        #region DataType
        public TextDataType DataType
        {
            get { return (TextDataType)GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }

        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(nameof(DataType), typeof(TextDataType), typeof(FormTextBox), new PropertyMetadata(TextDataType.String, OnPropertyChanged));
        #endregion

        #region Format
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(FormTextBox), new PropertyMetadata(null, OnPropertyChanged));
        #endregion

        #region FormattedText
        public string FormattedText
        {
            get { return (string)GetValue(FormattedTextProperty); }
            set { SetValue(FormattedTextProperty, value); }
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register(nameof(FormattedText), typeof(string), typeof(FormTextBox), new PropertyMetadata(null, OnPropertyChanged));
        #endregion

        #region Mode*
        public TextEditMode2 Mode
        {
            get { return (TextEditMode2)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FormTextBox;
            control.UpdateMode();
            control.UpdateVisualState();
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(TextEditMode2), typeof(FormTextBox), new PropertyMetadata(TextEditMode2.Auto, ModeChanged));
        #endregion

        protected override void OnApplyTemplate()
        {
            _borderElement = base.GetTemplateChild("BorderElement") as Border;
            _contentElement = base.GetTemplateChild("ContentElement") as Control;
            _displayContent = base.GetTemplateChild("DisplayContent") as Border;

            _isInitialized = true;

            UpdateMode();
            UpdateVisualState();

            base.OnApplyTemplate();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FormTextBox;
            control.ApplyTextFormat();
        }

        private void OnTextChanged(DependencyObject sender, DependencyProperty dp)
        {
            ApplyTextFormat();
        }

        private void ApplyTextFormat()
        {
            switch (DataType)
            {
                case TextDataType.Integer:
                    Int64.TryParse(Text, out Int64 n);
                    FormattedText = n.ToString(Format);
                    break;
                case TextDataType.Decimal:
                    Decimal.TryParse(Text, out decimal m);
                    FormattedText = m.ToString(Format);
                    break;
                case TextDataType.Double:
                    Double.TryParse(Text, out double d);
                    FormattedText = d.ToString(Format);
                    break;
                case TextDataType.String:
                default:
                    FormattedText = Text;
                    break;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            switch (DataType)
            {
                case TextDataType.Integer:
                    Int64.TryParse(Text, out Int64 n);
                    Text = n == 0 ? "" : n.ToString();
                    break;
                case TextDataType.Decimal:
                    Decimal.TryParse(Text, out decimal m);
                    Text = m == 0 ? "" : m.ToString();
                    break;
                case TextDataType.Double:
                    Double.TryParse(Text, out double d);
                    Text = d == 0 ? "" : d.ToString();
                    break;
                case TextDataType.String:
                default:
                    break;
            }

            if (Mode == TextEditMode2.Auto)
            {
                SetVisualState(TextVisualState.Focused);
            }

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (VisualState == TextVisualState.Focused)
            {
                SetVisualState(TextVisualState.Ready);
            }

            switch (DataType)
            {
                case TextDataType.Integer:
                    if (!Int64.TryParse(Text, out Int64 n))
                    {
                        Text = "0";
                    }
                    break;
                case TextDataType.Decimal:
                    if (!Decimal.TryParse(Text, out decimal m))
                    {
                        Text = "0";
                    }
                    break;
                case TextDataType.Double:
                    if (!Double.TryParse(Text, out double d))
                    {
                        Text = "0";
                    }
                    break;
                case TextDataType.String:
                default:
                    break;
            }

            base.OnLostFocus(e);
        }

        private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string str = args.NewText;
            if (String.IsNullOrEmpty(str) || str == "-")
            {
                return;
            }

            switch (DataType)
            {
                case TextDataType.Integer:
                    args.Cancel = !Int64.TryParse(str, out Int64 n);
                    break;
                case TextDataType.Decimal:
                    args.Cancel = !Decimal.TryParse(str, out decimal m);
                    break;
                case TextDataType.Double:
                    args.Cancel = !Double.TryParse(str, out double d);
                    break;
            }
        }

        private void UpdateMode()
        {
            switch (Mode)
            {
                case TextEditMode2.Auto:
                    VisualState = TextVisualState.Idle;
                    break;
                case TextEditMode2.ReadWrite:
                    VisualState = TextVisualState.Ready;
                    break;
            }
        }

        public void SetVisualState(TextVisualState visualState)
        {
            if (visualState != VisualState)
            {
                VisualState = visualState;
                UpdateVisualState();
                VisualStateChanged?.Invoke(this, visualState);
            }
        }

        private void UpdateVisualState()
        {
            if (_isInitialized)
            {
                switch (VisualState)
                {
                    case TextVisualState.Idle:
                        _borderElement.Opacity = 0.5;
                        _contentElement.Visibility = Visibility.Collapsed;
                        _displayContent.Background = TransparentBrush;
                        _displayContent.Visibility = Visibility.Visible;
                        break;
                    case TextVisualState.Ready:
                        _borderElement.Opacity = 1.0;
                        _contentElement.Visibility = Visibility.Collapsed;
                        _displayContent.Background = OpaqueBrush;
                        _displayContent.Visibility = Visibility.Visible;
                        break;
                    case TextVisualState.Focused:
                        _borderElement.Opacity = 1.0;
                        _contentElement.Visibility = Visibility.Visible;
                        _displayContent.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        readonly Brush OpaqueBrush = new SolidColorBrush(Colors.White);
    }
}
