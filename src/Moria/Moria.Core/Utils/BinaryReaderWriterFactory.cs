using System.IO;

namespace Moria.Core.Utils
{
    public class BinaryReaderWriterFactory : IBinaryReaderWriterFactory
    {
        public IBinaryReader CreateBinaryReader(Stream stream) => new BinaryReaderWrapper(stream);

        public IBinaryWriter CreateBinaryWriter(Stream stream) => new BinaryWriterWrapper(stream);
    }
}