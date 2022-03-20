using System;
using System.IO;
using System.Reflection;

namespace Moria.Core.Utils
{
    public class EmbeddedResourceReader : IEmbeddedResourceReader
    {
        public string ReadResourceAsString(string name)
        {
            return this.ReadResourceAsString(Assembly.GetCallingAssembly(), name);
        }

        public string ReadResourceAsString(Assembly assembly, string name)
        {
            var embeddedResourceStream = assembly.GetManifestResourceStream(name);
            if (embeddedResourceStream == null)
            {
                throw new InvalidOperationException($"Embedded resource {name} was not found.");
            }

            // ReSharper disable once AssignNullToNotNullAttribute, true but this is verified in a unittest.
            using (var streamReader = new StreamReader(embeddedResourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public byte[] ReadResourceAsBytes(Assembly assembly, string name)
        {
            var embeddedResourceStream = assembly.GetManifestResourceStream(name);
            if (embeddedResourceStream == null)
            {
                throw new InvalidOperationException($"Embedded resource {name} was not found.");
            }

            // ReSharper disable once AssignNullToNotNullAttribute, true but this is verified in a unittest.
            using (embeddedResourceStream)
            {
                var bytes = new byte[embeddedResourceStream.Length];
                embeddedResourceStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }
    }
}
