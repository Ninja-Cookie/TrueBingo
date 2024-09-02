using HarmonyLib;
using Reptile;
using Reptile.Phone;
using UnityEngine;
using TrueBingo.BingoSync;

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
                if (!BingoSyncHandler.ConnectedToRoom)
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
                    MarkItem(itemToSend, BingoSyncHandler.ObjectiveType.ItemPickup, pickupType);
            }
        }

        [HarmonyPatch(typeof(SaveSlotData), "UnlockCharacter", MethodType.Normal)]
        public static class SaveSlotData_UnlockCharacter_Patch
        {
            public static void Postfix(Characters character)
            {
                if (!BingoSyncHandler.ConnectedToRoom)
                    return;

                MarkItem(English.GetCharacterName(character).ToLower().FirstCharToUpper(), BingoSyncHandler.ObjectiveType.CharacterUnlock);
            }
        }

        [HarmonyPatch(typeof(NPC), "UnlockTaxi", MethodType.Normal)]
        public static class NPC_UnlockTaxi_Patch
        {
            public static void Postfix()
            {
                if (!BingoSyncHandler.ConnectedToRoom)
                    return;

                MarkItem("Save the Taxi Driver", BingoSyncHandler.ObjectiveType.TaxiDriver);
            }
        }

        [HarmonyPatch(typeof(StageProgress), "AddRep", MethodType.Normal)]
        public static class StageProgress_AddRep_Patch
        {
            public static void Postfix(int ___reputation, Stage ___stageID)
            {
                if (!BingoSyncHandler.ConnectedToRoom)
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
                    MarkItem(English.GetStageName(___stageID), BingoSyncHandler.ObjectiveType.Rep);
            }
        }

        private static async void MarkItem(string itemToSend, BingoSyncHandler.ObjectiveType objectiveType, Pickup.PickUpType? pickupType = null)
        {
            if (!BingoSyncHandler.ConnectedToRoom)
                return;

            await BingoSyncHandler.MarkSquare(itemToSend, objectiveType, pickupType);
        }
    }
}
