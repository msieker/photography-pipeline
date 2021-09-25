using System;
using UsbTest2.Services;
using UsbTest2.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UsbTest2
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
