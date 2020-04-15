namespace Moria.Core.Configs
{
    public class ConfigTreasureFlags
    {
        public uint TR_STATS { get; set; } = 0x0000003F; // the stats must be the low 6 bits
        public uint TR_STR { get; set; }= 0x00000001;
        public uint TR_INT { get; set; }= 0x00000002;
        public uint TR_WIS { get; set; }= 0x00000004;
        public uint TR_DEX { get; set; }= 0x00000008;
        public uint TR_CON { get; set; }= 0x00000010;
        public uint TR_CHR { get; set; } = 0x00000020;
        public uint TR_SEARCH { get; set; } = 0x00000040;
        public uint TR_SLOW_DIGEST { get; set; } = 0x00000080;
        public uint TR_STEALTH { get; set; } = 0x00000100;
        public uint TR_AGGRAVATE { get; set; } = 0x00000200;
        public uint TR_TELEPORT { get; set; } = 0x00000400;
        public uint TR_REGEN { get; set; }= 0x00000800;
        public uint TR_SPEED { get; set; } = 0x00001000;
        
        public uint TR_EGO_WEAPON { get; set; } = 0x0007E000;
        public uint TR_SLAY_DRAGON { get; set; }= 0x00002000;
        public uint TR_SLAY_ANIMAL { get; set; } = 0x00004000;
        public uint TR_SLAY_EVIL { get; set; } = 0x00008000;
        public uint TR_SLAY_UNDEAD { get; set; }= 0x00010000;
        public uint TR_FROST_BRAND { get; set; } = 0x00020000;
        public uint TR_FLAME_TONGUE { get; set; } = 0x00040000;
        
        public uint TR_RES_FIRE { get; set; }= 0x00080000;
        public uint TR_RES_ACID { get; set; }= 0x00100000;
        public uint TR_RES_COLD { get; set; } = 0x00200000;
        public uint TR_SUST_STAT { get; set; } = 0x00400000;
        public uint TR_FREE_ACT { get; set; } = 0x00800000;
        public uint TR_SEE_INVIS { get; set; }= 0x01000000;
        public uint TR_RES_LIGHT { get; set; } = 0x02000000;
        public uint TR_FFALL { get; set; }= 0x04000000;
        public uint TR_BLIND { get; set; }= 0x08000000;
        public uint TR_TIMID { get; set; } = 0x10000000;
        public uint TR_TUNNEL { get; set; } = 0x20000000;
        public uint TR_INFRA { get; set; } = 0x40000000;
        public uint TR_CURSED { get; set; } = 0x80000000;
    }
}