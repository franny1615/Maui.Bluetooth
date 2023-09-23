using CoreBluetooth;
using CoreFoundation;
using Foundation;

namespace Maui.Bluetooth;

public partial class BluetoothService : NSObject, ICBCentralManagerDelegate
{
    private CBCentralManager _centralManager;

    public BluetoothService() : base() {}

    public partial void Prepare()
    {
        _centralManager = new CBCentralManager(this, DispatchQueue.MainQueue);
    }

    public partial void Connect(IBTDevice device)
    {
        _centralManager.ConnectPeripheral((CBPeripheral) device.OSObject);
    }

    public partial void Disconnect(IBTDevice device)
    {
        _centralManager.CancelPeripheralConnection((CBPeripheral) device.OSObject);
    }

    public partial void SearchForDevices()
    {
        var options = new NSMutableDictionary
        {
            { CBCentralManager.ScanOptionAllowDuplicatesKey, new NSNumber(value: false) }
        };

        // Ideally user listens to OnBluetoothStateChanged event first, and if its powered on, call SearchForDevices()
        _centralManager.ScanForPeripherals(peripheralUuids: null, options: options);
    }

    [Export("centralManagerDidUpdateState:")]
    public void UpdatedState (CoreBluetooth.CBCentralManager central)
    {
        switch (central.State)
        {
            case CBManagerState.PoweredOn:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.PoweredOn });
                break;
            case CBManagerState.PoweredOff:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.PoweredOff });
                break;
            case CBManagerState.Resetting:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.Resetting });
                break;
            case CBManagerState.Unauthorized:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.Unauthorized });
                break;
            case CBManagerState.Unknown:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.Unknown });
                break;
            case CBManagerState.Unsupported:
                OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs { State = BluetoothState.Unsupported });
                break;
            default:
                break;
        }
    }

    [Foundation.Export("centralManager:didDiscoverPeripheral:advertisementData:RSSI:")]
    public void DiscoveredPeripheral (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral peripheral, 
        Foundation.NSDictionary advertisementData, 
        Foundation.NSNumber RSSI)
    {
        OnDeviceDiscovered?.Invoke(this, new BluetoothDeviceDiscoveredArgs
        {
            Device = new BTDevice
            {
                Name = peripheral.Name,
                OSObject = peripheral
            }
        });
    }

    [Foundation.Export("centralManager:didConnectPeripheral:")]
    public void ConnectedPeripheral (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral peripheral) 
    {
        OnDeviceConnected?.Invoke(this, new BluetoothDeviceConnectedArgs
        {
            Device = new BTDevice
            {
                Name = peripheral.Name,
                OSObject = peripheral
            }
        });
    }

    [Foundation.Export("centralManager:didDisconnectPeripheral:error:")]
    public void DisconnectedPeripheral (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral peripheral, 
        Foundation.NSError error) 
    {
        OnDeviceDisconnected?.Invoke(this, new BluetoothDeviceDisconnectedArgs
        {
            Device = new BTDevice
            {
                Name = peripheral.Name,
                OSObject = peripheral
            }
        });  
    }

    [Foundation.Export("centralManager:didFailToConnectPeripheral:error:")]
    public void FailedToConnectPeripheral (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral peripheral, 
        Foundation.NSError error) 
    {
        OnDeviceFailedToConnect?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
        {
            Device = new BTDevice
            {
                Name = peripheral.Name,
                OSObject = peripheral
            },
            ErrorMessage = error.LocalizedDescription
        });
    }

    [Foundation.Export("centralManager:didRetrieveConnectedPeripherals:")]
    public void RetrievedConnectedPeripherals (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral[] peripherals) 
    {
        if (peripherals.Length > 0)
        {
            List<IBTDevice> connectedDevices = new();
            for (int i = 0; i < peripherals.Length; i++)
            {
                connectedDevices.Add(new BTDevice
                {
                    Name = peripherals[i].Name,
                    OSObject = peripherals[i]
                });
            }
            OnRetrivedConnectedDevices?.Invoke(this, new ConnectedBluetoothDevicesArgs
            {
                ConnectedDevices = connectedDevices
            });
        }
    }

    [Foundation.Export("centralManager:didRetrievePeripherals:")]
    public void RetrievedPeripherals (
        CoreBluetooth.CBCentralManager central, 
        CoreBluetooth.CBPeripheral[] peripherals)  { }

    [Foundation.Export("centralManager:willRestoreState:")]
    public void WillRestoreState (CoreBluetooth.CBCentralManager central, Foundation.NSDictionary dict) { }
}
