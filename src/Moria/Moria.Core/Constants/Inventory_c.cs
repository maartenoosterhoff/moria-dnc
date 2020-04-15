namespace Moria.Core.Constants
{
    public static class Inventory_c
    {
        // Size of inventory array (DO NOT CHANGE)
        public const uint PLAYER_INVENTORY_SIZE = 34;

        // Inventory stacking `sub_category_id`s - these never stack
        public const uint ITEM_NEVER_STACK_MIN = 0;
        public const uint ITEM_NEVER_STACK_MAX = 63;
        // these items always stack with others of same `sub_category_id`s, always treated as
        // single objects, must be power of 2;
        public const uint ITEM_SINGLE_STACK_MIN = 64;
        public const uint ITEM_SINGLE_STACK_MAX = 192; // see NOTE below
                                                      // these items stack with others only if have same `sub_category_id`s and same `misc_use`,
                                                      // they are treated as a group for wielding, etc.
        public const uint ITEM_GROUP_MIN = 192;
        public const uint ITEM_GROUP_MAX = 255;
        // NOTE: items with `sub_category_id`s = 192 are treated as single objects,
        // but only stack with others of same `sub_category_id`s if have the same
        // `misc_use` value, only used for torches.

        // Size of an inscription in the Inventory_t. Notice alignment, must be 4*x + 1
        public const uint INSCRIP_SIZE = 13;
    }
}
