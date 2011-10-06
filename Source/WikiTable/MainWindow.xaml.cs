//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Text;
using System.Windows;

namespace WikiTable
{
    using System;

    /// <summary>
    /// Converts Tab separated table from the clipboard to a wiki formatted table.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.RefreshContent();
        }

        public void RefreshContent()
        {
            var text = Clipboard.GetText();
            var sb = new StringBuilder();
            bool isHeader = IncludeHeader.IsChecked.Value;
            foreach (var l in text.Split('\r'))
            {
                var line = l.Trim();
                if (line.Length == 0) continue;
                var fields = line.Split('\t');
                sb.Append(isHeader ? "||":"|");
                foreach (var field in fields)
                {
                    sb.Append(" ");
                    sb.Append(field.Trim());
                    sb.Append(isHeader ? " ||" : " |");                    
                }
                sb.AppendLine();
                isHeader = false;
            }
            textBox1.Text = sb.ToString();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshContent();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
    }
}
