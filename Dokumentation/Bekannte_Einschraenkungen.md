## Bekannte Einschränkungen

- Das Programm analysiert nur Videoformate, die von **VideoLoader** unterstützt werden (z.B. `.mp4`, `.avi`). Andere Formate führen zu einer Ausnahme.  

- Die Algorithmen **FaceAnalyzer** und **EmotionAnalyzer** arbeiten mit einem vortrainierten Modell: die Erkennungsqualität hängt von Beleuchtung, Videoauflösung und Gesichtsposition ab.  

- Das Programm ist für die Erkennung der Emotionen von **10 Spielern während eines psychologisch-zählbasierten Teamspiels** konzipiert.  

- Die Verarbeitung der Frames dauert bei langen Videos aufgrund der sequenziellen Analyse jedes Frames erheblich. 

- Keine Multithreading-Unterstützung: Frames werden nacheinander verarbeitet.  

- **CsvWriter** prüft keine doppelten Daten; ein erneuter Export kann Duplikate erzeugen.  

- Fehler beim Lesen von Videos oder fehlende Gesichter führen nicht zum Programmabbruch, die Daten für diese Frames fehlen jedoch im Ergebnis.  