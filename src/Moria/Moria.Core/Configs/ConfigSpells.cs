namespace Moria.Core.Configs
{
    public class ConfigSpells
    {
        // Class spell types
        public uint SPELL_TYPE_NONE { get; set; } = 0;
        public uint SPELL_TYPE_MAGE { get; set; } = 1;
        public uint SPELL_TYPE_PRIEST { get; set; } = 2;

        // offsets to spell names in spell_names[] array
        public uint NAME_OFFSET_SPELLS { get; set; } = 0;
        public uint NAME_OFFSET_PRAYERS { get; set; } = 31;
    }
}