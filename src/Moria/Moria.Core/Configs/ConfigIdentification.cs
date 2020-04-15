namespace Moria.Core.Configs
{
    public class ConfigIdentification
    {
        // id's used for object description, stored in objects_identified array
        public uint OD_TRIED { get; set; } = 0x1;
        public uint OD_KNOWN1 { get; set; } = 0x2;

        // id's used for item description, stored in i_ptr->ident
        public uint ID_MAGIK { get; set; } = 0x1;
        public uint ID_DAMD { get; set; } = 0x2;
        public uint ID_EMPTY { get; set; } = 0x4;
        public uint ID_KNOWN2 { get; set; } = 0x8;
        public uint ID_STORE_BOUGHT { get; set; } = 0x10;
        public uint ID_SHOW_HIT_DAM { get; set; } = 0x20;
        public uint ID_NO_SHOW_P1 { get; set; } = 0x40;
        public uint ID_SHOW_P1 { get; set; } = 0x80;
    }
}