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
    public const string Rumba = "Rumba";
    public const string Salsa = "Salsa";
    public const string Samba = "Samba";
    public const string Tango = "Tango";
    public const string WienerWalzer = "Wiener Walzer";

    // this exists for compat with the old naming convention and to make entering dances easier
    public static readonly FrozenDictionary<string, string> DanceSlugs = new Dictionary<string, string> {
        {"CCC", ChaChaCha },
        {"LW", LangsamerWalzer },
        {"JVE", Jive },
        {"DFX", Discofox },
        {"FXT", Foxtrott },
        {"FXTR", Foxtrott }, // legacy
        {"RMB", Rumba },
        {"WW", WienerWalzer },
        {"TGO", Tango },
        {"SMB", Samba },
        {"SLS", Salsa },
        {"RNR", RockNRoll },
        {"PT", Party },
        {"FS", Party }, // legacy, was called Freestyle
        {"PSDB", Pasodoble },
    }.ToFrozenDictionary();

    // when ever a dance string enters the program it checks whether it is a known slug and if so replaces it
    public static string FromSlug(string key) => DanceSlugs.TryGetValue(key.ToUpper(), out var name) ? name : key;
}
