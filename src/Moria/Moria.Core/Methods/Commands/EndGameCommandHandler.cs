using Moria.Core.Constants;
using Moria.Core.Data;
using Moria.Core.Methods.Commands.SpellCasting;
using Moria.Core.Resources;
using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods.Commands
{
    public class EndGameCommandHandler : ICommandHandler<EndGameCommand>
    {
        private readonly IGame game;
        private readonly IGameSave gameSave;
        private readonly IHelpers helpers;
        private readonly ITerminal terminal;
        private readonly IUiInventory uiInventory;
        private readonly IEventPublisher eventPublisher;

        public EndGameCommandHandler(
            IGame game,
            IGameSave gameSave,
            IHelpers helpers,
            ITerminal terminal,
            IUiInventory uiInventory,

            IEventPublisher eventPublisher
        )
        {
            this.game = game;
            this.gameSave = gameSave;
            this.helpers = helpers;
            this.terminal = terminal;
            this.uiInventory = uiInventory;
            this.eventPublisher = eventPublisher;
        }

        public void Handle(EndGameCommand command)
        {
            this.endGame();
        }

        // What happens upon dying -RAK-
        // Handles the gravestone and top-twenty routines -RAK-
        private void endGame()
        {
            var dg = State.Instance.dg;
            var game = State.Instance.game;

            this.terminal.printMessage(/*CNIL*/null);

            // flush all input
            this.terminal.flushInputBuffer();

            // If the game has been saved, then save sets turn back to -1,
            // which inhibits the printing of the tomb.
            if (dg.game_turn >= 0)
            {
                if (game.total_winner)
                {
                    this.kingly();
                }

                this.printTomb();
            }

            // Save the memory at least.
            if (game.character_generated && !game.character_saved)
            {
                this.gameSave.saveGame();
            }

            // add score to score file if applicable
            if (game.character_generated)
            {
                // Clear `game.character_saved`, strange thing to do, but it prevents
                // getKeyInput() from recursively calling endGame() when there has
                // been an eof on stdin detected.
                game.character_saved = false;
                Scores_m.recordNewHighScore();
                Scores_m.showScoresScreen();
            }
            this.terminal.eraseLine(new Coord_t(23, 0));

            this.game.exitProgram();
        }

        // Change the player into a King! -RAK-
        private void kingly()
        {
            var dg = State.Instance.dg;
            var py = State.Instance.py;

            // Change the character attributes.
            dg.current_level = 0;
            State.Instance.game.character_died_from = "Ripe Old Age";

            this.eventPublisher.Publish(new RestorePlayerLevelsCommand());
            //spellRestorePlayerLevels();

            py.misc.level += Player_c.PLAYER_MAX_LEVEL;
            py.misc.au += 250000;
            py.misc.max_exp += 5000000;
            py.misc.exp = py.misc.max_exp;

            this.printCrown();
        }

        // Let the player know they did good.
        private void printCrown()
        {
            Game_files_m.displayDeathFile(nameof(DataFilesResource.death_royal));
            if (Player_m.playerIsMale())
            {
                this.terminal.putString("King!", new Coord_t(17, 45));
            }
            else
            {
                this.terminal.putString("Queen!", new Coord_t(17, 45));
            }
            this.terminal.flushInputBuffer();
            this.terminal.waitForContinueKey(23);
        }

        // Prints the gravestone of the character -RAK-
        private void printTomb()
        {
            var py = State.Instance.py;
            var game = State.Instance.game;

            Game_files_m.displayDeathFile(nameof(DataFilesResource.death_tomb));

            var text = py.misc.name;
            this.terminal.putString(text, new Coord_t(6, (int)(26 - text.Length / 2)));

            if (!game.total_winner)
            {
                text = Player_m.playerRankTitle();
            }
            else
            {
                text = "Magnificent";
            }
            this.terminal.putString(text, new Coord_t(8, (int)(26 - text.Length / 2)));

            if (!game.total_winner)
            {
                text = Library.Instance.Player.classes[(int)py.misc.class_id].title;
            }
            else if (Player_m.playerIsMale())
            {
                text = "*King*";
            }
            else
            {
                text = "*Queen*";
            }
            this.terminal.putString(text, new Coord_t(10, (int)(26 - text.Length / 2)));

            text = py.misc.level.ToString();
            this.terminal.putString(text, new Coord_t(11, 30));

            text = py.misc.exp + " Exp";
            this.terminal.putString(text, new Coord_t(12, (int)(26 - text.Length / 2)));

            text = py.misc.au + " Au";
            this.terminal.putString(text, new Coord_t(13, (int)(26 - text.Length / 2)));

            text = State.Instance.dg.current_level.ToString();
            this.terminal.putString(text, new Coord_t(14, 34));

            text = game.character_died_from;
            this.terminal.putString(text, new Coord_t(16, (int)(26 - text.Length / 2)));

            this.helpers.humanDateString(out var day);
            text = day;
            this.terminal.putString(text, new Coord_t(17, (int)(26 - text.Length / 2)));

            retry:
            this.terminal.flushInputBuffer();

            this.terminal.putString("(ESC to abort, return to print on screen, or file name)", new Coord_t(23, 0));
            this.terminal.putString("Character record?", new Coord_t(22, 0));

            //vtype_t str = { '\0' };
            if (this.terminal.getStringInput(out var str, new Coord_t(22, 18), 60))
            {
                foreach (var item in State.Instance.py.inventory)
                {
                    Identification_m.itemSetAsIdentified((int)item.category_id, (int)item.sub_category_id);
                    Identification_m.spellItemIdentifyAndRemoveRandomInscription(item);
                }

                Player_m.playerRecalculateBonuses();

                if (str[0] != 0)
                {
                    if (!Game_files_m.outputPlayerCharacterToFile(str))
                    {
                        goto retry;
                    }
                }
                else
                {
                    this.terminal.clearScreen();
                    Ui_m.printCharacter();
                    this.terminal.putString("Type ESC to skip the inventory:", new Coord_t(23, 0));
                    if (this.terminal.getKeyInput() != Ui_c.ESCAPE)
                    {
                        this.terminal.clearScreen();
                        this.terminal.printMessage("You are using:");
                        this.uiInventory.displayEquipment(true, 0);
                        this.terminal.printMessage(/*CNIL*/null);
                        this.terminal.printMessage("You are carrying:");
                        this.terminal.clearToBottom(1);
                        this.uiInventory.displayInventory(0, py.pack.unique_items - 1, true, 0, /*CNIL*/null);
                        this.terminal.printMessage(/*CNIL*/null);
                    }
                }
            }
        }
    }
}