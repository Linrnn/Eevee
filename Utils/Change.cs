using Eevee.Define;
using System;

namespace Eevee.Utils
{
    public readonly struct Change<T> where T : IEquatable<T>, IFormattable
    {
        public readonly T Pre; // 旧值
        public readonly T Tar; // 新值

        public Change(in T pre, in T tar)
        {
            Pre = pre;
            Tar = tar;
        }
        public bool Equals() => Pre.Equals(Tar);

        public override string ToString() => ToString(Format.Fractional, Format.Use);
        public string ToString(string format) => ToString(format, Format.Use);
        public string ToString(IFormatProvider provider) => ToString(Format.Fractional, provider);
        public string ToString(string format, IFormatProvider provider) => $"Pre:{Pre.ToString(format, provider)}, Tar:{Tar.ToString(format, provider)}";
    }
}