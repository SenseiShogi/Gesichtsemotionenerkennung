using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Gesichtsemotionenerkennung.Services
{
    // IDisposable notwendig, da Mat-Objekte verwaltet werden
    public class SegmentationDetection : IDisposable
    {
        // Begrenzungsrechteck der erkannten Region
        public Rect BoundingBox { get; set; }

        // Vertrauen/Confidence der Erkennung
        public float Confidence { get; set; }

        // Klassenbezeichnung, z.B. "face"
        public string ClassLabel { get; set; } = "";

        // ID der Klasse
        public int ClassId { get; set; }

        // Ausgeschnittener Gesichtsbereich als Mat
        public Mat FaceImage { get; set; }

        public void Dispose()
        {
            FaceImage?.Dispose();
        }
    }

    public class YoloSegmentDetector : IDisposable
    {
        private readonly InferenceSession _session;
        private const int ModelInputSize = 640;
        private readonly float _confidenceThreshold;
        private readonly int _faceClassId;

        // Konstruktor: Modellpfad, Confidence-Schwelle und Face-Klassen-ID
        public YoloSegmentDetector(string modelPath, float confidenceThreshold = 0.3f, int faceClassId = 0)
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new ArgumentNullException(nameof(modelPath));

            _session = new InferenceSession(modelPath, new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            });

            _confidenceThreshold = confidenceThreshold;
            _faceClassId = faceClassId;
        }

        // Hauptmethode zur Erkennung von Segmenten im gegebenen Slot
        public List<SegmentationDetection> Detect(Mat slot)
        {
            var results = new List<SegmentationDetection>();
            if (slot == null || slot.Empty()) return results;

            int originalWidth = slot.Width;
            int originalHeight = slot.Height;

            // 1. Bildvorbereitung (Letterboxing)
            float scale = Math.Min((float)ModelInputSize / originalWidth, (float)ModelInputSize / originalHeight);
            int newWidth = (int)(originalWidth * scale);
            int newHeight = (int)(originalHeight * scale);

            using Mat resized = new Mat();
            Cv2.Resize(slot, resized, new Size(newWidth, newHeight), 0, 0, InterpolationFlags.Linear);

            using Mat canvas = Mat.Zeros(ModelInputSize, ModelInputSize, MatType.CV_8UC3);
            int topPad = (ModelInputSize - newHeight) / 2;
            int leftPad = (ModelInputSize - newWidth) / 2;
            Rect roiRect = new Rect(leftPad, topPad, newWidth, newHeight);
            resized.CopyTo(new Mat(canvas, roiRect));

            // 2. Konvertierung in Tensor
            using Mat rgb = new Mat();
            Cv2.CvtColor(canvas, rgb, ColorConversionCodes.BGR2RGB);
            float[] inputTensor = PrepareTensor(rgb);

            var tensor = new DenseTensor<float>(inputTensor, new int[] { 1, 3, ModelInputSize, ModelInputSize });
            string inputName = _session.InputMetadata.Keys.First();

            using var outputResults = _session.Run(new[] { NamedOnnxValue.CreateFromTensor(inputName, tensor) });

            // 3. Verarbeitung des Outputs (slot für Gesichtsausschnitt übergeben)
            results.AddRange(ProcessOutputs(outputResults, slot, scale, topPad, leftPad));

            return results;
        }

        // Wandelt das OpenCV-Mat in ein Float-Array für ONNX um
        private float[] PrepareTensor(Mat img)
        {
            float[] buffer = new float[3 * ModelInputSize * ModelInputSize];
            var indexer = img.GetGenericIndexer<Vec3b>();
            int channelSize = ModelInputSize * ModelInputSize;

            for (int y = 0; y < ModelInputSize; y++)
            {
                for (int x = 0; x < ModelInputSize; x++)
                {
                    Vec3b p = indexer[y, x];
                    int idx = y * ModelInputSize + x;

                    buffer[idx] = p.Item2 / 255f;                   // R
                    buffer[channelSize + idx] = p.Item1 / 255f;     // G
                    buffer[2 * channelSize + idx] = p.Item0 / 255f; // B
                }
            }
            return buffer;
        }

        // Verarbeitung der ONNX-Ausgaben und Erzeugung der SegmentationDetection-Objekte
        private IEnumerable<SegmentationDetection> ProcessOutputs(
            IReadOnlyCollection<NamedOnnxValue> outputs,
            Mat originalImage,
            float scale,
            int topPad,
            int leftPad)
        {
            var outputValue = outputs.First().AsTensor<float>();

            int dim1 = outputValue.Dimensions[1];
            int dim2 = outputValue.Dimensions[2];

            int numCandidates = Math.Max(dim1, dim2);
            bool isTransposed = dim1 < dim2;

            for (int i = 0; i < numCandidates; i++)
            {
                float conf, cx, cy, wBox, hBox;
                int classId;

                if (isTransposed)
                {
                    cx = outputValue[0, 0, i];
                    cy = outputValue[0, 1, i];
                    wBox = outputValue[0, 2, i];
                    hBox = outputValue[0, 3, i];
                    conf = outputValue[0, 4, i];
                    classId = (int)outputValue[0, 5, i];
                }
                else
                {
                    cx = outputValue[0, i, 0];
                    cy = outputValue[0, i, 1];
                    wBox = outputValue[0, i, 2];
                    hBox = outputValue[0, i, 3];
                    conf = outputValue[0, i, 4];
                    classId = (int)outputValue[0, i, 5];
                }

                if (conf < _confidenceThreshold) continue;

                Rect detectedRect = new Rect(
                    (int)(cx - wBox / 2),
                    (int)(cy - hBox / 2),
                    (int)wBox,
                    (int)hBox);

                Rect originalRect = ConvertDetectionToOriginal(detectedRect, scale, topPad, leftPad);

                // Randprüfung: Rechteck darf Originalbild nicht überschreiten
                Rect safeRect = new Rect(
                    Math.Max(0, originalRect.X),
                    Math.Max(0, originalRect.Y),
                    Math.Min(originalRect.Width, originalImage.Width - Math.Max(0, originalRect.X)),
                    Math.Min(originalRect.Height, originalImage.Height - Math.Max(0, originalRect.Y))
                );

                if (safeRect.Width < 5 || safeRect.Height < 5) continue;

                // Gesicht ausschneiden und klonen, um Speicherbindung zu lösen
                Mat faceChip = new Mat(originalImage, safeRect).Clone();

                yield return new SegmentationDetection
                {
                    BoundingBox = safeRect,
                    Confidence = conf,
                    ClassLabel = "face",
                    ClassId = classId,
                    FaceImage = faceChip
                };
            }
        }

        // Umrechnung des Rechtecks vom skalierten Input zum Originalbild
        private Rect ConvertDetectionToOriginal(Rect detectedRect, float scale, int topPad, int leftPad)
        {
            int x = (int)((detectedRect.X - leftPad) / scale);
            int y = (int)((detectedRect.Y - topPad) / scale);
            int w = (int)(detectedRect.Width / scale);
            int h = (int)(detectedRect.Height / scale);

            return new Rect(Math.Max(0, x), Math.Max(0, y), w, h);
        }

        public void Dispose() => _session?.Dispose();
    }
}