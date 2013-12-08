namespace WikiPad
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class VisibilityConverter : IValueConverter
    {
        public VisibilityConverter()
        {
            this.TrueVisibility = Visibility.Visible;
            this.FalseVisibility = Visibility.Collapsed;
            this.NullVisibility = Visibility.Collapsed;
            this.DefaultVisibility = Visibility.Visible;
        }

        public Visibility TrueVisibility { get; set; }

        public Visibility FalseVisibility { get; set; }
        
        public Visibility NullVisibility { get; set; }
        
        public Visibility DefaultVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? this.TrueVisibility : this.FalseVisibility;
            }

            if (value == null)
            {
                return this.NullVisibility;
            }

            return this.DefaultVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}