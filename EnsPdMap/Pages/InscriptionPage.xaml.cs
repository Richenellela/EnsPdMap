using EnsPdMap.Controls;
using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Pages;

public partial class InscriptionPage : ContentPage
{
    string _filiere = "";
    string _niveau = "";
    bool _showPwd;

    static readonly string[] Filieres = { "GI", "GE", "GM", "GC", "GP" };
    static readonly string[] Niveaux = { "N1", "N2", "N3", "N4", "N5" };

    public InscriptionPage()
    {
        InitializeComponent();
        BuildChips();
    }

    void BuildChips()
    {
        FilFlex.Children.Clear();
        foreach (var f in Filieres)
        {
            string value = f;
            var chip = Ui.Chip(f, f == _filiere, () => { _filiere = value; BuildChips(); });
            chip.Margin = new Thickness(0, 0, 8, 8);
            FilFlex.Children.Add(chip);
        }

        NivFlex.Children.Clear();
        foreach (var n in Niveaux)
        {
            string value = n;
            var chip = Ui.Chip(n, n == _niveau, () => { _niveau = value; BuildChips(); });
            chip.Margin = new Thickness(0, 0, 8, 8);
            NivFlex.Children.Add(chip);
        }
    }

    // null = neutre, true = valide, false = erreur
    void SetState(Border box, Icon icon, bool? ok)
    {
        if (ok == null)
        {
            box.Stroke = new SolidColorBrush(Pal.Line);
            box.BackgroundColor = Colors.White;
            icon.IsVisible = false;
        }
        else if (ok == true)
        {
            box.Stroke = new SolidColorBrush(Pal.Green);
            box.BackgroundColor = Colors.White;
            icon.Glyph = "ok";
            icon.IconColor = Pal.Green;
            icon.IsVisible = true;
        }
        else
        {
            box.Stroke = new SolidColorBrush(Pal.Red);
            box.BackgroundColor = Pal.RedSoft;
            icon.Glyph = "alert";
            icon.IconColor = Pal.Red;
            icon.IsVisible = true;
        }
    }

    void OnNameChanged(object sender, TextChangedEventArgs e)
    {
        string t = (e.NewTextValue ?? "").Trim();
        SetState(NameBox, NameIcon, t.Length == 0 ? (bool?)null : t.Length >= 3);
    }

    void OnMatChanged(object sender, TextChangedEventArgs e)
    {
        string t = (e.NewTextValue ?? "").Trim();
        if (t.Length == 0)
        {
            SetState(MatBox, MatIcon, null);
            MatHint.TextColor = Pal.Muted;
            return;
        }
        bool ok = AppState.IsValidMatricule(t);
        SetState(MatBox, MatIcon, ok);
        MatHint.TextColor = ok ? Pal.Green : Pal.Red;
    }

    void OnMailChanged(object sender, TextChangedEventArgs e)
    {
        string t = (e.NewTextValue ?? "").Trim();
        if (t.Length == 0)
        {
            SetState(MailBox, MailIcon, null);
            MailLabel.TextColor = Pal.Ink;
            MailHint.IsVisible = false;
            return;
        }
        bool ok = AppState.IsInstitutionalMail(t);
        SetState(MailBox, MailIcon, ok);
        MailLabel.TextColor = ok ? Pal.Ink : Pal.Red;
        MailHint.IsVisible = !ok;
    }

    void OnPwdChanged(object sender, TextChangedEventArgs e)
    {
        string p = e.NewTextValue ?? "";
        int score = 0;
        if (p.Length >= 8) score++;
        if (p.Any(char.IsDigit)) score++;
        if (p.Any(char.IsUpper) && p.Any(char.IsLower)) score++;
        if (p.Any(ch => !char.IsLetterOrDigit(ch))) score++;
        if (p.Length == 0) score = 0;
        if (p.Length < 8 && score > 1) score = 1;

        Color on = score <= 1 ? Pal.Red : (score == 2 ? Pal.Gold : Pal.Green);
        var segs = new[] { S1, S2, S3, S4 };
        for (int i = 0; i < segs.Length; i++)
            segs[i].Color = i < score ? on : Pal.Line;

        string[] labels = { "", "Faible", "Moyen", "Bon", "Fort" };
        StrengthLabel.Text = labels[score];
        StrengthLabel.TextColor = score <= 1 ? Pal.Red : (score == 2 ? Pal.GoldDark : Pal.Green);
    }

    void OnToggleEye(object sender, TappedEventArgs e)
    {
        _showPwd = !_showPwd;
        PwdEntry.IsPassword = !_showPwd;
        EyeIcon.Glyph = _showPwd ? "eyeoff" : "eye";
    }

    async void OnBack(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("//login");
    }

    void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    async void OnSubmit(object sender, EventArgs e)
    {
        string name = (NameEntry.Text ?? "").Trim();
        if (name.Length < 3) { ShowError("Saisissez votre nom complet."); return; }
        if (!AppState.IsValidMatricule(MatEntry.Text)) { ShowError("Matricule invalide (format attendu : 24G01622)."); return; }
        if (_filiere == "") { ShowError("Choisissez votre filière."); return; }
        if (_niveau == "") { ShowError("Choisissez votre niveau."); return; }
        if (!AppState.IsInstitutionalMail(MailEntry.Text)) { ShowError("Utilisez une adresse @univ-douala.cm."); return; }
        if ((PwdEntry.Text ?? "").Length < 8) { ShowError("Le mot de passe doit contenir au moins 8 caractères."); return; }
        if (!CharteCheck.IsChecked) { ShowError("Vous devez accepter la charte informatique."); return; }

        ErrorLabel.IsVisible = false;
        AppState.Register(name, _filiere, _niveau);
        await DisplayAlert("Compte créé", "Bienvenue sur EnsPdMap, " + AppState.FirstName + " !", "Continuer");
        await Nav.GoAsync("//accueil");
    }
}
