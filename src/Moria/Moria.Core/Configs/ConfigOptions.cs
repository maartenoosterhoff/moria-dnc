namespace Moria.Core.Configs
{
    public class ConfigOptions
    {
        public bool display_counts { get; set; } = true;          // Display rest/repeat counts
        public bool find_bound { get; set; } = false;             // Print yourself on a run (slower)
        public bool run_cut_corners { get; set; } = true;         // Cut corners while running
        public bool run_examine_corners { get; set; } = true;     // Check corners while running
        public bool run_ignore_doors { get; set; } = false;       // Run through open doors
        public bool run_print_self { get; set; } = false;         // Stop running when the map shifts
        public bool highlight_seams { get; set; } = false;        // Highlight magma and quartz veins
        public bool prompt_to_pickup { get; set; } = false;       // Prompt to pick something up
        public bool use_roguelike_keys { get; set; } = false;     // Use classic Roguelike keys
        public bool show_inventory_weights { get; set; } = false; // Display weights in inventory
        public bool error_beep_sound { get; set; } = true;        // Beep for invalid characters
    }
}