using Reptile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TrueBingo.BingoStageItems;

namespace TrueBingo
{
    public static class BingoHandleStage
    {
        public static bool HasLeftRespawn = false;
        public static Vector3 spawnPosition = Vector3.zero;

        public static void UpdateStage()
        {
            BingoConfig.HandleConfig();

            UpdateObjective();

            if (Patches.Patch_NewGame.newGame)
            {
                Patches.Patch_NewGame.newGame = false;

                WorldHandler worldHandler = WorldHandler.instance;
                Player player = worldHandler?.GetCurrentPlayer();

                if (player != null)
                {
                    SaveManager saveManager = Core.Instance?.SaveManager;

                    // Init Character
                    player.SetCharacter(BingoConfig.character);
                    player.InitVisual();

                    // Handle Movestyle
                    player.SetCurrentMoveStyleEquipped(BingoConfig.moveStyle);
                    player.InitAbilities();
                    player.GetValue<CharacterVisual>("characterVisual").SetMoveStyleVisualProps(player, BingoConfig.moveStyle);
                    player.SwitchToEquippedMovestyle(false, showEffect: false);

                    if (saveManager?.CurrentSaveSlot != null)
                        saveManager.CurrentSaveSlot.GetCharacterProgress(BingoConfig.character).moveStyle = BingoConfig.moveStyle;

                    // Handle Outfit
                    player.SetOutfit(BingoConfig.outfit);

                    // Handle Story Setup
                    saveManager?.CurrentSaveSlot?.SetupStoryCharactersLocked();
                    player.LockFortuneApp(true);

                    for (int i = 0; i < (int)Dances.MAX; i++)
                        saveManager?.CurrentSaveSlot?.UnlockDance(i);

                    // Handle Player Spawn
                    spawnPosition = BingoConfig.position;
                    Quaternion spawnRotation = player.transform.rotation;

                    if (spawnPosition == Vector3.zero)
                    {
                        List<PlayerSpawner> playerSpawners = worldHandler.SceneObjectsRegister?.playerSpawners;
                        System.Random random = BingoConfig.seed == 0 ? new System.Random() : new System.Random(BingoConfig.seed);

                        if (playerSpawners.Count > 0)
                        {
                            PlayerSpawner spawner = playerSpawners[random.Next(playerSpawners.Count)];
                            spawnPosition = spawner.transform.position;
                            spawnRotation = spawner.transform.rotation;
                        }
                    }

                    worldHandler.PlaceCurrentPlayerAt(spawnPosition, spawnRotation);

                    HasLeftRespawn = false;

                    // Save
                    saveManager.SaveCurrentSaveSlot();
                }
            }

            Stage stage = Utility.GetCurrentStage();
            SceneObjectsRegister sceneObjectsRegister = WorldHandler.instance?.SceneObjectsRegister;

            List<ProgressObject> progressObjects = sceneObjectsRegister?.progressObjects;

            if (sceneObjectsRegister != null )
            {
                if (BingoConfig.disableStory)
                {
                    foreach (var pickup in Resources.FindObjectsOfTypeAll<Collectable>().Where(x => x.GetValue<Pickup.PickUpType>("pickUpType") == Pickup.PickUpType.GRAFFITI_UNLOCKABLE))
                    {
                        pickup.canOpen = true;
                        pickup.InvokeMethod("SetAvailable", true);
                    }
                }

                if (progressObjects != null)
                {
                    ProgressObject[] objectsToDisable   = new ProgressObject[0];
                    ProgressObject[] objectsToEnable    = progressObjects.Where(x => GlobalProgressObjects.Contains(x.name)).ToArray();

                    switch (stage)
                    {
                        case Stage.hideout:
                            objectsToDisable = progressObjects.Where(x => HideoutProgressObjects.Contains(x.name)).ToArray();
                        break;

                        case Stage.downhill:
                            objectsToDisable = progressObjects.Where(x => DownhillProgressObjects.Contains(x.name)).ToArray();
                        break;

                        case Stage.square:
                            objectsToDisable = progressObjects.Where(x => SquareProgressObjects.Contains(x.name)).ToArray();
                        break;
                    }

                    foreach(var objectToDisable in objectsToDisable)
                        objectToDisable.gameObject.SetActive(false);

                    foreach (var objectToEnable in objectsToEnable)
                    {
                        objectToEnable.gameObject.SetActive(true);
                        objectToEnable.SetAvailable(true);
                        objectToEnable.SetTriggerable(true);
                    }
                }
            }

            UpdateStageProgress();

            if (BingoConfig.disableBMX)
                DisableBMXDoors();
        }

        private static void DisableBMXDoors()
        {
            BMXOnlyGateway bmxGate = Object.FindObjectOfType<BMXOnlyGateway>();

            if (bmxGate != null)
                bmxGate.InvokeMethod("SetState", 5);
        }

        public static void UpdateObjective()
        {
            Story.ObjectiveID newObjective = Story.ObjectiveID.HangOut;

            SaveSlotData saveSlotData = Core.Instance?.SaveManager?.CurrentSaveSlot;

            if (saveSlotData != null && saveSlotData.CurrentStoryObjective != newObjective)
            {
                saveSlotData.CurrentStoryObjective = newObjective;
                Core.Instance.SaveManager.SaveCurrentSaveSlot();

                WorldHandler.instance?.StoryManager?.UpdateObjectives();
                if (!(Mapcontroller.Instance == null))
                {
                    Mapcontroller.Instance.UpdateObjectivePins();
                }
            }
        }

        public static void UpdateWanted()
        {
            if (BingoConfig.disableCops)
                WantedManager.instance?.StopPlayerWantedStatus(false);
        }

        public static void UpdateStageProgress()
        {
            Stage stage = Utility.GetCurrentStage();
            SceneObjectsRegister sceneObjectsRegister = WorldHandler.instance?.SceneObjectsRegister;
            List<AProgressable> progressablesObjects = sceneObjectsRegister?.progressables;

            if (progressablesObjects != null)
            {
                // Not Needed - Just Backup For Unforseen Side-Effects
                // ------------------------
                AProgressable challenge = progressablesObjects.Find(x => PostGameChallenges.Contains(x.name));

                if (challenge != null)
                    challenge.gameObject.SetActive(true);
                // ------------------------

                AProgressable[] objectsToDisable    = new AProgressable[0];
                AProgressable[] objectsToEnable     = new AProgressable[0];

                AProgressable CombatEncounter_IreneUnlockChallenge = null;

                switch (stage)
                {
                    case Stage.tower:
                        objectsToDisable    = progressablesObjects.Where(x => TowerAProgressable_Disable.Contains(x.name)).ToArray();
                        objectsToEnable     = progressablesObjects.Where(x => TowerAProgressable_Enable.Contains(x.name)).ToArray();
                    break;

                    case Stage.square:
                        if (!BingoConfig.enableTaxi)
                            objectsToDisable = progressablesObjects.Where(x => SquareAProgressable_Disable.Contains(x.name)).ToArray();

                        objectsToEnable = progressablesObjects.Where(x => SquareAProgressable_Enable.Contains(x.name)).ToArray();
                    break;

                    case Stage.osaka:
                        objectsToDisable    = progressablesObjects.Where(x => OsakaAProgressable_Disable.Contains(x.name) || (!BingoConfig.enableBoss && OsakaBoss_Disable.Contains(x.name))).ToArray();
                        objectsToEnable     = progressablesObjects.Where(x => OsakaAProgressable_Enable.Contains(x.name)).ToArray();
                    break;

                    case Stage.Mall:
                        objectsToDisable    = progressablesObjects.Where(x => MallAProgressable_Disable.Contains(x.name)).ToArray();
                    break;

                    case Stage.pyramid:
                        objectsToDisable    = progressablesObjects.Where(x => PyramidAProgressable_Disable.Contains(x.name)).ToArray();
                        objectsToEnable     = progressablesObjects.Where(x => PyramidAProgressable_Enable.Contains(x.name)).ToArray();
                    break;

                    case Stage.downhill:
                        SaveSlotData saveSlotData = Core.Instance?.SaveManager?.CurrentSaveSlot;

                        if (saveSlotData != null && !saveSlotData.GetCharacterProgress(Characters.jetpackBossPlayer).unlocked)
                        {
                            CombatEncounter_IreneUnlockChallenge = progressablesObjects.Find(x => x.name == nameof(CombatEncounter_IreneUnlockChallenge));

                            if (CombatEncounter_IreneUnlockChallenge != null && CombatEncounter_IreneUnlockChallenge.gameObject.GetComponent<Components.Component_IreneTrigger>() == null)
                                CombatEncounter_IreneUnlockChallenge.gameObject.AddComponent<Components.Component_IreneTrigger>();
                        }
                    break;
                }

                foreach (var objectToDisable in objectsToDisable)
                    objectToDisable.gameObject.SetActive(false);

                foreach (var objectToEnable in objectsToEnable)
                    objectToEnable.gameObject.SetActive(true);
            }
        }
    }
}
