using System;
using System.Linq;

namespace Moria.Core.Utils
{
    public static class ArrayInitializer
    {
        public static T[] Initialize<T>(uint size) where T : new()
        {
            return Enumerable.Range(0, (int)size).Select(x => new T()).ToArray();
        }

        public static T[][] Initialize<T>(uint size1, uint size2) where T : new()
        {
            return Enumerable
                .Range(0, (int)size1)
                .Select(x => Enumerable
                    .Range(0, (int)size2)
                    .Select(y => new T())
                    .ToArray())
                .ToArray();
        }

        public static T[] InitializeWithDefault<T>(uint size, T defaultValue) =>
            Enumerable.Range(0, (int)size).Select(x => defaultValue).ToArray();

        public static T[][] InitializeWithDefault<T>(uint size1, uint size2) =>
            InitializeWithDefault<T>(size1, size2, default(T));

        public static T[][] InitializeWithDefault<T>(uint size1, uint size2, T defaultValue)
            => InitializeWithDefault(size1, size2, () => defaultValue);

        public static T[][] InitializeWithDefault<T>(uint size1, uint size2, Func<T> defaultValue) =>
            Enumerable
                .Range(0, (int)size1)
                .Select(x => Enumerable
                    .Range(0, (int)size2)
                    .Select(y => defaultValue())
                    .ToArray())
                .ToArray();
    }
}
