using System;
using System.Collections.Generic;
using ChargerNet.ViewModels;
using ChargerNet.Views;
using Xamarin.Forms;

namespace ChargerNet
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
