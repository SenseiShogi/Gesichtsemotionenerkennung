using OpenCvSharp;
using System;
using System.Collections.Generic;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Services
{
    // Klasse zur Speicherung des Endergebnisses für ein erkanntes Gesicht
    public class FaceAnalysisResult
    {
        public Rect BoundingBox { get; set; }            // Begrenzungsrechteck (BoundingBox) des Gesichts
        public float FaceConfidence { get; set; }       // Konfidenz des Gesichtserkennungs-Detektors
        public EmotionDetection Emotion { get; set; }   // Ergebnis der Emotionserkennung
    }

    public class SlotProcessor
    {
        private readonly YoloSegmentDetector _yoloDetector;
        private readonly EmotionAnalyzer _emotionAnalyzer;

        public SlotProcessor(YoloSegmentDetector yoloDetector, EmotionAnalyzer emotionAnalyzer)
        {
            _yoloDetector = yoloDetector ?? throw new ArgumentNullException(nameof(yoloDetector));
            _emotionAnalyzer = emotionAnalyzer ?? throw new ArgumentNullException(nameof(emotionAnalyzer));
        }

        /// <summary>
        /// Verarbeitet ein Slot-Mat, erkennt Gesichter und Emotionen.
        /// </summary>
        public List<FaceAnalysisResult> ProcessSlot(Mat slotMat)
        {
            var results = new List<FaceAnalysisResult>();

            if (slotMat == null || slotMat.Empty())
                return results;

            // 1. Gesichtserkennung. YOLO liefert eine Liste von SegmentationDetection,
            // wobei jedes FaceImage bereits ausgeschnitten ist.
            var detectedFaces = _yoloDetector.Detect(slotMat);

            foreach (var face in detectedFaces)
            {
                // 2. Emotionserkennung für jedes ausgeschnittene Gesicht
                EmotionDetection emotionResult = _emotionAnalyzer.Analyze(face.FaceImage);

                // 3. Ergebnis zusammenstellen
                results.Add(new FaceAnalysisResult
                {
                    BoundingBox = face.BoundingBox,
                    FaceConfidence = face.Confidence,
                    Emotion = emotionResult
                });

                // 4. Speicherfreigabe: FaceImage wird über Dispose freigegeben
                face.Dispose();
            }

            return results;
        }
    }
}