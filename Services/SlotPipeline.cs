using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Services
{
    public class SlotPipeline : IDisposable
    {
        private readonly FaceAnalyzer[] _faceAnalyzers;
        private readonly EmotionAnalyzer[] _emotionAnalyzers;
        private readonly string _outputFolder;
        private readonly int _roiSize;
        private readonly int _maxSlots;
        // Verschiedene Skalierungsfaktoren, um sicherzustellen, dass das Gesicht auch gefunden wird,
        // wenn es teilweise außerhalb des Standard-Slots liegt
        private readonly double[] _scales = new double[] { 1.0, 1.2, 0.8, 1.5, 0.5 };

        public string OutputFolder => _outputFolder;

        public SlotPipeline(Func<FaceAnalyzer> faceAnalyzerFactory,
                            Func<EmotionAnalyzer> emotionAnalyzerFactory,
                            string outputFolder,
                            int maxSlots = 10,
                            int roiSize = 640)
        {
            _maxSlots = maxSlots;
            _faceAnalyzers = new FaceAnalyzer[_maxSlots];
            _emotionAnalyzers = new EmotionAnalyzer[_maxSlots];

            // Initialisierung der Analyzers für jeden Slot
            for (int i = 0; i < _maxSlots; i++)
            {
                _faceAnalyzers[i] = faceAnalyzerFactory();
                _emotionAnalyzers[i] = emotionAnalyzerFactory();
            }

            _outputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));
            _roiSize = roiSize;

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public FrameSlotData ProcessSlot(PlayerSlot slot, Mat frame, int frameIndex, TimeSpan timestamp)
        {
            if (slot == null) throw new ArgumentNullException(nameof(slot));
            if (frame == null || frame.Empty()) return null!;

            int slotIndex = Math.Min(slot.Id, _maxSlots - 1);
            var faceAnalyzer = _faceAnalyzers[slotIndex];
            var emotionAnalyzer = _emotionAnalyzers[slotIndex];

            SegmentationDetection? bestDetection = null;
            Rect activeScaledRect = new Rect();

            // Iteration über verschiedene Skalierungen, um das Gesicht zu finden
            foreach (var scale in _scales)
            {
                var scaledRect = ScaleRect(slot.Bounds, scale, frame.Width, frame.Height);
                using var scaledRoi = new Mat(frame, scaledRect);

                // Gesichterkennung ohne Masken-Overlay
                List<SegmentationDetection> detections = faceAnalyzer.DetectFacesWithMask(scaledRoi);

                if (detections != null && detections.Count > 0)
                {
                    bestDetection = detections.OrderByDescending(d => d.Confidence).First();
                    activeScaledRect = scaledRect; // Merken, bei welchem Scale das Gesicht erkannt wurde

                    Console.WriteLine($"[DEBUG] Frame {frameIndex} | Slot {slot.Id} | Scale {scale:F2} | Gesicht gefunden! Conf {bestDetection.Confidence:F2}");
                    break;
                }
            }

            if (bestDetection == null)
            {
                // Kein Gesicht gefunden, alte Werte zurücksetzen
                slot.LastFaceFrame = null;
                slot.LastDetection = new EmotionDetection();
                return new FrameSlotData { FrameIndex = frameIndex, Timestamp = timestamp, FaceFrame = null!, EmotionData = slot.LastDetection };
            }

            // BoundingBox des Detektors ist relativ zum skalierten ROI
            using var currentScaledMat = new Mat(frame, activeScaledRect);
            Mat? faceRoi = faceAnalyzer.CropFace(currentScaledMat, bestDetection.BoundingBox);

            if (faceRoi == null || faceRoi.Empty())
            {
                slot.LastFaceFrame = null;
                return new FrameSlotData { FrameIndex = frameIndex, Timestamp = timestamp, FaceFrame = null!, EmotionData = new EmotionDetection() };
            }

            // Reiner Crop des Gesichts ohne Masken
            Mat faceToProcess = faceRoi.Clone();

            // Anpassen auf quadratische ROI für EmotionAnalyzer
            Mat resizedFace = new Mat();
            Cv2.Resize(faceToProcess, resizedFace, new Size(_roiSize, _roiSize));

            if (resizedFace.Type() != MatType.CV_8UC3)
                resizedFace.ConvertTo(resizedFace, MatType.CV_8UC3);

            // Speicherung für Debug / visuelle Kontrolle
            string faceFilePath = Path.Combine(_outputFolder, $"face_slot_{slot.Id}.png");
            try
            {
                Cv2.ImWrite(faceFilePath, resizedFace);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Slot {slot.Id} | Speicherfehler: {ex.Message}");
            }

            slot.LastFaceFrame = resizedFace;
            slot.OutputFilePath = faceFilePath;

            // Analyse der Emotionen anhand des zugeschnittenen Gesichts
            var emotionData = emotionAnalyzer.Analyze(resizedFace);
            slot.LastDetection = emotionData;

            // Freigabe temporärer Ressourcen
            faceToProcess.Dispose();

            return new FrameSlotData
            {
                FrameIndex = frameIndex,
                Timestamp = timestamp,
                FaceFrame = resizedFace,
                EmotionData = emotionData
            };
        }

        private Rect ScaleRect(Rect rect, double scale, int frameWidth, int frameHeight)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int newWidth = (int)(rect.Width * scale);
            int newHeight = (int)(rect.Height * scale);
            int newX = Math.Max(0, centerX - newWidth / 2);
            int newY = Math.Max(0, centerY - newHeight / 2);

            // Sicherstellen, dass das Rechteck innerhalb des Frames bleibt
            newWidth = Math.Min(frameWidth - newX, newWidth);
            newHeight = Math.Min(frameHeight - newY, newHeight);

            return new Rect(newX, newY, newWidth, newHeight);
        }

        public void Dispose()
        {
            foreach (var ea in _emotionAnalyzers) ea?.Dispose();
            // Falls FaceAnalyzer ebenfalls IDisposable ist, hier freigeben
        }
    }
}