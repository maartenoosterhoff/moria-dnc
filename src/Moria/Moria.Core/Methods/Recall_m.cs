using Moria.Core.Configs;
using Moria.Core.Data;
using Moria.Core.States;
using Moria.Core.Structures;
using static Moria.Core.Constants.Std_c;
using static Moria.Core.Constants.Monster_c;
using static Moria.Core.Constants.Ui_c;
using static Moria.Core.Methods.Monster_m;

namespace Moria.Core.Methods
{
    public interface IRecall
    {
        int memoryRecall(int monster_id);
        void recallMonsterAttributes(char command);
    }

    public class Recall_m : IRecall
    {
        private readonly ITerminal terminal;

        public Recall_m(
            ITerminal terminal
        )
        {
            this.terminal = terminal;
        }

        private T plural<T>(int count, T ss, T sp)
        {
            return count == 1 ? ss : sp;
        }

        //#define plural(c, ss, sp) ((c) == 1 ? (s) : (sp))

        // Number of kills needed for information.
        // the higher the level of the monster, the fewer the attacks you need,
        // the more damage an attack does, the more attacks you need.
        //#define knowdamage(l, a, d) ((4 + (l)) * (a) > 80 * (d))

        private bool knowDamage(int l, int a, int d) => (4 + l) * a > 80 * d;

        // Print out strings, filling up lines as we go.
        private void memoryPrint(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return;
            }

            var width = this.terminal.getConsoleWidth();

            while (p.Length > 0)
            {
                var pos = p.Length;
                if (pos> width - 2 && p.Contains(" "))
                {
                    pos = width - 2;
                    while (
                        pos >= p.Length ||
                        !char.IsWhiteSpace(p[pos]))
                    {
                        pos--;
                    }
                }

                var chunk = p.Substring(0, pos);
                this.terminal.putStringClearToEOL(chunk, new Coord_t(State.Instance.roff_print_line, 0));
                State.Instance.roff_print_line++;
                if (pos == p.Length)
                {
                    p = string.Empty;
                }
                else
                {
                    p = p.Substring(pos + 1).Trim();
                }
            }
            /*

            while (p.Length != 0)
            {
                var roff_buffer_pointer = p;
                //*roff_buffer_pointer = *p;

                if (*p == '\n' || roff_buffer_pointer >= State.Instance.roff_buffer.Length + State.Instance.roff_buffer.Length - 1)
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
                    putStringClearToEOL(roff_buffer, new Coord_t(State.Instance.roff_print_line, 0));
                    State.Instance.roff_print_line++;

                    char* r = State.Instance.roff_buffer;

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
                    State.Instance.roff_buffer_pointer++;
                }

                p++;
            }
            */
        }

        // Do we know anything about this monster?
        private bool memoryMonsterKnown(Recall_t memory)
        {
            var game = State.Instance.game;
            if (game.wizard_mode)
            {
                return true;
            }

            if (memory.movement != 0u || memory.defenses != 0u || memory.kills != 0u || memory.spells != 0u ||
                memory.deaths != 0u)
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

        private void memoryWizardModeInit(Recall_t memory, Creature_t creature)
        {
            memory.kills = (uint)SHRT_MAX;
            memory.wake = memory.ignore = UCHAR_MAX;

            var move = (uint)((creature.movement & Config.monsters_move.CM_4D2_OBJ) != 0 ? 1 : 0) * 8;
            move += (uint)((creature.movement & Config.monsters_move.CM_2D2_OBJ) != 0 ? 1 : 0) * 4;
            move += (uint)((creature.movement & Config.monsters_move.CM_1D2_OBJ) != 0 ? 1 : 0) * 2;
            move += (uint)((creature.movement & Config.monsters_move.CM_90_RANDOM) != 0 ? 1 : 0);
            move += (uint)((creature.movement & Config.monsters_move.CM_60_RANDOM) != 0 ? 1 : 0);

            memory.movement = (uint)((creature.movement & ~Config.monsters_move.CM_TREASURE) |
                                      (move << (int)Config.monsters_move.CM_TR_SHIFT));
            memory.defenses = creature.defenses;

            if ((creature.spells & Config.monsters_spells.CS_FREQ) != 0u)
            {
                memory.spells = (uint)(creature.spells | Config.monsters_spells.CS_FREQ);
            }
            else
            {
                memory.spells = creature.spells;
            }

            for (var i = 0; i < MON_MAX_ATTACKS; i++)
            {
                if (creature.damage[i] == 0)
                    break;
                memory.attacks[i] = UCHAR_MAX;
            }

            // A little hack to enable the display of info for Quylthulgs.
            if ((memory.movement & Config.monsters_move.CM_ONLY_MAGIC) != 0u)
            {
                memory.attacks[0] = UCHAR_MAX;
            }
        }

        // Conflict history.
        private void memoryConflictHistory(uint deaths, uint kills)
        {
            var desc = string.Empty;
            //vtype_t desc = { '\0' };

            if (deaths != 0u)
            {
                desc = $"{deaths:d} of the contributors to your monster memory {this.plural((int)deaths, "has", "have")}";
                //(void)sprintf(desc, "%d of the contributors to your monster memory %s", deaths, plural(deaths, "has", "have"));
                this.memoryPrint(desc);
                this.memoryPrint(" been killed by this creature, and ");
                if (kills == 0)
                {
                    this.memoryPrint("it is not ever known to have been defeated.");
                }
                else
                {
                    desc = $"at least {kills:d} of the beasts {this.plural((int)kills, "has", "have")} been exterminated.";
                    //(void)sprintf(desc, "at least %d of the beasts %s been exterminated.", kills, plural(kills, "has", "have"));
                    this.memoryPrint(desc);
                }
            }
            else if (kills != 0u)
            {
                desc = $"At least {kills:d} of these creatures {this.plural((int)kills, "has", "have")}";
                //(void)sprintf(desc, "At least %d of these creatures %s", kills, plural(kills, "has", "have"));
                this.memoryPrint(desc);
                this.memoryPrint(" been killed by contributors to your monster memory.");
            }
            else
            {
                this.memoryPrint("No known battles to the death are recalled.");
            }
        }

        // Immediately obvious.
        private bool memoryDepthFoundAt(uint level, uint kills)
        {
            var known = false;

            if (level == 0)
            {
                known = true;
                this.memoryPrint(" It lives in the town");
            }
            else if (kills != 0u)
            {
                known = true;

                // The Balrog is a level 100 monster, but appears at 50 feet.
                if (level > Config.monsters.MON_ENDGAME_LEVEL)
                {
                    level = Config.monsters.MON_ENDGAME_LEVEL;
                }

                var desc = $" It is normally found at depths of {level * 50:d} feet";
                //vtype_t desc = { '\0' };
                //(void)sprintf(desc, " It is normally found at depths of %d feet", level * 50);
                this.memoryPrint(desc);
            }

            return known;
        }

        private bool memoryMovement(uint rc_move, int monster_speed, bool is_known)
        {
            // the creatures_list speed value is 10 greater, so that it can be a uint8_t
            monster_speed -= 10;

            if ((rc_move & Config.monsters_move.CM_ALL_MV_FLAGS) != 0u)
            {
                if (is_known)
                {
                    this.memoryPrint(", and");
                }
                else
                {
                    this.memoryPrint(" It");
                    is_known = true;
                }

                this.memoryPrint(" moves");

                if ((rc_move & Config.monsters_move.CM_RANDOM_MOVE) != 0u)
                {
                    this.memoryPrint(
                        Library.Instance.Recall.recall_description_how_much
                            [(int)((rc_move & Config.monsters_move.CM_RANDOM_MOVE) >> 3)]);
                    this.memoryPrint(" erratically");
                }

                if (monster_speed == 1)
                {
                    this.memoryPrint(" at normal speed");
                }
                else
                {
                    if ((rc_move & Config.monsters_move.CM_RANDOM_MOVE) != 0u)
                    {
                        this.memoryPrint(", and");
                    }

                    if (monster_speed <= 0)
                    {
                        if (monster_speed == -1)
                        {
                            this.memoryPrint(" very");
                        }
                        else if (monster_speed < -1)
                        {
                            this.memoryPrint(" incredibly");
                        }

                        this.memoryPrint(" slowly");
                    }
                    else
                    {
                        if (monster_speed == 3)
                        {
                            this.memoryPrint(" very");
                        }
                        else if (monster_speed > 3)
                        {
                            this.memoryPrint(" unbelievably");
                        }

                        this.memoryPrint(" quickly");
                    }
                }
            }

            if ((rc_move & Config.monsters_move.CM_ATTACK_ONLY) != 0u)
            {
                if (is_known)
                {
                    this.memoryPrint(", but");
                }
                else
                {
                    this.memoryPrint(" It");
                    is_known = true;
                }

                this.memoryPrint(" does not deign to chase intruders");
            }

            if ((rc_move & Config.monsters_move.CM_ONLY_MAGIC) != 0u)
            {
                if (is_known)
                {
                    this.memoryPrint(", but");
                }
                else
                {
                    this.memoryPrint(" It");
                    is_known = true;
                }

                this.memoryPrint(" always moves and attacks by using magic");
            }

            return is_known;
        }

        // Kill it once to know experience, and quality (evil, undead, monstrous).
        // The quality of being a dragon is obvious.
        private void memoryKillPoints(uint creature_defense, uint monster_exp, uint level)
        {
            var py = State.Instance.py;

            this.memoryPrint(" A kill of this");

            if ((creature_defense & Config.monsters_defense.CD_ANIMAL) != 0)
            {
                this.memoryPrint(" natural");
            }

            if ((creature_defense & Config.monsters_defense.CD_EVIL) != 0)
            {
                this.memoryPrint(" evil");
            }

            if ((creature_defense & Config.monsters_defense.CD_UNDEAD) != 0)
            {
                this.memoryPrint(" undead");
            }

            // calculate the integer exp part, can be larger than 64K when first
            // level character looks at Balrog info, so must store in long
            var quotient = (int)monster_exp * (int)level / (int)py.misc.level;

            // calculate the fractional exp part scaled by 100,
            // must use long arithmetic to avoid overflow
            var remainder =
                (int)(((int)monster_exp * (int)level % (int)py.misc.level * (int)1000 / (int)py.misc.level +
                        5) / 10);

            char plural;
            if (quotient == 1 && remainder == 0)
            {
                plural = '\0';
            }
            else
            {
                plural = 's';
            }

            var desc = $" creature is worth {quotient:d}.{remainder,2:d} point{plural}";
            //vtype_t desc = { '\0' };
            //(void)sprintf(desc, " creature is worth %d.%02d point%c", quotient, remainder, plural);
            this.memoryPrint(desc);

            string p, q;

            if (py.misc.level / 10 == 1)
            {
                p = "th";
            }
            else
            {
                var ord = (int)py.misc.level % 10;
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

            desc = $" for a{q} {py.misc.level:d}{p} level character.";
            //(void)sprintf(desc, " for a%s %d%s level character.", q, py.misc.level, p);
            this.memoryPrint(desc);
        }

        // Spells known, if have been used against us.
        // Breath weapons or resistance might be known only because we cast spells at it.
        private void memoryMagicSkills(uint memory_spell_flags, uint monster_spell_flags,
            uint creature_spell_flags)
        {
            var known = true;

            var spell_flags = memory_spell_flags;

            for (var i = 0; (spell_flags & Config.monsters_spells.CS_BREATHE) != 0u; i++)
            {
                if ((spell_flags & (Config.monsters_spells.CS_BR_LIGHT << i)) != 0u)
                {
                    spell_flags &= ~(Config.monsters_spells.CS_BR_LIGHT << i);

                    if (known)
                    {
                        if ((monster_spell_flags & Config.monsters_spells.CS_FREQ) != 0u)
                        {
                            this.memoryPrint(" It can breathe ");
                        }
                        else
                        {
                            this.memoryPrint(" It is resistant to ");
                        }

                        known = false;
                    }
                    else if ((spell_flags & Config.monsters_spells.CS_BREATHE) != 0u)
                    {
                        this.memoryPrint(", ");
                    }
                    else
                    {
                        this.memoryPrint(" and ");
                    }

                    this.memoryPrint(Library.Instance.Recall.recall_description_breath[i]);
                }
            }

            known = true;

            for (var i = 0; (spell_flags & Config.monsters_spells.CS_SPELLS) != 0u; i++)
            {
                if ((spell_flags & (Config.monsters_spells.CS_TEL_SHORT << i)) != 0u)
                {
                    spell_flags &= ~(Config.monsters_spells.CS_TEL_SHORT << i);

                    if (known)
                    {
                        if ((memory_spell_flags & Config.monsters_spells.CS_BREATHE) != 0u)
                        {
                            this.memoryPrint(", and is also");
                        }
                        else
                        {
                            this.memoryPrint(" It is");
                        }

                        this.memoryPrint(" magical, casting spells which ");
                        known = false;
                    }
                    else if ((spell_flags & Config.monsters_spells.CS_SPELLS) != 0u)
                    {
                        this.memoryPrint(", ");
                    }
                    else
                    {
                        this.memoryPrint(" or ");
                    }

                    this.memoryPrint(Library.Instance.Recall.recall_description_spell[i]);
                }
            }

            if ((memory_spell_flags & (Config.monsters_spells.CS_BREATHE | Config.monsters_spells.CS_SPELLS)) != 0u)
            {
                // Could offset by level
                if ((monster_spell_flags & Config.monsters_spells.CS_FREQ) > 5)
                {
                    var temp = $"; 1 time in {creature_spell_flags & Config.monsters_spells.CS_FREQ:d}";
                    //vtype_t temp = { '\0' };
                    //(void)sprintf(temp, "; 1 time in %d", creature_spell_flags & Config.monsters_spells.CS_FREQ);
                    this.memoryPrint(temp);
                }

                this.memoryPrint(".");
            }
        }

        // Do we know how hard they are to kill? Armor class, hit die.
        private void memoryKillDifficulty(Creature_t creature, uint monster_kills)
        {
            // the higher the level of the monster, the fewer the kills you need
            // Original knowarmor macro inlined
            if (monster_kills <= 304u / (4u + creature.level))
            {
                return;
            }

            var description = string.Empty;
            //vtype_t description = { '\0' };

            description = $" It has an armor rating of {creature.ac:d}";
            //(void)sprintf(description, " It has an armor rating of %d", creature.ac);
            this.memoryPrint(description);

            description =
                $" and a{((creature.defenses & Config.monsters_defense.CD_MAX_HP) != 0 ? " maximized" : "")} life rating of {creature.hit_die.dice}d{creature.hit_die.sides}.";
            //(void)sprintf(description,                                                                           //
            //           " and a%s life rating of %dd%d.",                                                      //
            //               ((creature.defenses & Config.monsters_defense.CD_MAX_HP) != 0 ? " maximized" : ""), //
            //           creature.hit_die.dice,                                                                 //
            //           creature.hit_die.sides                                                                 //
            //);
            this.memoryPrint(description);
        }

        // Do we know how clever they are? Special abilities.
        private void memorySpecialAbilities(uint move)
        {
            var known = true;

            for (var i = 0; (move & Config.monsters_move.CM_SPECIAL) != 0u; i++)
            {
                if ((move & (Config.monsters_move.CM_INVISIBLE << i)) != 0u)
                {
                    move &= ~(Config.monsters_move.CM_INVISIBLE << i);

                    if (known)
                    {
                        this.memoryPrint(" It can ");
                        known = false;
                    }
                    else if ((move & Config.monsters_move.CM_SPECIAL) != 0u)
                    {
                        this.memoryPrint(", ");
                    }
                    else
                    {
                        this.memoryPrint(" and ");
                    }

                    this.memoryPrint(Library.Instance.Recall.recall_description_move[i]);
                }
            }

            if (!known)
            {
                this.memoryPrint(".");
            }
        }

        // Do we know its special weaknesses? Most defenses flags.
        private void memoryWeaknesses(uint defense)
        {
            var known = true;

            for (var i = 0; (defense & Config.monsters_defense.CD_WEAKNESS) != 0u; i++)
            {
                if ((defense & (Config.monsters_defense.CD_FROST << i)) != 0u)
                {
                    defense &= ~(Config.monsters_defense.CD_FROST << i);
                    if (known)
                    {
                        this.memoryPrint(" It is susceptible to ");
                        known = false;
                    }
                    else if ((defense & Config.monsters_defense.CD_WEAKNESS) != 0u)
                    {
                        this.memoryPrint(", ");
                    }
                    else
                    {
                        this.memoryPrint(" and ");
                    }

                    this.memoryPrint(Library.Instance.Recall.recall_description_weakness[i]);
                }
            }

            if (!known)
            {
                this.memoryPrint(".");
            }
        }

        // Do we know how aware it is?
        private void memoryAwareness(Creature_t creature, Recall_t memory)
        {
            if (memory.wake * memory.wake > creature.sleep_counter ||
                memory.ignore == UCHAR_MAX ||
                creature.sleep_counter == 0 && memory.kills >= 10)
            {
                this.memoryPrint(" It ");

                if (creature.sleep_counter > 200)
                {
                    this.memoryPrint("prefers to ignore");
                }
                else if (creature.sleep_counter > 95)
                {
                    this.memoryPrint("pays very little attention to");
                }
                else if (creature.sleep_counter > 75)
                {
                    this.memoryPrint("pays little attention to");
                }
                else if (creature.sleep_counter > 45)
                {
                    this.memoryPrint("tends to overlook");
                }
                else if (creature.sleep_counter > 25)
                {
                    this.memoryPrint("takes quite a while to see");
                }
                else if (creature.sleep_counter > 10)
                {
                    this.memoryPrint("takes a while to see");
                }
                else if (creature.sleep_counter > 5)
                {
                    this.memoryPrint("is fairly observant of");
                }
                else if (creature.sleep_counter > 3)
                {
                    this.memoryPrint("is observant of");
                }
                else if (creature.sleep_counter > 1)
                {
                    this.memoryPrint("is very observant of");
                }
                else if (creature.sleep_counter != 0)
                {
                    this.memoryPrint("is vigilant for");
                }
                else
                {
                    this.memoryPrint("is ever vigilant for");
                }

                var text = $" intruders, which it may notice from {10 * creature.area_affect_radius} feet.";
                //vtype_t text = { '\0' };
                //(void)sprintf(text, " intruders, which it may notice from %d feet.", 10 * creature.area_affect_radius);
                this.memoryPrint(text);
            }
        }

        // Do we know what it might carry?
        private void memoryLootCarried(uint creature_move, uint memory_move)
        {
            if ((memory_move & (Config.monsters_move.CM_CARRY_OBJ | Config.monsters_move.CM_CARRY_GOLD)) == 0u)
            {
                return;
            }

            this.memoryPrint(" It may");

            var carrying_chance = (uint)((memory_move & Config.monsters_move.CM_TREASURE) >>
                                          (int)Config.monsters_move.CM_TR_SHIFT);

            if (carrying_chance == 1)
            {
                if ((creature_move & Config.monsters_move.CM_TREASURE) == Config.monsters_move.CM_60_RANDOM)
                {
                    this.memoryPrint(" sometimes");
                }
                else
                {
                    this.memoryPrint(" often");
                }
            }
            else if (carrying_chance == 2 && (creature_move & Config.monsters_move.CM_TREASURE) ==
                (Config.monsters_move.CM_60_RANDOM | Config.monsters_move.CM_90_RANDOM))
            {
                this.memoryPrint(" often");
            }

            this.memoryPrint(" carry");

            string p;

            if ((memory_move & Config.monsters_move.CM_SMALL_OBJ) != 0u)
            {
                p = " small objects";
            }
            else
            {
                p = " objects";
            }

            if (carrying_chance == 1)
            {
                if ((memory_move & Config.monsters_move.CM_SMALL_OBJ) != 0u)
                {
                    p = " a small object";
                }
                else
                {
                    p = " an object";
                }
            }
            else if (carrying_chance == 2)
            {
                this.memoryPrint(" one or two");
            }
            else
            {
                var msg = $" up to {carrying_chance}";
                //vtype_t msg = { '\0' };
                //(void)sprintf(msg, " up to %d", carrying_chance);
                this.memoryPrint(msg);
            }

            if ((memory_move & Config.monsters_move.CM_CARRY_OBJ) != 0u)
            {
                this.memoryPrint(p);
                if ((memory_move & Config.monsters_move.CM_CARRY_GOLD) != 0u)
                {
                    this.memoryPrint(" or treasure");
                    if (carrying_chance > 1)
                    {
                        this.memoryPrint("s");
                    }
                }

                this.memoryPrint(".");
            }
            else if (carrying_chance != 1)
            {
                this.memoryPrint(" treasures.");
            }
            else
            {
                this.memoryPrint(" treasure.");
            }
        }

        private void memoryAttackNumberAndDamage(Recall_t memory, Creature_t creature)
        {
            // We know about attacks it has used on us, and maybe the damage they do.
            // known_attacks is the total number of known attacks, used for punctuation
            var known_attacks = 0;

            foreach (var attack in memory.attacks)
            {
                if (attack != 0u)
                {
                    known_attacks++;
                }
            }

            // attack_count counts the attacks as printed, used for punctuation
            var attack_count = 0;
            for (var i = 0; i < MON_MAX_ATTACKS; i++)
            {
                var attack_id = (int)creature.damage[i];
                if (attack_id == 0)
                {
                    break;
                }

                // don't print out unknown attacks
                if (memory.attacks[i] == 0u)
                {
                    continue;
                }

                var attack_type = Library.Instance.Creatures.monster_attacks[attack_id].type_id;
                var attack_description_id = Library.Instance.Creatures.monster_attacks[attack_id].description_id;
                var dice = Library.Instance.Creatures.monster_attacks[attack_id].dice;

                attack_count++;

                if (attack_count == 1)
                {
                    this.memoryPrint(" It can ");
                }
                else if (attack_count == known_attacks)
                {
                    this.memoryPrint(", and ");
                }
                else
                {
                    this.memoryPrint(", ");
                }

                if (attack_description_id > 19)
                {
                    attack_description_id = 0;
                }

                this.memoryPrint(Library.Instance.Recall.recall_description_attack_method[(int)attack_description_id]);

                if (attack_type != 1 || dice.dice > 0 && dice.sides > 0)
                {
                    this.memoryPrint(" to ");

                    if (attack_type > 24)
                    {
                        attack_type = 0;
                    }

                    this.memoryPrint(Library.Instance.Recall.recall_description_attack_type[(int)attack_type]);

                    if (dice.dice != 0 && dice.sides != 0)
                    {
                        if (this.knowDamage((int)creature.level, (int)memory.attacks[i], (int)(dice.dice * dice.sides)))
                        {
                            // Loss of experience
                            if (attack_type == 19)
                            {
                                this.memoryPrint(" by");
                            }
                            else
                            {
                                this.memoryPrint(" with damage");
                            }

                            var msg = $" {dice.dice}d{dice.sides}";
                            //vtype_t msg = { '\0' };
                            //(void)sprintf(msg, " %dd%d", dice.dice, dice.sides);
                            this.memoryPrint(msg);
                        }
                    }
                }
            }

            if (attack_count != 0)
            {
                this.memoryPrint(".");
            }
            else if (known_attacks > 0 && memory.attacks[0] >= 10)
            {
                this.memoryPrint(" It has no physical attacks.");
            }
            else
            {
                this.memoryPrint(" Nothing is known about its attack.");
            }
        }

        // Print out what we have discovered about this monster.
        public int memoryRecall(int monster_id)
        {
            var game = State.Instance.game;

            var memory = State.Instance.creature_recall[monster_id];
            var creature = Library.Instance.Creatures.creatures_list[monster_id];

            var saved_memory = new Recall_t();

            if (game.wizard_mode)
            {
                saved_memory = memory;
                this.memoryWizardModeInit(memory, creature);
            }

            State.Instance.roff_print_line = 0;
            State.Instance.roff_buffer_pointer = State.Instance.roff_buffer;

            var spells = (uint)(memory.spells & creature.spells & ~Config.monsters_spells.CS_FREQ);

            // the Config.monsters_move.CM_WIN property is always known, set it if a win monster
            var move = (uint)(memory.movement | (creature.movement & Config.monsters_move.CM_WIN));

            var defense = memory.defenses & creature.defenses;

            bool known;

            // Start the paragraph for the core monster description
            var msg = $"The {creature.name}:\n";
            //vtype_t msg = {'\0'};
            //(void) sprintf(msg, "The %s:\n", creature.name);
            this.memoryPrint(msg);

            this.memoryConflictHistory(memory.deaths, memory.kills);
            known = this.memoryDepthFoundAt(creature.level, memory.kills);
            known = this.memoryMovement(move, (int)creature.speed, known);

            // Finish off the paragraph with a period!
            if (known)
            {
                this.memoryPrint(".");
            }

            if (memory.kills != 0u)
            {
                this.memoryKillPoints(creature.defenses, creature.kill_exp_value, creature.level);
            }

            this.memoryMagicSkills(spells, memory.spells, creature.spells);

            this.memoryKillDifficulty(creature, memory.kills);

            this.memorySpecialAbilities(move);

            this.memoryWeaknesses(defense);

            if ((defense & Config.monsters_defense.CD_INFRA) != 0)
            {
                this.memoryPrint(" It is warm blooded");
            }

            if ((defense & Config.monsters_defense.CD_NO_SLEEP) != 0)
            {
                if ((defense & Config.monsters_defense.CD_INFRA) != 0)
                {
                    this.memoryPrint(", and");
                }
                else
                {
                    this.memoryPrint(" It");
                }

                this.memoryPrint(" cannot be charmed or slept");
            }

            if ((defense & (Config.monsters_defense.CD_NO_SLEEP | Config.monsters_defense.CD_INFRA)) != 0)
            {
                this.memoryPrint(".");
            }

            this.memoryAwareness(creature, memory);

            this.memoryLootCarried(creature.movement, move);

            this.memoryAttackNumberAndDamage(memory, creature);

            // Always know the win creature.
            if ((creature.movement & Config.monsters_move.CM_WIN) != 0u)
            {
                this.memoryPrint(" Killing one of these wins the game!");
            }

            this.memoryPrint("\n");
            this.terminal.putStringClearToEOL("--pause--", new Coord_t(State.Instance.roff_print_line, 0));

            if (game.wizard_mode)
            {
                memory = saved_memory;
            }

            return this.terminal.getKeyInput();
        }

        // Allow access to monster memory. -CJS-
        public void recallMonsterAttributes(char command)
        {
            var n = 0;
            char query;

            for (var i = (int)MON_MAX_CREATURES - 1; i >= 0; i--)
            {
                if (Library.Instance.Creatures.creatures_list[i].sprite == command && this.memoryMonsterKnown(State.Instance.creature_recall[i]))
                {
                    if (n == 0)
                    {
                        this.terminal.putString("You recall those details? [y/n]", new Coord_t(0, 40));
                        query = this.terminal.getKeyInput();

                        if (query != 'y' && query != 'Y')
                        {
                            break;
                        }

                        this.terminal.eraseLine(new Coord_t(0, 40));
                        this.terminal.terminalSaveScreen();
                    }

                    n++;

                    query = (char) this.memoryRecall(i);
                    this.terminal.terminalRestoreScreen();
                    if (query == ESCAPE)
                    {
                        break;
                    }
                }
            }
        }
    }
}