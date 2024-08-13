using HarmonyLib;
using Reptile;
using System.Linq;

namespace TrueBingo.Patches
{
    internal class Patch_ItemDisplay : HarmonyPatch
    {
        private static string charName = string.Empty;

        [HarmonyPatch(typeof(Pickup), "ApplyPickupType")]
        public static class Pickup_ApplyPickupType_Patch
        {
            public static void Prefix(AUnlockable unlockable, Pickup.PickUpType pickupType)
            {
                if (pickupType == Pickup.PickUpType.OUTFIT_UNLOCKABLE)
                {
                    OutfitUnlockable outfitUnlockable = unlockable as OutfitUnlockable;

                    if (outfitUnlockable != null)
                        charName = Core.Instance?.Localizer?.GetCharacterName(outfitUnlockable.character);
                }
            }
        }

        [HarmonyPatch(typeof(UIManager), "ShowNotification", typeof(string), typeof(string[]))]
        public static class UIManager_ShowNotification_Patch
        {
            public static void Prefix(ref string text, params string[] textsToInsert)
            {
                if (charName != string.Empty)
                {
                    int paramCount = textsToInsert.Count();

                    if (paramCount > 0)
                    {
                        for (int i = 0; i < paramCount; i++)
                        {
                            if (textsToInsert[i].Equals("???"))
                            {
                                textsToInsert[i] = charName;
                                break;
                            }
                        }
                    }

                    charName = string.Empty;
                }
            }
        }
    }
}
