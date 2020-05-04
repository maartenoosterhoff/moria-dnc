namespace Moria.Core.Methods.Commands.Player
{
    public class DisturbCommand : ICommand
    {
        public DisturbCommand(bool major_disturbance, bool light_disturbance)
        {
            this.MajorDisturbance = major_disturbance;
            this.LightDisturbance = light_disturbance;
        }

        public bool MajorDisturbance { get; }

        public bool LightDisturbance { get; }
    }
}