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

        PageFx.FadeIn(this);

        // le catalogue a pu changer (synchronisation) ; l'état ouvert/fermé évolue avec l'heure
        if (_stamp != Data.Stamp) BuildFreq();
        BuildCrowd();
        UpdateRentree();
    }

    string _nextId = "";

    /// <summary>Progression réelle : une démarche est « prête » quand toutes ses pièces sont cochées.</summary>
    void UpdateRentree()
    {
        int total = Data.Procedures.Count;
        int ready = 0;
        Procedure next = null;
        foreach (var p in Data.Procedures)
        {
            if (p.AllPiecesReady) ready++;
            else if (next == null) next = p;
        }

        RentreeCount.Text = ready + " / " + total;
        RentreeBar.Progress = total == 0 ? 0 : (double)ready / total;

        if (next == null)
        {
            _nextId = total > 0 ? Data.Procedures[0].Id : "";
            RentreeNext.Text = total == 0 ? "Aucune démarche disponible" : "Toutes vos démarches sont prêtes";
        }
        else
        {
            _nextId = next.Id;
            string todo = "";
            foreach (var piece in next.Pieces)
            {
                if (!piece.Ready) { todo = piece.Name; break; }
            }
            RentreeNext.Text = "Prochaine étape : " + next.Title + (todo.Length > 0 ? " · " + todo : "");
        }
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

    int _stamp = -1;

    void BuildLists()
    {
        BuildFreq();
        BuildCrowd();
    }

    void BuildFreq()
    {
        FreqList.Children.Clear();
        int shown = Math.Min(3, Data.Procedures.Count);
        for (int i = 0; i < shown; i++)
            FreqList.Children.Add(Ui.ProcedureRow(Data.Procedures[i], i < shown - 1));
        _stamp = Data.Stamp;
    }

    void BuildCrowd()
    {
        CrowdStack.Children.Clear();
        foreach (var s in Data.Services)
        {
            bool open = s.Status == "Ouvert";
            string stateText = open ? s.Crowd : "Fermé";
            Color tone = open ? Pal.Crowd(s.Crowd) : Pal.Muted;
            var dotRow = new HorizontalStackLayout { Spacing = 6 };
            dotRow.Children.Add(new BoxView
            {
                WidthRequest = 8,
                HeightRequest = 8,
                CornerRadius = 4,
                Color = tone,
                VerticalOptions = LayoutOptions.Center
            });
            dotRow.Children.Add(new Label { Text = stateText, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = tone });

            var col = new VerticalStackLayout { Spacing = 4 };
            col.Children.Add(dotRow);
            col.Children.Add(new Label { Text = s.Short, FontSize = 19, FontAttributes = FontAttributes.Bold });
            col.Children.Add(new Label
            {
                Text = open ? "Jusqu'à " + s.ClosesAt : s.NextOpening,
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
                Command = new Command(async () => await Nav.GoAsync("bureau?id=" + id))
            });
            CrowdStack.Children.Add(card);
        }
    }

    async void OnSearch(object sender, EventArgs e)
    {
        AppState.PendingQuery = (SearchEntry.Text ?? "").Trim();
        SearchEntry.Text = "";
        await Nav.GoAsync("//main/annuaire");
    }

    async void OnQuick(object sender, TappedEventArgs e)
    {
        string target = e.Parameter as string ?? "plan";
        if (target == "demarches")
            await Nav.GoAsync("demarches");
        else
            await Nav.GoAsync("//main/" + target);
    }

    async void OnSeeAll(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("demarches");
    }

    async void OnProgressCard(object sender, TappedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_nextId)) await Nav.GoAsync("procedure?id=" + _nextId);
    }
}
