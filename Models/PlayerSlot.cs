using Gesichtsemotionenerkennung.Services;
using OpenCvSharp;

namespace Gesichtsemotionenerkennung.Models
{
    public class PlayerSlot
    {
        public int Id { get; set; }

        // Grenzen des Slots im Frame (x, y, Breite, Höhe)
        public Rect Bounds { get; set; }

        // Letzter Face-Frame für diesen Slot
        public Mat LastFaceFrame { get; set; }

        // Pfad zur Ausgabedatei für eventuelles Überschreiben
        public string OutputFilePath { get; set; }

        // Letztes Emotionsergebnis für diesen Slot
        public EmotionDetection LastDetection { get; set; }
    }
}