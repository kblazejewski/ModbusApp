namespace ModbusApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

#if WINDOWS
    window.Width = 1200;
    window.Height = 600;
#endif

#if MACCATALYST
        window.MinimumWidth = 1200;
        window.MinimumHeight = 600;
        window.MaximumWidth = 1200;
        window.MaximumHeight = 600;
#endif

        return window;
    }
}