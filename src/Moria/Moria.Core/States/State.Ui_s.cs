using static Moria.Core.Constants.Ui_c;

namespace Moria.Core.States
{
    public partial class State
    {
        public bool screen_has_changed { get; set; }
        public bool message_ready_to_print { get; set; }
        public string[] messages = new string[MESSAGE_HISTORY_SIZE];
        public int last_message_id { get; set; }
        
        public int eof_flag { get; set; }
        public bool panic_save { get; set; }
    }
}
