namespace Maui.Bluetooth;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder UseMauiBluetooth(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IBluetoothService, BluetoothService>();

        return builder;
    }
}
