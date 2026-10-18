using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace EnsPdMap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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
