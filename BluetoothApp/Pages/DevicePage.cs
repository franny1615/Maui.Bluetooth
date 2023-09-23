
using Maui.Bluetooth;

namespace BluetoothApp.Pages;

public class DevicePage : ContentPage, IQueryAttributable
{
	private readonly IBluetoothService _bluetoothService;
	private BTDevice _btDevice;

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
				_disconnectButton
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
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		_connectButton.Clicked -= ConnectButtonClicked;
		_disconnectButton.Clicked -= DisconnectButtonClicked;
		_bluetoothService.OnDeviceConnected -= DeviceConnected;
		_bluetoothService.OnDeviceFailedToConnect -= FailedToConnectDevice;
		_bluetoothService.OnDeviceDisconnected -= DeviceDisconnected;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey(nameof(BTDevice)))
		{
			_btDevice = (BTDevice)query[nameof(BTDevice)];
			_deviceNameLabel.Text = _btDevice.Name;
		}
    }
}