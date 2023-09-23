using Maui.Bluetooth;

namespace BluetoothApp.Pages;

public class NavUtils
{
    public static void BTDeviceCardTapped(BTDevice device)
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(BTDevice), device }
        }; 

        Shell.Current.GoToAsync(nameof(DevicePage), parameters);
    }
}
