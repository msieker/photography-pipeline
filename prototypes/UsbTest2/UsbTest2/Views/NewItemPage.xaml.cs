using System;
using System.Collections.Generic;
using System.ComponentModel;
using UsbTest2.Models;
using UsbTest2.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UsbTest2.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}