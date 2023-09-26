namespace Maui.Bluetooth;

public partial class BTDevice
{
    public partial void DiscoverCharacteristics()
    {

    }
    
    public partial void DiscoverServices()
    {

    }

    public partial bool HasCharacteristicWithUUID(string uuid)
    {
        return false;
    }

    public partial byte[] ReadDataFromCharacteristicWithUUID(string uuid)
    {
        return new byte[0];
    }

    public partial void SendDataToCharacteristicWithUUID(string uuid, byte[] data)
    {
        
    }
}
