using Eevee.Define;
using System;
using System.Diagnostics;

namespace Eevee.Diagnosis
{
    /// <summary>
    /// 断言
    /// </summary>
    internal readonly struct Assert
    {
        #region null, not null
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsNull<T0, T1, T2>(object obj, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            if (obj != null)
                throw new Exception($"obj:{obj}\n{string.Format(message, arg0, arg1, arg2)}");
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsNotNull<T0, T1, T2>(object obj, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            if (obj == null)
                throw new Exception(string.Format(message, arg0, arg1, arg2));
        }
        #endregion

        #region true, false
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsTrue<T0, T1, T2>(bool condition, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            if (!condition)
                throw new Exception(string.Format(message, arg0, arg1, arg2));
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsFalse<T0, T1, T2>(bool condition, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            if (condition)
                throw new Exception(string.Format(message, arg0, arg1, arg2));
        }
        #endregion

        #region >, <, >=, <=, ==, !=
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsGreater<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IComparable<T>
        {
            if (lhs.CompareTo(rhs) <= 0)
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsLess<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IComparable<T>
        {
            if (lhs.CompareTo(rhs) >= 0)
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsGreaterEqual<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IComparable<T>
        {
            if (lhs.CompareTo(rhs) < 0)
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsLessEqual<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IComparable<T>
        {
            if (lhs.CompareTo(rhs) > 0)
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsEqual<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IEquatable<T>
        {
            if (!lhs.Equals(rhs))
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsNotEqual<T, T0, T1, T2>(T lhs, T rhs, string message, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default) where T : IEquatable<T>
        {
            if (lhs.Equals(rhs))
                throw new Exception($"lhs:{lhs}, rhs:{rhs}\n{string.Format(message, arg0, arg1, arg2)}");
        }
        #endregion
    }
}