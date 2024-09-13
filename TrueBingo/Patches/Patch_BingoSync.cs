using BingoSyncAPI;
using HarmonyLib;
using Reptile;
using Reptile.Phone;
using UnityEngine;
using TrueBingo.BingoSyncManager;

namespace TrueBingo.Patches
{
    internal class Patch_BingoSync : HarmonyPatch
    {
        static readonly IGameTextLocalizer English = new TextMeshProGameTextLocalizer(SystemLanguage.English, SystemLanguage.English, Core.Instance.localizerData, Core.Instance.platformData.languageData, Core.Instance.platformData.buttonSpriteData, Core.Instance.GameInput);

        [HarmonyPatch(typeof(Pickup), "ApplyPickupType", MethodType.Normal)]
        public static class Pickup_ApplyPickupType_Patch
        {
            public static void Postfix(AUnlockable unlockable, Pickup.PickUpType pickupType)
            {
                if (TrueBingoSync.bingoSync.Status != BingoSync.ConnectionStatus.Connected)
                    return;

                string itemToSend = string.Empty;

                switch (pickupType)
                {
                    case Pickup.PickUpType.MUSIC_UNLOCKABLE:            itemToSend = (unlockable as MusicTrack).Title;                          break;
                    case Pickup.PickUpType.GRAFFITI_UNLOCKABLE:         itemToSend = (unlockable as GraffitiAppEntry).Title;                    break;
                    case Pickup.PickUpType.MOVESTYLE_SKIN_UNLOCKABLE:   itemToSend = English.GetSkinText((unlockable as MoveStyleSkin).Title);  break;
                    case Pickup.PickUpType.OUTFIT_UNLOCKABLE:           itemToSend = $"{English.GetCharacterName((unlockable as OutfitUnlockable).character).ToLower().FirstCharToUpper()}: {English.GetSkinText((unlockable as OutfitUnlockable).outfitName)}"; break;
                }

                if (itemToSend != string.Empty)
                    MarkObjective(itemToSend, TrueBingoSync.ObjectiveType.ItemPickup, pickupType);
            }
        }

        [HarmonyPatch(typeof(SaveSlotData), "UnlockCharacter", MethodType.Normal)]
        public static class SaveSlotData_UnlockCharacter_Patch
        {
            public static void Postfix(Characters character)
            {
                if (TrueBingoSync.bingoSync.Status != BingoSync.ConnectionStatus.Connected)
                    return;

                MarkObjective(English.GetCharacterName(character).ToLower().FirstCharToUpper(), TrueBingoSync.ObjectiveType.CharacterUnlock);
            }
        }

        [HarmonyPatch(typeof(NPC), "UnlockTaxi", MethodType.Normal)]
        public static class NPC_UnlockTaxi_Patch
        {
            public static void Postfix()
            {
                if (TrueBingoSync.bingoSync.Status != BingoSync.ConnectionStatus.Connected)
                    return;

                MarkObjective("Save the Taxi Driver", TrueBingoSync.ObjectiveType.TaxiDriver);
            }
        }

        [HarmonyPatch(typeof(StageProgress), "AddRep", MethodType.Normal)]
        public static class StageProgress_AddRep_Patch
        {
            public static void Postfix(int ___reputation, Stage ___stageID)
            {
                if (TrueBingoSync.bingoSync.Status != BingoSync.ConnectionStatus.Connected)
                    return;

                int reputationToCheck = 999;

                switch(___stageID)
                {
                    case Stage.downhill:
                    case Stage.tower:
                        reputationToCheck = 142;
                    break;

                    case Stage.hideout:
                        reputationToCheck = 108;
                    break;

                    case Stage.square:
                        reputationToCheck = 107;
                    break;

                    case Stage.pyramid:
                    case Stage.Mall:
                        reputationToCheck = 148;
                    break;

                    case Stage.osaka:
                        reputationToCheck = 184;
                    break;
                }

                if (___reputation >= reputationToCheck)
                    MarkObjective(English.GetStageName(___stageID), TrueBingoSync.ObjectiveType.Rep);
            }
        }

        private static void MarkObjective(string itemToSend, TrueBingoSync.ObjectiveType objectiveType, Pickup.PickUpType? pickupType = null)
        {
            TrueBingoSync.MarkObjective(itemToSend, objectiveType, pickupType);
        }
    }
}
