namespace Gesichtsemotionenerkennung
{
    public static class ProjectStructure
    {
        public static readonly string[] Files = new string[]
        {
            // Controllers
            "Controllers/VideoController.cs",

            // Models
            "Models/EmotionDetection.cs",
            "Models/FrameSlotData.cs",
            "Models/PlayerFrameData.cs",
            "Models/PlayerSlot.cs",
            "Models/Settings.cs",
            "Models/UnifiedFrameContext.cs",

            // Services
            "Services/EmotionAnalyzer.cs",
            "Services/FaceAnalyzer.cs",
            "Services/FrameSplitter.cs",
            "Services/SlotPipeline.cs",
            "Services/SlotProcessor.cs",
            "Services/VideoAggregator.cs",
            "Services/VideoLoader.cs",
            "Services/YoloSegmentDetector.cs",

            // Utils
            "Utils/Geometry.cs",
            "Utils/Smoothing.cs",
            "Utils/Visualization.cs",

            // Views
            "Views/CsvWriter.cs"
        };
    }
}