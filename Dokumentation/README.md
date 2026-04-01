# Gesichtsemotionenerkennung

## 1. Projektbeschreibung

Das Projekt **Gesichtsemotionenerkennung** dient der automatischen Analyse der Emotionen von 10 Spielern während eines psychologisch-zählbasierten Teamspiels.  
Es ermöglicht, Videos zu verarbeiten, Gesichter zu erkennen, Emotionen zu bestimmen und die Ergebnisse in einer konsolidierten CSV-Datei auszugeben.

---

## 2. Funktionen

- **VideoLoader**: Laden und Decodieren von Videos (.mp4, .avi).  
- **FaceAnalyzer**: Erkennung von Gesichtern in jedem Frame.  
- **EmotionAnalyzer**: Bestimmung der Emotionen jedes erkannten Gesichts.  
- **SlotProcessor / SlotPipeline**: Verarbeitung der Emotionen in Slots.  
- **VideoAggregator**: Aggregation der Slot-Daten über das gesamte Video.  
- **CsvWriter**: Erstellung einer CSV-Datei mit den Ergebnissen.  

---

## 3. Technische Umsetzung

- **Sprache / Plattform**: C# (.NET 6+)  
- **Architektur**: MVC + Services  
- **Wichtige Klassen**:

  **Controllers**: VideoController  
  **Models**: EmotionDetection, FrameSlotData, PlayerFrameData, PlayerSlot, Settings, UnifiedFrameContext  
  **Services**: EmotionAnalyzer, FaceAnalyzer, SlotPipeline, SlotProcessor, VideoAggregator, VideoLoader, YoloSegmentDetector  
  **Utils**: Geometry, Smoothing, Visualization  
  **Views**: CsvWriter  

- **Bibliotheken / Frameworks**:  
  - [OpenCVSharp](https://github.com/shimat/opencvsharp) für Videoverarbeitung  
  - [Microsoft.ML.OnnxRuntime](https://www.nuget.org/packages/Microsoft.ML.OnnxRuntime/) für Emotionserkennung  

---

## 4. Aufbau- und Startanweisungen

1. Repository klonen:  
   ```bash
   git clone https://github.com/SenseiShogi/Gesichtsemotionenerkennung

2. Projekt in Visual Studio oder einer anderen .NET-Umgebung öffnen.

3. Abhängigkeiten installieren:
- OpenCVSharp
- Microsoft.ML.OnnxRuntime

4. Projekt bauen (Build Solution).
VideoController starten und Pfad zum Video angeben.
CSV-Datei mit den Ergebnissen wird im angegebenen Verzeichnis erstellt.

5. Bekannte Einschränkungen
Das Programm analysiert nur Videoformate, die von VideoLoader unterstützt werden (z.B. .mp4, .avi). Andere Formate führen zu einer Ausnahme.
Die Algorithmen FaceAnalyzer und EmotionAnalyzer arbeiten mit einem vortrainierten Modell: die Erkennungsqualität hängt von Beleuchtung, Videoauflösung und Gesichtsposition ab.
Das Programm ist für die Erkennung der Emotionen von 10 Spielern während eines psychologisch-zählbasierten Teamspiels konzipiert.
Die Verarbeitung der Frames dauert bei langen Videos aufgrund der sequenziellen Analyse jedes Frames erheblich.
Keine Multithreading-Unterstützung: Frames werden nacheinander verarbeitet.
CsvWriter prüft keine doppelten Daten; ein erneuter Export kann Duplikate erzeugen.
Fehler beim Lesen von Videos oder fehlende Gesichter führen nicht zum Programmabbruch, die Daten für diese Frames fehlen jedoch im Ergebnis.

6. Lizenz
Das Projekt ist Open-Source (Lizenz nach Wahl, z.B. MIT oder GPL).