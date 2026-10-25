namespace EnsPdMap;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        EnsPdMap.Models.Data.Init();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new AppShell());
    }
}
