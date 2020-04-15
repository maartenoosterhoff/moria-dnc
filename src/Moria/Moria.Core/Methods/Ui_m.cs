using Moria.Core.States;
using Moria.Core.Structures;

namespace Moria.Core.Methods
{
    public static class Ui_m
    {
        // Is the given coordinate within the screen panel boundaries -RAK-
        public static bool coordInsidePanel(Coord_t coord)
        {
            var dg = State.Instance.dg;
            var valid_y = coord.y >= dg.panel.top && coord.y <= dg.panel.bottom;
            var valid_x = coord.x >= dg.panel.left && coord.x <= dg.panel.right;

            return valid_y && valid_x;
        }
    }
}
