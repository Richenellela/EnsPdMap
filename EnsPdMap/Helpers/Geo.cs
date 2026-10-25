using EnsPdMap.Models;

namespace EnsPdMap.Helpers;

public struct PointD
{
    public double X;
    public double Y;

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static double Dist(PointD a, PointD b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public class RouteResult
{
    public List<PointD> Points = new List<PointD>();
    public double Length;
    public List<string> Instructions = new List<string>();
}

/// <summary>Conversions GPS ⇄ plan, caps, et calcul d'itinéraire sur le réseau d'allées du campus.</summary>
public static class Geo
{
    const double EarthRadius = 6371000;
    public const double WalkMetersPerMinute = 78; // ≈ 4,7 km/h

    // ---------- GPS -> plan ----------

    public static PointD ToSchematic(Calibration c, double lat, double lon)
    {
        double lat0 = c.OriginLat * Math.PI / 180;
        double e = (lon - c.OriginLon) * Math.PI / 180 * Math.Cos(lat0) * EarthRadius;
        double n = (lat - c.OriginLat) * Math.PI / 180 * EarthRadius;
        double th = c.RotationDeg * Math.PI / 180;
        double x = c.Scale * (e * Math.Cos(th) - n * Math.Sin(th));
        double y = c.Scale * (e * Math.Sin(th) + n * Math.Cos(th));
        return new PointD(c.AnchorX + x, c.AnchorY + y);
    }

    /// <summary>Déplacement Est / Nord en mètres entre l'origine de l'étalonnage et un point GPS.</summary>
    public static PointD EastNorth(double originLat, double originLon, double lat, double lon)
    {
        double lat0 = originLat * Math.PI / 180;
        double e = (lon - originLon) * Math.PI / 180 * Math.Cos(lat0) * EarthRadius;
        double n = (lat - originLat) * Math.PI / 180 * EarthRadius;
        return new PointD(e, n);
    }

    /// <summary>Étalonnage avec un seul point : on pose le plan sur la position GPS actuelle.</summary>
    public static Calibration FromOneAnchor(double lat, double lon, PointD schematic)
    {
        return new Calibration { OriginLat = lat, OriginLon = lon, AnchorX = schematic.X, AnchorY = schematic.Y, RotationDeg = 0, Scale = 1 };
    }

    /// <summary>Étalonnage à deux points : calcule aussi la rotation et l'échelle du plan.</summary>
    public static Calibration FromTwoAnchors(Calibration first, double lat2, double lon2, PointD schematic2)
    {
        var e2 = EastNorth(first.OriginLat, first.OriginLon, lat2, lon2);
        var s1 = new PointD(first.AnchorX, first.AnchorY);
        double sx = schematic2.X - s1.X, sy = schematic2.Y - s1.Y;
        double angleS = Math.Atan2(sy, sx);
        double angleE = Math.Atan2(e2.Y, e2.X);
        double rot = (angleS - angleE) * 180 / Math.PI;
        double len = Math.Sqrt(e2.X * e2.X + e2.Y * e2.Y);
        double scale = len < 0.1 ? 1 : Math.Sqrt(sx * sx + sy * sy) / len;
        scale = Math.Max(0.5, Math.Min(2.0, scale));
        return new Calibration { OriginLat = first.OriginLat, OriginLon = first.OriginLon, AnchorX = first.AnchorX, AnchorY = first.AnchorY, RotationDeg = rot, Scale = scale };
    }

    // ---------- Caps ----------

    /// <summary>Cap réel (0° = nord, sens horaire) pour aller d'un point du plan à un autre.</summary>
    public static double TrueBearing(Calibration c, PointD from, PointD to)
    {
        double dx = (to.X - from.X) / c.Scale, dy = (to.Y - from.Y) / c.Scale;
        double th = -c.RotationDeg * Math.PI / 180;
        double e = dx * Math.Cos(th) - dy * Math.Sin(th);
        double n = dx * Math.Sin(th) + dy * Math.Cos(th);
        double deg = Math.Atan2(e, n) * 180 / Math.PI;
        return (deg + 360) % 360;
    }

    /// <summary>Cap compas (0° = nord) converti en angle sur le plan (radians, y vers le haut).</summary>
    public static double HeadingToSchematic(Calibration c, double headingDeg)
    {
        double h = headingDeg * Math.PI / 180;
        double e = Math.Sin(h), n = Math.Cos(h);
        double th = c.RotationDeg * Math.PI / 180;
        double x = e * Math.Cos(th) - n * Math.Sin(th);
        double y = e * Math.Sin(th) + n * Math.Cos(th);
        return Math.Atan2(y, x);
    }

    public static string Cardinal(double bearingDeg)
    {
        string[] names = { "le nord", "le nord-est", "l'est", "le sud-est", "le sud", "le sud-ouest", "l'ouest", "le nord-ouest" };
        int i = (int)Math.Round(bearingDeg / 45.0) % 8;
        return names[i];
    }

    /// <summary>Position du lieu par rapport à la direction dans laquelle regarde l'utilisateur.</summary>
    public static string RelativeSide(double headingSchematic, PointD from, PointD to)
    {
        double ta = Math.Atan2(to.Y - from.Y, to.X - from.X);
        double diff = ta - headingSchematic;
        while (diff > Math.PI) diff -= 2 * Math.PI;
        while (diff <= -Math.PI) diff += 2 * Math.PI;
        double deg = diff * 180 / Math.PI;
        if (Math.Abs(deg) < 30) return "devant vous";
        if (Math.Abs(deg) > 150) return "derrière vous";
        return deg > 0 ? "sur votre gauche" : "sur votre droite";
    }

    // ---------- Itinéraire ----------

    /// <summary>Plus court chemin sur les allées, de n'importe quel point du plan jusqu'à l'entrée d'un lieu.</summary>
    public static RouteResult Route(Campus campus, Calibration cal, PointD start, string targetNodeId, string destinationLabel)
    {
        var result = new RouteResult();
        var target = campus.Node(targetNodeId);
        if (target == null || campus.Nodes.Count == 0) return result;

        // Dijkstra depuis la cible : dist[n] = distance restante, next[n] = nœud suivant vers la cible
        var dist = new Dictionary<string, double>();
        var next = new Dictionary<string, string>();
        var done = new HashSet<string>();
        foreach (var n in campus.Nodes) dist[n.Id] = double.MaxValue;
        dist[target.Id] = 0;

        while (true)
        {
            string cur = null;
            double best = double.MaxValue;
            foreach (var kv in dist)
            {
                if (!done.Contains(kv.Key) && kv.Value < best) { best = kv.Value; cur = kv.Key; }
            }
            if (cur == null) break;
            done.Add(cur);

            foreach (var e in campus.Edges)
            {
                string other = e.A == cur ? e.B : (e.B == cur ? e.A : null);
                if (other == null || done.Contains(other)) continue;
                var a = campus.Node(cur);
                var b = campus.Node(other);
                if (a == null || b == null) continue;
                double d = best + PointD.Dist(new PointD(a.X, a.Y), new PointD(b.X, b.Y));
                if (d < dist[other])
                {
                    dist[other] = d;
                    next[other] = cur;
                }
            }
        }

        // Projection du point de départ sur l'allée la plus proche
        double bestPerp = double.MaxValue;
        PointD bestProj = start;
        CampusEdge bestEdge = null;
        foreach (var e in campus.Edges)
        {
            var a = campus.Node(e.A);
            var b = campus.Node(e.B);
            if (a == null || b == null) continue;
            double abx = b.X - a.X, aby = b.Y - a.Y;
            double len2 = abx * abx + aby * aby;
            double t = len2 < 1e-9 ? 0 : ((start.X - a.X) * abx + (start.Y - a.Y) * aby) / len2;
            t = Math.Max(0, Math.Min(1, t));
            var proj = new PointD(a.X + t * abx, a.Y + t * aby);
            double d = PointD.Dist(start, proj);
            if (d < bestPerp) { bestPerp = d; bestProj = proj; bestEdge = e; }
        }
        if (bestEdge == null) return result;

        var na = campus.Node(bestEdge.A);
        var nb = campus.Node(bestEdge.B);
        double viaA = PointD.Dist(bestProj, new PointD(na.X, na.Y)) + dist[na.Id];
        double viaB = PointD.Dist(bestProj, new PointD(nb.X, nb.Y)) + dist[nb.Id];
        string firstNode = viaA <= viaB ? na.Id : nb.Id;

        result.Points.Add(start);
        if (bestPerp > 0.5) result.Points.Add(bestProj);

        string node = firstNode;
        int guard = 0;
        while (node != null && guard++ < 200)
        {
            var n = campus.Node(node);
            result.Points.Add(new PointD(n.X, n.Y));
            if (node == target.Id) break;
            string nx;
            node = next.TryGetValue(node, out nx) ? nx : null;
        }

        // nettoyage : retire les points quasi identiques
        var clean = new List<PointD>();
        foreach (var p in result.Points)
        {
            if (clean.Count == 0 || PointD.Dist(clean[clean.Count - 1], p) > 0.5) clean.Add(p);
        }
        result.Points = clean;

        for (int i = 1; i < clean.Count; i++) result.Length += PointD.Dist(clean[i - 1], clean[i]);
        result.Instructions = BuildInstructions(cal, clean, destinationLabel);
        return result;
    }

    static string Round5(double meters)
    {
        int m = (int)(Math.Round(meters / 5.0) * 5);
        if (m < 5) m = 5;
        return m + " m";
    }

    static List<string> BuildInstructions(Calibration cal, List<PointD> pts, string destination)
    {
        var steps = new List<string>();
        if (pts.Count < 2)
        {
            steps.Add("Vous êtes arrivé : " + destination + ".");
            return steps;
        }

        // regroupe les segments quasi alignés
        var legs = new List<(PointD From, PointD To)>();
        PointD legStart = pts[0];
        double legAngle = Math.Atan2(pts[1].Y - pts[0].Y, pts[1].X - pts[0].X);
        for (int i = 1; i < pts.Count; i++)
        {
            double ang = Math.Atan2(pts[i].Y - pts[i - 1].Y, pts[i].X - pts[i - 1].X);
            double diff = Math.Abs(NormalizeAngle(ang - legAngle)) * 180 / Math.PI;
            if (i > 1 && diff > 25)
            {
                legs.Add((legStart, pts[i - 1]));
                legStart = pts[i - 1];
                legAngle = ang;
            }
        }
        legs.Add((legStart, pts[pts.Count - 1]));

        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            double meters = PointD.Dist(leg.From, leg.To);
            double ang = Math.Atan2(leg.To.Y - leg.From.Y, leg.To.X - leg.From.X);
            string prefix;
            if (i == 0)
            {
                prefix = "Marchez vers " + Cardinal(TrueBearing(cal, leg.From, leg.To));
            }
            else
            {
                var prev = legs[i - 1];
                double prevAng = Math.Atan2(prev.To.Y - prev.From.Y, prev.To.X - prev.From.X);
                double turn = NormalizeAngle(ang - prevAng) * 180 / Math.PI;
                if (Math.Abs(turn) < 25) prefix = "Continuez tout droit";
                else if (turn > 0) prefix = Math.Abs(turn) > 120 ? "Faites demi-tour à gauche" : "Tournez à gauche";
                else prefix = Math.Abs(turn) > 120 ? "Faites demi-tour à droite" : "Tournez à droite";
            }
            steps.Add(prefix + " sur " + Round5(meters) + ".");
        }
        steps.Add("Vous êtes arrivé : " + destination + ".");
        return steps;
    }

    static double NormalizeAngle(double a)
    {
        while (a > Math.PI) a -= 2 * Math.PI;
        while (a <= -Math.PI) a += 2 * Math.PI;
        return a;
    }

    public static string FormatDistance(double meters)
    {
        if (meters >= 1000) return (meters / 1000.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',') + " km";
        return Round5(meters);
    }
}
