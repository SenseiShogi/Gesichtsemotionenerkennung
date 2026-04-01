using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace Gesichtsemotionenerkennung.Services
{
    public class VideoLoader : IDisposable
    {
        // OpenCv VideoCapture-Objekt zum Laden von Videos
        private readonly VideoCapture _cap;

        // Frames pro Sekunde des Videos
        private readonly double _fps;

        // Gesamtzahl der Frames
        public int TotalFrames { get; }

        // Breite des Videos
        public int Width { get; }

        // Höhe des Videos
        public int Height { get; }

        public VideoLoader(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath))
                throw new ArgumentNullException(nameof(videoPath));

            _cap = new VideoCapture(videoPath);
            if (!_cap.IsOpened())
                throw new InvalidOperationException($"Video kann nicht geöffnet werden: {videoPath}");

            _fps = _cap.Fps;
            if (_fps <= 0)
                throw new InvalidOperationException("FPS konnte nicht gelesen werden");

            TotalFrames = _cap.FrameCount;
            Width = _cap.FrameWidth;
            Height = _cap.FrameHeight;
        }

        // Generator für Frames, optional mit Überspringen von Frames (frameStep)
        public IEnumerable<(Mat frame, int originalIndex, double timestamp)> FrameGenerator(int frameStep = 1)
        {
            if (frameStep < 1) frameStep = 1;

            int index = 0;
            Mat frame = new Mat();

            while (_cap.Read(frame))
            {
                if (index % frameStep == 0)
                {
                    double timestamp = index / _fps;
                    yield return (frame.Clone(), index, timestamp); // Clone zur Entkopplung vom Originalframe
                }
                index++;
            }
        }

        // Freigabe der Ressourcen
        public void Dispose()
        {
            _cap?.Release();
            _cap?.Dispose();
        }
    }
}