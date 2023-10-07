using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
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

        _receiver = new Receiver();
        _receiver.ReceivedDevice = (device) =>
        {
            OnDeviceDiscovered?.Invoke(this, new BluetoothDeviceDiscoveredArgs
            {
                Device = device
            });
        };

        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) == Permission.Granted ||
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) == Permission.Granted ||
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) == Permission.Granted)
        {
            OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
            {
                State = BluetoothState.PoweredOn
            });
        }

        if ( _adapter != null && !_adapter.IsEnabled)
        {
            PermissionsRequester.RequestPermissions(
                new string[] 
                {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.AccessCoarseLocation
                }, 
                (requestedPermissions, results) => 
                {
                    int grantedCount = 0;
                    foreach(var res in results)
                    {
                        if (res == Permission.Granted)
                        {
                            grantedCount += 1;
                        }
                    }

                    if (grantedCount == requestedPermissions.Length)
                    {
                        OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
                        {
                            State = BluetoothState.PoweredOn
                        });
                    }
                });
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

        bool started = _adapter.StartDiscovery();
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"Was able to start discovery >>> {started}");
        #endif
    }

    public partial void Stop()
    {
        Platform.CurrentActivity.UnregisterReceiver(_receiver);

        _adapter.CancelDiscovery();
        _adapter.Dispose();
        _adapter = null;

        _manager.Dispose();
        _manager = null;
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

public static class PermissionsRequester
{
    public static void RequestPermissions(
        string[] permissions, 
        Action<string[], Permission[]> completion)
    {
        Fragment frag = new PermissionsFragment(permissions, completion);
        FragmentTransaction transaction = Platform.CurrentActivity.FragmentManager.BeginTransaction();
        transaction.Add(frag, null).Commit();
    }
}

public class PermissionsFragment : Fragment
{
    public const string PERMISSIONS_KEY = "kRequestPermissions";
    private const int PERMISSIONS_REQUEST = 6969;

    private Action<string[], Permission[]> _requestedPermissionResult = null;
    private string[] _permissions = null;

    public PermissionsFragment(string[] permissions, Action<string[], Permission[]> result)
    {
        _requestedPermissionResult = result;
        _permissions = permissions;
    }

    public override void OnAttach(Activity activity)
    {
        base.OnAttach(activity);
        RequestPermissions(_permissions, PERMISSIONS_REQUEST);
    }

    public override void OnRequestPermissionsResult(
        int requestCode, 
        string[] permissions, 
        [GeneratedEnum] Permission[] grantResults)
    {
        if (requestCode == PERMISSIONS_REQUEST)
        {
            _requestedPermissionResult?.Invoke(permissions, grantResults);
        }
    }
}