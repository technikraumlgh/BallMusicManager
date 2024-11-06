namespace BallMusicManager.Domain;

public static class Dance
{
    // this exists for compat with the old naming convention and to make entering dances easier
    public static readonly FrozenDictionary<string, string> DanceSlugs = new Dictionary<string, string> {
        {"CCC", "ChaChaCha" },
        {"LW", "Langsamer Walzer" },
        {"JVE", "Jive" },
        {"DFX", "Discofox" },
        {"FXT", "Foxtrott" },
        {"FXTR", "Foxtrott" }, // legacy
        {"RMB", "Rumba" },
        {"WW", "Wiener Walzer" },
        {"TGO", "Tango" },
        {"SMB", "Samba" },
        {"SLS", "Salsa" },
        {"RNR", "Rock 'n' Roll" },
        {"PT", "Party" },
        {"FS", "Party" }, // legacy, was called Freestyle
    }.ToFrozenDictionary();

    // when ever a dance string enters the program it checks whether it is a known slug and if so replaces it
    public static string FromSlug(string key) => DanceSlugs.Get(key.ToUpper()).Or(key);
}
