using CoreBluetooth;
using Foundation;

namespace Maui.Bluetooth;

public partial class BTDevice : NSObject, ICBPeripheralDelegate
{
    public partial void DiscoverCharacteristics()
    {
        CBPeripheral peripheral = (CBPeripheral)OSObject;
        peripheral.DiscoverCharacteristics(null);
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

        });
    }
}
