namespace Maui.Bluetooth;

public class DiscoveredServiceArgs : EventArgs 
{
    public object BluetoothDeviceObject { get; set; }
}
public class DiscoveredCharacteristicsArgs : EventArgs 
{
    public object BluetoothDeviceObject { get; set; }
}

public class CharacteristicPostedNotificationArgs : EventArgs
{
    public string CharacteristicUUID { get; set; }
    public byte[] Data { get; set; }
}

public interface IBTDevice
{
    public event EventHandler<DiscoveredServiceArgs> OnDiscoveredDeviceService;
    public event EventHandler<DiscoveredCharacteristicsArgs> OnDiscoveredCharacteristics;
    public event EventHandler<CharacteristicPostedNotificationArgs> OnCharacteristicPostedNotification;

    public string Name { get; set; } 
    public object OSObject { get; set; }

    public void DiscoverServices(string[] serviceUUIDs);
    public void DiscoverCharacteristics();
    public bool HasCharacteristicWithUUID(string uuid);
    public void ReadDataFromCharacteristicWithUUID(string uuid, Action<byte[]> completion);
    public void SendDataToCharacteristicWithUUID(string uuid, byte[] data, Action<byte[]> completion);
    public void SubscribeToCharacteristicWithUUID(string uuid);
}

public partial class BTDevice : IBTDevice
{
    public string Name { get; set; }
    public object OSObject { get; set; }

    public event EventHandler<DiscoveredServiceArgs> OnDiscoveredDeviceService;
    public event EventHandler<DiscoveredCharacteristicsArgs> OnDiscoveredCharacteristics;
    public event EventHandler<CharacteristicPostedNotificationArgs> OnCharacteristicPostedNotification;

    public partial void DiscoverCharacteristics();
    public partial void DiscoverServices(string[] serviceUUIDs);
    public partial bool HasCharacteristicWithUUID(string uuid);
    public partial void ReadDataFromCharacteristicWithUUID(string uuid, Action<byte[]> completion);
    public partial void SendDataToCharacteristicWithUUID(string uuid, byte[] data, Action<byte[]> completion);
    public partial void SubscribeToCharacteristicWithUUID(string uuid);
}
