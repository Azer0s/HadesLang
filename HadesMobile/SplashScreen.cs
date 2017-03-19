using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Accounts;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace HadesMobile
{
    [Activity(MainLauncher = true, NoHistory = true, Label = "HadesMobile", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait,Theme = "@android:style/Theme.Material.NoActionBar.Fullscreen")]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SplashScreen);
            var startupWork = new Task(Startup);
            startupWork.Start();
        }


        // Simulates background work that happens behind the splash screen
        protected async void Startup()
        {
            await Task.Delay(2500);

            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}