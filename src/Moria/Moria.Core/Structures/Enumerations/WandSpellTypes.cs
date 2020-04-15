namespace Moria.Core.Structures.Enumerations
{
    public enum WandSpellTypes
    {
        WandLight = 1, // TODO: name would clash with PlayerEquipment::Light
        LightningBolt,
        FrostBolt,
        FireBolt,
        StoneToMud,
        Polymorph,
        HealMonster,
        HasteMonster,
        SlowMonster,
        ConfuseMonster,
        SleepMonster,
        DrainLife,
        TrapDoorDestruction,
        WandMagicMissile, // TODO: name would clash with MagicSpellFlags::MagicMissile
        WallBuilding,
        CloneMonster,
        TeleportAway,
        Disarming,
        LightningBall,
        ColdBall,
        FireBall,
        StinkingCloud,
        AcidBall,
        Wonder
    }
}
