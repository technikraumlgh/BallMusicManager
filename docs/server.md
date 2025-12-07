# Server

## Ändern des Themes
Theme beim starten des Servers mit `--theme <spring|winter>` angeben oder die vorgefertigen scripts ausführen.  
Die Farben des QR-Codes können in [Program.cs](../BallMusic.Server/Program.cs) `OutputQRCode` geändert werden.

## Authentication
Beim starten gibt der Server ein API key aus welcher bei jedem post request angegeben werden muss.  
Um ein neues Token zu generieren den Server einfach neustarten. Alle Clients *sollten* sich automatisch neu verbinden.  
Der Server kann mit dem Argument `--noauth` ohne Authentication gestartet werden.  
*Die Authentication ist nicht wirklich sicher, aber genung um nervige Siebies abzuweren.*

## Neue Themes
Im Ordner `src` sind die Dateien aller Themes. Diese können nach belieben angepasst werden.  
In `<theme>.css` sind die jeweiligen Farben definiert.  
Es können eigene Themes erstellt werden.  
Dafür muss eine entsprechende `.css` angelegt werden, welche alle Farben definiert.  
Optional können mit `<theme>.html` weitere Elemente hinzugefüget werden und mit `<theme>.js` scripte ausgeführt werden.  
**Wichtig: Achtet bei allen Themes darauf dass diese nicht zu viel Strom verbrauchen damit die Tablets im laufe des Abends nicht leer gehen!**
