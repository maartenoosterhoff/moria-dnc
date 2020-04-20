namespace Moria.Core.Structures.Enumerations
{
    public enum TrapTypes
    {
        OpenPit = 1,
        ArrowPit,
        CoveredPit,
        TrapDoor,
        SleepingGas,
        HiddenObject,
        DartOfStr,
        Teleport,
        Rockfall,
        CorrodingGas,
        SummonMonster,
        FireTrap,
        AcidTrap,
        PoisonGasTrap, // TODO: name would clash with MagicSpellFlags::PoisonGas
        BlindingGas,
        ConfuseGas,
        SlowDart,
        DartOfCon,
        SecretDoor,
        ScareMonster = 99,
        GeneralStore = 101,
        Armory,
        Weaponsmith,
        Temple,
        Alchemist,
        MagicShop,
    };
}
