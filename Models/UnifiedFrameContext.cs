using System;

namespace Gesichtsemotionenerkennung.Models
{
    public class UnifiedFrameContext
    {
        // Index des Frames innerhalb der Videosequenz
        public int FrameIndex { get; set; }

        // Zeitstempel des Frames
        public TimeSpan Timestamp { get; set; }

        // ID des Spieler-Slots, zu dem dieser Frame gehört
        public int SlotId { get; set; }

        // Erkanntes emotionales Ausdrucksverhalten, z.B. "Happiness", "Sadness"
        public string Emotion { get; set; } = "";

        // Vertrauenswert der Emotionserkennung (0.0 bis 1.0)
        public float Confidence { get; set; }

        // Valenzwert der Emotion (-1.0 bis 1.0), beschreibt positiv/negativ
        public float Valence { get; set; }

        // Arousal-Wert (0.0 bis 1.0), beschreibt Intensität oder Aktivierungsgrad
        public float Arousal { get; set; }

        // BoundingBox-Koordinaten als int[], da Rect-Koordinaten Ganzzahlen sind
        public int[] BBox { get; set; } = Array.Empty<int>();
    }
}