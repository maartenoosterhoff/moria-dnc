using Moria.Core.Configs;
using Moria.Core.States;
using Moria.Core.Structures.Enumerations;

namespace Moria.Core.Methods.Commands.SpellCasting
{
    public class RemoveCurseFromAllItemsCommandHandler :
        ICommandHandler<RemoveCurseFromAllItemsCommand>,
        ICommandHandler<RemoveCurseFromAllItemsCommand, bool>
    {
        void ICommandHandler<RemoveCurseFromAllItemsCommand>.Handle(RemoveCurseFromAllItemsCommand command)
        {
            this.spellRemoveCurseFromAllItems();
        }

        bool ICommandHandler<RemoveCurseFromAllItemsCommand, bool>.Handle(RemoveCurseFromAllItemsCommand command)
        { 
            return this.spellRemoveCurseFromAllItems();
        }

        // Removes curses from items in inventory -RAK-
        private bool spellRemoveCurseFromAllItems()
        {
            var py = State.Instance.py;

            var removed = false;

            for (var id = (int)PlayerEquipment.Wield; id <= (int)PlayerEquipment.Outer; id++)
            {
                if ((py.inventory[id].flags & Config.treasure_flags.TR_CURSED) != 0u)
                {
                    py.inventory[id].flags &= ~Config.treasure_flags.TR_CURSED;
                    Player_m.playerRecalculateBonuses();
                    removed = true;
                }
            }

            return removed;
        }
    }
}