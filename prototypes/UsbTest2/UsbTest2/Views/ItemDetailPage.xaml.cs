using System.ComponentModel;
using UsbTest2.ViewModels;
using Xamarin.Forms;

namespace UsbTest2.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}