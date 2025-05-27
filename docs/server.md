# Server

## Authentication
Beim starten gibt der Server ein API key aus welcher bei jedem post request angegeben werden muss.<br>
Um ein neues Token zu generieren den Server einfach neustarten. Alle Clients *sollten* sich automatisch neu verbinden.<br>
Der Server kann mit dem Argument `--noauth` ohne Authentication gestartet werden.<br>
*Die Authentication is nicht wirklich sicher, aber genung um nervige Siebies mit zu viel Hackerfilmen abzuhalten*<br>