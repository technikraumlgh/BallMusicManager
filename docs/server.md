# Server

## Ändern des Themes
In [SongDisplay.html](../BallMusic.Server/SongDisplay.html) `<html data-theme="...">` auf `winter` oder `spring` setzen.<br>
Die Farben des QR-Codes können in [Program.cs](../BallMusic.Server/Program.cs) `OutputQRCode` geändert werden.

## Authentication
Beim starten gibt der Server ein API key aus welcher bei jedem post request angegeben werden muss.<br>
Um ein neues Token zu generieren den Server einfach neustarten. Alle Clients *sollten* sich automatisch neu verbinden.<br>
Der Server kann mit dem Argument `--noauth` ohne Authentication gestartet werden.<br>
*Die Authentication ist nicht wirklich sicher, aber genung um nervige Siebies abzuhalten.*

## CSS Files?
SongDisplay.html hat alle styles inline definiert, da der Server keinen resource endpoint hat. Wenn dich das nerft musst du entweder ein Endpoint nur für das css anlegen (siehe `/snow.js`) oder einen vernünftigen resource endpoint programmieren