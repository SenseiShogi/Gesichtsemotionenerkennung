using System;

namespace Gesichtsemotionenerkennung.Models
{
    public class EmotionDetection
    {
        // Emotionale Ausdrucksform, z.B. "Happiness", "Sadness" oder "Unknown"
        public string Expression { get; set; } = "Unknown";

        // Vertrauen/Zuversicht der Vorhersage (0.0 bis 1.0)
        public float Confidence { get; set; } = 0f;

        // Vektor-Repräsentation des Gesichts für Embedding-basierte Analysen
        public float[] Embedding { get; set; } = Array.Empty<float>();

        // Valenzwert (-1.0 bis 1.0), beschreibt positive/negative Emotion
        public float Valence { get; set; } = 0f;

        // Arousal-Wert (0.0 bis 1.0), beschreibt Aktivierungsgrad der Emotion
        public float Arousal { get; set; } = 0f;
    }
}