using CoreBluetooth;
using Foundation;

namespace Maui.Bluetooth;

public partial class BTDevice : NSObject, ICBPeripheralDelegate
{
    private Action<byte[]> _readCharecteristicCompletion;
    private Action<byte[]> _writeCharecteristicCompletion;

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
    
    public partial void DiscoverServices(string[] serviceUUIDs)
    {
        CBPeripheral peripheral = (CBPeripheral)OSObject;
        peripheral.Delegate = this;
        
        CBUUID[] uuids = new CBUUID[serviceUUIDs.Length];
        for (int i = 0; i < serviceUUIDs.Length; i++)
        {
            uuids[i] = CBUUID.FromString(serviceUUIDs[i]);
        }
        peripheral.DiscoverServices(uuids);
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
        CBPeripheral peripheral = (CBPeripheral) OSObject;
        
        foreach(CBService service in peripheral.Services)
        {
            foreach(CBCharacteristic characteristic in service.Characteristics)
            {
                if (characteristic.UUID.Uuid.ToLower() == uuid.ToLower())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public partial void ReadDataFromCharacteristicWithUUID(string uuid, Action<byte[]> completion)
    {
        CBPeripheral peripheral = (CBPeripheral) OSObject;
        peripheral.Delegate = this;

        CBCharacteristic characteristic = null;
        _readCharecteristicCompletion = completion;

        foreach (CBService service in peripheral.Services)
        {
            foreach (CBCharacteristic charc in service.Characteristics)
            {
                if (charc.UUID.Uuid.ToLower() == uuid.ToLower())
                {
                    characteristic = charc;
                }
            }
        }

        peripheral.ReadValue(characteristic);
    }

    [Foundation.Export("peripheral:didUpdateValueForCharacteristic:error:")]
    public virtual void UpdatedCharacterteristicValue(
        CoreBluetooth.CBPeripheral peripheral, 
        CoreBluetooth.CBCharacteristic characteristic, 
        Foundation.NSError error)
    {
        if (characteristic.Value != null)
        {
            _readCharecteristicCompletion?.Invoke(characteristic.Value.ToArray());
        }
        else
        {
            _readCharecteristicCompletion?.Invoke(new byte[] {});
        }
    }

    [Foundation.Export("peripheral:didUpdateValueForDescriptor:error:")]
    public virtual void UpdatedValue(
        CoreBluetooth.CBPeripheral peripheral, 
        CoreBluetooth.CBDescriptor descriptor, 
        Foundation.NSError error)
    {
    }

    public partial void SendDataToCharacteristicWithUUID(string uuid, byte[] data, Action<byte[]> completion)
    {
        CBPeripheral peripheral = (CBPeripheral) OSObject;
        peripheral.Delegate = this;
        CBCharacteristic characteristic = null;

        foreach (CBService service in peripheral.Services)
        {
            foreach (CBCharacteristic charc in service.Characteristics)
            {
                if (charc.UUID.Uuid.ToLower() == uuid.ToLower())
                {
                    characteristic = charc;
                }
            }
        }

        _writeCharecteristicCompletion = completion;
        peripheral.WriteValue(NSData.FromArray(data), characteristic, CBCharacteristicWriteType.WithResponse);
    }

    [Foundation.Export("peripheral:didWriteValueForCharacteristic:error:")]
    public virtual void WroteCharacteristicValue(
        CoreBluetooth.CBPeripheral peripheral, 
        CoreBluetooth.CBCharacteristic characteristic, 
        Foundation.NSError error)
    {
        peripheral.Delegate = this;
        peripheral.ReadValue(characteristic);
        _readCharecteristicCompletion = _writeCharecteristicCompletion;
    }
}
