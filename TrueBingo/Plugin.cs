﻿using BepInEx;
using HarmonyLib;
using Reptile;
using TrueBingo.BingoSyncManager;
using UnityEngine;

namespace TrueBingo
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid      = "ninjacookie.brc.truebingo";
        public const string pluginName      = "TrueBingo";
        public const string pluginVersion   = "1.2.4";

        public static GameObject BingoSyncGUI;

        public void Awake()
        {
            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();

            BingoConfig.InitConfigs();

            StageManager.OnStageInitialized += BingoHandleStage.UpdateStage;
            Core.OnUpdate += BingoHandleStage.UpdateObjective;
            Core.OnUpdate += BingoHandleStage.UpdateWanted;
            Core.OnAlwaysUpdate += TrueBingoSync.Update;

            BingoSyncGUI = new GameObject("BingoSyncGUI", typeof(BingoSyncGUI));
            DontDestroyOnLoad(BingoSyncGUI);
        }
    }
}
