namespace Moria.Core.Utils
{
    public interface IBinaryReader
    {
        bool IsEof();

        void Close();

        byte ReadByte();
    }
}