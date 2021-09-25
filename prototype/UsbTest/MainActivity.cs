using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Hardware.Usb;

using Android.Content;
using Android.Util;
using Xamarin.Forms;
using View = Android.Views.View;

namespace UsbTest
{
    public class UsbConnectedEvent
    {
        public UsbDevice Device { get; set; }
    }

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] {UsbManager.ActionUsbDeviceAttached})]
    public class UsbConnectedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug($"UsbTest", $"{UsbManager.ActionUsbDeviceAttached}");
            if (intent.Extras != null && intent.Extras.ContainsKey(UsbManager.ExtraDevice))
            {
                var device = (UsbDevice)intent.Extras.Get(UsbManager.ExtraDevice);
                Log.Debug("UsbTest", $"Plugged device: {device.DeviceClass}");

                MessagingCenter.Send(this, nameof(UsbConnectedEvent), new UsbConnectedEvent
                {
                    Device = device,
                });
            }
        }
    }
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private UsbManager _usbManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            _usbManager = (UsbManager)this.GetSystemService(Context.UsbService);

            MessagingCenter.Subscribe<UsbConnectedReceiver, UsbConnectedEvent>(this, nameof(UsbConnectedEvent), UsbConnected);
        }

        private void UsbConnected(UsbConnectedReceiver receiver, UsbConnectedEvent e)
        {

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
