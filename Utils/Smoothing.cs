using System;
using System.Collections.Generic;
using System.Linq;

namespace Gesichtsemotionenerkennung.Utils
{
    // --- Gleitender Durchschnitt (Moving Average) für stabile Emotionswerte ---
    public class Smoothing
    {
        // Größe des Fensters für gleitenden Durchschnitt
        private readonly int _windowSize;

        // Puffer für Valenz pro Spieler
        private readonly Dictionary<int, Queue<float>> _valenceBuffers = new();

        // Puffer für Arousal pro Spieler
        private readonly Dictionary<int, Queue<float>> _arousalBuffers = new();

        // Puffer für Emotionen pro Spieler (Dictionary<Emotion, Confidence>)
        private readonly Dictionary<int, Queue<Dictionary<string, float>>> _emotionBuffers = new();

        public Smoothing(int windowSize = 5)
        {
            _windowSize = Math.Max(1, windowSize);
        }

        // Berechnet geglätteten Wert für Valenz/Arousal
        private float SmoothValue(int playerId, float newValue, Dictionary<int, Queue<float>> buffer)
        {
            if (!buffer.ContainsKey(playerId))
                buffer[playerId] = new Queue<float>();

            var queue = buffer[playerId];
            queue.Enqueue(newValue);

            if (queue.Count > _windowSize)
                queue.Dequeue();

            return queue.Average();
        }

        // Berechnet die geglättete Emotion basierend auf Confidence-Werten der letzten Frames
        public string GetSmoothedEmotion(int playerId, string currentEmotion, float confidence, string[] allEmotions)
        {
            if (string.IsNullOrEmpty(currentEmotion) || allEmotions == null || allEmotions.Length == 0)
                return "Unknown";

            if (!_emotionBuffers.ContainsKey(playerId))
                _emotionBuffers[playerId] = new Queue<Dictionary<string, float>>();

            var currentFrameDict = allEmotions.ToDictionary(e => e, e => 0f);
            if (currentFrameDict.ContainsKey(currentEmotion))
                currentFrameDict[currentEmotion] = confidence;

            var queue = _emotionBuffers[playerId];
            queue.Enqueue(currentFrameDict);

            if (queue.Count > _windowSize)
                queue.Dequeue();

            return allEmotions
                .Select(e => new
                {
                    Name = e,
                    AvgScore = queue.Average(dict => dict.TryGetValue(e, out var v) ? v : 0f)
                })
                .OrderByDescending(x => x.AvgScore)
                .First().Name;
        }

        // Glättet den Valence-Wert für einen Spieler
        public float SmoothValence(int playerId, float value) => SmoothValue(playerId, value, _valenceBuffers);

        // Glättet den Arousal-Wert für einen Spieler
        public float SmoothArousal(int playerId, float value) => SmoothValue(playerId, value, _arousalBuffers);
    }
}