namespace WikiPad
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class WebBrowserBehavior : DependencyObject
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(WebBrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wb = d as WebBrowser;
            if (wb != null)
            {
                wb.NavigateToString((string)e.NewValue);
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
        "Source",
        typeof(Uri),
        typeof(WebBrowserBehavior),
        new FrameworkPropertyMetadata(OnSourceChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static Uri GetSource(WebBrowser d)
        {
            return (Uri)d.GetValue(SourceProperty);
        }

        public static void SetSource(WebBrowser d, Uri value)
        {
            d.SetValue(SourceProperty, value);
        }

        static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wb = d as WebBrowser;
            if (wb != null)
            {
                wb.Source = (Uri)e.NewValue;
            }
        }
    }
}