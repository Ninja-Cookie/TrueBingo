using BepInEx;
using HarmonyLib;
using Reptile;

namespace TrueBingo
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid      = "ninjacookie.brc.truebingo";
        public const string pluginName      = "TrueBingo";
        public const string pluginVersion   = "0.1.4";

        public void Awake()
        {
            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();

            BingoConfig.InitConfigs();

            StageManager.OnStageInitialized += BingoHandleStage.UpdateStage;
            Core.OnUpdate += BingoHandleStage.UpdateObjective;
            Core.OnUpdate += BingoHandleStage.UpdateWanted;
        }
    }
}
