using Eevee.Define;
using Eevee.Diagnosis;
using System;
using System.Diagnostics;

namespace Eevee.Fixed
{
    internal readonly struct Check
    {
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Extents(int extents, string paramName) => Assert.GreaterEqual<ArgumentOutOfRangeException, AssertArgs<string, int>, int>(extents, 0, paramName, "{0}:{1} < 0", new AssertArgs<string, int>(paramName, extents));
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Extents(Fixed64 extents, string paramName) => Assert.GreaterEqual<ArgumentOutOfRangeException, AssertArgs<string, Fixed64>, Fixed64>(extents, 0, paramName, "{0}:{1} < 0", new AssertArgs<string, Fixed64>(paramName, extents));

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Deg0To360(Fixed64 angle, string paramName)
        {
            Assert.GreaterEqual<ArgumentOutOfRangeException, AssertArgs<string, Fixed64>, Fixed64>(angle, Fixed64.Zero, paramName, "{0}:{1} < 0", new AssertArgs<string, Fixed64>(paramName, angle));
            Assert.Less<ArgumentOutOfRangeException, AssertArgs<string, Fixed64>, Fixed64>(angle, Maths.Deg360, paramName, "{0}:{1} >= 360", new AssertArgs<string, Fixed64>(paramName, angle));
        }
    }
}