using EnsPdMap.Controls;
using EnsPdMap.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

[QueryProperty(nameof(ItemId), "id")]
public partial class BureauPage : ContentPage
{
    Service _s;

    public string ItemId
    {
        set
        {
            _s = Data.GetService(value);
            Load();
        }
    }

    public BureauPage()
    {
        InitializeComponent();
    }

    void Load()
    {
        TitleLabel.Text = _s.SheetTitle;
        bool open = _s.Status == "Ouvert";
        StatusLabel.Text = _s.Status;
        StatusLabel.TextColor = open ? Pal.Green : Pal.Red;
        StatusBadge.BackgroundColor = open ? Pal.GreenSoft : Pal.RedSoft;
        PlaceLabel.Text = _s.Place;
        LandmarkLabel.Text = _s.Landmark;
        QueueLabel.Text = _s.Queue;

        if (open)
        {
            QueueBox.BackgroundColor = Pal.Sand;
            ClosesLabel.Text = "Ferme à " + _s.ClosesAt;
            ClosesLabel.TextColor = Pal.Green;
        }
        else
        {
            QueueBox.BackgroundColor = Pal.RedSoft;
            QueueLabel.TextColor = Pal.Red;
            ClosesLabel.Text = "Fermé";
            ClosesLabel.TextColor = Pal.Red;
        }

        BuildHours();
        BuildContacts();
    }

    void BuildHours()
    {
        HoursList.Children.Clear();
        for (int i = 0; i < _s.Hours.Count; i++)
        {
            string day = _s.Hours[i][0];
            string time = _s.Hours[i][1];
            bool today = i == 0;
            bool closed = time == "Fermé";

            var left = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
            left.Children.Add(new Label
            {
                Text = day,
                FontSize = 16,
                FontAttributes = today ? FontAttributes.Bold : FontAttributes.None
            });
            if (today)
                left.Children.Add(new Label
                {
                    Text = "Aujourd'hui",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Pal.GoldDark,
                    VerticalOptions = LayoutOptions.Center
                });

            var row = new Grid { Padding = new Thickness(14, 12) };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.Add(left, 0, 0);
            row.Add(new Label
            {
                Text = time,
                FontSize = 16,
                FontAttributes = today ? FontAttributes.Bold : FontAttributes.None,
                TextColor = closed ? Pal.Red : Pal.Ink,
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);

            var wrap = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                StrokeThickness = 0,
                BackgroundColor = today ? Pal.Sand : Colors.Transparent,
                Content = row
            };
            HoursList.Children.Add(wrap);
        }
    }

    void AddContact(string glyph, string title, string value, bool divider)
    {
        var texts = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        texts.Children.Add(new Label { Text = title, FontSize = 14, TextColor = Pal.Muted });
        texts.Children.Add(new Label { Text = value, FontSize = 17, FontAttributes = FontAttributes.Bold });

        var row = new Grid { ColumnSpacing = 16, Padding = new Thickness(0, 14) };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.Add(new Icon { Glyph = glyph, IconColor = Pal.Muted, Size = 24, VerticalOptions = LayoutOptions.Center }, 0, 0);
        row.Add(texts, 1, 0);
        ContactList.Children.Add(row);
        if (divider) ContactList.Children.Add(new BoxView { HeightRequest = 1, Color = Pal.Line });
    }

    void BuildContacts()
    {
        ContactList.Children.Clear();
        AddContact("user", "Responsable", _s.Manager, true);
        AddContact("phone", "Téléphone", _s.Phone, true);
        AddContact("mail", "E-mail", _s.Email, false);
    }

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void OnCall(object sender, TappedEventArgs e)
    {
        try
        {
            if (PhoneDialer.Default.IsSupported)
                PhoneDialer.Default.Open(_s.Phone.Replace(" ", ""));
            else
                await DisplayAlert("Appeler", "Numéro : " + _s.Phone, "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Appeler", "Numéro : " + _s.Phone, "OK");
        }
    }

    async void OnMail(object sender, TappedEventArgs e)
    {
        try
        {
            var message = new EmailMessage
            {
                Subject = "Demande d'information - " + _s.Name,
                To = new List<string> { _s.Email }
            };
            await Email.Default.ComposeAsync(message);
        }
        catch (Exception)
        {
            await DisplayAlert("E-mail", "Adresse : " + _s.Email, "OK");
        }
    }

    async void OnRoute(object sender, TappedEventArgs e)
    {
        AppState.PlanTarget = _s.Id;
        await Shell.Current.GoToAsync("//plan");
    }
}
