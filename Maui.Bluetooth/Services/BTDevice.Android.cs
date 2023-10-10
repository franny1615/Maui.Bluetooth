using Android.Bluetooth;
using Android.Runtime;

namespace Maui.Bluetooth;

public partial class BTDevice : BluetoothGattCallback
{
    public event EventHandler<BluetoothStateEventArgs> StateChanged;

    public override void OnConnectionStateChange(
        BluetoothGatt gatt, 
        [GeneratedEnum] GattStatus status, 
        [GeneratedEnum] ProfileState newState)
    {
        base.OnConnectionStateChange(gatt, status, newState);
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

    }
    
    public partial void DiscoverServices(string[] serviceUUIDs)
    {
        BluetoothDevice device = (BluetoothDevice)OSObject;
        
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
