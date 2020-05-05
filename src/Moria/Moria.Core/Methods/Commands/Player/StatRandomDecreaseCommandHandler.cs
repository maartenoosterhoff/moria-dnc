using Moria.Core.States;

namespace Moria.Core.Methods.Commands.Player
{
    public class StatRandomDecreaseCommandHandler :
        ICommandHandler<StatRandomDecreaseCommand>,
        ICommandHandler<StatRandomDecreaseCommand, bool>
    {
        private readonly IRnd rnd;
        private readonly ITerminalEx terminalEx;

        public StatRandomDecreaseCommandHandler(
            IRnd rnd,
            ITerminalEx terminalEx
        )
        {
            this.rnd = rnd;
            this.terminalEx = terminalEx;
        }
        void ICommandHandler<StatRandomDecreaseCommand>.Handle(StatRandomDecreaseCommand command)
        {
            this.playerStatRandomDecrease(command.Stat);
        }

        bool ICommandHandler<StatRandomDecreaseCommand, bool>.Handle(StatRandomDecreaseCommand command)
        {
            return this.playerStatRandomDecrease(command.Stat);
        }

        // Decreases a stat by one randomized level -RAK-
        private bool playerStatRandomDecrease(int stat)
        {
            var py = State.Instance.py;

            var new_stat = (int)py.stats.current[stat];

            if (new_stat <= 3)
            {
                return false;
            }

            if (new_stat >= 19 && new_stat < 117)
            {
                var loss = (((118 - new_stat) >> 1) + 1) >> 1;
                new_stat += -this.rnd.randomNumber(loss) - loss;

                if (new_stat < 18)
                {
                    new_stat = 18;
                }
            }
            else
            {
                new_stat--;
            }

            py.stats.current[stat] = (uint)new_stat;

            Player_stats_m.playerSetAndUseStat(stat);
            this.terminalEx.displayCharacterStats(stat);

            return true;
        }
    }
}