using System.Collections.Generic;
using Moria.Core.Structures;

namespace Moria.Core.Data
{
    public class Store_owners_d
    {
        public Store_owners_d()
        {
            this.store_owners = CreateStoreOwners().AsReadOnly();
            this.speech_sale_accepted = CreateSpeechSaleAccepted().AsReadOnly();
            this.speech_selling_haggle_final = CreateSpeechSellingHaggleFinal().AsReadOnly();
            this.speech_selling_haggle = CreateSpeechSellingHaggle().AsReadOnly();
            this.speech_buying_haggle_final = CreateSpeechBuyingHaggleFinal().AsReadOnly();
            this.speech_buying_haggle = CreateSpeechBuyingHaggle().AsReadOnly();
            this.speech_insulted_haggling_done = CreateSpeechInsultedHagglingDone().AsReadOnly();
            this.speech_get_out_of_my_store = CreateSpeechGetOutOfMyStore().AsReadOnly();
            this.speech_haggling_try_again = CreateSpeechHagglingTryAgain().AsReadOnly();
            this.speech_sorry = CreateSpeechSorry().AsReadOnly();
        }

        public IReadOnlyList<Owner_t> store_owners { get; }
            //ArrayInitializer.Initialize<Owner_t>(Store_c.MAX_OWNERS);

        public IReadOnlyList<string> speech_sale_accepted { get; }
        public IReadOnlyList<string> speech_selling_haggle_final { get; }
        public IReadOnlyList<string> speech_selling_haggle { get; } = new string[16];
        public IReadOnlyList<string> speech_buying_haggle_final { get; } = new string[3];
        public IReadOnlyList<string> speech_buying_haggle { get; } = new string[15];
        public IReadOnlyList<string> speech_insulted_haggling_done { get; } = new string[5];
        public IReadOnlyList<string> speech_get_out_of_my_store { get; } = new string[3];
        public IReadOnlyList<string> speech_haggling_try_again { get; } = new string[10];
        public IReadOnlyList<string> speech_sorry { get; } = new string[3];

        private static List<Owner_t> CreateStoreOwners()
        {
            return new List<Owner_t>
            {
                new Owner_t("Erick the Honest       (Human)      General Store",   250, 175, 108, 4, 0, 12),
                new Owner_t("Mauglin the Grumpy     (Dwarf)      Armory",        32000, 200, 112, 4, 5,  5),
                new Owner_t("Arndal Beast-Slayer    (Half-Elf)   Weaponsmith",   10000, 185, 110, 5, 1,  8),
                new Owner_t("Hardblow the Humble    (Human)      Temple",         3500, 175, 109, 6, 0, 15),
                new Owner_t("Ga-nat the Greedy      (Gnome)      Alchemist",     12000, 220, 115, 4, 4,  9),
                new Owner_t("Valeria Starshine      (Elf)        Magic Shop",    32000, 175, 110, 5, 2, 11),
                new Owner_t("Andy the Friendly      (Halfling)   General Store",   200, 170, 108, 5, 3, 15),
                new Owner_t("Darg-Low the Grim      (Human)      Armory",        10000, 190, 111, 4, 0,  9),
                new Owner_t("Oglign Dragon-Slayer   (Dwarf)      Weaponsmith",   32000, 195, 112, 4, 5,  8),
                new Owner_t("Gunnar the Paladin     (Human)      Temple",         5000, 185, 110, 5, 0, 23),
                new Owner_t("Mauser the Chemist     (Half-Elf)   Alchemist",     10000, 190, 111, 5, 1,  8),
                new Owner_t("Gopher the Great!      (Gnome)      Magic Shop",    20000, 215, 113, 6, 4, 10),
                new Owner_t("Lyar-el the Comely     (Elf)        General Store",   300, 165, 107, 6, 2, 18),
                new Owner_t("Mauglim the Horrible   (Half-Orc)   Armory",         3000, 200, 113, 5, 6,  9),
                new Owner_t("Ithyl-Mak the Beastly  (Half-Troll) Weaponsmith",    3000, 210, 115, 6, 7,  8),
                new Owner_t("Delilah the Pure       (Half-Elf)   Temple",        25000, 180, 107, 6, 1, 20),
                new Owner_t("Wizzle the Chaotic     (Halfling)   Alchemist",     10000, 190, 110, 6, 3,  8),
                new Owner_t("Inglorian the Mage     (Human?)     Magic Shop",    32000, 200, 110, 7, 0, 10),
            };
        }

        private static List<string> CreateSpeechSaleAccepted()
        {
            return new List<string>
            {
                "Done!",
                "Accepted!",
                "Fine.",
                "Agreed!",
                "Ok.",
                "Taken!",
                "You drive a hard bargain, but taken.",
                "You'll force me bankrupt, but it's a deal.",
                "Sigh.  I'll take it.",
                "My poor sick children may starve, but done!",
                "Finally!  I accept.",
                "Robbed again.",
                "A pleasure to do business with you!",
                "My spouse will skin me, but accepted.",
            };
        }

        private static List<string> CreateSpeechSellingHaggleFinal()
        {
            return new List<string>
            {
                "%A2 is my final offer; take it or leave it.",
                "I'll give you no more than %A2.",
                "My patience grows thin.  %A2 is final.",
            };
        }

        private static List<string> CreateSpeechSellingHaggle()
        {
            return new List<string>
            {
                "%A1 for such a fine item?  HA!  No less than %A2.",
                "%A1 is an insult!  Try %A2 gold pieces.",
                "%A1?!?  You would rob my poor starving children?",
                "Why, I'll take no less than %A2 gold pieces.",
                "Ha!  No less than %A2 gold pieces.",
                "Thou knave!  No less than %A2 gold pieces.",
                "%A1 is far too little, how about %A2?",
                "I paid more than %A1 for it myself, try %A2.",
                "%A1?  Are you mad?!?  How about %A2 gold pieces?",
                "As scrap this would bring %A1.  Try %A2 in gold.",
                "May the fleas of 1000 Orcs molest you.  I want %A2.",
                "My mother you can get for %A1, this costs %A2.",
                "May your chickens grow lips.  I want %A2 in gold!",
                "Sell this for such a pittance?  Give me %A2 gold.",
                "May the Balrog find you tasty!  %A2 gold pieces?",
                "Your mother was a Troll!  %A2 or I'll tell.",
            };
        }

        private static List<string> CreateSpeechBuyingHaggleFinal()
        {
            return new List<string>
            {
                "I'll pay no more than %A1; take it or leave it.",
                "You'll get no more than %A1 from me.",
                "%A1 and that's final.",
            };
        }

        private static List<string> CreateSpeechBuyingHaggle()
        {
            return new List<string>
            {
                "%A2 for that piece of junk?  No more than %A1.",
                "For %A2 I could own ten of those.  Try %A1.",
                "%A2?  NEVER!  %A1 is more like it.",
                "Let's be reasonable. How about %A1 gold pieces?",
                "%A1 gold for that junk, no more.",
                "%A1 gold pieces and be thankful for it!",
                "%A1 gold pieces and not a copper more.",
                "%A2 gold?  HA!  %A1 is more like it.",
                "Try about %A1 gold.",
                "I wouldn't pay %A2 for your children, try %A1.",
                "*CHOKE* For that!?  Let's say %A1.",
                "How about %A1?",
                "That looks war surplus!  Say %A1 gold.",
                "I'll buy it as scrap for %A1.",
                "%A2 is too much, let us say %A1 gold.",
            };
        }

        private static List<string> CreateSpeechInsultedHagglingDone()
        {
            return new List<string>
            {
                "ENOUGH!  You have abused me once too often!",
                "THAT DOES IT!  You shall waste my time no more!",
                "This is getting nowhere.  I'm going home!",
                "BAH!  No more shall you insult me!",
                "Begone!  I have had enough abuse for one day.",
            };
        }

        private static List<string> CreateSpeechGetOutOfMyStore()
        {
            return new List<string>
            {
                "Out of my place!", "out... Out... OUT!!!",
                "Come back tomorrow.", "Leave my place.  Begone!",
                "Come back when thou art richer.",
            };
        }

        private static List<string> CreateSpeechHagglingTryAgain()
        {
            return new List<string>
            {
                "You will have to do better than that!",
                "That's an insult!",
                "Do you wish to do business or not?",
                "Hah!  Try again.",
                "Ridiculous!",
                "You've got to be kidding!",
                "You'd better be kidding!",
                "You try my patience.",
                "I don't hear you.",
                "Hmmm, nice weather we're having.",
            };
        }

        private static List<string> CreateSpeechSorry()
        {
            return new List<string>
            {
                "I must have heard you wrong.", "What was that?",
                "I'm sorry, say that again.", "What did you say?",
                "Sorry, what was that again?",
            };
        }
    }
}
