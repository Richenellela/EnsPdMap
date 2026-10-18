using EnsPdMap.Controls;
using EnsPdMap.Models;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class HorsLignePage : ContentPage
{
    public HorsLignePage()
    {
        InitializeComponent();
        EcoSwitch.IsToggled = AppState.UltraEco;
        BuildDocs();
    }

    void AddDoc(string title, string detail, bool divider, string route)
    {
        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        texts.Children.Add(new Label { Text = title, FontSize = 18, FontAttributes = FontAttributes.Bold });
        texts.Children.Add(new Label { Text = detail, FontSize = 14, TextColor = Pal.Muted });

        var row = new Grid { ColumnSpacing = 14, Padding = new Thickness(0, 16) };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.Add(new Icon { Glyph = "ok", IconColor = Pal.Green, Size = 28, VerticalOptions = LayoutOptions.Center }, 0, 0);
        row.Add(texts, 1, 0);
        row.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Shell.Current.GoToAsync(route))
        });
        DocList.Children.Add(row);
        if (divider) DocList.Children.Add(new BoxView { HeightRequest = 1, Color = Pal.Line });
    }

    void BuildDocs()
    {
        DocList.Children.Clear();
        AddDoc("Retrait d'attestation", "Formulaire + checklist · 184 Ko", true, "procedure?id=attestation");
        AddDoc("Plan du campus PK17", "Carte vectorielle · 96 Ko", true, "//plan");
        AddDoc("Annuaire des bureaux", "24 services · 62 Ko", false, "//annuaire");
    }

    void OnEcoToggled(object sender, ToggledEventArgs e)
    {
        AppState.UltraEco = e.Value;
        EcoDesc.Text = e.Value
            ? "Désactive les images et réduit les animations sans 4G."
            : "Mode ultra-éco désactivé : images et animations activées.";
    }
}
