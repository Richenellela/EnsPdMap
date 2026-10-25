using EnsPdMap.Models;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace EnsPdMap.Helpers;

/// <summary>Suivi de la position GPS et du cap (boussole). Toutes les erreurs sont converties en message d'état.</summary>
public static class Locator
{
    public static event Action Changed;

    public static bool HasFix;
    public static double Lat;
    public static double Lon;
    public static double Accuracy = 50;
    public static double HeadingDeg = double.NaN;
    public static string Status = "";

    // Mode démo : on place « l'utilisateur » en touchant la carte (utile sans GPS ni étalonnage)
    public static bool Demo;
    public static PointD DemoPoint = new PointD(0, 0);

    static bool _running;
    static int _loopId;
    static DateTime _lastFixUtc = DateTime.MinValue;
    static double _lastNotifiedHeading = -999;

    static void Notify()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var handler = Changed;
            if (handler != null) handler();
        });
    }

    public static void SetDemoPoint(PointD p)
    {
        DemoPoint = p;
        Notify();
    }

    public static async Task StartAsync()
    {
        if (_running) return;

        if (Demo)
        {
            Status = "Mode démo";
            Notify();
            return;
        }

        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
                permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
            {
                Status = "Localisation refusée : autorisez-la dans les réglages du téléphone.";
                Notify();
                return;
            }

            _running = true;
            Status = "Recherche du signal GPS…";
            Notify();

            StartCompass();

            // première position rapide
            try
            {
                var first = await Geolocation.Default.GetLastKnownLocationAsync();
                if (first != null) Apply(first);
            }
            catch (Exception)
            {
                // sans importance : l'écoute continue prend le relais
            }

            Geolocation.Default.LocationChanged += OnLocationChanged;
            try
            {
                await Geolocation.Default.StartListeningForegroundAsync(
                    new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2)));
            }
            catch (FeatureNotEnabledException)
            {
                throw;
            }
            catch (Exception)
            {
                // certains téléphones refusent l'écoute continue : le sondage ci-dessous prend le relais
            }

            // Sondage de secours : demande une position toutes les quelques secondes
            // (précision moyenne puis réseau) quand l'écoute continue ne donne rien, par exemple à l'intérieur.
            int id = ++_loopId;
            _ = PollLoop(id);
        }
        catch (FeatureNotEnabledException)
        {
            Status = "GPS désactivé : activez la localisation du téléphone.";
            Notify();
        }
        catch (Exception ex)
        {
            Status = "GPS indisponible : " + ex.Message;
            CrashLog.Write(ex, "GPS");
            Notify();
        }
    }

    static async Task PollLoop(int id)
    {
        int misses = 0;
        while (_running && id == _loopId)
        {
            try
            {
                bool recent = (DateTime.UtcNow - _lastFixUtc).TotalSeconds < 6;
                if (!recent)
                {
                    Location location = null;
                    try
                    {
                        location = await Geolocation.Default.GetLocationAsync(
                            new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(6)));
                    }
                    catch (FeatureNotEnabledException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // on essaie une précision plus faible
                    }

                    if (location == null)
                    {
                        try
                        {
                            location = await Geolocation.Default.GetLocationAsync(
                                new GeolocationRequest(GeolocationAccuracy.Lowest, TimeSpan.FromSeconds(6)));
                        }
                        catch (FeatureNotEnabledException)
                        {
                            throw;
                        }
                        catch (Exception)
                        {
                            // toujours rien
                        }
                    }

                    if (location != null)
                    {
                        misses = 0;
                        Apply(location);
                    }
                    else
                    {
                        misses++;
                        if (misses >= 3 && !HasFix)
                        {
                            Status = "Signal GPS faible : placez-vous à l'extérieur, ou utilisez le mode démo.";
                            Notify();
                        }
                    }
                }
            }
            catch (FeatureNotEnabledException)
            {
                Status = "GPS désactivé : activez la localisation du téléphone.";
                Notify();
            }
            catch (Exception)
            {
                // on réessaie au prochain tour
            }

            await Task.Delay(2500);
        }
    }

    public static void Stop()
    {
        if (!_running) return;
        _running = false;
        _loopId++;
        try
        {
            Geolocation.Default.LocationChanged -= OnLocationChanged;
            Geolocation.Default.StopListeningForeground();
        }
        catch (Exception)
        {
            // rien à faire
        }
        try
        {
            if (Compass.Default.IsSupported && Compass.Default.IsMonitoring)
            {
                Compass.Default.ReadingChanged -= OnCompass;
                Compass.Default.Stop();
            }
        }
        catch (Exception)
        {
            // rien à faire
        }
    }

    static void StartCompass()
    {
        try
        {
            if (Compass.Default.IsSupported && !Compass.Default.IsMonitoring)
            {
                Compass.Default.ReadingChanged += OnCompass;
                Compass.Default.Start(SensorSpeed.UI);
            }
        }
        catch (Exception)
        {
            HeadingDeg = double.NaN;
        }
    }

    static void OnCompass(object sender, CompassChangedEventArgs e)
    {
        double h = e.Reading.HeadingMagneticNorth;
        if (double.IsNaN(HeadingDeg))
        {
            HeadingDeg = h;
        }
        else
        {
            double diff = ((h - HeadingDeg + 540) % 360) - 180;
            HeadingDeg = (HeadingDeg + diff * 0.25 + 360) % 360;
        }
        double change = Math.Abs(((HeadingDeg - _lastNotifiedHeading + 540) % 360) - 180);
        if (change > 4)
        {
            _lastNotifiedHeading = HeadingDeg;
            Notify();
        }
    }

    static void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
    {
        Apply(e.Location);
    }

    static void Apply(Location location)
    {
        Lat = location.Latitude;
        Lon = location.Longitude;
        Accuracy = location.Accuracy ?? 50;
        HasFix = true;
        _lastFixUtc = DateTime.UtcNow;
        Status = "";
        Notify();
    }
}
