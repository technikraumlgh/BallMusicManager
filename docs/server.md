# Server

## Ändern des Themes
In [SongDisplay.html](../BallMusic.Server/SongDisplay.html) `<html data-theme="...">` auf `winter` oder `spring` setzen.

## Authentication
Beim starten gibt der Server ein API key aus welcher bei jedem post request angegeben werden muss.<br>
Um ein neues Token zu generieren den Server einfach neustarten. Alle Clients *sollten* sich automatisch neu verbinden.<br>
Der Server kann mit dem Argument `--noauth` ohne Authentication gestartet werden.<br>
*Die Authentication ist nicht wirklich sicher, aber genung um nervige Siebies abzuhalten*<br>