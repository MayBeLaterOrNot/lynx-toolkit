
namespace NuGetManager
{
    using System.Windows;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using NuGetManager.Properties;

    public class MainViewModel
    {
        private INuGetClient ExeClient => new NuGetExeClient(this.ApiKey);
        private readonly INuGetClient client = new NuGetCoreClient();

        public MainViewModel()
        {
            this.Update();
        }

        public string Filter
        {
            get
            {
                return Settings.Default.Filter;
            }

            set
            {
                Settings.Default.Filter = value;
                Settings.Default.Save();
                this.Update();
            }
        }

        public string ApiKey
        {
            get
            {
                return Settings.Default.ApiKey;
            }

            set
            {
                Settings.Default.ApiKey = value;
                Settings.Default.Save();
                this.Update();
            }
        }

        public void Update()
        {
            var dispatcher = Application.Current.Dispatcher;
            Task.Run(
                () =>
                    {
                        dispatcher.Invoke(() => this.Packages.Clear());
                        foreach (var p in this.client.GetVersions(this.Filter))
                        {
                            dispatcher.Invoke(() => this.Packages.Add(p));
                        }
                    });
        }

        public ObservableCollection<Package> Packages { get; } = new ObservableCollection<Package>();

        public void Delete()
        {
            var client = this.ExeClient;
            foreach (var package in this.Packages.Where(p => p.IsSelected).ToArray())
            {
                Task.Factory.StartNew(() => client.Delete(package));
                package.IsListed = false;
            }
        }
    }
}