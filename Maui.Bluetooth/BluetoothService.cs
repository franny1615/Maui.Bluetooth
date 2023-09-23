namespace Maui.Bluetooth;

public class BluetoothStateEventArgs : EventArgs
{
    public BluetoothState State { get; set; }
}

public class BluetoothDeviceDiscoveredArgs : EventArgs
{
    public BTDevice Device { get; set; }
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
    public string UUID { get; set; } 
}

public interface IBluetoothService
{
    public event EventHandler<BluetoothStateEventArgs> OnBluetoothStateChanged;
    public event EventHandler<BluetoothDeviceDiscoveredArgs> OnDeviceDiscovered;

    public void Prepare();
    public void SearchForDevices();
    public void ConnectTo(string deviceUuid, Action completion);
}

public partial class BluetoothService : IBluetoothService
{
    public event EventHandler<BluetoothStateEventArgs> OnBluetoothStateChanged;
    public event EventHandler<BluetoothDeviceDiscoveredArgs> OnDeviceDiscovered;

    public partial void Prepare();
    public partial void SearchForDevices();
    public partial void ConnectTo(string deviceUuid, Action completion);
}
