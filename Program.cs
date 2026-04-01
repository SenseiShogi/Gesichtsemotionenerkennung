using System;
using System.IO;
using Gesichtsemotionenerkennung.Controllers;

namespace Gesichtsemotionenerkennung
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== START DES EMOTIONSANALYSESYSTEMS ===");

            // --- EINGABE DES VIDEO-PFADES ---
            Console.Write("Bitte geben Sie den Pfad zur Videodatei ein: ");
            string? inputPath = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                Console.WriteLine("[FEHLER] Die Datei existiert nicht oder der Pfad ist leer.");
                return;
            }

            // --- EINGABE DES FRAME-SCHRITTS ---
            Console.Write("Bitte geben Sie den Frame-Schritt ein (z.B. 1 = jedes Frame, 5000 = alle 5000 ms): ");
            string? stepInput = Console.ReadLine()?.Trim();
            int frameStep = 5000; // Standardwert
            if (!string.IsNullOrEmpty(stepInput) && int.TryParse(stepInput, out int parsedStep) && parsedStep > 0)
            {
                frameStep = parsedStep;
            }

            Console.WriteLine($"\nVideo: {Path.GetFileName(inputPath)} | Frame-Schritt: {frameStep}");

            // --- PFAD ZU MODELLEN UND AUSGABEORDNER ---
            string projectRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");
            string yoloModelPath = Path.Combine(projectRoot, "Assets", "yolo", "yolov12n-face.onnx");
            string emotionModelPath = Path.Combine(projectRoot, "Assets", "hsemotion", "hsemotion_1280.onnx");
            string outputFolder = Path.Combine(projectRoot, "output");

            // --- ÜBERPRÜFUNG DER MODELLDATEIEN ---
            if (!File.Exists(yoloModelPath))
            {
                Console.WriteLine($"[FEHLER] YOLO-Modell nicht gefunden unter: {yoloModelPath}");
                return;
            }
            if (!File.Exists(emotionModelPath))
            {
                Console.WriteLine($"[FEHLER] Emotionsmodell nicht gefunden unter: {emotionModelPath}");
                return;
            }

            try
            {
                // --- INITIALISIERUNG DES VIDEO-CONTROLLERS ---
                var controller = new VideoController(
                    yoloModelPath,
                    emotionModelPath,
                    outputFolder
                );

                // --- START DER VERARBEITUNG ---
                controller.ProcessVideos(inputPath, frameStep);

                Console.WriteLine("\n[ERFOLG] Verarbeitung vollständig abgeschlossen.");
                Console.WriteLine($"Ergebnisse gespeichert in Ordner: {outputFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[KRITISCHER FEHLER]: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nBeliebige Taste zum Beenden drücken...");
            Console.ReadKey();
        }
    }
}