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

/// <summary>Client de l'API EnsPdMap. Toute erreur réseau est capturée : l'application reste utilisable hors ligne.</summary>
public static class ApiClient
{
    // Émulateur Android : 10.0.2.2 désigne le PC. Téléphone réel : http://IP-DU-PC:5080
    public const string DefaultUrl = "http://10.0.2.2:5080";
    const string UrlKey = "api_url";

    static readonly HttpClient Http = new HttpClient();
    static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static string Token = "";

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
                    var auth = await response.Content.ReadFromJsonAsync<ApiAuthResponse>(JsonOptions, cts.Token);
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
        return "Erreur " + (int)response.StatusCode + ".";
    }

    /// <summary>Remplace le catalogue local par celui de l'API. Retourne false si le serveur est injoignable.</summary>
    public static async Task<bool> SyncCatalogAsync(int timeoutSeconds = 5)
    {
        try
        {
            using (var cts = Timeout(timeoutSeconds))
            {
                var services = await Http.GetFromJsonAsync<List<ApiService>>(BaseUrl + "/api/services", JsonOptions, cts.Token);
                var procedures = await Http.GetFromJsonAsync<List<ApiProcedure>>(BaseUrl + "/api/procedures", JsonOptions, cts.Token);
                if (services == null || services.Count == 0 || procedures == null || procedures.Count == 0)
                    return false;

                var localServices = new List<Service>();
                foreach (var s in services) localServices.Add(Map(s));
                var localProcedures = new List<Procedure>();
                foreach (var p in procedures) localProcedures.Add(Map(p));

                Data.Replace(localServices, localProcedures);
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    static Service Map(ApiService s)
    {
        var service = new Service
        {
            Id = s.Id, Code = s.Code, Name = s.Name, Short = s.ShortName, SheetTitle = s.SheetTitle,
            Description = s.Description, Category = s.Category, Block = s.Block, Floor = s.Floor,
            ListLoc = s.ListLoc, Place = s.Place, Landmark = s.Landmark, Status = s.Status, Crowd = s.Crowd,
            ClosesAt = s.ClosesAt, Manager = s.Manager, Phone = s.Phone, Email = s.Email, Queue = s.Queue,
            Distance = s.Distance, Minutes = s.Minutes
        };
        foreach (var h in s.Hours)
            service.Hours.Add(new[] { h.Day, h.Time });
        return service;
    }

    static Procedure Map(ApiProcedure p)
    {
        var procedure = new Procedure
        {
            Id = p.Id, Category = p.Category, Title = p.Title, Delay = p.Delay, Cost = p.Cost, Office = p.Office,
            DoorNumber = p.DoorNumber, BadgeTop = p.BadgeTop, BadgeMain = p.BadgeMain, ServiceId = p.ServiceId,
            FormSize = p.FormSize, FicheFile = p.FicheFile, LocateLabel = p.LocateLabel
        };
        foreach (var x in p.Pieces)
            procedure.Pieces.Add(new Piece { Name = x.Name, Detail = x.Detail });
        foreach (var x in p.Steps)
            procedure.Steps.Add(new Step { Title = x.Title, Detail = x.Detail });
        return procedure;
    }
}
