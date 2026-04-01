using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.Numerics;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Utils
{
    public static class Visualization
    {
        /// <summary>
        /// Zeichnet Begrenzungsrechtecke und Mittelpunkte der Slots/Gesichter auf dem Frame
        /// </summary>
        public static void DrawDetections(Mat frame, List<PlayerFrameData> detections)
        {
            foreach (var d in detections)
            {
                if (d.BBox == null || d.BBox.Length != 4) continue;

                // Extraktion der BoundingBox-Koordinaten
                int x = (int)d.BBox[0];
                int y = (int)d.BBox[1];
                int width = (int)d.BBox[2];
                int height = (int)d.BBox[3];

                // Zeichnet ein grünes Rechteck um das Gesicht/Slot
                Cv2.Rectangle(frame, new Rect(x, y, width, height), Scalar.Green, 2);

                // Zeichnet den Mittelpunkt, falls vorhanden
                if (d.Center.HasValue)
                {
                    Cv2.Circle(frame, (int)d.Center.Value.X, (int)d.Center.Value.Y, 3, Scalar.Red, -1);
                }
            }
        }
    }
}