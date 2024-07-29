using Ametrin.Utils;
using Ametrin.Utils.Optional;
using System.Collections.Frozen;

namespace BallMusicManager.Domain;

public static class Dance{
    public static readonly FrozenDictionary<string, string> DanceKeys = new Dictionary<string, string>{
        {"CCC", "ChaChaCha" },
        {"LW", "Langsamer Walzer" },
        {"JVE", "Jive" },
        {"DFX", "Discofox" },
        {"FXTR", "Foxtrott" },
        {"RMB", "Rumba" },
        {"WW", "Wiener Walzer" },
        {"TGO", "Tango" },
        {"SMB", "Samba" },
        {"SLS", "Salsa" },
        {"RNR", "Rock'n Roll" },
        {"PT", "Party" },
        //{"FS", "Freestyle" }, // legacy
    }.ToFrozenDictionary();

    public static string FromKey(string key) => DanceKeys.Get(key).Reduce(key);
}
