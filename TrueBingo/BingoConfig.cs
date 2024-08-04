using BepInEx;
using BepInEx.Configuration;
using Reptile;
using System;
using System.IO;
using UnityEngine;
using static TrueBingo.BingoConfigTypes;

namespace TrueBingo
{
    public static class BingoConfig
    {
        private static ConfigFile config_char;
        private static ConfigFile config_world;

        private static  string  config_char_path_full;
        private static  string  config_world_path_full;

        private const   string  config_foldername           = "TrueBingo";
        private const   string  config_char_filename        = "Character";
        private const   string  config_world_filename       = "World";
        private const   string  config_filetype             = "cfg";

        private const   string  config_selection_char       = "Character Settings";
        private const   string  config_selection_world      = "World Settings";

        private static  ConfigEntry characterEntry;
        private const   string      characterEntry_char     = "Character";
        private const   string      characterEntry_style    = "Style";
        private const   string      characterEntry_outfit   = "Outfit";

        private static  ConfigEntry worldEntry;
        private const   string      worldEntry_stage        = "Starting Stage";
        private const   string      worldEntry_pos          = "Starting Position";
        private const   string      worldEntry_seed         = "Seed";
        private const   string      worldEntry_story        = "Disable Story";
        private const   string      worldEntry_bmx          = "Disable BMX Doors";
        private const   string      worldEntry_taxi         = "Enable Taxi Fight";
        private const   string      worldEntry_boss         = "Enable Final Boss Trigger";
        private const   string      worldEntry_cops         = "Disable Cops";

        private const BingoConfigTypes.Characters   characterEntry_char_default     = BingoConfigTypes.Characters.Red;
        private const BingoConfigTypes.Styles       characterEntry_style_default    = BingoConfigTypes.Styles.Skateboard;
        private const BingoConfigTypes.Outfits      characterEntry_outfit_default   = BingoConfigTypes.Outfits.Spring;
        private const BingoConfigTypes.Stages       worldEntry_stage_default        = BingoConfigTypes.Stages.Hideout;
        private const int                           worldEntry_seed_default         = 0;
        private static readonly Vector3             worldEntry_pos_default          = new Vector3(-5, 3, 43);
        private const bool                          worldEntry_disableStory_default = false;
        private const bool                          worldEntry_disableBMX_default   = false;
        private const bool                          worldEntry_enableTaxi_default   = false;
        private const bool                          worldEntry_enableBoss_default   = false;
        private const bool                          worldEntry_disableCops_default  = true;

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

        public static void InitConfigs()
        {
            config_char_path_full = GetFilePath(config_foldername, config_char_filename, config_filetype);
            config_char = new ConfigFile(config_char_path_full, true);

            config_world_path_full = GetFilePath(config_foldername, config_world_filename, config_filetype);
            config_world = new ConfigFile(config_world_path_full, true);

            FillConfigs();
        }

        private static string GetFilePath(string foldername, string filename, string filetype)
        {
            return Path.Combine(Paths.ConfigPath, $@"{foldername}\{filename}.{filetype}");
        }

        private static void FillConfigs()
        {
            characterEntry = new ConfigEntry(config_char, config_selection_char);
            BindConfig(characterEntry,  characterEntry_char,    characterEntry_char_default);
            BindConfig(characterEntry,  characterEntry_style,   characterEntry_style_default);
            BindConfig(characterEntry,  characterEntry_outfit,  characterEntry_outfit_default);

            worldEntry = new ConfigEntry(config_world, config_selection_world);
            BindConfig(worldEntry,      worldEntry_stage,       worldEntry_stage_default);
            BindConfig(worldEntry,      worldEntry_pos,         worldEntry_pos_default,         "If Zero'd, uses random Stage spawn, otherwise spawns player at this position.\nDefault is relative to Hideout.\nWarning: Random can place you in closed OldHead areas.");
            BindConfig(worldEntry,      worldEntry_seed,        worldEntry_seed_default,        "If Stage Random, and/or Spawn Random, uses this seed.\n0 = Fully Random Each Time");
            BindConfig(worldEntry,      worldEntry_story,       worldEntry_disableStory_default,"Disable Story Events such as Challenges.\nEnables Challenge Graffiti Pickups.");
            BindConfig(worldEntry,      worldEntry_bmx,         worldEntry_disableBMX_default);
            BindConfig(worldEntry,      worldEntry_taxi,        worldEntry_enableTaxi_default);
            BindConfig(worldEntry,      worldEntry_boss,        worldEntry_enableBoss_default,  "Enables the Trigger which lets you enter the Final Boss area, located at the start of the final Mataan area after the vent.\nOnly has an effect if Story is also enabled.");
            BindConfig(worldEntry,      worldEntry_cops,        worldEntry_disableCops_default);
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
            ReloadConfigs();

            // Char Entry
            if (characterEntry.TryGetEntry(characterEntry_char,     out BingoConfigTypes.Characters entry_char))    { character = GetCharacter(entry_char); }
            if (characterEntry.TryGetEntry(characterEntry_style,    out Styles entry_style))                        { moveStyle = GetStyle(entry_style); }
            if (characterEntry.TryGetEntry(characterEntry_outfit,   out Outfits entry_outfit))                      { outfit = GetOutfit(entry_outfit); }

            // World Entry
            if (worldEntry.TryGetEntry(worldEntry_seed,             out int entry_seed))                            { seed = entry_seed; }
            if (worldEntry.TryGetEntry(worldEntry_stage,            out Stages entry_stage))
            {
                Stages stageToGet = entry_stage;
                System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);

                stage = GetStage(stageToGet == Stages.Random ? (Stages)random.Next((int)Stages.Random) : stageToGet);
            }
            if (worldEntry.TryGetEntry(worldEntry_pos,      out Vector3 entry_pos))     { position      = entry_pos; }
            if (worldEntry.TryGetEntry(worldEntry_story,    out bool entry_story))      { disableStory  = entry_story; }
            if (worldEntry.TryGetEntry(worldEntry_bmx,      out bool entry_bmx))        { disableBMX    = entry_bmx; }
            if (worldEntry.TryGetEntry(worldEntry_taxi,     out bool entry_taxi))       { enableTaxi    = entry_taxi; }
            if (worldEntry.TryGetEntry(worldEntry_boss,     out bool entry_boss))       { enableBoss    = entry_boss; }
            if (worldEntry.TryGetEntry(worldEntry_cops,     out bool entry_cops))       { disableCops   = entry_cops; }
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

        private static void ReloadConfigs()
        {
            config_char .Reload();
            config_world.Reload();
        }
    }
}
