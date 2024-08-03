using Reptile;
using System.Collections.Generic;
using UnityEngine;

namespace TrueBingo.Components
{
    internal class Component_IreneTrigger : MonoBehaviour
    {
        private ProgressObject  BackToPrinceTrigger;
        private ProgressObject  ProgressObject_Bel;
        private ProgressObject  ProgressObject_AmbushEnemies;

        private WorldHandler    worldHandler;
        private Player          player;
        private SaveSlotData    saveSlotData;

        Encounter encounter;

        private const string encounterName = "CombatEncounter_IreneUnlockChallenge";

        private bool ShouldBeActive => !BackToPrinceTrigger.isActive && !ProgressObject_Bel.isActive && encounter == null && !player.IsBusyWithSequence();

        public void Awake()
        {
            worldHandler    = WorldHandler.instance;
            player          = worldHandler?.GetCurrentPlayer();
            saveSlotData    = Core.Instance?.SaveManager?.CurrentSaveSlot;

            List<ProgressObject> progressObjects = WorldHandler.instance?.SceneObjectsRegister?.progressObjects;

            if (progressObjects != null)
            {
                BackToPrinceTrigger             = progressObjects.Find(x => x.name == nameof(BackToPrinceTrigger));
                ProgressObject_Bel              = progressObjects.Find(x => x.name == nameof(ProgressObject_Bel));
                ProgressObject_AmbushEnemies    = progressObjects.Find(x => x.name == nameof(ProgressObject_AmbushEnemies));
            }

            Core.OnAlwaysUpdate += CheckShouldBeActive;
        }

        public void CheckShouldBeActive()
        {
            if (saveSlotData != null)
            {
                bool charUnlocked = saveSlotData.GetCharacterProgress(Characters.jetpackBossPlayer).unlocked;

                if (!charUnlocked && worldHandler != null && player != null && BackToPrinceTrigger != null && ProgressObject_Bel != null)
                {
                    encounter = worldHandler?.GetValue<Encounter>("currentEncounter");

                    gameObject.SetActive((encounter?.name == encounterName) || ShouldBeActive);
                    ProgressObject_AmbushEnemies.gameObject.SetActive(ShouldBeActive);
                }
                else if (charUnlocked)
                {
                    Core.OnAlwaysUpdate -= CheckShouldBeActive;
                    ProgressObject_AmbushEnemies.gameObject.SetActive(false);
                }
            }
        }

        public void OnDestroy()
        {
            Core.OnAlwaysUpdate -= CheckShouldBeActive;
        }
    }
}
