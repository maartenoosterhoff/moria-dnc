namespace Moria.Core.Configs
{
    public class ConfigMonsters
    {
        public uint MON_CHANCE_OF_NEW { get; } = 160;            // 1/x chance of new monster each round
        public uint MON_MAX_SIGHT { get; } = 20;                 // Maximum dis a creature can be seen
        public uint MON_MAX_SPELL_CAST_DISTANCE { get; } = 20;   // Maximum dis creature spell can be cast
        public uint MON_MAX_MULTIPLY_PER_LEVEL { get; } = 75;    // Maximum reproductions on a level
        public uint MON_MULTIPLY_ADJUST { get; } = 7;            // High value slows multiplication
        public uint MON_CHANCE_OF_NASTY { get; } = 50;           // 1/x chance of high level creature
        public uint MON_MIN_PER_LEVEL { get; } = 14;             // Minimum number of monsters/level
        public uint MON_MIN_TOWNSFOLK_DAY { get; } = 4;          // Number of people on town level (day)
        public uint MON_MIN_TOWNSFOLK_NIGHT { get; } = 8;        // Number of people on town level (night)
        public uint MON_ENDGAME_MONSTERS { get; } = 2;           // Total number of "win" creatures
        public uint MON_ENDGAME_LEVEL { get; } = 50;             // Level where winning creatures begin
        public uint MON_SUMMONED_LEVEL_ADJUST { get; } = 2;      // Adjust level of summoned creatures
        public uint MON_PLAYER_EXP_DRAINED_PER_HIT { get; } = 2; // Percent of player exp drained per hit
        public uint MON_MIN_INDEX_ID { get; } = 2;               // Minimum index in m_list (1 = py, 0 = no mon)
        public uint SCARE_MONSTER { get; } = 99;
    }
}