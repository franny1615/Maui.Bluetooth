using BluetoothApp.Pages;

namespace BluetoothApp;

public class AppShell : Shell
{
    public AppShell()
    {
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

        Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(MainPage)) });
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(DevicePage), typeof(DevicePage));
    }
}
