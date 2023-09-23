using Maui.Bluetooth;

namespace BluetoothApp.Pages;

public class NavUtils
{
    public static void BTDeviceCardTapped(IBTDevice device)
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(IBTDevice), device }
        }; 

        Shell.Current.GoToAsync(nameof(DevicePage), parameters);
    }
}
