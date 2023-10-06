
using Maui.Bluetooth;
using System.Text;

namespace BluetoothApp.Pages;

public class DevicePage : ContentPage, IQueryAttributable
{
	private readonly IBluetoothService _bluetoothService;
	private IBTDevice _btDevice;

	private Label _deviceNameLabel = new()
	{
		FontSize = 32,
		FontAttributes = FontAttributes.None,
		HorizontalTextAlignment = TextAlignment.Center
	};

	private Label _connectionStatus = new()
	{
		FontSize = 24,
		FontAttributes = FontAttributes.Bold,
		Text = "Disconnected",
		HorizontalTextAlignment = TextAlignment.Center
	};

	private Button _connectButton = new()
	{
		Text = "Connect Device",
		TextColor = Colors.White,
		BackgroundColor = Colors.Green,
		FontSize = 24
	};

	private Button _disconnectButton = new()
	{
		Text = "Disconnect Device",
		TextColor = Colors.White,
		BackgroundColor = Colors.Red,
		FontSize = 24
	};

	private Label _dataLabel = new()
	{
		Text = "Data has not been read yet",
		FontSize = 32
	};

	public DevicePage(IBluetoothService bluetoothService)
	{
		Title = "Device Information";
		BindingContext = this;
		_bluetoothService = bluetoothService;

		Content = new VerticalStackLayout
		{
			Spacing = 8,
			Children = 
			{
				_deviceNameLabel,
				_connectionStatus,
				_connectButton,
				_disconnectButton,
				_dataLabel
			}
		};
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		_connectButton.Clicked += ConnectButtonClicked;
		_disconnectButton.Clicked += DisconnectButtonClicked;
		_bluetoothService.OnDeviceConnected += DeviceConnected;
		_bluetoothService.OnDeviceFailedToConnect += FailedToConnectDevice;
		_bluetoothService.OnDeviceDisconnected += DeviceDisconnected;
    }

	private void ConnectButtonClicked(object sender, EventArgs e)
	{
		_bluetoothService.Connect(_btDevice);
		_connectionStatus.Text = "Connecting...";
	}

	private void DisconnectButtonClicked(object sender, EventArgs e)
	{
		_bluetoothService.Disconnect(_btDevice);
		_connectionStatus.Text = "Disconnecting...";
	}

	private void DeviceConnected(object sender, BluetoothDeviceConnectedArgs e)
	{
		#if DEBUG
		System.Diagnostics.Debug.WriteLine($"{e.Device.Name} has been connected");
		#endif
		_connectionStatus.Text = "Connected";
		_btDevice = e.Device;
		_btDevice.OnDiscoveredDeviceService += DiscoveredDeviceService;
		_btDevice.OnDiscoveredCharacteristics += DiscoveredCharacteristics;
		_btDevice.DiscoverServices(new string[] { "4fafc201-1fb5-459e-8fcc-c5c9c331914b" });
	}

	private void DiscoveredDeviceService(object sender, DiscoveredServiceArgs e)
	{
		_btDevice.OSObject = e.BluetoothDeviceObject;
		_btDevice.DiscoverCharacteristics();
	}

	private void DiscoveredCharacteristics(object sender, DiscoveredCharacteristicsArgs e)
	{
		_btDevice.OSObject = e.BluetoothDeviceObject;
		if (_btDevice.HasCharacteristicWithUUID("beb5483e-36e1-4688-b7f5-ea07361b26a8"))
		{
			_btDevice.ReadDataFromCharacteristicWithUUID("beb5483e-36e1-4688-b7f5-ea07361b26a8", (data) =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					_dataLabel.Text = Encoding.UTF8.GetString(data, 0, data.Length);
				});
			});
		}
	}

	private void FailedToConnectDevice(object sender, BluetoothDeviceConnectionFailureArgs e)
	{
		#if DEBUG	
		System.Diagnostics.Debug.WriteLine($"{e.Device.Name} failed to connect {e.ErrorMessage}");
		#endif
		_connectionStatus.Text = $"Failed To Connect {e.ErrorMessage}";
	}

	private void DeviceDisconnected(object sender, BluetoothDeviceDisconnectedArgs e)
	{
		#if DEBUG
		System.Diagnostics.Debug.WriteLine($"{e.Device.Name} has been disconnected");
		#endif
		_connectionStatus.Text = $"Disconnected";
		if (_btDevice != null)
		{
			_btDevice.OnDiscoveredDeviceService -= DiscoveredDeviceService;
			_btDevice.OnDiscoveredCharacteristics -= DiscoveredCharacteristics;
		}
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		_connectButton.Clicked -= ConnectButtonClicked;
		_disconnectButton.Clicked -= DisconnectButtonClicked;
		_bluetoothService.OnDeviceConnected -= DeviceConnected;
		_bluetoothService.OnDeviceFailedToConnect -= FailedToConnectDevice;
		_bluetoothService.OnDeviceDisconnected -= DeviceDisconnected;
		if (_btDevice != null)
		{
			_btDevice.OnDiscoveredDeviceService -= DiscoveredDeviceService;
			_btDevice.OnDiscoveredCharacteristics -= DiscoveredCharacteristics;
		}
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey(nameof(IBTDevice)))
		{
			_btDevice = (IBTDevice)query[nameof(IBTDevice)];
			_deviceNameLabel.Text = _btDevice.Name;
		}
    }
}