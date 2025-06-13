# Server

## Ändern des Themes
Theme beim starten des Servers mit `--theme <spring|winter>` angeben oder die vorgefertigen scripts ausführen.<br>
Die Farben des QR-Codes können in [Program.cs](../BallMusic.Server/Program.cs) `OutputQRCode` geändert werden.

## Authentication
Beim starten gibt der Server ein API key aus welcher bei jedem post request angegeben werden muss.<br>
Um ein neues Token zu generieren den Server einfach neustarten. Alle Clients *sollten* sich automatisch neu verbinden.<br>
Der Server kann mit dem Argument `--noauth` ohne Authentication gestartet werden.<br>
*Die Authentication ist nicht wirklich sicher, aber genung um nervige Siebies abzuweren.*

## Neue Themes
Im Ordner `src` sind die Dateien aller Themes. Diese können nach belieben angepasst werden.<br>
In `<theme>.css` sind die jeweiligen Farben definiert.<br>
Es können eigene Themes erstellt werden.<br>
Dafür muss eine entsprechende `.css` angelegt werden, welche alle Farben definiert.<br>
Optional können mit `<theme>.html` weitere Elemente hinzugefüget werden und mit `<theme>.js` scripte ausgeführt werden.<br>
**Wichtig: Achtet bei allen Themes darauf dass diese nicht zu viel Strom verbrauchen damit die Tablets im laufe des Abends nicht leer gehen!**
