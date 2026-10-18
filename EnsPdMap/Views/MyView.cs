using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts; // Ajouté pour AbsoluteLayoutFlags

namespace EnsPdMap.Views
{
    // Helper pour positionner une vue dans un AbsoluteLayout.
    public static class MyViewHelper
    {
        public static void SetPositionProportional(View myView)
        {
            if (myView == null) throw new ArgumentNullException(nameof(myView));
            AbsoluteLayout.SetLayoutFlags(myView, AbsoluteLayoutFlags.PositionProportional);
        }
    }
}