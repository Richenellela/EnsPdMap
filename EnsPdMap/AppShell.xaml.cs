using EnsPdMap.Pages;

namespace EnsPdMap;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages ouvertes "par-dessus" (avec bouton retour)
        Routing.RegisterRoute("procedure", typeof(ProcedurePage));
        Routing.RegisterRoute("bureau", typeof(BureauPage));
        Routing.RegisterRoute("demarches", typeof(DemarchesPage));
        Routing.RegisterRoute("etalonnage", typeof(EtalonnagePage));
    }
}
