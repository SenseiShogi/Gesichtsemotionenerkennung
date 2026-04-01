using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenCvSharp;
using Gesichtsemotionenerkennung.Services;
using Gesichtsemotionenerkennung.Models;
using Gesichtsemotionenerkennung.Views;

namespace Gesichtsemotionenerkennung.Controllers
{
    public class VideoController
    {
        private readonly YoloSegmentDetector _yoloDetector;
        private readonly EmotionAnalyzer _emotionAnalyzer;
        private readonly SlotProcessor _slotProcessor;
        private readonly string _outputFolder;

        public VideoController(string yoloModelPath,
                               string emotionModelPath,
                               string outputFolder)
        {
            if (string.IsNullOrEmpty(yoloModelPath)) throw new ArgumentNullException(nameof(yoloModelPath));
            if (string.IsNullOrEmpty(emotionModelPath)) throw new ArgumentNullException(nameof(emotionModelPath));
            _outputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));

            // Modelle einmalig initialisieren (schwere Modelle)
            _yoloDetector = new YoloSegmentDetector(yoloModelPath);
            _emotionAnalyzer = new EmotionAnalyzer(emotionModelPath);

            // Orchestrator für Slot-Verarbeitung initialisieren
            _slotProcessor = new SlotProcessor(_yoloDetector, _emotionAnalyzer);

            // Ausgabeordner sicherstellen
            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        /// <summary>
        /// Verarbeitung von Videos oder eines einzelnen Videopfads
        /// </summary>
        public void ProcessVideos(string inputPath, int frameStep = 5000)
        {
            string path = inputPath.Trim('"').Replace("\\", "/");
            var videoFiles = File.Exists(path)
                ? new List<string> { path } // Einzelnes Video
                : new List<string>(Directory.GetFiles(path, "*.mp4")); // Alle MP4-Videos im Verzeichnis

            foreach (var videoFile in videoFiles)
            {
                try
                {
                    ProcessSingleVideo(videoFile, frameStep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FATAL] Fehler bei der Verarbeitung von {videoFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Verarbeitung eines einzelnen Videos
        /// </summary>
        private void ProcessSingleVideo(string videoFilePath, int frameStep)
        {
            string videoName = Path.GetFileNameWithoutExtension(videoFilePath);
            LogInfo($"Starte Verarbeitung: {videoName}");

            using var loader = new VideoLoader(videoFilePath);
            Settings.InitializeSlots(loader.Width, loader.Height); // Slot-Definition initialisieren

            // Liste zum Speichern aller Frame-Ergebnisse für CSV
            var allFramesData = new List<UnifiedFrameContext>();

            Stopwatch sw = Stopwatch.StartNew();
            int processedCount = 0;
            int totalFrames = loader.TotalFrames;

            foreach (var (frame, frameIndex, timestampSeconds) in loader.FrameGenerator(frameStep))
            {
                processedCount++;
                TimeSpan timestamp = TimeSpan.FromSeconds(timestampSeconds);

                foreach (var slot in Settings.PlayerSlots)
                {
                    // 1. Slot-Bereich aus dem Frame ausschneiden
                    using var slotMat = new Mat(frame, slot.Bounds);

                    // 2. Slot durch das Modell-Kaskade verarbeiten
                    var results = _slotProcessor.ProcessSlot(slotMat);

                    foreach (var res in results)
                    {
                        // Ergebnisse für CSV speichern
                        allFramesData.Add(new UnifiedFrameContext
                        {
                            FrameIndex = frameIndex,
                            Timestamp = timestamp,
                            SlotId = slot.Id,
                            Emotion = res.Emotion.Expression,
                            Confidence = res.Emotion.Confidence,
                            Valence = res.Emotion.Valence,
                            Arousal = res.Emotion.Arousal,
                            BBox = new int[] { res.BoundingBox.X, res.BoundingBox.Y, res.BoundingBox.Width, res.BoundingBox.Height }
                        });

                        LogInfo($"[Slot {slot.Id}] {res.Emotion.Expression} ({res.Emotion.Confidence:F2})");
                    }
                }

                UpdateProgressDisplay(processedCount, totalFrames / frameStep, sw.Elapsed);
            }

            // 3. Ergebnisse nach Video in CSV speichern
            string csvPath = Path.Combine(_outputFolder, $"{videoName}_results.csv");
            CsvWriter.Write(allFramesData, csvPath);

            sw.Stop();
            LogInfo($"Fertig: {videoName}. Gesamtergebnisse: {allFramesData.Count}");
        }

        /// <summary>
        /// Fortschrittsanzeige während der Videoverarbeitung
        /// </summary>
        private void UpdateProgressDisplay(int current, int total, TimeSpan elapsed)
        {
            double progress = total > 0 ? (double)current / total * 100 : 100;
            Console.Write($"\r[PROGRESS] {progress:F1}% | Frames verarbeitet: {current} | Zeit: {elapsed:mm\\:ss}");
        }

        private void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");
    }
}