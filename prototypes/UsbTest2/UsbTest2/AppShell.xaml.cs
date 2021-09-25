using System;
using System.Collections.Generic;
using UsbTest2.ViewModels;
using UsbTest2.Views;
using Xamarin.Forms;

namespace UsbTest2
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
