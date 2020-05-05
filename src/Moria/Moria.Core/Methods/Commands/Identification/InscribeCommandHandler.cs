using Moria.Core.Constants;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands.Identification
{
    public class InscribeCommandHandler : ICommandHandler<InscribeCommand>
    {
        private readonly IIdentification identification;
        private readonly ITerminal terminal;
        private readonly IUiInventory uiInventory;

        public InscribeCommandHandler(
            IIdentification identification,
            ITerminal terminal,
            IUiInventory uiInventory
        )
        {
            this.identification = identification;
            this.terminal = terminal;
            this.uiInventory = uiInventory;
        }

        public void Handle(InscribeCommand command)
        {
            this.itemInscribe();
        }

        // Add a comment to an object description. -CJS-
        public void itemInscribe()
        {
            var py = State.Instance.py;
            if (py.pack.unique_items == 0 && py.equipment_count == 0)
            {
                this.terminal.printMessage("You are not carrying anything to inscribe.");
                return;
            }

            var item_id = 0;
            if (!this.uiInventory.inventoryGetInputForItemId(out item_id, "Which one? ", 0, (int)Inventory_c.PLAYER_INVENTORY_SIZE, null /*CNIL*/, null /*CNIL*/))
            {
                return;
            }

            //obj_desc_t msg = { '\0' };
            this.identification.itemDescription(out var msg, py.inventory[item_id], true);

            //obj_desc_t inscription = { '\0' };
            var inscription = $"Inscribing {msg}";
            //(void)sprintf(inscription, "Inscribing %s", msg);

            this.terminal.printMessage(inscription);

            if (!string.IsNullOrEmpty(py.inventory[item_id].inscription))
                //if (py.inventory[item_id].inscription[0] != '\0')
            {
                inscription = $"Replace {py.inventory[item_id].inscription} New inscription:";
                //(void)sprintf(inscription, "Replace %s New inscription:", py.inventory[item_id].inscription);
            }
            else
            {
                inscription = "Inscription: ";
                //(void)strcpy(inscription, "Inscription: ");
            }

            var msg_len = 78 - msg.Length;//(int)strlen(msg);
            if (msg_len > 12)
            {
                msg_len = 12;
            }

            this.terminal.putStringClearToEOL(inscription, new Coord_t(0, 0));

            if (this.terminal.getStringInput(out inscription, new Coord_t(0, inscription.Length), msg_len))
            {
                this.identification.itemReplaceInscription(py.inventory[item_id], inscription);
            }
        }
    }
}