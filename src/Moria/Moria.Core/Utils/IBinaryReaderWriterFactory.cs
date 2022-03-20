using System.IO;

namespace Moria.Core.Utils
{
    public interface IBinaryReaderWriterFactory
    {
        IBinaryReader CreateBinaryReader(Stream stream);

        IBinaryWriter CreateBinaryWriter(Stream stream);
    }
}
