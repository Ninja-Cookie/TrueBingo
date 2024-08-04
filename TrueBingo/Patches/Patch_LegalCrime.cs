using HarmonyLib;
using Reptile;

namespace TrueBingo.Patches
{
    internal class Patch_LegalCrime : HarmonyPatch
    {
        [HarmonyPatch(typeof(WantedManager), "ProcessCrime")]
        public static class ProcessCrime_Patch
        {
            public static bool Prefix()
            {
                return !BingoConfig.disableCops;
            }
        }
    }
}
