namespace Maui.Bluetooth;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder UseMauiBluetooth(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<IBluetoothService, BluetoothService>();

        return builder;
    }
}
