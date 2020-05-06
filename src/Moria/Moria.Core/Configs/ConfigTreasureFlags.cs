namespace Moria.Core.Configs
{
    public class ConfigTreasureFlags
    {
        public uint TR_STATS { get; } = 0x0000003F; // the stats must be the low 6 bits
        public uint TR_STR { get; }= 0x00000001;
        public uint TR_INT { get; }= 0x00000002;
        public uint TR_WIS { get; }= 0x00000004;
        public uint TR_DEX { get; }= 0x00000008;
        public uint TR_CON { get; }= 0x00000010;
        public uint TR_CHR { get; } = 0x00000020;
        public uint TR_SEARCH { get; } = 0x00000040;
        public uint TR_SLOW_DIGEST { get; } = 0x00000080;
        public uint TR_STEALTH { get; } = 0x00000100;
        public uint TR_AGGRAVATE { get; } = 0x00000200;
        public uint TR_TELEPORT { get; } = 0x00000400;
        public uint TR_REGEN { get; }= 0x00000800;
        public uint TR_SPEED { get; } = 0x00001000;
        
        public uint TR_EGO_WEAPON { get; } = 0x0007E000;
        public uint TR_SLAY_DRAGON { get; }= 0x00002000;
        public uint TR_SLAY_ANIMAL { get; } = 0x00004000;
        public uint TR_SLAY_EVIL { get; } = 0x00008000;
        public uint TR_SLAY_UNDEAD { get; }= 0x00010000;
        public uint TR_FROST_BRAND { get; } = 0x00020000;
        public uint TR_FLAME_TONGUE { get; } = 0x00040000;
        
        public uint TR_RES_FIRE { get; }= 0x00080000;
        public uint TR_RES_ACID { get; }= 0x00100000;
        public uint TR_RES_COLD { get; } = 0x00200000;
        public uint TR_SUST_STAT { get; } = 0x00400000;
        public uint TR_FREE_ACT { get; } = 0x00800000;
        public uint TR_SEE_INVIS { get; }= 0x01000000;
        public uint TR_RES_LIGHT { get; } = 0x02000000;
        public uint TR_FFALL { get; }= 0x04000000;
        public uint TR_BLIND { get; }= 0x08000000;
        public uint TR_TIMID { get; } = 0x10000000;
        public uint TR_TUNNEL { get; } = 0x20000000;
        public uint TR_INFRA { get; } = 0x40000000;
        public uint TR_CURSED { get; } = 0x80000000;
    }
}