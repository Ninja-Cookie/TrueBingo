using HarmonyLib;
using Reptile;
using UnityEngine;
using static Reptile.Story;

namespace TrueBingo.Patches
{
    internal class Patch_NewGame : HarmonyPatch
    {
        public static bool newGame = false;

        [HarmonyPatch(typeof(BaseModule), "StartNewGame")]
        public static class BaseModule_StartNewGame_Patch
        {
            public static bool Prefix(BaseModule __instance)
            {
                newGame = true;

                BingoConfig.HandleConfig();
                __instance.LoadStage(BingoConfig.stage);
                return false;
            }
        }

        [HarmonyPatch(typeof(ActiveOnChapter), "OnStageInitialized")]
        public static class ActiveOnChapter_OnStageInitialized_Patch
        {
            public static void Prefix()
            {
                WorldHandler.instance?.StoryManager?.SetStoryObjective(Story.ObjectiveID.HangOut);
            }

            public static void Postfix(ActiveOnChapter __instance)
            {
                if (Utility.GetCurrentStage() != Stage.hideout)
                    __instance.gameObject.SetActive((!BingoConfig.disableStory || __instance.chapters.Contains(Chapter.CHAPTER_6)) && __instance.name != "BeforeFinalBossElephants");
                else
                    __instance.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(BaseModule), "HandleStageFullyLoaded")]
        public static class BaseModule_HandleStageFullyLoaded_Patch
        {
            public static void Postfix()
            {
                SaveManager saveManager = Core.Instance?.SaveManager;

                WorldHandler worldHandler = WorldHandler.instance;
                Player player = worldHandler?.GetCurrentPlayer();

                if (player != null && saveManager?.CurrentSaveSlot != null)
                {
                    Stage currentStage = Utility.GetCurrentStage();

                    saveManager.CurrentSaveSlot.GetStageProgress(currentStage).respawnPos = player.transform.position;
                    saveManager.CurrentSaveSlot.GetStageProgress(currentStage).respawnRot = player.transform.rotation.eulerAngles;
                    saveManager.SaveCurrentSaveSlotImmediate();
                }

                BingoHandleStage.UpdateStageProgress(worldHandler?.SceneObjectsRegister, Utility.GetCurrentStage());
            }
        }

        [HarmonyPatch(typeof(WorldHandler), "SavePosToNearestReachedRespawner")]
        public static class WorldHandler_SavePosToNearestReachedRespawner_Patch
        {
            public static bool Prefix(WorldHandler __instance)
            {
                if (!BingoHandleStage.HasLeftRespawn)
                {
                    Player player = __instance.GetCurrentPlayer();

                    if (player != null)
                    {
                        if (Vector3.Distance(player.transform.position, BingoHandleStage.spawnPosition) > 2f)
                        {
                            BingoHandleStage.HasLeftRespawn = true;
                            return true;
                        }
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
