using Gesichtsemotionenerkennung.Models;
using OpenCvSharp;
using System.Collections.Generic;

namespace Gesichtsemotionenerkennung.Services
{
    public class FrameSplitter
    {
        /// <summary>
        /// Teilt einen Frame in Slots gemäß der angegebenen PlayerSlot-Definitionen
        /// </summary>
        /// <param name="frame">Original-Frame</param>
        /// <param name="slots">Liste der Slots (PlayerSlot)</param>
        /// <returns>Dictionary SlotId -> Mat des entsprechenden Frames</returns>
        public Dictionary<int, Mat> Split(Mat frame, IReadOnlyList<PlayerSlot> slots)
        {
            var result = new Dictionary<int, Mat>();

            foreach (var slot in slots)
            {
                // Grenzen validieren, um außerhalb des Frames zu verhindern
                int x = Math.Max(0, slot.Bounds.X);
                int y = Math.Max(0, slot.Bounds.Y);
                int width = Math.Min(frame.Width - x, slot.Bounds.Width);
                int height = Math.Min(frame.Height - y, slot.Bounds.Height);

                // Ungültige Slots überspringen
                if (width <= 0 || height <= 0) continue;

                // ROI (Region of Interest) erstellen und kopieren
                Rect r = new Rect(x, y, width, height);
                result[slot.Id] = new Mat(frame, r).Clone();
            }

            return result;
        }
    }
}