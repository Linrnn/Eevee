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
        internal static void Extents(int extents, string paramName) => Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<string, int>, int>(extents, 0, paramName, "{0}:{1} < 0", new DiagnosisArgs<string, int>(paramName, extents));
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Extents(Fixed64 extents, string paramName) => Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<string, Fixed64>, Fixed64>(extents, 0, paramName, "{0}:{1} < 0", new DiagnosisArgs<string, Fixed64>(paramName, extents));

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Deg0To360(Fixed64 angle, string paramName)
        {
            Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<string, Fixed64>, Fixed64>(angle, Fixed64.Zero, paramName, "{0}:{1} < 0", new DiagnosisArgs<string, Fixed64>(paramName, angle));
            Assert.Less<ArgumentOutOfRangeException, DiagnosisArgs<string, Fixed64>, Fixed64>(angle, Maths.Deg360, paramName, "{0}:{1} >= 360", new DiagnosisArgs<string, Fixed64>(paramName, angle));
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void NotZero(in Vector2D dir)
        {
            Assert.NotEqual<ArgumentException, DiagnosisArgs, Vector2D>(dir, Vector2D.Zero, nameof(dir), "dir is (0, 0)");
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void NotZero(in Vector3D dir)
        {
            Assert.NotEqual<ArgumentException, DiagnosisArgs, Vector3D>(dir, Vector3D.Zero, nameof(dir), "dir is (0, 0, 0)");
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Normal(in Vector2D dir)
        {
            Assert.Equal<ArgumentException, DiagnosisArgs<Vector2D>, Vector2D>(dir, dir.Normalized(), nameof(dir), "dir:{0} isn't normal", new DiagnosisArgs<Vector2D>(dir));
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Normal(in Vector3D dir)
        {
            Assert.Equal<ArgumentException, DiagnosisArgs<Vector3D>, Vector3D>(dir, dir.Normalized(), nameof(dir), "dir:{0} isn't normal", new DiagnosisArgs<Vector3D>(dir));
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Segment(in Vector2D start, in Vector2D end)
        {
            Assert.NotEqual<ArgumentException, DiagnosisArgs<Vector2D>, Vector2D>(start, end, nameof(start), "start == end, value:{0}", new DiagnosisArgs<Vector2D>(end));
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Segment(Vector2DInt start, Vector2DInt end)
        {
            Assert.NotEqual<ArgumentException, DiagnosisArgs<Vector2DInt>, Vector2DInt>(start, end, nameof(start), "start == end, value:{0}", new DiagnosisArgs<Vector2DInt>(end));
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Polygon(int count)
        {
            Assert.GreaterEqual<ArgumentOutOfRangeException, DiagnosisArgs<int>, int>(count, 3, nameof(count), "Polygon must have at least 3 points. but count is {0}.", new DiagnosisArgs<int>(count));
        }
    }
}