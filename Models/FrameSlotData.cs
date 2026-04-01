using Gesichtsemotionenerkennung.Services;
using OpenCvSharp;
using System;

namespace Gesichtsemotionenerkennung.Models
{
    public class FrameSlotData
    {
        // Index des Frames innerhalb des Videos
        public int FrameIndex { get; set; }

        // Zeitstempel des Frames
        public TimeSpan Timestamp { get; set; }

        // Ausgeschnittener Gesicht-Frame (ROI)
        public Mat FaceFrame { get; set; }

        // Ergebnisse der Emotionserkennung für diesen Frame
        public EmotionDetection EmotionData { get; set; }
    }
}