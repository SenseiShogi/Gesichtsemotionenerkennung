using OpenCvSharp;
using System;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Gesichtsemotionenerkennung.Models;

namespace Gesichtsemotionenerkennung.Services
{
    public class EmotionAnalyzer : IDisposable
    {
        // ONNX InferenceSession für Emotionsanalyse
        private readonly InferenceSession _session;

        // Eingabegröße des Modells (quadratisches ROI)
        private readonly int _modelInputSize;

        // Labels der Emotionen
        private readonly string[] _emotionLabels;

        public EmotionAnalyzer(string modelPath, int modelInputSize = 224, string[] emotionLabels = null)
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new ArgumentNullException(nameof(modelPath));

            _modelInputSize = modelInputSize;
            _emotionLabels = emotionLabels ?? new string[]
            { "neutral", "happiness", "sadness", "surprise", "anger", "disgust", "fear", "contempt" };

            _session = new InferenceSession(modelPath, new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            });
        }

        // Analysiert Emotionen auf dem gegebenen ROI
        public EmotionDetection Analyze(Mat roi)
        {
            if (roi == null || roi.Empty())
                return new EmotionDetection();

            // Größe anpassen
            using var resized = new Mat();
            Cv2.Resize(roi, resized, new Size(_modelInputSize, _modelInputSize));

            // BGR -> RGB
            using var rgb = new Mat();
            Cv2.CvtColor(resized, rgb, ColorConversionCodes.BGR2RGB);

            // Tensor vorbereiten
            float[] input = PrepareTensor(rgb);
            var tensor = new DenseTensor<float>(input, new int[] { 1, 3, _modelInputSize, _modelInputSize });

            // Eingabename des Modells
            string inputName = _session.InputMetadata.Keys.First();

            using var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor(inputName, tensor) });

            var detection = new EmotionDetection();

            // Expression extrahieren (Emotionserkennung)
            var exprOutput = results.FirstOrDefault(r => r.Name.ToLower().Contains("expression") || r.Name == "output");
            if (exprOutput != null)
            {
                var logits = exprOutput.AsEnumerable<float>().ToArray();
                var probs = Softmax(logits);
                int maxIdx = probs.Length > 0 ? Array.IndexOf(probs, probs.Max()) : -1;

                detection.Expression = maxIdx >= 0 && maxIdx < _emotionLabels.Length ? _emotionLabels[maxIdx] : "Unknown";
                detection.Confidence = maxIdx >= 0 ? probs[maxIdx] : 0f;
            }

            // Valence extrahieren (falls unterstützt)
            var valOutput = results.FirstOrDefault(r => r.Name.ToLower().Contains("valence"));
            if (valOutput != null)
                detection.Valence = valOutput.AsEnumerable<float>().FirstOrDefault();

            // Arousal extrahieren (falls unterstützt)
            var arousalOutput = results.FirstOrDefault(r => r.Name.ToLower().Contains("arousal"));
            if (arousalOutput != null)
                detection.Arousal = arousalOutput.AsEnumerable<float>().FirstOrDefault();

            return detection;
        }

        // Bereitet das Tensor-Array aus dem Bild vor (RGB Planar, Normalisierung)
        private float[] PrepareTensor(Mat img)
        {
            float[] buffer = new float[3 * _modelInputSize * _modelInputSize];
            var indexer = img.GetGenericIndexer<Vec3b>();
            int size = _modelInputSize * _modelInputSize;

            // ImageNet Normalisierung (wichtig für HSEmotion)
            float[] mean = { 0.485f, 0.456f, 0.406f };
            float[] std = { 0.229f, 0.224f, 0.225f };

            for (int y = 0; y < _modelInputSize; y++)
            {
                for (int x = 0; x < _modelInputSize; x++)
                {
                    Vec3b p = indexer[y, x];
                    int idx = y * _modelInputSize + x;

                    // Format: RGB Planar mit Normalisierung
                    buffer[idx] = ((p.Item2 / 255f) - mean[0]) / std[0]; // R
                    buffer[size + idx] = ((p.Item1 / 255f) - mean[1]) / std[1]; // G
                    buffer[2 * size + idx] = ((p.Item0 / 255f) - mean[2]) / std[2]; // B
                }
            }

            return buffer;
        }

        // Softmax-Funktion für Wahrscheinlichkeiten
        private static float[] Softmax(float[] logits)
        {
            if (logits.Length == 0) return Array.Empty<float>();
            float max = logits.Max();
            float sumExp = logits.Sum(l => (float)Math.Exp(l - max));
            return logits.Select(l => (float)Math.Exp(l - max) / sumExp).ToArray();
        }

        // Freigabe der Ressourcen
        public void Dispose() => _session?.Dispose();
    }
}