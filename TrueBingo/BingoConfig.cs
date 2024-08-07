using BepInEx;
using BepInEx.Configuration;
using Reptile;
using System.IO;
using UnityEngine;
using static TrueBingo.BingoConfigTypes;

namespace TrueBingo
{
    public static class BingoConfig
    {
        private static ConfigFile config_char;
        private static ConfigFile config_world;

        private static string config_char_path_full;
        private static string config_world_path_full;

        // Files
        // -----------------------------------
        private const   string              config_foldername       = "TrueBingo";
        private const   string              config_char_filename    = "Character";
        private const   string              config_world_filename   = "World";
        private const   string              config_filetype         = "cfg";
        // -----------------------------------

        // Selections
        // -----------------------------------
        private const   string              config_selection_char   = "Character Settings";
        private const   string              config_selection_world  = "World Settings";
        // -----------------------------------

        // Character Entry
        // -----------------------------------
        private static  ConfigEntry         characterEntry;
        private const   string              characterEntry_char     = "Character";
        private const   string              characterEntry_style    = "Style";
        private const   string              characterEntry_outfit   = "Outfit";
        // -----------------------------------

        // World Entry
        // -----------------------------------
        private static  ConfigEntry         worldEntry;
        private const   string              worldEntry_stage        = "Starting Stage";
        private const   string              worldEntry_pos          = "Starting Position";
        private const   string              worldEntry_seed         = "Seed";
        private const   string              worldEntry_story        = "Disable Story";
        private const   string              worldEntry_bmx          = "Disable BMX Doors";
        private const   string              worldEntry_taxi         = "Enable Taxi Fight";
        private const   string              worldEntry_boss         = "Enable Final Boss Trigger";
        private const   string              worldEntry_cops         = "Disable Cops";
        private const   string              worldEntry_roboskip     = "Open Teleport Robo-Posts";
        private const   string              worldEntry_cutscene     = "Allow Skipping All Cutscenes";
        private const   string              worldEntry_fastcutscene = "Fast Cutscene Skip";
        // -----------------------------------

        public static Reptile.Characters    character;
        public static MoveStyle             moveStyle;
        public static int                   outfit;
        public static Stage                 stage;
        public static int                   seed;
        public static Vector3               position;
        public static bool                  disableStory;
        public static bool                  disableBMX;
        public static bool                  enableTaxi;
        public static bool                  enableBoss;
        public static bool                  disableCops;
        public static bool                  roboSkip;
        public static bool                  cutsceneSkip;
        public static bool                  fastCutscene;

        public static void InitConfigs()
        {
            CreateConfig();
            FillConfigs();
        }

        private static void CreateConfig()
        {
            config_char_path_full = GetFilePath(config_foldername, config_char_filename, config_filetype);
            config_char = new ConfigFile(config_char_path_full, true);

            config_world_path_full = GetFilePath(config_foldername, config_world_filename, config_filetype);
            config_world = new ConfigFile(config_world_path_full, true);
        }

        private static string GetFilePath(string foldername, string filename, string filetype)
        {
            return Path.Combine(Paths.ConfigPath, $@"{foldername}\{filename}.{filetype}");
        }

        private static void FillConfigs()
        {
            characterEntry = new ConfigEntry(config_char, config_selection_char);
            BindConfig(characterEntry,  characterEntry_char,    BingoConfigTypes.Characters.Red);
            BindConfig(characterEntry,  characterEntry_style,   BingoConfigTypes.Styles.Skateboard);
            BindConfig(characterEntry,  characterEntry_outfit,  BingoConfigTypes.Outfits.Spring);

            worldEntry = new ConfigEntry(config_world, config_selection_world);
            BindConfig(worldEntry,      worldEntry_stage,       BingoConfigTypes.Stages.Hideout);
            BindConfig(worldEntry,      worldEntry_pos,         new Vector3(-5, 3, 43),
                "If Zero'd, uses random Stage spawn, otherwise spawns player at this position.\n" +
                "Default is relative to Hideout.\n" +
                "Warning: Random can place you in closed OldHead areas."
            );
            BindConfig(worldEntry,      worldEntry_seed,        0,
                "If Stage Random, and/or Spawn Random, uses this seed.\n" +
                "0 = Fully Random Each Time"
            );
            BindConfig(worldEntry,      worldEntry_story,       false,
                "Disable Story Events such as Challenges.\n" +
                "Enables Challenge Graffiti Pickups."
            );
            BindConfig(worldEntry,      worldEntry_bmx,         false);
            BindConfig(worldEntry,      worldEntry_taxi,        true);
            BindConfig(worldEntry,      worldEntry_boss,        false,
                "Enables the Trigger which lets you enter the Final Boss area, located at the start of the final Mataan area after the vent.\n" +
                "Only has an effect if Story is also enabled."
            );
            BindConfig(worldEntry,      worldEntry_cops,        true);
            BindConfig(worldEntry,      worldEntry_roboskip,    true);
            BindConfig(worldEntry,      worldEntry_cutscene,    true);
            BindConfig(worldEntry,      worldEntry_fastcutscene,true);
        }

        private struct ConfigEntry
        {
            public ConfigFile   configFile;
            public string       selection;

            public ConfigEntry(ConfigFile configFile, string selection)
            {
                this.configFile = configFile;
                this.selection  = selection;
            }
        }

        private static void BindConfig<T>(ConfigEntry configEntry, string key, T defaultValue, string description = "")
        {
            configEntry.configFile.Bind<T>(configEntry.selection, key, defaultValue, description);
        }

        public static void HandleConfig()
        {
            InitConfigs();
            ReloadConfigs();
        }

        private static void ReloadConfigs()
        {
            config_char .Reload();
            config_world.Reload();

            UpdateConfigAll();
        }

        private static void UpdateConfigAll()
        {
            // Char
            if (characterEntry.TryGetEntry(characterEntry_char,     out BingoConfigTypes.Characters entry_char))    { character = GetCharacter(entry_char); }
            if (characterEntry.TryGetEntry(characterEntry_style,    out Styles                      entry_style))   { moveStyle = GetStyle(entry_style); }
            if (characterEntry.TryGetEntry(characterEntry_outfit,   out Outfits                     entry_outfit))  { outfit    = GetOutfit(entry_outfit); }

            // World
            characterEntry.UpdateConfig(worldEntry_seed, ref seed);

            if (worldEntry.TryGetEntry(worldEntry_stage, out Stages entry_stage))
            {
                Stages stageToGet = entry_stage;
                System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);

                stage = GetStage(stageToGet == Stages.Random ? (Stages)random.Next((int)Stages.Random) : stageToGet);
            }

            worldEntry.UpdateConfig(worldEntry_pos,     ref position);
            worldEntry.UpdateConfig(worldEntry_story,   ref disableStory);
            worldEntry.UpdateConfig(worldEntry_bmx,     ref disableBMX);
            worldEntry.UpdateConfig(worldEntry_taxi,    ref enableTaxi);
            worldEntry.UpdateConfig(worldEntry_boss,    ref enableBoss);
            worldEntry.UpdateConfig(worldEntry_cops,    ref disableCops);
            worldEntry.UpdateConfig(worldEntry_roboskip,ref roboSkip);
            worldEntry.UpdateConfig(worldEntry_cutscene,ref cutsceneSkip);
            worldEntry.UpdateConfig(worldEntry_fastcutscene,ref fastCutscene);
        }

        private static void UpdateConfig<T>(this ConfigEntry configEntry, string entry, ref T option)
        {
            if (configEntry.TryGetEntry(entry, out T entry_value))
                option = entry_value;
        }

        private static bool TryGetEntry<T>(this ConfigEntry configEntry, string key, out T entryValue)
        {
            if (configEntry.configFile.TryGetEntry(configEntry.selection, key, out ConfigEntry<T> entry))
            {
                entryValue = entry.Value;
                return true;
            }
            entryValue = default;
            return false;
        }
    }
}
