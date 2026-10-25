using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnsPdMap.Models;
using Microsoft.Maui.Storage;

namespace EnsPdMap.Helpers;

public class AuthResult
{
    public bool Ok;
    public bool NetworkError;
    public string Message = "";
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public AuthStudent Student { get; set; }
}

public class AuthStudent
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Matricule { get; set; } = "";
    public string Email { get; set; } = "";
    public string Filiere { get; set; } = "";
    public string Niveau { get; set; } = "";
}

/// <summary>Client de l'API EnsPdMap. Toute erreur réseau est capturée : l'application reste utilisable hors ligne.</summary>
public static class ApiClient
{
    // Émulateur Android : 10.0.2.2 désigne le PC. Téléphone réel : http://IP-DU-PC:5080
    public const string DefaultUrl = "http://10.0.2.2:5080";
    const string UrlKey = "api_url";

    static readonly HttpClient Http = new HttpClient();
    static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    static string _token = "";

    public static string Token
    {
        get { return _token; }
        set
        {
            _token = value ?? "";
            Http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(_token)
                ? null
                : new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    public static bool IsSignedIn
    {
        get { return !string.IsNullOrEmpty(_token); }
    }

    public static string BaseUrl
    {
        get { return Preferences.Default.Get(UrlKey, DefaultUrl); }
        set { Preferences.Default.Set(UrlKey, (value ?? "").Trim().TrimEnd('/')); }
    }

    static CancellationTokenSource Timeout(int seconds)
    {
        return new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
    }

    public static async Task<bool> PingAsync()
    {
        try
        {
            using (var cts = Timeout(4))
            {
                var response = await Http.GetAsync(BaseUrl + "/api/health", cts.Token);
                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    // ---------------- Compte ----------------

    public static Task<AuthResult> LoginAsync(string identifier, string password)
    {
        return SendAuthAsync("/api/auth/login", new { identifier, password });
    }

    public static Task<AuthResult> RegisterAsync(string fullName, string matricule, string email, string filiere, string niveau, string password)
    {
        return SendAuthAsync("/api/auth/register", new { fullName, matricule, email, filiere, niveau, password });
    }

    static async Task<AuthResult> SendAuthAsync(string path, object payload)
    {
        try
        {
            using (var cts = Timeout(6))
            {
                var response = await Http.PostAsJsonAsync(BaseUrl + path, payload, JsonOptions, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, cts.Token);
                    if (auth == null || auth.Student == null)
                        return new AuthResult { Message = "Réponse inattendue du serveur." };

                    Token = auth.Token;
                    AppState.Register(auth.Student.FullName, auth.Student.Filiere, auth.Student.Niveau);
                    return new AuthResult { Ok = true };
                }
                return new AuthResult { Message = await ReadMessage(response) };
            }
        }
        catch (Exception)
        {
            return new AuthResult { NetworkError = true, Message = "Serveur injoignable." };
        }
    }

    static async Task<string> ReadMessage(HttpResponseMessage response)
    {
        try
        {
            string body = await response.Content.ReadAsStringAsync();
            using (var doc = JsonDocument.Parse(body))
            {
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    JsonElement message;
                    if (root.TryGetProperty("message", out message) && message.ValueKind == JsonValueKind.String)
                        return message.GetString() ?? "";

                    JsonElement errors;
                    if (root.TryGetProperty("errors", out errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errors.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                                return prop.Value[0].GetString() ?? "";
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // corps non JSON : message générique ci-dessous
        }
        if ((int)response.StatusCode == 401) return "Session expirée : reconnectez-vous.";
        return "Erreur " + (int)response.StatusCode + ".";
    }

    // ---------------- Catalogue (services, démarches, campus) ----------------

    /// <summary>Remplace le catalogue local par celui de l'API et, si connecté, reprend l'avancement du compte.</summary>
    public static async Task<bool> SyncCatalogAsync(int timeoutSeconds = 5)
    {
        try
        {
            CatalogFile file;
            using (var cts = Timeout(timeoutSeconds))
            {
                file = await Http.GetFromJsonAsync<CatalogFile>(BaseUrl + "/api/catalogue", JsonOptions, cts.Token);
            }
            if (file == null || file.Services.Count == 0 || file.Procedures.Count == 0)
                return false;

            Data.Apply(file, true);
        }
        catch (Exception)
        {
            return false;
        }

        if (IsSignedIn) await PullProgressAsync();
        return true;
    }

    // ---------------- Avancement des démarches ----------------

    public static async Task PullProgressAsync()
    {
        try
        {
            using (var cts = Timeout(5))
            {
                var items = await Http.GetFromJsonAsync<List<ApiProgress>>(BaseUrl + "/api/me/progress", JsonOptions, cts.Token);
                if (items != null) ProgressStore.MergeFromServer(items);
            }
        }
        catch (Exception)
        {
            // on garde l'avancement du téléphone
        }
    }

    public static async Task PushProgressAsync(string procedureId, ProgressRecord record)
    {
        if (!IsSignedIn) return;
        try
        {
            using (var cts = Timeout(5))
            {
                var body = new { readyPieces = record.ReadyPieces, doneSteps = record.DoneSteps, bookmarked = record.Bookmarked };
                await Http.PutAsJsonAsync(BaseUrl + "/api/me/progress/" + Uri.EscapeDataString(procedureId), body, JsonOptions, cts.Token);
            }
        }
        catch (Exception)
        {
            // sera renvoyé à la prochaine modification
        }
    }

    // ---------------- Affluence signalée ----------------

    public static async Task<AuthResult> ReportCrowdAsync(string serviceId, string level)
    {
        if (!IsSignedIn)
            return new AuthResult { Message = "Connectez-vous avec votre compte pour signaler l'affluence." };
        try
        {
            using (var cts = Timeout(6))
            {
                var response = await Http.PostAsJsonAsync(BaseUrl + "/api/services/" + Uri.EscapeDataString(serviceId) + "/crowd", new { level }, JsonOptions, cts.Token);
                if (response.IsSuccessStatusCode) return new AuthResult { Ok = true };
                return new AuthResult { Message = await ReadMessage(response) };
            }
        }
        catch (Exception)
        {
            return new AuthResult { NetworkError = true, Message = "Serveur injoignable." };
        }
    }

    // ---------------- Étalonnage du plan ----------------

    public static async Task<AuthResult> PushCalibrationAsync(Calibration c)
    {
        if (!IsSignedIn)
            return new AuthResult { Message = "Connectez-vous avec votre compte pour partager l'étalonnage." };
        try
        {
            using (var cts = Timeout(6))
            {
                var response = await Http.PostAsJsonAsync(BaseUrl + "/api/campus/calibration", c, JsonOptions, cts.Token);
                if (response.IsSuccessStatusCode) return new AuthResult { Ok = true };
                return new AuthResult { Message = await ReadMessage(response) };
            }
        }
        catch (Exception)
        {
            return new AuthResult { NetworkError = true, Message = "Serveur injoignable." };
        }
    }
}
