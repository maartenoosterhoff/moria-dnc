namespace Moria.Core.Structures.Enumerations
{
    public enum FoodMagicTypes
    {
        Poison = 1,
        Blindness,
        Paranoia,
        Confusion,
        Hallucination,
        CurePoison,
        CureBlindness,
        CureParanoia,
        CureConfusion,
        Weakness,
        Unhealth,
        // 12-15 are no longer used
        RestoreSTR = 16,
        RestoreCON,
        RestoreINT,
        RestoreWIS,
        RestoreDEX,
        RestoreCHR,
        FirstAid,
        MinorCures,
        LightCures,
        // 25 no longer used
        MajorCures = 26,
        PoisonousFood,
    };
}
