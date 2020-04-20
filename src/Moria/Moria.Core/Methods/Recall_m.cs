using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures;
using Moria.Core.Structures.Enumerations;
using System;
using static Moria.Core.Constants.Dungeon_c;
using static Moria.Core.Constants.Dungeon_tile_c;
using static Moria.Core.Constants.Inventory_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Constants.Player_c;
using static Moria.Core.Constants.Store_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Treasure_c;
using static Moria.Core.Methods.Dice_m;
using static Moria.Core.Methods.Dungeon_los_m;
using static Moria.Core.Methods.Dungeon_m;
using static Moria.Core.Methods.Game_m;
using static Moria.Core.Methods.Game_objects_m;
using static Moria.Core.Methods.Helpers_m;
using static Moria.Core.Methods.Identification_m;
using static Moria.Core.Methods.Inventory_m;
using static Moria.Core.Methods.Mage_spells_m;
using static Moria.Core.Methods.Player_magic_m;
using static Moria.Core.Methods.Spells_m;
using static Moria.Core.Methods.Monster_m;
using static Moria.Core.Methods.Store_inventory_m;
using static Moria.Core.Methods.Std_m;
using static Moria.Core.Methods.Player_run_m;
using static Moria.Core.Methods.Player_eat_m;
using static Moria.Core.Methods.Player_traps_m;
using static Moria.Core.Methods.Player_m;
using static Moria.Core.Methods.Ui_io_m;
using static Moria.Core.Methods.Ui_m;
using static Moria.Core.Methods.Player_stats_m;

namespace Moria.Core.Methods
{
    public static class Recall_m
    {
        public static T Plural<T>(int count, T ss, T sp)
        {
            return count == 1 ? ss : sp;
        }
        //#define plural(c, ss, sp) ((c) == 1 ? (s) : (sp))

        // Number of kills needed for information.
        // the higher the level of the monster, the fewer the attacks you need,
        // the more damage an attack does, the more attacks you need.
        //#define knowdamage(l, a, d) ((4 + (l)) * (a) > 80 * (d))

        public static bool KnowDamage(int l, int a, int d) => ((4 + (l)) * (a) > 80 * (d));

        // Print out strings, filling up lines as we go.
        public static void memoryPrint(string p)
        {
            while (*p != 0)
            {
                *roff_buffer_pointer = *p;

                if (*p == '\n' || roff_buffer_pointer >= roff_buffer + sizeof(roff_buffer) - 1)
                {
                    char* q = roff_buffer_pointer;
                    if (*p != '\n')
                    {
                        while (*q != ' ')
                        {
                            q--;
                        }
                    }
                    *q = 0;
                    putStringClearToEOL(roff_buffer, Coord_t{ roff_print_line, 0});
                    roff_print_line++;

                    char* r = roff_buffer;

                    while (q < roff_buffer_pointer)
                    {
                        q++;
                        *r = *q;
                        r++;
                    }
                    roff_buffer_pointer = r;
                }
                else
                {
                    roff_buffer_pointer++;
                }
                p++;
            }
        }

        // Do we know anything about this monster?
        public static bool memoryMonsterKnown(Recall_t memory)
        {
            var game = State.Instance.game;
            if (game.wizard_mode)
            {
                return true;
            }

            if ((memory.movement != 0u) || (memory.defenses != 0u) || (memory.kills != 0u) || (memory.spells != 0u) || (memory.deaths != 0u))
            {
                return true;
            }

            foreach (var attack in memory.attacks)
            {
                if (attack != 0u)
                {
                    return true;
                }
            }

            return false;
        }

        public static void memoryWizardModeInit(Recall_t memory, Creature_t creature)
        {
            memory.kills = (uint16_t)SHRT_MAX;
            memory.wake = memory.ignore = UCHAR_MAX;

            uint32_t move = (uint32_t)((creature.movement & config::monsters::move::CM_4D2_OBJ) != 0) * 8;
            move += (uint32_t)((creature.movement & config::monsters::move::CM_2D2_OBJ) != 0) * 4;
            move += (uint32_t)((creature.movement & config::monsters::move::CM_1D2_OBJ) != 0) * 2;
            move += (uint32_t)((creature.movement & config::monsters::move::CM_90_RANDOM) != 0);
            move += (uint32_t)((creature.movement & config::monsters::move::CM_60_RANDOM) != 0);

            memory.movement = (uint32_t)((creature.movement & ~config::monsters::move::CM_TREASURE) | (move << config::monsters::move::CM_TR_SHIFT));
            memory.defenses = creature.defenses;

            if ((creature.spells & config::monsters::spells::CS_FREQ) != 0u)
            {
                memory.spells = (uint32_t)(creature.spells | config::monsters::spells::CS_FREQ);
            }
            else
            {
                memory.spells = creature.spells;
            }

            for (int i = 0; i < MON_MAX_ATTACKS; i++)
            {
                if (creature.damage[i] == 0)
                    break;
                memory.attacks[i] = UCHAR_MAX;
            }

            // A little hack to enable the display of info for Quylthulgs.
            if ((memory.movement & config::monsters::move::CM_ONLY_MAGIC) != 0u)
            {
                memory.attacks[0] = UCHAR_MAX;
            }
        }

        // Conflict history.
        static void memoryConflictHistory(uint16_t deaths, uint16_t kills)
        {
            vtype_t desc = { '\0' };

            if (deaths != 0u)
            {
                (void)sprintf(desc, "%d of the contributors to your monster memory %s", deaths, plural(deaths, "has", "have"));
                memoryPrint(desc);
                memoryPrint(" been killed by this creature, and ");
                if (kills == 0)
                {
                    memoryPrint("it is not ever known to have been defeated.");
                }
                else
                {
                    (void)sprintf(desc, "at least %d of the beasts %s been exterminated.", kills, plural(kills, "has", "have"));
                    memoryPrint(desc);
                }
            }
            else if (kills != 0u)
            {
                (void)sprintf(desc, "At least %d of these creatures %s", kills, plural(kills, "has", "have"));
                memoryPrint(desc);
                memoryPrint(" been killed by contributors to your monster memory.");
            }
            else
            {
                memoryPrint("No known battles to the death are recalled.");
            }
        }

        // Immediately obvious.
        static bool memoryDepthFoundAt(uint8_t level, uint16_t kills)
        {
            bool known = false;

            if (level == 0)
            {
                known = true;
                memoryPrint(" It lives in the town");
            }
            else if (kills != 0u)
            {
                known = true;

                // The Balrog is a level 100 monster, but appears at 50 feet.
                if (level > config::monsters::MON_ENDGAME_LEVEL)
                {
                    level = config::monsters::MON_ENDGAME_LEVEL;
                }

                vtype_t desc = { '\0' };
                (void)sprintf(desc, " It is normally found at depths of %d feet", level * 50);
                memoryPrint(desc);
            }

            return known;
        }

        static bool memoryMovement(uint32_t rc_move, int monster_speed, bool is_known)
        {
            // the creatures_list speed value is 10 greater, so that it can be a uint8_t
            monster_speed -= 10;

            if ((rc_move & config::monsters::move::CM_ALL_MV_FLAGS) != 0u)
            {
                if (is_known)
                {
                    memoryPrint(", and");
                }
                else
                {
                    memoryPrint(" It");
                    is_known = true;
                }

                memoryPrint(" moves");

                if ((rc_move & config::monsters::move::CM_RANDOM_MOVE) != 0u)
                {
                    memoryPrint(recall_description_how_much[(rc_move & config::monsters::move::CM_RANDOM_MOVE) >> 3]);
                    memoryPrint(" erratically");
                }

                if (monster_speed == 1)
                {
                    memoryPrint(" at normal speed");
                }
                else
                {
                    if ((rc_move & config::monsters::move::CM_RANDOM_MOVE) != 0u)
                    {
                        memoryPrint(", and");
                    }

                    if (monster_speed <= 0)
                    {
                        if (monster_speed == -1)
                        {
                            memoryPrint(" very");
                        }
                        else if (monster_speed < -1)
                        {
                            memoryPrint(" incredibly");
                        }
                        memoryPrint(" slowly");
                    }
                    else
                    {
                        if (monster_speed == 3)
                        {
                            memoryPrint(" very");
                        }
                        else if (monster_speed > 3)
                        {
                            memoryPrint(" unbelievably");
                        }
                        memoryPrint(" quickly");
                    }
                }
            }

            if ((rc_move & config::monsters::move::CM_ATTACK_ONLY) != 0u)
            {
                if (is_known)
                {
                    memoryPrint(", but");
                }
                else
                {
                    memoryPrint(" It");
                    is_known = true;
                }

                memoryPrint(" does not deign to chase intruders");
            }

            if ((rc_move & config::monsters::move::CM_ONLY_MAGIC) != 0u)
            {
                if (is_known)
                {
                    memoryPrint(", but");
                }
                else
                {
                    memoryPrint(" It");
                    is_known = true;
                }

                memoryPrint(" always moves and attacks by using magic");
            }

            return is_known;
        }

        // Kill it once to know experience, and quality (evil, undead, monstrous).
        // The quality of being a dragon is obvious.
        static void memoryKillPoints(uint16_t creature_defense, uint16_t monster_exp, uint8_t level)
        {
            memoryPrint(" A kill of this");

            if ((creature_defense & config::monsters::defense::CD_ANIMAL) != 0)
            {
                memoryPrint(" natural");
            }
            if ((creature_defense & config::monsters::defense::CD_EVIL) != 0)
            {
                memoryPrint(" evil");
            }
            if ((creature_defense & config::monsters::defense::CD_UNDEAD) != 0)
            {
                memoryPrint(" undead");
            }

            // calculate the integer exp part, can be larger than 64K when first
            // level character looks at Balrog info, so must store in long
            int32_t quotient = (int32_t)monster_exp * level / py.misc.level;

            // calculate the fractional exp part scaled by 100,
            // must use long arithmetic to avoid overflow
            int remainder = (uint32_t)((((int32_t)monster_exp * level % py.misc.level) * (int32_t)1000 / py.misc.level + 5) / 10);

            char plural;
            if (quotient == 1 && remainder == 0)
            {
                plural = '\0';
            }
            else
            {
                plural = 's';
            }

            vtype_t desc = { '\0' };
            (void)sprintf(desc, " creature is worth %d.%02d point%c", quotient, remainder, plural);
            memoryPrint(desc);

            const char* p, *q;

            if (py.misc.level / 10 == 1)
            {
                p = "th";
            }
            else
            {
                int ord = py.misc.level % 10;
                if (ord == 1)
                {
                    p = "st";
                }
                else if (ord == 2)
                {
                    p = "nd";
                }
                else if (ord == 3)
                {
                    p = "rd";
                }
                else
                {
                    p = "th";
                }
            }

            if (py.misc.level == 8 || py.misc.level == 11 || py.misc.level == 18)
            {
                q = "n";
            }
            else
            {
                q = "";
            }

            (void)sprintf(desc, " for a%s %d%s level character.", q, py.misc.level, p);
            memoryPrint(desc);
        }

        // Spells known, if have been used against us.
        // Breath weapons or resistance might be known only because we cast spells at it.
        static void memoryMagicSkills(uint32_t memory_spell_flags, uint32_t monster_spell_flags, uint32_t creature_spell_flags)
        {
            bool known = true;

            uint32_t spell_flags = memory_spell_flags;

            for (int i = 0; (spell_flags & config::monsters::spells::CS_BREATHE) != 0u; i++)
            {
                if ((spell_flags & (config::monsters::spells::CS_BR_LIGHT << i)) != 0u)
                {
                    spell_flags &= ~(config::monsters::spells::CS_BR_LIGHT << i);

                    if (known)
                    {
                        if ((monster_spell_flags & config::monsters::spells::CS_FREQ) != 0u)
                        {
                            memoryPrint(" It can breathe ");
                        }
                        else
                        {
                            memoryPrint(" It is resistant to ");
                        }
                        known = false;
                    }
                    else if ((spell_flags & config::monsters::spells::CS_BREATHE) != 0u)
                    {
                        memoryPrint(", ");
                    }
                    else
                    {
                        memoryPrint(" and ");
                    }
                    memoryPrint(recall_description_breath[i]);
                }
            }

            known = true;

            for (int i = 0; (spell_flags & config::monsters::spells::CS_SPELLS) != 0u; i++)
            {
                if ((spell_flags & (config::monsters::spells::CS_TEL_SHORT << i)) != 0u)
                {
                    spell_flags &= ~(config::monsters::spells::CS_TEL_SHORT << i);

                    if (known)
                    {
                        if ((memory_spell_flags & config::monsters::spells::CS_BREATHE) != 0u)
                        {
                            memoryPrint(", and is also");
                        }
                        else
                        {
                            memoryPrint(" It is");
                        }
                        memoryPrint(" magical, casting spells which ");
                        known = false;
                    }
                    else if ((spell_flags & config::monsters::spells::CS_SPELLS) != 0u)
                    {
                        memoryPrint(", ");
                    }
                    else
                    {
                        memoryPrint(" or ");
                    }
                    memoryPrint(recall_description_spell[i]);
                }
            }

            if ((memory_spell_flags & (config::monsters::spells::CS_BREATHE | config::monsters::spells::CS_SPELLS)) != 0u)
            {
                // Could offset by level
                if ((monster_spell_flags & config::monsters::spells::CS_FREQ) > 5)
                {
                    vtype_t temp = { '\0' };
                    (void)sprintf(temp, "; 1 time in %d", creature_spell_flags & config::monsters::spells::CS_FREQ);
                    memoryPrint(temp);
                }
                memoryPrint(".");
            }
        }

        // Do we know how hard they are to kill? Armor class, hit die.
        static void memoryKillDifficulty(Creature_t const &creature, uint32_t monster_kills)
{
    // the higher the level of the monster, the fewer the kills you need
    // Original knowarmor macro inlined
    if (monster_kills <= 304u / (4u + creature.level))
    {
        return;
    }

    vtype_t description = { '\0' };

        (void) sprintf(description, " It has an armor rating of %d", creature.ac);
        memoryPrint(description);

        (void) sprintf(description,                                                                           //
                   " and a%s life rating of %dd%d.",                                                      //
                       ((creature.defenses & config::monsters::defense::CD_MAX_HP) != 0 ? " maximized" : ""), //
                   creature.hit_die.dice,                                                                 //
                   creature.hit_die.sides                                                                 //
    );
    memoryPrint(description);
    }

    // Do we know how clever they are? Special abilities.
    static void memorySpecialAbilities(uint32_t move)
    {
        bool known = true;

        for (int i = 0; (move & config::monsters::move::CM_SPECIAL) != 0u; i++)
        {
            if ((move & (config::monsters::move::CM_INVISIBLE << i)) != 0u)
            {
                move &= ~(config::monsters::move::CM_INVISIBLE << i);

                if (known)
                {
                    memoryPrint(" It can ");
                    known = false;
                }
                else if ((move & config::monsters::move::CM_SPECIAL) != 0u)
                {
                    memoryPrint(", ");
                }
                else
                {
                    memoryPrint(" and ");
                }
                memoryPrint(recall_description_move[i]);
            }
        }

        if (!known)
        {
            memoryPrint(".");
        }
    }

    // Do we know its special weaknesses? Most defenses flags.
    static void memoryWeaknesses(uint32_t defense)
    {
        bool known = true;

        for (int i = 0; (defense & config::monsters::defense::CD_WEAKNESS) != 0u; i++)
        {
            if ((defense & (config::monsters::defense::CD_FROST << i)) != 0u)
            {
                defense &= ~(config::monsters::defense::CD_FROST << i);
                if (known)
                {
                    memoryPrint(" It is susceptible to ");
                    known = false;
                }
                else if ((defense & config::monsters::defense::CD_WEAKNESS) != 0u)
                {
                    memoryPrint(", ");
                }
                else
                {
                    memoryPrint(" and ");
                }
                memoryPrint(recall_description_weakness[i]);
            }
        }

        if (!known)
        {
            memoryPrint(".");
        }
    }

    // Do we know how aware it is?
    static void memoryAwareness(Creature_t const &creature, Recall_t const &memory)
    {
        if (memory.wake * memory.wake > creature.sleep_counter || memory.ignore == UCHAR_MAX || (creature.sleep_counter == 0 && memory.kills >= 10))
        {
            memoryPrint(" It ");

            if (creature.sleep_counter > 200)
            {
                me
        }
        }
