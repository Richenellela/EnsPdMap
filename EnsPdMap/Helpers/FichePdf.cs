using EnsPdMap.Models;

namespace EnsPdMap.Helpers;

/// <summary>Génère la fiche PDF d'une démarche (même contenu que l'écran) avec le moteur PDF d'Android.</summary>
public static class FichePdf
{
    public static void Create(Procedure p, string path)
    {
#if ANDROID
        var doc = new Android.Graphics.Pdf.PdfDocument();
        try
        {
            var writer = new FicheWriter(doc);
            writer.Write(p);
            using (var stream = File.Create(path))
            {
                doc.WriteTo(stream);
            }
        }
        finally
        {
            doc.Close();
        }
#else
        throw new PlatformNotSupportedException("La génération de fiches PDF n'est disponible que sur Android.");
#endif
    }
}

#if ANDROID
internal class FicheWriter
{
    const int PageW = 595;
    const int PageH = 842;
    const int Margin = 40;

    readonly Android.Graphics.Pdf.PdfDocument _doc;
    Android.Graphics.Pdf.PdfDocument.Page _page;
    Android.Graphics.Canvas _canvas;
    float _y;
    int _pageNo;

    readonly Android.Graphics.Paint _body = MakePaint("#0F172A", 11.5f, false);
    readonly Android.Graphics.Paint _bold = MakePaint("#0F172A", 12f, true);
    readonly Android.Graphics.Paint _muted = MakePaint("#5B6778", 10f, false);
    readonly Android.Graphics.Paint _title = MakePaint("#0A2342", 20f, true);
    readonly Android.Graphics.Paint _section = MakePaint("#0A2342", 14f, true);
    readonly Android.Graphics.Paint _gold = MakePaint("#7A5A10", 10f, true);
    readonly Android.Graphics.Paint _white = MakePaint("#FFFFFF", 10f, true);
    readonly Android.Graphics.Paint _navyFill = MakePaint("#0A2342", 10f, false);
    readonly Android.Graphics.Paint _goldFill = MakePaint("#C08B1C", 10f, false);
    readonly Android.Graphics.Paint _lineFill = MakePaint("#D9DEE8", 10f, false);
    readonly Android.Graphics.Paint _stroke = MakePaint("#0F172A", 10f, false);

    public FicheWriter(Android.Graphics.Pdf.PdfDocument doc)
    {
        _doc = doc;
        _stroke.SetStyle(Android.Graphics.Paint.Style.Stroke);
        _stroke.StrokeWidth = 1.3f;
    }

    static Android.Graphics.Paint MakePaint(string color, float size, bool bold)
    {
        var paint = new Android.Graphics.Paint(Android.Graphics.PaintFlags.AntiAlias);
        paint.Color = Android.Graphics.Color.ParseColor(color);
        paint.TextSize = size;
        paint.SetTypeface(bold ? Android.Graphics.Typeface.DefaultBold : Android.Graphics.Typeface.Default);
        return paint;
    }

    void NewPage()
    {
        if (_page != null) _doc.FinishPage(_page);
        _pageNo++;
        var info = new Android.Graphics.Pdf.PdfDocument.PageInfo.Builder(PageW, PageH, _pageNo).Create();
        _page = _doc.StartPage(info);
        _canvas = _page.Canvas;
        _y = Margin;
    }

    void Need(float height)
    {
        if (_y + height > PageH - Margin) NewPage();
    }

    List<string> Wrap(string text, Android.Graphics.Paint paint, float width)
    {
        var lines = new List<string>();
        foreach (var paragraph in (text ?? "").Split('\n'))
        {
            string current = "";
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = current.Length == 0 ? word : current + " " + word;
                if (current.Length == 0 || paint.MeasureText(candidate) <= width)
                    current = candidate;
                else
                {
                    lines.Add(current);
                    current = word;
                }
            }
            lines.Add(current);
        }
        return lines;
    }

    void Para(string text, Android.Graphics.Paint paint, float indent, float gapAfter)
    {
        if (string.IsNullOrEmpty(text)) return;
        float width = PageW - 2 * Margin - indent;
        float lineHeight = paint.TextSize * 1.35f;
        foreach (var line in Wrap(text, paint, width))
        {
            Need(lineHeight);
            _canvas.DrawText(line, Margin + indent, _y + paint.TextSize, paint);
            _y += lineHeight;
        }
        _y += gapAfter;
    }

    void Section(string title)
    {
        Need(40);
        _y += 12;
        _canvas.DrawText(title, Margin, _y + 14, _section);
        _y += 20;
        _canvas.DrawRect(Margin, _y, PageW - Margin, _y + 1, _lineFill);
        _y += 10;
    }

    void Box(string text, string background, string foreground, float indent)
    {
        var fill = MakePaint(background, 10f, false);
        var ink = MakePaint(foreground, 10.5f, true);
        float width = PageW - 2 * Margin - indent - 20;
        var lines = Wrap(text, ink, width);
        float lineHeight = ink.TextSize * 1.35f;
        float height = lines.Count * lineHeight + 12;

        Need(height + 4);
        _canvas.DrawRoundRect(Margin + indent, _y, PageW - Margin, _y + height, 6f, 6f, fill);
        float ty = _y + 6;
        foreach (var line in lines)
        {
            _canvas.DrawText(line, Margin + indent + 10, ty + ink.TextSize, ink);
            ty += lineHeight;
        }
        _y += height + 6;
    }

    public void Write(Procedure p)
    {
        NewPage();

        // bandeau
        _canvas.DrawRect(0, 0, PageW, 84, _navyFill);
        _canvas.DrawRect(0, 84, PageW, 88, _goldFill);
        var big = MakePaint("#FFFFFF", 22f, true);
        var sub = MakePaint("#B8C2D3", 10.5f, false);
        _canvas.DrawText("EnsPdMap", Margin, 44, big);
        _canvas.DrawText("ENSPD – UDO · Campus PK17 · Fiche de procédure", Margin, 64, sub);
        _y = 108;

        _canvas.DrawText(p.Category.ToUpperInvariant(), Margin, _y + 10, _gold);
        _y += 22;
        Para(p.Title, _title, 0, 6);
        Para(p.Overview, _body, 0, 8);

        string delay = string.IsNullOrEmpty(p.Delay) ? "—" : p.Delay;
        Para("Délai : " + delay + "     Coût : " + p.Cost + "     Bureau : " + p.Office, _bold, 0, 4);
        if (!string.IsNullOrEmpty(p.Audience)) Para("Pour qui : " + p.Audience, _body, 0, 2);
        if (!string.IsNullOrEmpty(p.Where)) Para("Où : " + p.Where, _body, 0, 2);

        // pièces
        Section("Pièces à fournir");
        foreach (var piece in p.Pieces)
        {
            Need(44);
            _canvas.DrawRoundRect(Margin, _y + 1, Margin + 12, _y + 13, 2.5f, 2.5f, _stroke);
            Para(piece.Name, _bold, 22, 0);
            Para(piece.Detail, _muted, 22, 0);
            Para(piece.How, _muted, 22, 6);
        }

        // parcours
        Section("Parcours détaillé");
        for (int i = 0; i < p.Steps.Count; i++)
        {
            var step = p.Steps[i];
            Need(60);
            float cy = _y + 8;
            _canvas.DrawCircle(Margin + 9, cy, 9, i == 0 ? _goldFill : _navyFill);
            string number = (i + 1).ToString();
            _canvas.DrawText(number, Margin + 9 - _white.MeasureText(number) / 2, cy + 3.5f, _white);

            Para(step.Title, _bold, 28, 0);
            if (!string.IsNullOrEmpty(step.Duration)) Para("Durée : " + step.Duration, _muted, 28, 0);
            Para(step.Detail, _body, 28, 3);
            if (!string.IsNullOrEmpty(step.Tip)) Box("Astuce : " + step.Tip, "#DCEBE2", "#14603A", 28);
            if (!string.IsNullOrEmpty(step.Warning)) Box("Attention : " + step.Warning, "#FBEDEC", "#B42318", 28);
            _y += 4;
        }

        // questions fréquentes
        if (p.Faq.Count > 0)
        {
            Section("Questions fréquentes");
            foreach (var item in p.Faq)
            {
                Para(item.Q, _bold, 0, 0);
                Para(item.A, _body, 0, 6);
            }
        }

        // rappels
        Section("À retenir");
        Box("Pour toute démarche administrative, munissez-vous de vos quitus de l'année en cours (ceux de l'année précédente pour le relevé de notes).", "#F5E6C8", "#7A5A10", 0);
        Box("Aucune démarche n'est payante. Les frais de scolarité se règlent à la banque UBA, hors application.", "#F5E6C8", "#7A5A10", 0);

        Need(30);
        _y += 8;
        Para("Fiche générée par EnsPdMap le " + DateTime.Now.ToString("dd/MM/yyyy") + " · vérifiez les informations auprès du service concerné.", _muted, 0, 0);

        _doc.FinishPage(_page);
    }
}
#endif
