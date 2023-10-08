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
using Java.Lang.Ref;

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

        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothConnect) == Permission.Granted &&
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.Bluetooth) == Permission.Granted &&
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) == Permission.Granted &&
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
        {
            OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
            {
                State = BluetoothState.PoweredOn
            });
        } 
        else if (_adapter != null && _adapter.IsEnabled)
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
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.BluetoothScan) == Permission.Denied ||
            ContextCompat.CheckSelfPermission(Platform.CurrentActivity, Manifest.Permission.AccessCoarseLocation) == Permission.Denied)
        {
            OnBluetoothStateChanged?.Invoke(this, new BluetoothStateEventArgs
            {
                State = BluetoothState.Unauthorized
            });
            return;
        }

        PermissionsRequester.RequestBluetoothAdapterEnable((enabled) =>
        {
            StartDeviceSearch();
        });
    }

    private void StartDeviceSearch()
    {
        var pairedDevices = _adapter.BondedDevices;
        if (pairedDevices != null && pairedDevices.Count > 0)
        {
            List<IBTDevice> devices = new();
            foreach (BluetoothDevice dvc in pairedDevices)
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

        _receiver = new Receiver
        {
            ReceivedDevice = (device) =>
            {
                OnDeviceDiscovered?.Invoke(this, new BluetoothDeviceDiscoveredArgs
                {
                    Device = device
                });
            }
        };

        IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
        Platform.CurrentActivity.RegisterReceiver(_receiver, filter);

        _adapter.StartDiscovery();
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
        Fragment frag = new PermissionsFragment(
            permissions: permissions,
            requestPermissionResult: completion);
        FragmentTransaction transaction = Platform.CurrentActivity.FragmentManager.BeginTransaction();
        transaction.Add(frag, null).Commit();
    }

    public static void RequestBluetoothAdapterEnable(Action<bool> completion)
    {
        Fragment frag = new PermissionsFragment(enableBluetoothAdapterResult: completion);
        FragmentTransaction transaction = Platform.CurrentActivity.FragmentManager.BeginTransaction();
        transaction.Add(frag, null).Commit();
    }
}

public class PermissionsFragment : Fragment
{
    public const string PERMISSIONS_KEY = "kRequestPermissions";
    private const int PERMISSIONS_REQUEST = 6969;
    private const int ENABLE_BT_REQUEST = 12531;

    private Action<bool> _enableBluetoothAdapterResult = null;
    private Action<string[], Permission[]> _requestedPermissionResult = null;
    private string[] _permissions = null;

    public PermissionsFragment(
        string[] permissions = null, 
        Action<string[], Permission[]> requestPermissionResult = null,
        Action<bool> enableBluetoothAdapterResult = null)
    {
        _requestedPermissionResult = requestPermissionResult;
        _enableBluetoothAdapterResult = enableBluetoothAdapterResult;
        _permissions = permissions;
    }

    public override void OnAttach(Activity activity)
    {
        base.OnAttach(activity);
        if (_permissions != null && _permissions.Length > 0)
        {
            RequestPermissions(_permissions, PERMISSIONS_REQUEST);
        }
        else if (_enableBluetoothAdapterResult != null)
        {
            Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(enableBluetooth, ENABLE_BT_REQUEST);
        }
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
        this.Activity.FragmentManager.BeginTransaction().Remove(this).Commit();
    }

    public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == ENABLE_BT_REQUEST)
        {
            _enableBluetoothAdapterResult?.Invoke(resultCode == Result.Ok);
        }
        this.Activity.FragmentManager.BeginTransaction().Remove(this).Commit();
    }
}