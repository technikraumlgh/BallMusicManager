# Playlist File (.plz)
sowohl die Playlisten als auch die Library Dateien sind ZIP-Dateien. Sie können also einfach als Archiv geöffnet werden (z.B. mit 7-Zip). **Ändere NIEMALS eine Datei von Hand**. Es wird nicht funktionieren.<br>
Dateien innerhalb einer Playlist-Datei:
- manifest.json Enthält wichtige Informationen zur Datei (Version, Anzahl Songs, Datum gespeichert)
- song_list.json enthält alle Metadaten (z.B. Titel, Künstler, Tanz)

Die Audiodatein der Songs werden einzeln in der ZIP gespeichert, bennant als Hash der Datei.
Um die Audiodatei eines Songs zu finden muss der Hash aus den Metadaten gelesen werden und dann die danach benannte Datei gelesen werden.