﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace BVGF_Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize
                                   | ConfigChanges.Orientation
                                   | ConfigChanges.UiMode
                                   | ConfigChanges.ScreenLayout
                                   | ConfigChanges.SmallestScreenSize
                                   | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // 👇 This method must be added
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // 🔐 Let MAUI handle the permission result
            Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
