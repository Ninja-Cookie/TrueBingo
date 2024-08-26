using HarmonyLib;
using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace TrueBingo.Patches
{
    internal class Patch_RepLabel : HarmonyPatch
    {
        [HarmonyPatch(typeof(Player), "SetRepLabel")]
        public static class Player_SetRepLabel_Patch
        {
            public static bool Prefix(GameplayUI ___ui)
            {
                if (!BingoConfig.repDisplay)
                    return true;

                StageProgress progress = Core.Instance?.SaveManager?.CurrentSaveSlot?.GetCurrentStageProgress();

                if (progress != null)
                {
                    ___ui.repLabel.text = progress.reputation.ToString();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "RepDisplayUpdate")]
        public static class Player_RepDisplayUpdate_Patch
        {
            public static bool Prefix(ref float ___showRepLingerTimer, ref int ___showAddRep, ref float ___showingREP)
            {
                if (!BingoConfig.repDisplay)
                    return true;

                ___showRepLingerTimer   = 99f;
                ___showAddRep           = 0;
                ___showingREP           = -1f;

                return true;
            }
        }

        public static List<Transform> activeUI = new List<Transform>();

        [HarmonyPatch(typeof(Player), "StartGraffitiMode")]
        public static class Player_StartGraffitiMode_Patch
        {
            public static void Postfix(GameplayUI ___ui)
            {
                if (BingoConfig.repDisplay)
                {
                    if (activeUI.Count > 0)
                        activeUI.Clear();

                    ___ui.gameplayScreen.gameObject.SetActive(true);

                    foreach (var uiElement in ___ui.gameplayScreen.GetAllChildren())
                    {
                        if (uiElement != null && !uiElement.name.ToLower().Contains("rep") && uiElement.gameObject.activeSelf)
                        {
                            activeUI.Add(uiElement);
                            uiElement.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), "EndGraffitiMode")]
        public static class Player_EndGraffitiMode_Patch
        {
            public static void Prefix(GameplayUI ___ui)
            {
                if (BingoConfig.repDisplay && activeUI != null && activeUI.Count > 0)
                {
                    foreach (var comp in activeUI)
                    {
                        comp?.gameObject?.SetActive(true);
                    }

                    activeUI.Clear();
                }
            }
        }
    }
}
