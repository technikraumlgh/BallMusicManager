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
        {"RNR", "Rock n Roll" },
        {"PT", "Party" },
        {"FS", "Party" },
        //{"FS", "Freestyle" }, // legacy, use Party
    }.ToFrozenDictionary();

    // when ever a dance string enters the program it checks whether it is a known slug and replaces it
    public static string FromSlog(string key) => DanceSlugs.Get(key).Or(key);
}
