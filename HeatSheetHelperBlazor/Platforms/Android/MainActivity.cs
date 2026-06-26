using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace HeatSheetHelperBlazor;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Ensure the app's layout does not draw under the status/navigation bars.
        // Setting decor to fit system windows keeps UI inside the system bar insets.
        WindowCompat.SetDecorFitsSystemWindows(Window, true);
    }
}
