namespace TrueBingo
{
    public static class BingoStageItems
    {
        // Progress Objects
        // ----------------------------------------------------------------------------------
        public static readonly string[] HideoutProgressObjects = new string[5]
        {
            "GarageDoorBMXClosed",
            "GarageDoorInlineClosed",
            "SquareWall",
            "GateOsaka",
            "GarageDoorSBClosed"
        };

        public static readonly string[] DownhillProgressObjects = new string[9]
        {
            "ProgressObject_basketWallToSquare",
            "ProgressObject_ForceTalkToBel",
            "ProgressObject_basketWallWithCars",
            "ProgressObject_basketWall2Gate",
            "ProgressObject_basketWall2",
            "FranksIntroTrigger",
            "BarricadeExit",
            "FranksBarricadeChunks1",
            "BarricadeChunks1"
        };

        public static readonly string[] SquareProgressObjects = new string[4]
        {
            "ProgressObject_SolaceTrigger",
            "Wall",
            "ProgressObject_SquareIntro_1",
            "ProgressObject_SquareIntro_2"
        };

        public static readonly string[] GlobalProgressObjects = new string[6]
        {
            "AllGraffitiProgressObject",
            "collectables_Hideout",
            "ProgressObject_MapPickup",
            "ProgressObject_MapUnlockPickup",
            "ProgressObject_Map_Pickup",
            "ProgressObject_MapUnlock"
        };
        // ----------------------------------------------------------------------------------

        // AProgressable Objects
        // ----------------------------------------------------------------------------------
        public static readonly string[] TowerAProgressable_Disable = new string[2]
        {
            "ProgressObject_WallToTower",
            "ProgressObject_TowerDoor"
        };

        public static readonly string[] TowerAProgressable_Enable = new string[1]
        {
            "ProgressObject_FrankGraffiti"
        };

        public static readonly string[] SquareAProgressable_Disable = new string[3]
        {
            "SaveTaxiDriverCops",
            "NPC_TaxiDriver",
            "SaveTaxiDriverCombatEncounter"
        };

        public static readonly string[] SquareAProgressable_Enable = new string[1]
        {
            "FrankGraffiti"
        };

        public static readonly string[] OsakaAProgressable_Disable = new string[5]
        {
            "ProgressObject_StartTrigger",
            "LionStatueModelClosed",
            "IntroSnakeBoss",
            "ProgressObject_Gate",
            "ProgressObjectGateArena"
        };

        public readonly static string[] OsakaAProgressable_Enable = new string[1]
        {
            "LionStatueModelOpen"
        };

        public readonly static string[] MallAProgressable_Disable = new string[3]
        {
            "ProgressObject_HallwayDoor",
            "ProgressObject_FirstProgressDoor",
            "ProgressObject_SecondProgressDoor"
        };

        public readonly static string[] PyramidAProgressable_Disable = new string[3]
        {
            "ProgressObject_DoorOutOfLab",
            "REPdoor",
            "ProgressObject_DoorToHigherAreas"
        };

        public readonly static string[] PyramidAProgressable_Enable = new string[1]
        {
            "ProgressObject_FerriesToIsland"
        };

        public readonly static string[] PostGameChallenges = new string[6]
        {
            "NPC_Eclipe_UnlockChallenge",
            "NPC_DJ_UnlockChallenge",
            "NPC_Futurism_UnlockChallenge",
            "NPC_DOTEXE_UnlockChallenge",
            "NPC_DevilTheory_UnlockChallenge",
            "NPC_Frank_UnlockChallenge"
        };
        // ----------------------------------------------------------------------------------
    }
}
