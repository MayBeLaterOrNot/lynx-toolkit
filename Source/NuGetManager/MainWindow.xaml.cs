using System.Windows.Input;

namespace NuGetManager
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            var vm = new MainViewModel();
            this.DataContext = vm;
            this.PackageList.KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        vm.Delete();
                        break;
                    case Key.F5:
                        vm.Update();
                        break;
                }
            };
        }
    }
}
