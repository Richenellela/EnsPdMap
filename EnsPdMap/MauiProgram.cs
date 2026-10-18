using EnsPdMap.Helpers;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace EnsPdMap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex) CrashLog.Write(ex, "AppDomain");
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            CrashLog.Write(e.Exception, "Task");
            e.SetObserved();
        };

        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Supprime le trait de soulignement natif des champs de saisie sur Android
        EntryHandler.Mapper.AppendToMapping("SansSoulignement", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

        return builder.Build();
    }
}
