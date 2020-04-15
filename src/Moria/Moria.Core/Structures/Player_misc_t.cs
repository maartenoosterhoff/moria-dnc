namespace Moria.Core.Structures
{
    public class Player_misc_t
    {
        public string name { get; set; }
        public bool gender { get; set; }
        public int date_of_birth { get; set; }
        public int au { get; set; }
        public int max_exp { get; set; }
        public int exp { get; set; }
        public uint exp_fraction { get; set; }
        public uint age { get; set; }
        public uint height { get; set; }
        public uint weight { get; set; }
        public uint level { get; set; }
        public uint max_dungeon_depth { get; set; }
        public int chance_in_search { get; set; }
        public int fos { get; set; }
        public int bth { get; set; }
        public int bth_with_bows { get; set; }
        public int mana { get; set; }
        public int max_hp { get; set; }
        public int plusses_to_hit { get; set; }
        public int plusses_to_damage { get; set; }
        public int ac { get; set; }
        public int magical_ac { get; set; }
        public int display_to_hit { get; set; }
        public int display_to_damage { get; set; }
        public int display_ac { get; set; }
        public int display_to_ac { get; set; }
        public int disarm { get; set; }
        public int saving_throw { get; set; }
        public int social_class { get; set; }
        public int stealth_factor { get; set; }
        public uint class_id { get; set; }
        public uint race_id { get; set; }
        public uint hit_die { get; set; }
        public uint experience_factor { get; set; }
        public int current_mana { get; set; }
        public uint current_mana_fraction { get; set; }
        public int current_hp { get; set; }
        public uint current_hp_fraction { get; set; }
        public string[] history { get; set; } = new string[4];
    }
}