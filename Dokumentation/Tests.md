| Test           | Eingabedaten        | Erwartetes Ergebnis      | Tatsächliches Ergebnis         | Fehler |
|----------------|---------------------|--------------------------|--------------------------------|--------|
| VideoLoader    | video.mp4           | VideoData Objekt geladen | Erfolgreich                    | keine  |
| FaceAnalyzer   | Frame mit Gesicht   | 1 Gesicht erkannt        | 1 Gesicht erkannt              | keine  |
| EmotionAnalyzer| Frame mit Gesicht   | EmotionData              | Korrekte Emotionswerte         | keine  |
| SlotProcessor  | EmotionData         | SlotData                 | Richtige Slots                 | keine  |
| VideoAggregator| Liste von SlotData  | AggregatedResult         | Konsolidierte Daten            | keine  |
| CsvWriter      | AggregatedResult    | CSV-Datei erstellt       | CSV-Datei erstellt             | keine  |
