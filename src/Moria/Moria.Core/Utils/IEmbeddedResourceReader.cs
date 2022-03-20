namespace Moria.Core.Utils
{
    using System.Reflection;

    public interface IEmbeddedResourceReader
    {
        string ReadResourceAsString(string name);

        string ReadResourceAsString(Assembly assembly, string name);

        byte[] ReadResourceAsBytes(Assembly assembly, string name);
    }
}
