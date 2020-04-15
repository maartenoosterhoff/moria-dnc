namespace Moria.Core.Structures.Enumerations
{
    public enum StaffSpellTypes
    {
        StaffLight = 1, // TODO: name would clash with PlayerEquipment::Light
        DetectDoorsStairs,
        TrapLocation,
        TreasureLocation,
        ObjectLocation,
        Teleportation,
        Earthquakes,
        Summoning,
        // skipping 9
        Destruction = 10,
        Starlight,
        HasteMonsters,
        SlowMonsters,
        SleepMonsters,
        CureLightWounds,
        DetectInvisible,
        Speed,
        Slowness,
        MassPolymorph,
        RemoveCurse,
        DetectEvil,
        Curing,
        DispelEvil,
        // skipping 24
        Darkness = 25,
        // skipping lots...
        StoreBoughtFlag = 32,
    }
}
