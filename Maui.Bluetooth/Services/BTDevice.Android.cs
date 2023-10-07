namespace Maui.Bluetooth;

public partial class BTDevice
{
    public partial void DiscoverCharacteristics()
    {

    }
    
    public partial void DiscoverServices(string[] serviceUUIDs)
    {

    }

    public partial bool HasCharacteristicWithUUID(string uuid)
    {
        return false;
    }

    public partial void ReadDataFromCharacteristicWithUUID(string uuid, Action<byte[]> completion)
    {
    }

    public partial void SendDataToCharacteristicWithUUID(string uuid, byte[] data, Action<byte[]> completion)
    {
        
    }

    public partial void SubscribeToCharacteristicWithUUID(string uuid) { }
}
