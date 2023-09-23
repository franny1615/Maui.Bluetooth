namespace BluetoothApp;

public class App : Application
{
    public App()
    {
        MainPage = new AppShell();
        AddStyles();
    }

    private void AddStyles()
    {
        if (Current?.Resources.MergedDictionaries is not { } mergedDictionaries)
        {
            return;
        }

        mergedDictionaries.Clear();
        mergedDictionaries.Add(new global::Resources.Styles.Colors());
        mergedDictionaries.Add(new global::Resources.Styles.Styles());
    }
}
