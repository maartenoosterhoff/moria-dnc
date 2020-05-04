﻿using Moria.Core.Constants;
using Moria.Core.States;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class RechargeItemCommandHandler :
        ICommandHandler<RechargeItemCommand>,
        ICommandHandler<RechargeItemCommand, bool>
    {
        private readonly IUiInventory uiInventory;
        private readonly IIdentification identification;
        private readonly IInventoryManager inventoryManager;
        private readonly IRnd rnd;
        private readonly ITerminal terminal;

        public RechargeItemCommandHandler(
            IUiInventory uiInventory,
            IIdentification identification,
            IInventoryManager inventoryManager,
            IRnd rnd,
            ITerminal terminal
        )
        {
            this.uiInventory = uiInventory;
            this.identification = identification;
            this.inventoryManager = inventoryManager;
            this.rnd = rnd;
            this.terminal = terminal;
        }

        void ICommandHandler<RechargeItemCommand>.Handle(RechargeItemCommand command)
        {
            this.spellRechargeItem(command.NumberOfCharges);
        }

        bool ICommandHandler<RechargeItemCommand, bool>.Handle(RechargeItemCommand command)
        {
            return this.spellRechargeItem(command.NumberOfCharges);
        }

        // Recharge a wand, staff, or rod.  Sometimes the item breaks. -RAK-
        private bool spellRechargeItem(int number_of_charges)
        {
            var py = State.Instance.py;
            int item_pos_start = 0, item_pos_end = 0;
            if (!this.inventoryManager.inventoryFindRange((int)Treasure_c.TV_STAFF, (int)Treasure_c.TV_WAND, out item_pos_start, out item_pos_end))
            {
                this.terminal.printMessage("You have nothing to recharge.");
                return false;
            }

            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Recharge which item?", item_pos_start, item_pos_end, /*CNIL*/null, /*CNIL*/null))
            {
                return false;
            }

            var item = py.inventory[item_id];

            // recharge  I = recharge(20) = 1/6  failure for empty 10th level wand
            // recharge II = recharge(60) = 1/10 failure for empty 10th level wand
            //
            // make it harder to recharge high level, and highly charged wands,
            // note that `fail_chance` can be negative, so check its value before
            // trying to call rnd.randomNumber().
            var fail_chance = number_of_charges + 50 - (int)item.depth_first_found - item.misc_use;

            // Automatic failure.
            if (fail_chance < 19)
            {
                fail_chance = 1;
            }
            else
            {
                fail_chance = this.rnd.randomNumber(fail_chance / 10);
            }

            if (fail_chance == 1)
            {
                this.terminal.printMessage("There is a bright flash of light.");
                this.inventoryManager.inventoryDestroyItem(item_id);
            }
            else
            {
                number_of_charges = number_of_charges / ((int)item.depth_first_found + 2) + 1;
                item.misc_use += 2 + this.rnd.randomNumber(number_of_charges);

                if (this.identification.spellItemIdentified(item))
                {
                    this.identification.spellItemRemoveIdentification(item);
                }

                this.identification.itemIdentificationClearEmpty(item);
            }

            return true;
        }
    }
}