using System.IO;

namespace Moria.Core.Utils
{
    public class BinaryReaderWrapper : IBinaryReader
    {
        private readonly BinaryReader binaryReader;

        public BinaryReaderWrapper(Stream stream)
        {
            this.binaryReader = new BinaryReader(stream);
        }

        public bool IsEof() => this.binaryReader.BaseStream.Position == this.binaryReader.BaseStream.Length;

        public void Close() => this.binaryReader.Close();

        public byte ReadByte() => this.binaryReader.ReadByte();
    }
}