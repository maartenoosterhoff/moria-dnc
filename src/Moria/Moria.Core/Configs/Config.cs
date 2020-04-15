namespace Moria.Core.Configs
{
    public static class Config
    {
        public static ConfigFiles files { get; set; } = new ConfigFiles();

        public static ConfigDungeon dungeon { get; set; } = new ConfigDungeon();

        public static ConfigDungeonObjects dungeon_objects { get; set; } = new ConfigDungeonObjects();

        public static ConfigMonsters monsters { get; set; } = new ConfigMonsters();

        public static ConfigMonstersDefense monsters_defense { get; set; } = new ConfigMonstersDefense();

        public static ConfigMonstersMove monsters_move { get; set; } = new ConfigMonstersMove();

        public static ConfigMonstersSpells monsters_spells { get; set; } = new ConfigMonstersSpells();

        public static ConfigTreasure treasure { get; set; } = new ConfigTreasure();

        public static ConfigTreasureChests treasure_chests { get; set; } = new ConfigTreasureChests();

        public static ConfigTreasureFlags treasure_flags { get; set; } = new ConfigTreasureFlags();

        public static ConfigStores stores { get; set; } = new ConfigStores();

        public static ConfigIdentification identification { get; set; } = new ConfigIdentification();

        public static ConfigOptions options { get; set; } = new ConfigOptions();

        public static ConfigPlayer player { get; set; } = new ConfigPlayer();

        public static ConfigPlayerStatus player_status { get; set; } = new ConfigPlayerStatus();

        public static ConfigSpells spells { get; set; } = new ConfigSpells();
    }
}
