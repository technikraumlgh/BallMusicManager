namespace BallMusic.Domain;

public static class Dance
{
    public const string ChaChaCha = "ChaChaCha";
    public const string Discofox = "Discofox";
    public const string Foxtrott = "Foxtrott";
    public const string Jive = "Jive";
    public const string LangsamerWalzer = "Langsamer Walzer";
    public const string RockNRoll = "Rock 'n' Roll";
    public const string Party = "Party";
    public const string Pasodoble = "Pasodoble";
    public const string Quickstep = "Quickstep";
    public const string Rumba = "Rumba";
    public const string Salsa = "Salsa";
    public const string Samba = "Samba";
    public const string Slowfox = "Slowfox";
    public const string Tango = "Tango";
    public const string WienerWalzer = "Wiener Walzer";

    // this exists for compat with the old naming convention and to make entering dances easier
    public static readonly FrozenDictionary<string, string> DanceSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
        {"CCC", ChaChaCha },
        {"LW", LangsamerWalzer },
        {"JVE", Jive },
        {"DFX", Discofox },
        {"FXT", Foxtrott },
        {"RMB", Rumba },
        {"WW", WienerWalzer },
        {"TGO", Tango },
        {"SMB", Samba },
        {"SLS", Salsa },
        {"RNR", RockNRoll },
        {"PT", Party },
        {"FS", Party }, // legacy, was called Freestyle
        {"PSDB", Pasodoble },
        {"SFX", Slowfox },
        {"QS", Quickstep },
    }.ToFrozenDictionary();

    // when ever a dance string enters the program it checks whether it is a known slug and if so replaces it
    public static string FromSlug(string key) => DanceSlugs.TryGetValue(key, out var name) ? name : key;
}
