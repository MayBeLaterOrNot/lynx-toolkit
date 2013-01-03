using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
    public static class ApplicationCommands
    {
        static ApplicationCommands()
        {
            Exit = new RoutedUICommand() { Text = "Exit" };
            Exit.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
        }

        public static RoutedUICommand Exit { get; private set; }
    }
}
