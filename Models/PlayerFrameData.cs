using System.Numerics;

namespace Gesichtsemotionenerkennung.Models
{
    public class PlayerFrameData
    {
        public int SlotId { get; set; }

        // Begrenzungsrechteck des Gesichts (BBox: x, y, Breite, Höhe)
        public float[] BBox { get; set; } = new float[4];

        // Mittelpunkt des Gesichts
        public Vector2? Center { get; set; }

        // Emotionsdaten
        public string DominantEmotion { get; set; } = "Neutral";
        public float EmotionConfidence { get; set; } = 0f;

        // Zusätzlich für Yolo+EmotionAnalyzer
        public float Valence { get; set; } = 0f;
        public float Arousal { get; set; } = 0f;

        // Optional: Aktivitäts- oder Sprachstatus usw.
        public bool IsSpeaking { get; set; } = false;
    }
}