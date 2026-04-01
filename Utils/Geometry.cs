using System;
using System.Collections.Generic;
using OpenCvSharp;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Utils
{
    public static class Geometry
    {
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

        public static List<PlayerSlot> GeneratePlayerSlots(int frameWidth, int frameHeight)
        {
            var slots = new List<PlayerSlot>();

            // Skalierungsfaktoren für unterschiedliche Frame-Größen
            double scaleX = frameWidth / 1920.0;
            double scaleY = frameHeight / 1080.0;

            foreach (var kvp in FixedPlayerRects)
            {
                var baseRect = kvp.Value;

                // Rechteck an aktuelle Framegröße anpassen
                var scaledRect = new Rect(
                    Math.Max(0, (int)(baseRect.X * scaleX)),
                    Math.Max(0, (int)(baseRect.Y * scaleY)),
                    Math.Max(1, (int)(baseRect.Width * scaleX)),
                    Math.Max(1, (int)(baseRect.Height * scaleY))
                );

                slots.Add(new PlayerSlot
                {
                    Id = kvp.Key,
                    Bounds = scaledRect
                });
            }

            return slots;
        }

        // Gibt die Koordinaten des Rect als float[] zurück
        public static float[] ScaleRect(Rect rect, int frameWidth, int frameHeight)
        {
            // Hier könnte man bei Bedarf die Skalierung auf die Frame-Größe berücksichtigen,
            // aktuell wird nur die ursprüngliche Rect-Koordinate zurückgegeben
            return new float[]
            {
                (float)rect.X,
                (float)rect.Y,
                (float)rect.Width,
                (float)rect.Height
            };
        }
    }
}