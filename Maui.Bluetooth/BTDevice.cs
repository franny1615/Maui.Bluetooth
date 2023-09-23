namespace Maui.Bluetooth;

public class DiscoveredServiceArgs : EventArgs
{

}

public interface IBTDevice
{
    public event EventHandler<DiscoveredServiceArgs> OnDiscoveredDeviceService;

    public string Name { get; set; } 
    public object OSObject { get; set; }

    public void DiscoverServices();
    public void DiscoverCharacteristics();
}

public partial class BTDevice : IBTDevice
{
    public string Name { get; set; }
    public object OSObject { get; set; }

    public event EventHandler<DiscoveredServiceArgs> OnDiscoveredDeviceService;

    public partial void DiscoverCharacteristics();
    public partial void DiscoverServices();
}
