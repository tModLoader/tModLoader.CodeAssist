using System;

namespace tModLoader.CodeAssist
{
    [Flags]
    public enum tModLoaderVersion
    {
        Unknown = 0,
        tModLoader_1_3 = 1,
        tModLoader_1_4 = 2,
        tModLoader_1_3_Or_1_4 = tModLoader_1_3 | tModLoader_1_4 // 3
    }
}