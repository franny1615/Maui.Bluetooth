using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Markup;
using Maui.Bluetooth;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BluetoothApp.Views;

public class BTDeviceCard : ContentView
{
	public static readonly BindableProperty BTDeviceProperty = BindableProperty.Create(
		nameof(BTDeviceProperty),
		typeof(BTDevice),
		typeof(BTDeviceCard),
		defaultValue: null
	);

	public BTDevice BTDevice 
	{
		get => (BTDevice)GetValue(BTDeviceProperty);
		set => SetValue(BTDeviceProperty, value);
	}

	public static readonly BindableProperty ClickedProperty = BindableProperty.Create(
		nameof(BTDeviceProperty),
		typeof(Action<BTDevice>),
		typeof(BTDeviceCard),
		defaultValue: null
	);

	public Action<BTDevice> Clicked
	{
		get => (Action<BTDevice>)GetValue(ClickedProperty);
		set => SetValue(ClickedProperty, value);
	}

	public BTDeviceCard() { }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
		if (propertyName == BTDeviceProperty.PropertyName)
		{
			Render();
		}
    }

	private void Render()
	{
		if(BTDevice == null)
		{
			return;
		}

		var name = new Label
		{
			FontSize = 21,
			HorizontalTextAlignment = TextAlignment.Start,
			Text = BTDevice.Name
		};
		var uuid = new Label
		{
			FontSize = 16,
			HorizontalTextAlignment = TextAlignment.Start,
			Text = BTDevice.UUID
		};

		Content = new Border
		{
			Padding = 8,
			Stroke = Colors.Gray,
			StrokeShape = new RoundRectangle { CornerRadius = 5 },
			Content = new Grid
			{
				RowDefinitions = Rows.Define(Star, Auto),
				Children = 
				{
					name.Row(0),
					uuid.Row(1)
				}
			}
		}.TapGesture(async () => {
			await this.FadeTo(0);
			await this.FadeTo(1);

			Clicked?.Invoke(BTDevice);
		});
	}
}