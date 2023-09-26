using CoreBluetooth;
using Foundation;

namespace Maui.Bluetooth;

public partial class BTDevice : NSObject, ICBPeripheralDelegate
{
    public partial void DiscoverCharacteristics()
    {
        CBPeripheral peripheral = (CBPeripheral)OSObject;
        if (peripheral.Services != null)
        {
            foreach(var service in peripheral.Services)
            {
                peripheral.DiscoverCharacteristics(service);
            }
        }
    }
    
    public partial void DiscoverServices()
    {
        CBPeripheral peripheral = (CBPeripheral)OSObject;
        peripheral.DiscoverServices(null);
    }

    [Foundation.Export("peripheral:didDiscoverServices:")]
    public void DiscoveredService (CoreBluetooth.CBPeripheral peripheral, Foundation.NSError error)
    {
        OnDiscoveredDeviceService?.Invoke(this, new DiscoveredServiceArgs
        {
            BluetoothDeviceObject = peripheral,
        });
    }

    [Foundation.Export("peripheral:didDiscoverCharacteristicsForService:error:")]
    public void DiscoveredCharacteristic (
        CoreBluetooth.CBPeripheral peripheral, 
        CoreBluetooth.CBService service, 
        Foundation.NSError error)
    {
        OnDiscoveredCharacteristics?.Invoke(this, new DiscoveredCharacteristicsArgs
        {
            BluetoothDeviceObject = peripheral
        });
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
