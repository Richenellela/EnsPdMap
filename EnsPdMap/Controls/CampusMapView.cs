using EnsPdMap.Helpers;
using EnsPdMap.Models;
using Microsoft.Maui.Graphics;

namespace EnsPdMap.Controls;

public class MapTapArgs : EventArgs
{
    public PointD Point;
    public string PlaceId = "";   // vide si le toucher n'est sur aucun bâtiment cliquable
}

/// <summary>Carte du campus dessinée en vectoriel : bâtiments, allées, itinéraire, destination, position, boussole et échelle.</summary>
public class CampusMapView : GraphicsView, IDrawable
{
    public Campus Campus = new Campus();
    public bool HasUser;
    public PointD User;
    public double UserAccuracy = 10;
    public double UserHeading = double.NaN;      // radians sur le plan, NaN si inconnu
    public string SelectedPlaceId = "";
    public List<PointD> RoutePoints = new List<PointD>();
    public bool HasDestination;
    public PointD Destination;
    public string DestinationLabel = "";
    public double NorthAngle = Math.PI / 2;      // direction du vrai nord sur le plan (radians)
    public HashSet<string> Dimmed = new HashSet<string>();

    // zones recouvertes par la barre de recherche (haut) et la fiche repliée (bas)
    public double InsetTop = 205;
    public double InsetBottom = 255;

    public event EventHandler<MapTapArgs> MapTapped;

    double _scale = 3;
    double _offX, _offY;
    double _startScale, _startOffX, _startOffY;
    bool _fitted;

    static readonly Color Ground = Color.FromArgb("#E8EEF3");
    static readonly Color RoadEdge = Color.FromArgb("#B8C4D2");
    static readonly Color Roof = Color.FromArgb("#D3DDE8");
    static readonly Color RoofEdge = Color.FromArgb("#8FA0B4");
    static readonly Color BaseWall = Color.FromArgb("#9FB0C3");
    static readonly Color Tree = Color.FromArgb("#C6DCC4");
    static readonly Color TreeDark = Color.FromArgb("#B2CFAF");
    static readonly Color Blue = Color.FromArgb("#0A66C2");

    public CampusMapView()
    {
        Drawable = this;

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;
        GestureRecognizers.Add(pan);

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinch;
        GestureRecognizers.Add(pinch);

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTap;
        GestureRecognizers.Add(tap);

        SizeChanged += (s, e) =>
        {
            if (!_fitted && Width > 0 && Height > 0)
            {
                _fitted = true;
                FitAll();
            }
        };
    }

    // ---------- repères écran <-> plan ----------

    double CenterX { get { return Width / 2; } }
    double CenterY { get { return InsetTop + (Height - InsetTop - InsetBottom) / 2; } }

    float SX(double x) { return (float)(CenterX + _offX + x * _scale); }
    float SY(double y) { return (float)(CenterY + _offY - y * _scale); }

    PointD ToWorld(Point p)
    {
        return new PointD((p.X - CenterX - _offX) / _scale, -(p.Y - CenterY - _offY) / _scale);
    }

    static IEnumerable<PointD> Corners(CampusPlace p)
    {
        double a = p.Angle * Math.PI / 180;
        double c = Math.Cos(a), s = Math.Sin(a);
        double[][] signs = { new[] { -1.0, -1.0 }, new[] { 1.0, -1.0 }, new[] { 1.0, 1.0 }, new[] { -1.0, 1.0 } };
        foreach (var sg in signs)
        {
            double lx = sg[0] * p.W / 2, ly = sg[1] * p.H / 2;
            yield return new PointD(p.X + lx * c - ly * s, p.Y + lx * s + ly * c);
        }
    }

    // ---------- caméra ----------

    public void FitTo(IEnumerable<PointD> points, double margin = 30)
    {
        if (Width <= 0 || Height <= 0) return;
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
        int count = 0;
        foreach (var p in points)
        {
            count++;
            minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
            minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
        }
        if (count == 0) return;

        double bw = Math.Max(maxX - minX, 30), bh = Math.Max(maxY - minY, 30);
        double vw = Math.Max(Width - 2 * margin, 50);
        double vh = Math.Max(Height - InsetTop - InsetBottom - 2 * margin, 50);
        _scale = Math.Max(0.8, Math.Min(10, Math.Min(vw / bw, vh / bh)));
        double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
        _offX = -cx * _scale;
        _offY = cy * _scale;
        Invalidate();
    }

    public void FitAll()
    {
        var pts = new List<PointD>();
        foreach (var place in Campus.Places)
            foreach (var c in Corners(place)) pts.Add(c);
        foreach (var n in Campus.Nodes) pts.Add(new PointD(n.X, n.Y));
        FitTo(pts, 22);
    }

    public void CenterOn(PointD p)
    {
        _offX = -p.X * _scale;
        _offY = p.Y * _scale;
        Invalidate();
    }

    public void ZoomBy(double factor)
    {
        double old = _scale;
        _scale = Math.Max(0.8, Math.Min(14, old * factor));
        double f = _scale / old;
        _offX *= f;
        _offY *= f;
        Invalidate();
    }

    // ---------- gestes ----------

    void OnPan(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
        {
            _startOffX = _offX;
            _startOffY = _offY;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            _offX = _startOffX + e.TotalX;
            _offY = _startOffY + e.TotalY;
            Invalidate();
        }
    }

    void OnPinch(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _startScale = _scale;
            _startOffX = _offX;
            _startOffY = _offY;
        }
        else if (e.Status == GestureStatus.Running && _startScale > 0)
        {
            double newScale = Math.Max(0.8, Math.Min(14, _startScale * e.Scale));
            double f = newScale / _startScale;
            _scale = newScale;
            _offX = _startOffX * f;
            _offY = _startOffY * f;
            Invalidate();
        }
    }

    void OnTap(object sender, TappedEventArgs e)
    {
        Point? pos = e.GetPosition(this);
        if (pos == null) return;
        PointD world = ToWorld(pos.Value);

        string hit = "";
        foreach (var p in Campus.Places)
        {
            if (string.IsNullOrEmpty(p.ServiceId)) continue;
            double a = p.Angle * Math.PI / 180;
            double dx = world.X - p.X, dy = world.Y - p.Y;
            double lx = dx * Math.Cos(a) + dy * Math.Sin(a);
            double ly = -dx * Math.Sin(a) + dy * Math.Cos(a);
            double slack = 4 / _scale;
            if (Math.Abs(lx) <= p.W / 2 + slack && Math.Abs(ly) <= p.H / 2 + slack)
            {
                hit = p.Id;
                break;
            }
        }

        var handler = MapTapped;
        if (handler != null) handler(this, new MapTapArgs { Point = world, PlaceId = hit });
    }

    // ---------- dessin ----------

    void RoundedPlace(ICanvas canvas, CampusPlace p, float dx, float dy, float inflate, float radius, bool fill)
    {
        float w = (float)(p.W * _scale) + 2 * inflate;
        float h = (float)(p.H * _scale) + 2 * inflate;
        canvas.SaveState();
        canvas.Translate(SX(p.X) + dx, SY(p.Y) + dy);
        canvas.Rotate((float)(-p.Angle));
        if (fill) canvas.FillRoundedRectangle(-w / 2, -h / 2, w, h, radius);
        else canvas.DrawRoundedRectangle(-w / 2, -h / 2, w, h, radius);
        canvas.RestoreState();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Ground;
        canvas.FillRectangle(dirtyRect);
        if (Campus == null) return;

        // zones vertes avec quelques arbres
        foreach (var place in Campus.Places)
        {
            if (place.Kind != "green") continue;
            canvas.FillColor = Pal.Park;
            RoundedPlace(canvas, place, 0, 0, 0, 12, true);

            if (_scale >= 1.6)
            {
                float r = (float)Math.Max(3, 2.6 * _scale);
                for (double gx = place.X - place.W / 2 + 6; gx < place.X + place.W / 2 - 3; gx += 11)
                {
                    for (double gy = place.Y - place.H / 2 + 6; gy < place.Y + place.H / 2 - 3; gy += 11)
                    {
                        canvas.FillColor = TreeDark;
                        canvas.FillCircle(SX(gx) + 1.5f, SY(gy) + 1.5f, r);
                        canvas.FillColor = Tree;
                        canvas.FillCircle(SX(gx), SY(gy), r);
                    }
                }
            }
        }

        // allées : bord, surface blanche, ligne centrale pointillée
        float roadWidth = (float)Math.Max(6, 7 * _scale);
        canvas.StrokeLineCap = LineCap.Round;
        foreach (var edge in Campus.Edges)
        {
            var a = Campus.Node(edge.A);
            var b = Campus.Node(edge.B);
            if (a == null || b == null) continue;
            canvas.StrokeColor = RoadEdge;
            canvas.StrokeSize = roadWidth + 3;
            canvas.DrawLine(SX(a.X), SY(a.Y), SX(b.X), SY(b.Y));
        }
        foreach (var edge in Campus.Edges)
        {
            var a = Campus.Node(edge.A);
            var b = Campus.Node(edge.B);
            if (a == null || b == null) continue;
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = roadWidth;
            canvas.DrawLine(SX(a.X), SY(a.Y), SX(b.X), SY(b.Y));
        }
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.StrokeDashPattern = new float[] { 6, 7 };
        canvas.StrokeColor = Color.FromArgb("#D3DBE5");
        canvas.StrokeSize = 1.5f;
        foreach (var edge in Campus.Edges)
        {
            var a = Campus.Node(edge.A);
            var b = Campus.Node(edge.B);
            if (a == null || b == null) continue;
            canvas.DrawLine(SX(a.X), SY(a.Y), SX(b.X), SY(b.Y));
        }
        canvas.StrokeDashPattern = null;
        canvas.StrokeLineCap = LineCap.Round;

        // bâtiments : socle sombre + toit arrondi
        foreach (var place in Campus.Places)
        {
            if (place.Kind == "green") continue;
            bool selected = place.Id == SelectedPlaceId;
            canvas.Alpha = Dimmed.Contains(place.Id) ? 0.45f : 1f;

            canvas.FillColor = selected ? Pal.GoldDark : BaseWall;
            RoundedPlace(canvas, place, 0, 4, 0, 9, true);
            canvas.FillColor = selected ? Pal.Gold : Roof;
            RoundedPlace(canvas, place, 0, 0, 0, 9, true);
            canvas.StrokeColor = selected ? Pal.Navy : RoofEdge;
            canvas.StrokeSize = selected ? 3 : 1.5f;
            RoundedPlace(canvas, place, 0, 0, 0, 9, false);

            float width = (float)(place.W * _scale);
            if (!string.IsNullOrEmpty(place.Name) && width > 46)
            {
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.FontColor = selected ? Colors.White : Pal.Navy;
                canvas.FontSize = 12;
                canvas.DrawString(place.Name, SX(place.X) - width / 2 + 4, SY(place.Y) - 12, width - 8, 24,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            canvas.Alpha = 1f;
        }

        // entrée principale
        var entrance = Campus.Node(Campus.Entrance);
        if (entrance != null)
        {
            float ex = SX(entrance.X), ey = SY(entrance.Y);
            canvas.FillColor = Pal.Navy;
            canvas.FillRoundedRectangle(ex - 28, ey + 10, 56, 21, 10.5f);
            canvas.FontColor = Colors.White;
            canvas.FontSize = 11;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.DrawString("Entrée", ex - 28, ey + 10, 56, 21, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // itinéraire
        if (RoutePoints.Count >= 2)
        {
            var route = new PathF();
            route.MoveTo(SX(RoutePoints[0].X), SY(RoutePoints[0].Y));
            for (int i = 1; i < RoutePoints.Count; i++)
                route.LineTo(SX(RoutePoints[i].X), SY(RoutePoints[i].Y));

            canvas.StrokeLineJoin = LineJoin.Round;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 10;
            canvas.DrawPath(route);
            canvas.StrokeColor = Pal.Navy;
            canvas.StrokeSize = 5.5f;
            canvas.DrawPath(route);
        }

        // destination (épingle + étiquette)
        if (HasDestination)
        {
            float dx = SX(Destination.X), dy = SY(Destination.Y);
            canvas.FillColor = Pal.Gold;
            canvas.FillCircle(dx, dy, 11);
            canvas.StrokeColor = Pal.Navy;
            canvas.StrokeSize = 3;
            canvas.DrawCircle(dx, dy, 11);
            canvas.FillColor = Colors.White;
            canvas.FillCircle(dx, dy, 3.5f);

            if (!string.IsNullOrEmpty(DestinationLabel))
            {
                float lw = DestinationLabel.Length * 7f + 20;
                canvas.FillColor = Pal.Navy;
                canvas.FillRoundedRectangle(dx - lw / 2, dy - 46, lw, 24, 12);
                canvas.FontColor = Colors.White;
                canvas.FontSize = 12;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(DestinationLabel, dx - lw / 2, dy - 46, lw, 24, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        // utilisateur
        if (HasUser)
        {
            float ux = SX(User.X), uy = SY(User.Y);
            float radius = (float)Math.Max(12, Math.Min(120, UserAccuracy * _scale));
            canvas.FillColor = Color.FromArgb("#2E0A66C2");
            canvas.FillCircle(ux, uy, radius);

            if (!double.IsNaN(UserHeading))
            {
                double theta = -UserHeading;
                var cone = new PathF();
                cone.MoveTo(ux + (float)(26 * Math.Cos(theta)), uy + (float)(26 * Math.Sin(theta)));
                cone.LineTo(ux + (float)(13 * Math.Cos(theta + 2.4)), uy + (float)(13 * Math.Sin(theta + 2.4)));
                cone.LineTo(ux + (float)(13 * Math.Cos(theta - 2.4)), uy + (float)(13 * Math.Sin(theta - 2.4)));
                cone.Close();
                canvas.FillColor = Blue;
                canvas.FillPath(cone);
            }

            canvas.FillColor = Colors.White;
            canvas.FillCircle(ux, uy, 10.5f);
            canvas.FillColor = Blue;
            canvas.FillCircle(ux, uy, 7.5f);

            canvas.FillColor = Blue;
            canvas.FillRoundedRectangle(ux - 22, uy + 16, 44, 19, 9.5f);
            canvas.FontColor = Colors.White;
            canvas.FontSize = 11;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.DrawString("Vous", ux - 22, uy + 16, 44, 19, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        DrawCompass(canvas);
        DrawScaleBar(canvas);
    }

    void DrawCompass(ICanvas canvas)
    {
        float cx = 36, cy = (float)(InsetTop + 24);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx, cy, 19);
        canvas.StrokeColor = Pal.Line;
        canvas.StrokeSize = 1.5f;
        canvas.DrawCircle(cx, cy, 19);

        double theta = -NorthAngle;
        var north = new PathF();
        north.MoveTo(cx + (float)(15 * Math.Cos(theta)), cy + (float)(15 * Math.Sin(theta)));
        north.LineTo(cx + (float)(6 * Math.Cos(theta + 2.2)), cy + (float)(6 * Math.Sin(theta + 2.2)));
        north.LineTo(cx + (float)(6 * Math.Cos(theta - 2.2)), cy + (float)(6 * Math.Sin(theta - 2.2)));
        north.Close();
        canvas.FillColor = Pal.Red;
        canvas.FillPath(north);

        var south = new PathF();
        south.MoveTo(cx - (float)(13 * Math.Cos(theta)), cy - (float)(13 * Math.Sin(theta)));
        south.LineTo(cx + (float)(6 * Math.Cos(theta + 2.2)), cy + (float)(6 * Math.Sin(theta + 2.2)));
        south.LineTo(cx + (float)(6 * Math.Cos(theta - 2.2)), cy + (float)(6 * Math.Sin(theta - 2.2)));
        south.Close();
        canvas.FillColor = Pal.BlockLine;
        canvas.FillPath(south);

        canvas.FontColor = Pal.Navy;
        canvas.FontSize = 10;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.DrawString("N", cx - 10, cy - 36, 20, 14, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    void DrawScaleBar(ICanvas canvas)
    {
        double[] choices = { 5, 10, 20, 25, 50, 100, 200 };
        double meters = choices[0];
        foreach (var c in choices)
        {
            if (c * _scale <= 110) meters = c;
        }
        float length = (float)(meters * _scale);
        float x = 18, y = (float)(Height - InsetBottom + 14);

        canvas.StrokeColor = Pal.Navy;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.DrawLine(x, y, x + length, y);
        canvas.DrawLine(x, y - 5, x, y + 5);
        canvas.DrawLine(x + length, y - 5, x + length, y + 5);
        canvas.StrokeLineCap = LineCap.Round;

        canvas.FontColor = Pal.Navy;
        canvas.FontSize = 11;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.DrawString(meters + " m", x, y - 22, length + 30, 16, HorizontalAlignment.Left, VerticalAlignment.Center);
    }
}
