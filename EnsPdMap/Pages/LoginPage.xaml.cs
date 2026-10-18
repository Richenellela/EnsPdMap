using EnsPdMap.Helpers;
using EnsPdMap.Models;

namespace EnsPdMap.Pages;

public partial class LoginPage : ContentPage
{
    bool _showPwd;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        string log = CrashLog.ReadAndClear();
        if (!string.IsNullOrEmpty(log))
            await DisplayAlert("Dernière erreur détectée", log.Length > 1500 ? log.Substring(0, 1500) : log, "OK");
    }

    void OnToggleEye(object sender, TappedEventArgs e)
    {
        _showPwd = !_showPwd;
        PwdEntry.IsPassword = !_showPwd;
        EyeIcon.Glyph = _showPwd ? "eyeoff" : "eye";
    }

    async void OnForgot(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Mot de passe oublié",
            "Un lien de réinitialisation sera envoyé à votre adresse @univ-douala.cm (fonction à brancher sur le back-end).",
            "OK");
    }

    void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    async void OnLogin(object sender, EventArgs e)
    {
        string id = (IdEntry.Text ?? "").Trim();
        string pwd = PwdEntry.Text ?? "";

        if (!AppState.IsValidMatricule(id) && !AppState.IsInstitutionalMail(id))
        {
            ShowError("Entrez un matricule valide (ex. 24G01622) ou une adresse @univ-douala.cm.");
            return;
        }
        if (pwd.Length < 8)
        {
            ShowError("Le mot de passe doit contenir au moins 8 caractères.");
            return;
        }

        ErrorLabel.IsVisible = false;
        AppState.SignInStudent();
        await Nav.GoAsync("//accueil");
    }

    async void OnVisitor(object sender, EventArgs e)
    {
        AppState.SignInVisitor();
        await Nav.GoAsync("//accueil");
    }

    async void OnCreateAccount(object sender, TappedEventArgs e)
    {
        await Nav.GoAsync("//inscription");
    }
}
