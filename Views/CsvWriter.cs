using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Views
{
    public static class CsvWriter
    {
        public static void Write(List<UnifiedFrameContext> data, string outputPath)
        {
            if (data == null || data.Count == 0)
            {
                Console.WriteLine("\n[CSV] Keine Daten zum Speichern.");
                return;
            }

            var sb = new StringBuilder();
            // CSV-Header
            sb.AppendLine("FrameIndex;Timestamp_Sec;SlotId;Emotion;Confidence;Valence;Arousal;Box_X;Box_Y;Box_W;Box_H");

            var ci = CultureInfo.InvariantCulture;

            foreach (var ctx in data)
            {
                // Extrahiert die BoundingBox-Koordinaten für Excel
                string x = "0", y = "0", w = "0", h = "0";
                if (ctx.BBox != null && ctx.BBox.Length == 4)
                {
                    x = ctx.BBox[0].ToString();
                    y = ctx.BBox[1].ToString();
                    w = ctx.BBox[2].ToString();
                    h = ctx.BBox[3].ToString();
                }

                sb.AppendLine(string.Format(ci,
                    "{0};{1:F4};{2};{3};{4:F2};{5:F2};{6:F2};{7};{8};{9};{10}",
                    ctx.FrameIndex,
                    ctx.Timestamp.TotalSeconds,
                    ctx.SlotId,
                    ctx.Emotion ?? "None",
                    ctx.Confidence,
                    ctx.Valence,
                    ctx.Arousal,
                    x, y, w, h
                ));
            }

            try
            {
                // Prüft, ob der Ordner existiert
                string dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Verwendet FileStream mit FileShare.ReadWrite, um "Access Denied"-Fehler zu vermeiden
                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }

                Console.WriteLine($"\n[CSV] Erfolg: {outputPath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"\n[FEHLER] Datei ist gesperrt oder in Benutzung: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[FEHLER] CSV Schreibfehler: {ex.Message}");
            }
        }
    }
}