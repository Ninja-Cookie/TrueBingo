using HarmonyLib;
using Reptile;

namespace TrueBingo.Patches
{
    internal class Patch_NPCAvilability : HarmonyPatch
    {
        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnPlayer")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnPlayer_Patch
        {
            public static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnCharacter")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnCharacter_Patch
        {
            public static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnCharacterSelect")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnCharacterSelect_Patch
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
