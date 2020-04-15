namespace Moria.Core.Structures
{
    public class Player_flags_t
    {
        public uint status { get; set; }
        public int rest { get; set; }
        public int blind { get; set; }
        public int paralysis { get; set; }
        public int confused { get; set; }
        public int food { get; set; }
        public int food_digested { get; set; }
        public int protection { get; set; }
        public int speed { get; set; }
        public int fast { get; set; }
        public int slow { get; set; }
        public int afraid { get; set; }
        public int poisoned { get; set; }
        public int image { get; set; }
        public int protect_evil { get; set; }
        public int invulnerability { get; set; }
        public int heroism { get; set; }
        public int super_heroism { get; set; }
        public int blessed { get; set; }
        public int heat_resistance { get; set; }
        public int cold_resistance { get; set; }
        public int detect_invisible { get; set; }
        public int word_of_recall { get; set; }
        public int see_infra { get; set; }
        public int timed_infra { get; set; }
        public bool see_invisible { get; set; }
        public bool teleport { get; set; }
        public bool free_action { get; set; }
        public bool slow_digest { get; set; }
        public bool aggravate { get; set; }
        public bool resistant_to_fire { get; set; }
        public bool resistant_to_cold { get; set; }
        public bool resistant_to_acid { get; set; }
        public bool regenerate_hp { get; set; }
        public bool resistant_to_light { get; set; }
        public bool free_fall { get; set; }
        public bool sustain_str { get; set; }
        public bool sustain_int { get; set; }
        public bool sustain_wis { get; set; }
        public bool sustain_con { get; set; }
        public bool sustain_dex { get; set; }
        public bool sustain_chr { get; set; }
        public bool confuse_monster { get; set; }

        public uint new_spells_to_learn { get; set; }
        public uint spells_learnt { get; set; }
        public uint spells_worked { get; set; }
        public uint spells_forgotten { get; set; }
        public uint[] spells_learned_order { get; set; } = new uint[32];
    }
}