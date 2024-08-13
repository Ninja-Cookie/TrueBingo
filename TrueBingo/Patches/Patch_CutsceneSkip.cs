using HarmonyLib;
using Reptile;
using UnityEngine.Playables;

namespace TrueBingo.Patches
{
    internal class Patch_CutsceneSkip : HarmonyPatch
    {
        [HarmonyPatch(typeof(SequenceHandler), "UpdateSequenceHandler")]
        public static class UpdateSequenceHandler_Patch
        {
            public static void Prefix(SequenceHandler __instance, Player ___player, PlayableDirector ___sequence, ref float ___skipFadeDuration, ref float ___skipStartTimer)
            {
                if (BingoConfig.fastCutscene)
                {
                    ___skipFadeDuration = 0f;
                    ___skipStartTimer = 1.5f;
                }

                if (BingoConfig.cutsceneSkip && ___player != null && ___player.IsBusyWithSequence() && ___sequence != null && !___sequence.ToString().Contains("ChangeOutfitSequence"))
                {
                    if (__instance.GetValue<int>("skipTextActiveState") == 0)
                        __instance.SetValue<int>("skipTextActiveState", 1);
                }
            }
        }
    }
}
