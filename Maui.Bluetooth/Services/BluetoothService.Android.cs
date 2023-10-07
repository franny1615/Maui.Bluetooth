using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Maui.Bluetooth;

public partial class BluetoothService
{
    private BluetoothManager _manager = null;
    private BluetoothAdapter _adapter = null;

    private Receiver _receiver;

    public partial void Prepare()
    {
        _manager = (BluetoothManager)Platform.CurrentActivity.GetSystemService(Context.BluetoothService);
        _adapter = _manager.Adapter;

        if ( _adapter != null && !_adapter.IsEnabled)
        {
            ActivityCompat.RequestPermissions(
                Platform.CurrentActivity,
                new String[]
                {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.BluetoothScan
                },
                2);

            // TODO: I have to know that they gave the permission to send powered on state
            // https://stackoverflow.com/questions/31502650/android-m-request-permission-non-activity

            _receiver = new Receiver();
            _receiver.ReceivedDevice = (device) =>
            {
                OnDeviceDiscovered?.Invoke(this, new BluetoothDeviceDiscoveredArgs
                {
                    Device = device
                });
            };
        }
    }

    public partial void Connect(IBTDevice device)
    {
    }

    public partial void Disconnect(IBTDevice device)
    {
    }

    public partial void SearchForDevices()
    {
        if (_adapter == null) 
        {
            OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
            { 
                State = BluetoothState.Unsupported
            });
            return;
        }

        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) == Permission.Denied ||
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) == Permission.Denied ||
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) == Permission.Denied)
        {
            OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
            {
                State = BluetoothState.Unauthorized
            });
            return;
        }

        Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
        Platform.CurrentActivity.StartActivityForResult(enableBluetooth, 1);

        var pairedDevices = _adapter.BondedDevices;
        if ( pairedDevices != null  && pairedDevices.Count > 0)
        {
            List<IBTDevice> devices = new();
            foreach(BluetoothDevice dvc in pairedDevices)
            {
                devices.Add(new BTDevice
                {
                    OSObject = dvc,
                    Name = dvc.Name,
                });
            }

            OnRetrivedConnectedDevices?.Invoke(this, new ConnectedBluetoothDevicesArgs
            {
                ConnectedDevices = devices
            });
        }

        IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        Platform.CurrentActivity.RegisterReceiver(_receiver, filter);

        _adapter.StartDiscovery();
    }

    public partial void Stop()
    {
        Platform.CurrentActivity.UnregisterReceiver(_receiver);
        _adapter.CancelDiscovery();
    }
}

public class Receiver : BroadcastReceiver
{
    public Action<IBTDevice> ReceivedDevice;

    public override void OnReceive(Context context, Intent intent)
    {
        string action = intent.Action;
        if (action == BluetoothDevice.ActionFound)
        {
            BluetoothDevice device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            if (device != null)
            {
                ReceivedDevice?.Invoke(new BTDevice 
                { 
                    OSObject = device,
                    Name = device.Name,
                });
            }
        }
    }
}