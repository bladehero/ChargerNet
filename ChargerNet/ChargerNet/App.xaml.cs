using System;
using Xamarin.Forms;
using ChargerNet.Services;
using ChargerNet.Models;
using System.IO;
using System.Reflection;
using ChargerNet.Globals;

namespace ChargerNet
{
    public partial class App : Application
    {
        public App()
        {
            Xamarin.Forms.Device.SetFlags(new string[] { "Expander_Experimental" });
            InitializeComponent();

            InitializeDatabase();
            var connection = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Variables.DatabaseName);
            DependencyService.RegisterSingleton(connection);
            DependencyService.Register<IDataStore<User>, UserStore>();
            DependencyService.Register<IDataStore<Charger>, ChargerStore>();
            DependencyService.Register<IDataStore<UserCharger>, UserChargerStore>();

            MainPage = new AppShell();
        }

        private void InitializeDatabase()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Variables.DatabaseName);
            if (!File.Exists(path))
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using (var stream = assembly.GetManifestResourceStream($"{nameof(ChargerNet)}.{Variables.DatabaseName}"))
                {
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        stream.CopyTo(fs);
                        fs.Flush();
                    }
                }
            }
        }

        protected override void OnStart()
        {
            Shell.Current.GoToAsync("//LoginPage").Wait();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
