using HarmonyLib;
using Reptile;

namespace TrueBingo.Patches
{
    internal class Patch_NPCAvailability : HarmonyPatch
    {
        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnPlayer")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnPlayer_Patch
        {
            public static bool Prefix(SceneObjectsRegister ___sceneObjectsRegister)
            {
                EnableNPCs(___sceneObjectsRegister);
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnCharacter")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnCharacter_Patch
        {
            public static bool Prefix(SceneObjectsRegister ___sceneObjectsRegister)
            {
                EnableNPCs(___sceneObjectsRegister);
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldHandler), "SetNPCAvailabilityBasedOnCharacterSelect")]
        public static class WorldHandler_SetNPCAvailabilityBasedOnCharacterSelect_Patch
        {
            public static bool Prefix(SceneObjectsRegister ___sceneObjectsRegister)
            {
                EnableNPCs(___sceneObjectsRegister);
                return false;
            }
        }

        public static void EnableNPCs(SceneObjectsRegister sceneObjectsRegister)
        {
            foreach (NPC npc in sceneObjectsRegister.NPCs)
            {
                npc.InvokeMethod("SetAvailable", true);
            }
        }
    }
}
