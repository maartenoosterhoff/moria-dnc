using System.Collections.Generic;

namespace Moria.Core.Data
{
    public class Recall_d
    {
        public Recall_d()
        {
            this.recall_description_attack_type = CreateRecallDescriptionAttackType().AsReadOnly();
            this.recall_description_attack_method = CreateRecallDescriptionAttackMethod().AsReadOnly();
            this.recall_description_how_much = CreateRecallDescriptionHowMuch().AsReadOnly();
            this.recall_description_move = CreateRecallDescriptionMove().AsReadOnly();
            this.recall_description_spell = CreateRecallDescriptionSpell().AsReadOnly();
            this.recall_description_breath = CreateRecallDescriptionBreath().AsReadOnly();
            this.recall_description_weakness = CreateRecallDescriptionWeakness().AsReadOnly();
        }

        public IReadOnlyList<string> recall_description_attack_type { get; }
        public IReadOnlyList<string> recall_description_attack_method { get; }
        public IReadOnlyList<string> recall_description_how_much { get; }
        public IReadOnlyList<string> recall_description_move { get; }
        public IReadOnlyList<string> recall_description_spell { get; }
        public IReadOnlyList<string> recall_description_breath { get; }
        public IReadOnlyList<string> recall_description_weakness { get; }

        private static List<string> CreateRecallDescriptionAttackType()
        {
            return new List<string>
            {
                "do something undefined",
                "attack",
                "weaken",
                "confuse",
                "terrify",
                "shoot flames",
                "shoot acid",
                "freeze",
                "shoot lightning",
                "corrode",
                "blind",
                "paralyse",
                "steal money",
                "steal things",
                "poison",
                "reduce dexterity",
                "reduce constitution",
                "drain intelligence",
                "drain wisdom",
                "lower experience",
                "call for help",
                "disenchant",
                "eat your food",
                "absorb light",
                "absorb charges",
            };
        }

        private static List<string> CreateRecallDescriptionAttackMethod()
        {
            return new List<string>
            {
                "make an undefined advance",
                "hit",
                "bite",
                "claw",
                "sting",
                "touch",
                "kick",
                "gaze",
                "breathe",
                "spit",
                "wail",
                "embrace",
                "crawl on you",
                "release spores",
                "beg",
                "slime you",
                "crush",
                "trample",
                "drool",
                "insult",
            };
        }

        private static List<string> CreateRecallDescriptionHowMuch()
        {
            return new List<string>
            {
                " not at all", " a bit", "", " quite", " very", " most", " highly", " extremely",
            };
        }

        private static List<string> CreateRecallDescriptionMove()
        {
            return new List<string>
            {
                "move invisibly", "open doors", "pass through walls", "kill weaker creatures", "pick up objects", "breed explosively",
            };
        }

        private static List<string> CreateRecallDescriptionSpell()
        {
            return new List<string>
            {
                "teleport short distances",
                "teleport long distances",
                "teleport its prey",
                "cause light wounds",
                "cause serious wounds",
                "paralyse its prey",
                "induce blindness",
                "confuse",
                "terrify",
                "summon a monster",
                "summon the undead",
                "slow its prey",
                "drain mana",
                "unknown 1",
                "unknown 2",
            };
        }

        private static List<string> CreateRecallDescriptionBreath()
        {
            return new List<string>
            {
                "lightning", "poison gases", "acid", "frost", "fire",
            };
        }
        
        private static List<string> CreateRecallDescriptionWeakness()
        {
            return new List<string>
            {
                "frost", "fire", "poison", "acid", "bright light", "rock remover",
            };
        }
    }
}
