using BluetoothApp.Pages;

namespace BluetoothApp;

public class AppShell : Shell
{
    public AppShell()
    {
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
        Items.Add(new ShellContent
        {
            Title = "Home",
            ContentTemplate = new DataTemplate(typeof(MainPage)),
            Route = "Home"
        });
    }
}
