namespace Maui.Bluetooth;

public class BluetoothStateEventArgs : EventArgs
{
    public BluetoothState State { get; set; }
}

public class BluetoothDeviceDiscoveredArgs : EventArgs
{
    public IBTDevice Device { get; set; }
}

public class BluetoothDeviceConnectedArgs : EventArgs
{
    public IBTDevice Device { get; set; }
}

public class BluetoothDeviceDisconnectedArgs : EventArgs
{
    public IBTDevice Device { get; set; }
}

public class BluetoothDeviceConnectionFailureArgs : EventArgs
{
    public IBTDevice Device { get; set; }
    public string ErrorMessage { get; set; }
}

public class ConnectedBluetoothDevicesArgs : EventArgs
{
    public List<IBTDevice> ConnectedDevices { get; set; }
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

public interface IBluetoothService
{
    public event EventHandler<BluetoothStateEventArgs> OnBluetoothStateChanged;
    public event EventHandler<BluetoothDeviceDiscoveredArgs> OnDeviceDiscovered;
    public event EventHandler<BluetoothDeviceConnectedArgs> OnDeviceConnected;
    public event EventHandler<BluetoothDeviceDisconnectedArgs> OnDeviceDisconnected;
    public event EventHandler<BluetoothDeviceConnectionFailureArgs> OnDeviceFailedToConnect;
    public event EventHandler<ConnectedBluetoothDevicesArgs> OnRetrivedConnectedDevices;

    public void Prepare();
    public void Stop();
    public void SearchForDevices();
    public void Connect(IBTDevice device);
    public void Disconnect(IBTDevice device);
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
    public partial void Stop();
    public partial void SearchForDevices();
    public partial void Connect(IBTDevice device);
    public partial void Disconnect(IBTDevice device);
}
