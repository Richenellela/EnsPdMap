using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls;

public static class LayoutHelpers
{
    // Utilitaire : applique PositionProportional à n'importe quelle vue passée en paramètre.
    // Remplacez l'appel direct avec myView par LayoutHelpers.SetPositionProportional(votreView);
    public static void SetPositionProportional(View v)
    {
        if (v == null) return;
        AbsoluteLayout.SetLayoutFlags(v, AbsoluteLayoutFlags.PositionProportional);
    }
}