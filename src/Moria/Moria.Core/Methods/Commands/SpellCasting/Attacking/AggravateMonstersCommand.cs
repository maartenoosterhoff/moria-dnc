namespace Moria.Core.Methods.Commands.SpellCasting.Attacking
{
    public class AggravateMonstersCommand : ICommand
    {
        public AggravateMonstersCommand(int affect_distance)
        {
            this.AffectDistance = affect_distance;
        }

        public int AffectDistance { get; }
    }
}