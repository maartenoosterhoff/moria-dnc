using static Moria.Core.Constants.Game_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public uint[] objects_identified { get; } = new uint[OBJECT_IDENT_SIZE];
    }
}