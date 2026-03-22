namespace CriticalSpin.Model
{
    public enum RewardType
    {
        // Currency (0 - 49)
        Gold = 0,
        Cash = 1,

        // Chests (50 - 99)
        ChestBronze = 50,
        ChestSilver = 51,
        ChestStandard = 52,
        ChestGold = 53,
        ChestBig = 54,
        ChestSmall = 55,
        ChestSuper = 56,
        ChestBronzeNolight = 57,
        ChestSilverNolight = 58,
        ChestStandardNolight = 59,
        ChestGoldNolight = 60,
        ChestBigNolight = 61,
        ChestSmallNolight = 62,
        ChestSuperNolight = 63,
        ChestGoldStash = 64,

        // Weapons (100 - 149)
        Shotgun = 100,
        ShotgunT3 = 101,
        Rifle = 102,
        RifleT2 = 103,
        SMG = 104,
        SMGT3 = 105,
        Sniper = 106,
        SniperT3 = 107,
        MeleeT2 = 108,
        Knife = 109,
        Pistol = 110,
        PistolAlt = 111,
        Submachine = 112,
        SMGT1 = 113,
        ShotgunT1 = 114,
        ShotgunT2 = 115,
        RifleT1 = 116,
        SniperT1 = 117,


        // Armor (150 - 199)
        Armor = 150,
        Vest = 151,

        // Special (200 - 249)
        Bomb = 200
    }

    public enum SpinType
    {
        Bronze,
        Silver,
        Golden
    }

    public enum GameState
    {
        Idle,
        Spinning,
        ShowingResult,
        BombExploded,
        ReviveOffer,
        Cashout,
        GameOver
    }
}
