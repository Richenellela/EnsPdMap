using EnsPdMap.Helpers;
using EnsPdMap.Models;

namespace EnsPdMap.Pages;

public partial class DemarchesPage : ContentPage
{
    public DemarchesPage()
    {
        InitializeComponent();
        for (int i = 0; i < Data.Procedures.Count; i++)
            ProcList.Children.Add(Ui.ProcedureRow(Data.Procedures[i], i < Data.Procedures.Count - 1));
    }

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
