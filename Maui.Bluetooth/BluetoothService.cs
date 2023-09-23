namespace Maui.Bluetooth;

public class BluetoothStateEventArgs : EventArgs
{
    public BluetoothState State { get; set; }
}

public class BluetoothDeviceDiscoveredArgs : EventArgs
{
    public BTDevice Device { get; set; }
}

public class BluetoothDeviceConnectedArgs : EventArgs
{
    public BTDevice Device { get; set; }
}

public class BluetoothDeviceDisconnectedArgs : EventArgs
{
    public BTDevice Device { get; set; }
}

public class BluetoothDeviceConnectionFailureArgs : EventArgs
{
    public BTDevice Device { get; set; }
    public string ErrorMessage { get; set; }
}

public class ConnectedBluetoothDevicesArgs : EventArgs
{
    public List<BTDevice> ConnectedDevices { get; set; }
}

public enum BluetoothState 
{
    PoweredOn,
    PoweredOff,
    Resetting,
    Unauthorized,
    Unknown,
    Unsupported
}

public class BTDevice 
{
    public string Name { get; set; } 
    public object OSObject { get; set; }
}

public interface IBluetoothService
{
    public event EventHandler<BluetoothStateEventArgs> OnBluetoothStateChanged;
    public event EventHandler<BluetoothDeviceDiscoveredArgs> OnDeviceDiscovered;
    public event EventHandler<BluetoothDeviceConnectedArgs> OnDeviceConnected;
    public event EventHandler<BluetoothDeviceDisconnectedArgs> OnDeviceDisconnected;
    public event EventHandler<BluetoothDeviceConnectionFailureArgs> OnDeviceFailedToConnect;
    public event EventHandler<ConnectedBluetoothDevicesArgs> OnRetrivedConnectedDevices;

    public void Prepare();
    public void SearchForDevices();
    public void Connect(BTDevice device);
    public void Disconnect(BTDevice device);
}

public partial class BluetoothService : IBluetoothService
{
    public event EventHandler<BluetoothStateEventArgs> OnBluetoothStateChanged;
    public event EventHandler<BluetoothDeviceDiscoveredArgs> OnDeviceDiscovered;
    public event EventHandler<BluetoothDeviceConnectedArgs> OnDeviceConnected;
    public event EventHandler<BluetoothDeviceDisconnectedArgs> OnDeviceDisconnected;
    public event EventHandler<BluetoothDeviceConnectionFailureArgs> OnDeviceFailedToConnect;
    public event EventHandler<ConnectedBluetoothDevicesArgs> OnRetrivedConnectedDevices;

    public partial void Prepare();
    public partial void SearchForDevices();
    public partial void Connect(BTDevice device);
    public partial void Disconnect(BTDevice device);
}
