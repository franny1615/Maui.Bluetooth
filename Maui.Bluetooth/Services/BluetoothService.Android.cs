using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.Content;
using Java.Util;
using Microsoft.Maui.Platform;

namespace Maui.Bluetooth;

public partial class BluetoothService
{
    private BluetoothManager _manager = null;
    private BluetoothAdapter _adapter = null;

    private ConnectDeviceThread _connectThread = null;

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
        try
        {
            // purely cleanup
            if (_connectThread != null)
            {
                _connectThread.Cancel();
                _connectThread.Dispose();
                _connectThread = null;
            }
        }
        catch { }

        try
        {
            _connectThread = new ConnectDeviceThread(_adapter, device, Platform.CurrentActivity);
            _connectThread.OnDeviceConnected += OnDeviceConnected;
            _connectThread.OnDeviceDisconnected += OnDeviceDisconnected;
            _connectThread.OnDeviceConnectionFailure += OnDeviceFailedToConnect;
            _connectThread.Run();
        }
        catch (Java.Lang.Exception ex) 
        {
            OnDeviceFailedToConnect?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
            {
                Device = device,
                ErrorMessage = ex.Message
            });
        }
    }

    public partial void Disconnect(IBTDevice device)
    {
        try
        {
            _connectThread.Cancel();
            _connectThread.OnDeviceConnected -= OnDeviceConnected;
            _connectThread.OnDeviceDisconnected -= OnDeviceDisconnected;
            _connectThread.OnDeviceConnectionFailure -= OnDeviceFailedToConnect;
        }
        catch (Java.Lang.Exception e)
        {
            OnDeviceFailedToConnect?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
            {
                Device = device,
                ErrorMessage = $"Disconnection failure: {e.Message}"
            });
        }
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
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if (intent.GetParcelableExtra(BluetoothDevice.ExtraDevice, Java.Lang.Class.FromType(typeof(BluetoothDevice))) is BluetoothDevice device)
                {
                    ReceivedDevice?.Invoke(new BTDevice
                    {
                        OSObject = device,
                        Name = device.Name,
                    });
                }
            }
            else if (intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) is BluetoothDevice device)
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

#region Permissions
public static class PermissionsRequester
{
    public static void RequestPermissions(
        string[] permissions, 
        Action<string[], Permission[]> completion)
    {
        var frag = new PermissionsFragment(
            permissions: permissions,
            requestPermissionResult: completion);
        Platform.CurrentActivity
            .GetFragmentManager()
            .BeginTransaction()
            .Add(frag, null)
            .Commit();
    }

    public static void RequestBluetoothAdapterEnable(Action<bool> completion)
    {
        var frag = new PermissionsFragment(enableBluetoothAdapterResult: completion);
        Platform.CurrentActivity
            .GetFragmentManager()
            .BeginTransaction()
            .Add(frag, null)
            .Commit();
    }
}

public class PermissionsFragment : AndroidX.Fragment.App.Fragment
{
    public const string PERMISSIONS_KEY = "kRequestPermissions";

    private Action<bool> _enableBluetoothAdapterResult = null;
    private Action<string[], Permission[]> _requestedPermissionResult = null;
    private string[] _permissions = null;

    private ActivityResultLauncher _permissionLauncher;

    public PermissionsFragment(
        string[] permissions = null, 
        Action<string[], Permission[]> requestPermissionResult = null,
        Action<bool> enableBluetoothAdapterResult = null)
    {
        _requestedPermissionResult = requestPermissionResult;
        _enableBluetoothAdapterResult = enableBluetoothAdapterResult;
        _permissions = permissions;
    }

    public override void OnAttach(Context context)
    {
        base.OnAttach(context);

        if (_permissions != null && _permissions.Length > 0)
        {
            _permissionLauncher = RegisterForActivityResult(
                new ActivityResultContracts.RequestMultiplePermissions(),
                new ResultCallback
                {
                    Completion = (result) =>
                    {
                        if (result is HashMap map)
                        {
                            DealWithPermissionRequestResult(map);
                        }

                        Activity
                            .GetFragmentManager()
                            .BeginTransaction()
                            .Remove(this)
                            .Commit();
                    }
                });

            _permissionLauncher.Launch(_permissions);
        }
        else if (_enableBluetoothAdapterResult != null)
        {
            _permissionLauncher = RegisterForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                new ResultCallback
                {
                    Completion = (result) =>
                    {
                        if (result is AndroidX.Activity.Result.ActivityResult res)
                        {
                            _enableBluetoothAdapterResult?.Invoke(res.ResultCode == (int)Result.Ok);
                        }

                        Activity
                            .GetFragmentManager()
                            .BeginTransaction()
                            .Remove(this)
                            .Commit();
                    }
                });

            Intent enableBluetooth = new Intent(BluetoothAdapter.ActionRequestEnable);
            _permissionLauncher.Launch(enableBluetooth);
        }
    }

    private void DealWithPermissionRequestResult(HashMap map)
    {
        var set = map.KeySet();

        string[] keys = new string[set.Count];
        Permission[] permissions = new Permission[set.Count];
        for (int i = 0; i < set.Count; i++) { permissions[i] = Permission.Denied; }

        int index = 0;
        foreach (string key in set)
        {
            keys[index] = (string)key;
            var granted = map.Get(key);
            if (granted is Java.Lang.Boolean g)
            {
                permissions[index] = g == Java.Lang.Boolean.True ? Permission.Granted : Permission.Denied;
            }
            index++;
        }

        _requestedPermissionResult?.Invoke(keys, permissions);
    }
}

public class ResultCallback : Java.Lang.Object, IActivityResultCallback
{
    public Action<Java.Lang.Object> Completion { get; set; }

    public void OnActivityResult(Java.Lang.Object p0)
    {
        Completion?.Invoke(p0);
    }
}
#endregion

#region Connect Device
public class ConnectDeviceThread : Java.Lang.Thread
{
    private Context _context;
    private BluetoothSocket _socket;
    private BluetoothGatt _gatt;

    private BluetoothAdapter _bluetoothAdapter;
    private IBTDevice _device;

    public event EventHandler<BluetoothDeviceConnectionFailureArgs> OnDeviceConnectionFailure;
    public event EventHandler<BluetoothDeviceConnectedArgs> OnDeviceConnected;  
    public event EventHandler<BluetoothDeviceDisconnectedArgs> OnDeviceDisconnected;

    public ConnectDeviceThread(BluetoothAdapter adapter, IBTDevice device, Context context)
    {
        _bluetoothAdapter = adapter;
        _device = device;
        _context = context;
        try
        {
            BluetoothDevice dev = (BluetoothDevice)device.OSObject;
            if (dev.Type == BluetoothDeviceType.Le)
            {
                _gatt = dev.ConnectGatt(_context, false, (BTDevice) device);
                ((BTDevice)device).StateChanged += (s, e) =>
                {
                    if (e.State == BluetoothState.PoweredOn)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            OnDeviceConnected?.Invoke(this, new BluetoothDeviceConnectedArgs
                            {
                                Device = _device
                            });
                        });
                    }
                };
            }
            else
            {
                // TODO: maybe figure out connecting regular devices
            }
        }
        catch (Java.Lang.Exception e)
        {
            OnDeviceConnectionFailure?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
            {
                Device = _device,
                ErrorMessage = e.Message
            });
        }
    }

    public override void Run()
    {
        base.Run();
        _bluetoothAdapter.CancelDiscovery();

        try
        {
            if (_socket != null)
            {
                _socket.Connect();
                OnDeviceConnected?.Invoke(this, new BluetoothDeviceConnectedArgs
                {
                    Device = _device
                });
            }
        }
        catch (Java.Lang.Exception e)
        {
            OnDeviceConnectionFailure?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
            {
                Device = _device,
                ErrorMessage = e.Message    
            });
        }
    }

    public void Cancel()
    {
        try
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
            else if (((BluetoothDevice) _device.OSObject).Type == BluetoothDeviceType.Le)
            {
                _gatt.Close();
                _gatt = null;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnDeviceDisconnected?.Invoke(this, new BluetoothDeviceDisconnectedArgs
                {
                    Device = _device
                });
            });

            this.Join();      
        }
        catch (Java.Lang.Exception e)
        {
            OnDeviceConnectionFailure?.Invoke(this, new BluetoothDeviceConnectionFailureArgs
            {
                Device = _device,
                ErrorMessage = $"Disconnection failure {e.Message}"
            });
        }
    }
}
#endregion