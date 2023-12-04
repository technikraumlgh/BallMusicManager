using System.Collections.Frozen;
using Ametrin.Utils.Registry;

namespace BallMusicManager.Domain;

public static class Dance{
    private static readonly IRegistry<string, string> DanceKeys = new Dictionary<string, string>{
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
        {"FS", "Freestyle" },
    }.ToRegistry();

    public static string FromKey(string key) => DanceKeys.TryGet(key).Reduce(key);
}
