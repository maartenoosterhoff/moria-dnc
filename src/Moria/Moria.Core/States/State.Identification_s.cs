using Moria.Core.Structures.Enumerations;
using static Moria.Core.Constants.Identification_c;
using static Moria.Core.Constants.Game_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public uint[] objects_identified { get; set; } = new uint[OBJECT_IDENT_SIZE];

        //// Following are arrays for descriptive pieces
        //public string[] colors = new string[MAX_COLORS];
        //public string[] mushrooms = new string[MAX_MUSHROOMS];
        //public string[] woods = new string[MAX_WOODS];
        //public string[] metals = new string[MAX_METALS];
        //public string[] rocks = new string[MAX_ROCKS];
        //public string[] amulets = new string[MAX_AMULETS];

        //public string[] magic_item_titles = new string[MAX_TITLES];
    }
}