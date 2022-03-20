using System.IO;

namespace Moria.Core.Utils
{
    public class BinaryWriterWrapper : IBinaryWriter
    {
        private readonly BinaryWriter binaryWriter;

        public BinaryWriterWrapper(Stream stream)
        {
            this.binaryWriter = new BinaryWriter(stream);
        }

        public void Write(byte value) => this.binaryWriter.Write(value);
    }
}