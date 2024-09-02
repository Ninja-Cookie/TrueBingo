using HarmonyLib;
using Reptile;
using TrueBingo.BingoSync;

namespace TrueBingo.Patches
{
    internal class Patch_Inputs : HarmonyPatch
    {
        [HarmonyPatch(typeof(UserInputHandler), "PollInputs")]
        public static class UserInputHandler_PollInputs_Patch
        {
            public static bool Prefix(ref UserInputHandler.InputBuffer __result)
            {
                if (BingoSyncGUI.GUIOpen)
                {
                    __result = new UserInputHandler.InputBuffer();
                    return false;
                }
                return true;
            }
        }
    }
}
