using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using Gesichtsemotionenerkennung.Utils;

namespace Gesichtsemotionenerkennung.Models
{
    public static class Settings
    {
        // Basisverzeichnis der Anwendung
        public static readonly string AppRoot = AppDomain.CurrentDomain.BaseDirectory;

        // Pfad zum YOLO-Gesichtsmodell
        public static readonly string YoloModelPath = Path.Combine(AppRoot, "Assets", "yolo", "yolov12n-face.onnx");

        private static List<PlayerSlot> _playerSlots = new List<PlayerSlot>();
        public static List<PlayerSlot> PlayerSlots
        {
            get => _playerSlots;
            set => _playerSlots = value ?? new List<PlayerSlot>();
        }

        // Basis-Koordinaten für FullHD (1920x1080)
        private static readonly Dictionary<int, Rect> FixedPlayerRects = new Dictionary<int, Rect>
        {
            { 1,  new Rect(961, 146, 473, 265) },
            { 2,  new Rect(1434, 145, 473, 265) },
            { 3,  new Rect(1434, 425, 473, 265) },
            { 4,  new Rect(1434, 707, 473, 265) },
            { 5,  new Rect(961, 707, 473, 265) },
            { 6,  new Rect(486, 708, 473, 265) },
            { 7,  new Rect(12, 709, 473, 265) },
            { 8,  new Rect(12, 425, 473, 265) },
            { 9,  new Rect(12, 145, 473, 265) },
            { 10, new Rect(486, 146, 473, 265) }
        };

        // Initialisierung der PlayerSlots basierend auf der aktuellen Framegröße
        public static void InitializeSlots(int width, int height)
        {
            _playerSlots.Clear();

            // Skalierungsfaktoren für unterschiedliche Frame-Größen
            float scaleX = width / 1920f;
            float scaleY = height / 1080f;

            foreach (var kvp in FixedPlayerRects)
            {
                var rect = kvp.Value;

                // Rechteck an aktuelle Framegröße anpassen
                Rect scaledRect = new Rect(
                    (int)(rect.X * scaleX),
                    (int)(rect.Y * scaleY),
                    (int)(rect.Width * scaleX),
                    (int)(rect.Height * scaleY)
                );

                // PlayerSlot ohne Parameter-Konstruktor erstellen
                var slot = new PlayerSlot
                {
                    Id = kvp.Key,
                    Bounds = scaledRect
                };

                _playerSlots.Add(slot);
            }
        }

        // Sampling-Rate für Audio
        public const int SamplingRate = 16000;

        // Anzahl der Frames, die beim Verarbeiten übersprungen werden
        public const int FrameSkip = 1;
    }
}