using Android.Bluetooth;
using Android.Runtime;

namespace Maui.Bluetooth;

public partial class BTDevice : BluetoothGattCallback
{
    public BluetoothGatt _gatt; 
    public event EventHandler<BluetoothStateEventArgs> StateChanged;

    private Action<byte[]> _readCharecteristicCompletion;
    private Action<byte[]> _writeCharecteristicCompletion;

    public override void OnConnectionStateChange(
        BluetoothGatt gatt, 
        [GeneratedEnum] GattStatus status, 
        [GeneratedEnum] ProfileState newState)
    {
        base.OnConnectionStateChange(gatt, status, newState);
        _gatt = gatt;
        switch (newState)
        {
            case ProfileState.Connected:
                StateChanged?.Invoke(this, new BluetoothStateEventArgs
                {
                    State = BluetoothState.PoweredOn
                });
                break;
            case ProfileState.Disconnected:
                StateChanged?.Invoke(this, new BluetoothStateEventArgs
                {
                    State = BluetoothState.PoweredOff
                });
                break;
        }
    }

    public partial void DiscoverCharacteristics()
    {
        // only need to discover services, they come with the characteristics already.
        OnDiscoveredCharacteristics?.Invoke(this, new DiscoveredCharacteristicsArgs
        {
            BluetoothDeviceObject = this.OSObject
        });
    }
    
    public partial void DiscoverServices(string[] serviceUUIDs)
    {
        _gatt.DiscoverServices();
    }

    public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
    {
        base.OnServicesDiscovered(gatt, status);
        OnDiscoveredDeviceService?.Invoke(this, new DiscoveredServiceArgs
        {
            BluetoothDeviceObject = this.OSObject
        });
    }

    public partial bool HasCharacteristicWithUUID(string uuid)
    {
        return FindCharacteristicWithUUID(uuid) != null;
    }

    public partial void ReadDataFromCharacteristicWithUUID(string uuid, Action<byte[]> completion)
    {
        _readCharecteristicCompletion = completion;
        
        var characteristic = FindCharacteristicWithUUID(uuid);
        if (characteristic != null)
        {
            _gatt.ReadCharacteristic(characteristic);
        }
    }

    public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, [GeneratedEnum] GattStatus status)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            base.OnCharacteristicRead(gatt, characteristic, value, status);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _readCharecteristicCompletion?.Invoke(value);
            });
        }
    }

    public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            var value = characteristic.GetValue();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _readCharecteristicCompletion?.Invoke(value);
            });
        }
    }

    public partial void SendDataToCharacteristicWithUUID(string uuid, byte[] data, Action<byte[]> completion)
    {
        _writeCharecteristicCompletion = completion;
        var characteristic = FindCharacteristicWithUUID(uuid);
        if (characteristic != null)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                _gatt.WriteCharacteristic(characteristic, data, (int)GattWriteType.Default);
            }
            else
            {
                characteristic.SetValue(data);
                _gatt.WriteCharacteristic(characteristic);
            }
        }
    }

    public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
    {
        base.OnCharacteristicWrite(gatt, characteristic, status);
        if (status == GattStatus.Success)
        {
            ReadDataFromCharacteristicWithUUID(characteristic.Uuid.ToString(), _writeCharecteristicCompletion);
        }
    }

    public partial void SubscribeToCharacteristicWithUUID(string uuid) 
    {
        var characteristic = FindCharacteristicWithUUID(uuid);
        if (characteristic != null)
        {
            _gatt.SetCharacteristicNotification(characteristic, true);
        }
    }

    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            base.OnCharacteristicChanged(gatt, characteristic, value);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnCharacteristicPostedNotification?.Invoke(this, new CharacteristicPostedNotificationArgs
                {
                    CharacteristicUUID = characteristic.Uuid.ToString(),
                    Data = value
                });
            });
        }
    }

    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            var value = characteristic.GetValue();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnCharacteristicPostedNotification?.Invoke(this, new CharacteristicPostedNotificationArgs
                {
                    CharacteristicUUID = characteristic.Uuid.ToString(),
                    Data = value
                });
            });
        }
    }

    private BluetoothGattCharacteristic FindCharacteristicWithUUID(string uuid)
    {
        var services = _gatt.Services;
        foreach (var service in services)
        {
            foreach (var characteristic in service.Characteristics)
            {
                if (characteristic.Uuid.ToString().ToLower() == uuid.ToLower())
                {
                    return characteristic;
                }
            }
        }

        return null;
    }
}
