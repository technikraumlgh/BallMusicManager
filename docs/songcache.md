## Song Cache
Unfortunally NAudio cannot play audio files from zip archives directly. To solve this we have to export the file  before playing it. `SongCache.CacheFromArchive(SongBuilder)` returns a fixed file location depending on where the song was stored before.<br>
The Creator exports files on demand (when you try to play them) and then keeps them in cache until being closed (So playing a file multiple times will only export it once).<br>
The Player caches all songs contained in a playlist while opening it. This way we have no delay between songs and we don't risk any crashes from zip decompression.

The Song cache is a hidden folder called `$cache` in the executing folder. It can be found by enabling `View>Show>Hidden Items` in the file explorer.