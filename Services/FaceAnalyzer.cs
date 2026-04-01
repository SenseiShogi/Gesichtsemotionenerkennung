using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace Gesichtsemotionenerkennung.Services
{
    public class FaceAnalyzer
    {
        private readonly YoloSegmentDetector _yoloSegment;
        // Konstante bleibt, falls sie an anderer Stelle im Projekt verwendet wird,
        // auch wenn sie für die Gesichtserkennung aktuell keine Funktion hat.
        private const int MaskResolution = 28;

        public FaceAnalyzer(YoloSegmentDetector yoloSegmentDetector)
        {
            _yoloSegment = yoloSegmentDetector ?? throw new ArgumentNullException(nameof(yoloSegmentDetector));
        }

        /// <summary>
        /// Methode zur Kompatibilität erhalten. Gibt nun die gefundenen Gesichter zurück.
        /// Logik zur Maskenerstellung entfernt, da Face-Modelle keine Masken erzeugen.
        /// </summary>
        public List<SegmentationDetection> DetectFacesWithMask(Mat slotRoi)
        {
            if (slotRoi == null || slotRoi.Empty())
                return new List<SegmentationDetection>();

            // Rohdetektionen aus dem aktualisierten Detektor abrufen
            var detectionsRaw = _yoloSegment.Detect(slotRoi);
            var results = new List<SegmentationDetection>();

            foreach (var det in detectionsRaw)
            {
                // ÄNDERUNG: Prüfen des "face"-Klassenlabels.
                // Wenn die Gesichts-ID in der Modell = 0 ist, haben wir ClassLabel im Detektor auf "face" gesetzt.
                if (det.ClassLabel != "face" || det.Confidence < 0.25f)
                    continue;

                // MASKENLOGIK ENTFERNT:
                // Face-Modelle besitzen kein RawMask. Um null-Fehler zu vermeiden, überspringen wir die Maske.

                results.Add(det);
            }

            return results;
        }

        /// <summary>
        /// Schneidet das Gesicht aus dem Originalbild (Slot) anhand des erkannten Rechtecks aus.
        /// </summary>
        public Mat? CropFace(Mat slotRoi, Rect faceRect)
        {
            if (slotRoi == null || slotRoi.Empty())
                return null;

            // Validierung der Koordinaten, um Mat-Grenzen nicht zu überschreiten
            int x = Math.Max(faceRect.X, 0);
            int y = Math.Max(faceRect.Y, 0);

            // Sicherstellen, dass Breite und Höhe innerhalb des Bildes liegen
            int w = Math.Min(faceRect.Width, slotRoi.Width - x);
            int h = Math.Min(faceRect.Height, slotRoi.Height - y);

            if (w <= 0 || h <= 0)
                return null;

            // Gibt eine neue Matrix (Region of Interest) zurück
            return new Mat(slotRoi, new Rect(x, y, w, h));
        }
    }
}