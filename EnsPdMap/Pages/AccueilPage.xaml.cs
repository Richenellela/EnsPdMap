using System.Globalization;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class AccueilPage : ContentPage
{
    public AccueilPage()
    {
        InitializeComponent();
        BuildLists();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        string date = Today();
        DateLabel.Text = AppState.IsVisitor ? date : date + " · " + AppState.Filiere + " · " + AppState.Niveau;
        HelloLabel.Text = "Bonjour " + AppState.FirstName;
        InitialsLabel.Text = AppState.Initials;
    }

    static string Today()
    {
        try
        {
            string d = DateTime.Now.ToString("dddd d MMMM", new CultureInfo("fr-FR"));
            return char.ToUpper(d[0]) + d.Substring(1);
        }
        catch
        {
            return DateTime.Now.ToString("dd/MM/yyyy");
        }
    }

    void BuildLists()
    {
        FreqList.Children.Clear();
        for (int i = 0; i < Data.Procedures.Count; i++)
            FreqList.Children.Add(Ui.ProcedureRow(Data.Procedures[i], i < Data.Procedures.Count - 1));

        CrowdStack.Children.Clear();
        foreach (var s in Data.Services)
        {
            var dotRow = new HorizontalStackLayout { Spacing = 6 };
            dotRow.Children.Add(new BoxView
            {
                WidthRequest = 8,
                HeightRequest = 8,
                CornerRadius = 4,
                Color = Pal.Crowd(s.Crowd),
                VerticalOptions = LayoutOptions.Center
            });
            dotRow.Children.Add(new Label { Text = s.Crowd, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Pal.Crowd(s.Crowd) });

            var col = new VerticalStackLayout { Spacing = 4 };
            col.Children.Add(dotRow);
            col.Children.Add(new Label { Text = s.Short, FontSize = 19, FontAttributes = FontAttributes.Bold });
            col.Children.Add(new Label
            {
                Text = s.Status == "Fermé" ? "Fermé actuellement" : "Jusqu'à " + s.ClosesAt,
                FontSize = 14,
                TextColor = Pal.Muted
            });

            var card = new Border
            {
                WidthRequest = 170,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                Stroke = new SolidColorBrush(Pal.Line),
                StrokeThickness = 1.5,
                BackgroundColor = Colors.White,
                Padding = new Thickness(16),
                Content = col
            };
            string id = s.Id;
            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await Shell.Current.GoToAsync("bureau?id=" + id))
            });
            CrowdStack.Children.Add(card);
        }
    }

    async void OnSearch(object sender, EventArgs e)
    {
        AppState.PendingQuery = (SearchEntry.Text ?? "").Trim();
        SearchEntry.Text = "";
        await Shell.Current.GoToAsync("//annuaire");
    }

    async void OnQuick(object sender, TappedEventArgs e)
    {
        string target = e.Parameter as string ?? "plan";
        if (target == "demarches")
            await Shell.Current.GoToAsync("demarches");
        else
            await Shell.Current.GoToAsync("//" + target);
    }

    async void OnSeeAll(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("demarches");
    }

    async void OnProgressCard(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("procedure?id=attestation");
    }
}
