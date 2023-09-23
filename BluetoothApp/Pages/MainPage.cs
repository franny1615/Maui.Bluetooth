using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Markup;
using Maui.Bluetooth;
using BluetoothApp.Views;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BluetoothApp.Pages;

public class MainPage : ContentPage
{
	public ObservableCollection<BTDevice> BTDevices { get; set; } = new();
	private readonly IBluetoothService _bluetoothService;

	private CollectionView _devicesList = new()
	{
		ItemTemplate = new DataTemplate(() => 
		{
			var view = new BTDeviceCard();
			view.SetBinding(BTDeviceCard.BTDeviceProperty, ".");
			view.Clicked = NavUtils.BTDeviceCardTapped;

			return view;
		}),
		ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
		{
			ItemSpacing = 8
		}
	};

	public MainPage(IBluetoothService bluetoothService)
	{
		BindingContext = this;
		_bluetoothService = bluetoothService;		

		Title = "Home";
		_devicesList.SetBinding(CollectionView.ItemsSourceProperty, nameof(BTDevices));
		Content = new Grid
		{
			Children = 
			{
				_devicesList
			}
		};
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		_bluetoothService.Prepare();
		_bluetoothService.OnBluetoothStateChanged += BTStateChanged;
		_bluetoothService.OnDeviceDiscovered += BTDeviceDiscoverd;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		_bluetoothService.OnBluetoothStateChanged -= BTStateChanged;
		_bluetoothService.OnDeviceDiscovered -= BTDeviceDiscoverd;
    }

    private void BTStateChanged(object sender, BluetoothStateEventArgs e)
	{
		#if DEBUG
		System.Diagnostics.Debug.WriteLine($"BTState >>> {e.State}");
		#endif
		if (e.State == BluetoothState.PoweredOn)
		{
			_bluetoothService.SearchForDevices();
		}
	}

	private void BTDeviceDiscoverd(object sender, BluetoothDeviceDiscoveredArgs e)
	{
		bool found = false;
		foreach(var device in BTDevices)
		{
			if (device.Name == e.Device.Name)
			{
				found = true;

				// underlying object may contain updated info
				device.OSObject = e.Device.OSObject; 
				break;
			}
		}

		if (!found && !string.IsNullOrEmpty(e.Device.Name))
		{
			#if DEBUG
			System.Diagnostics.Debug.WriteLine($"Added Device >>> {e.Device.Name}");
			#endif
			BTDevices.Add(e.Device);
		}
	}
}