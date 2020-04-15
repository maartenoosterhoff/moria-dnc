namespace Moria.Core.Configs
{
    public class ConfigTreasure
    {
        public uint MIN_TREASURE_LIST_ID { get; set; } = 1;           // Minimum treasure_list index used
        public uint TREASURE_CHANCE_OF_GREAT_ITEM { get; set; } = 12; // 1/n Chance of item being a Great Item

        // Magic Treasure Generation constants
        public uint LEVEL_STD_OBJECT_ADJUST { get; set; } = 125; // Adjust STD per level * 100
        public uint LEVEL_MIN_OBJECT_STD { get; set; } = 7;      // Minimum STD
        public uint LEVEL_TOWN_OBJECTS { get; set; } = 7;        // Town object generation level
        public uint OBJECT_BASE_MAGIC { get; set; } = 15;        // Base amount of magic
        public uint OBJECT_MAX_BASE_MAGIC { get; set; } = 70;    // Max amount of magic
        public uint OBJECT_CHANCE_SPECIAL { get; set; } = 6;     // magic_chance/# special magic
        public uint OBJECT_CHANCE_CURSED { get; set; } = 13;     // 10*magic_chance/# cursed items

        // Constants describing limits of certain objects
        public uint OBJECT_LAMP_MAX_CAPACITY { get; set; } = 15000; // Maximum amount that lamp can be filled
        public uint OBJECT_BOLTS_MAX_RANGE { get; set; } = 18;       // Maximum range of bolts and balls
        public uint OBJECTS_RUNE_PROTECTION { get; set; } = 3000;   // Rune of protection resistance
    }          
}