using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class IdentifyItemCommandHandler :
        ICommandHandler<IdentifyItemCommand>,
        ICommandHandler<IdentifyItemCommand, bool>
    {
        private readonly IUiInventory uiInventory;

        public IdentifyItemCommandHandler(
            IUiInventory uiInventory
        )
        {
            this.uiInventory = uiInventory;
        }

        void ICommandHandler<IdentifyItemCommand>.Handle(IdentifyItemCommand command)
        {
            this.spellIdentifyItem();
        }

        bool ICommandHandler<IdentifyItemCommand, bool>.Handle(IdentifyItemCommand command)
        {
            return this.spellIdentifyItem();
        }

        // Identify an object -RAK-
        private bool spellIdentifyItem()
        {
            var py = State.Instance.py;
            if (!this.uiInventory.inventoryGetInputForItemId(out var item_id, "Item you wish identified?", 0, (int)Inventory_c.PLAYER_INVENTORY_SIZE, /*CNIL*/null, /*CNIL*/null))
            {
                return false;
            }

            Identification_m.itemIdentify(py.inventory[item_id], ref item_id);

            var item = py.inventory[item_id];
            Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);

            var description = string.Empty;
            //obj_desc_t description = { '\0' };
            Identification_m.itemDescription(ref description, item, true);

            string msg;
            //obj_desc_t msg = { '\0' };
            if (item_id >= (int)PlayerEquipment.Wield)
            {
                Player_m.playerRecalculateBonuses();
                msg = $"{this.uiInventory.playerItemWearingDescription(item_id)}: {description}";
                //(void)sprintf(msg, "%s: %s", playerItemWearingDescription(item_id), description);
            }
            else
            {
                msg = $"{item_id + 97} {description}";
                //(void)sprintf(msg, "%c %s", item_id + 97, description);
            }
            Ui_io_m.printMessage(msg);

            return true;
        }
    }
}