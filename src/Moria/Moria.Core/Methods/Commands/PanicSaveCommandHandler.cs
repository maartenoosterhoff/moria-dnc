using Moria.Core.States;

namespace Moria.Core.Methods.Commands
{
    public class PanicSaveCommandHandler : ICommandHandler<PanicSaveCommand>
    {
        private readonly IGameSave gameSave;
        private readonly IEventPublisher eventPublisher;

        public PanicSaveCommandHandler(
            IGameSave gameSave,
            IEventPublisher eventPublisher
        )
        {
            this.gameSave = gameSave;
            this.eventPublisher = eventPublisher;
        }

        public void Handle(PanicSaveCommand command)
        {
            // just in case, to make sure that the process eventually dies
            State.Instance.panic_save = true;

            State.Instance.game.character_died_from = "(end of input: panic saved)";
            //(void)strcpy(game.character_died_from, "(end of input: panic saved)");
            if (!this.gameSave.saveGame())
            {
                State.Instance.game.character_died_from = "panic: unexpected eof";
                //(void)strcpy(game.character_died_from, "panic: unexpected eof");
                State.Instance.game.character_is_dead = true;
            }

            this.eventPublisher.Publish(new EndGameCommand());
            //endGame();
        }
    }
}