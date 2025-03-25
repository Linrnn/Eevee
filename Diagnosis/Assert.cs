using Eevee.Define;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
        internal static void IsNull<TException, TArgs>(object obj, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (obj != null)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsNotNull<TException, TArgs>(object obj, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (obj == null)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        #endregion

        #region true, false
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsTrue<TException, TArgs>(bool condition, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (!condition)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsFalse<TException, TArgs>(bool condition, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (condition)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        #endregion

        #region >, <, >=, <=, ==, !=
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsGreater<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TComparable : IComparable<TComparable>
        {
            if (lhs.CompareTo(rhs) <= 0)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsLess<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TComparable : IComparable<TComparable>
        {
            if (lhs.CompareTo(rhs) >= 0)
                ThrowException<TException, TArgs>(paramName, format, args);
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsGreaterEqual<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TComparable : IComparable<TComparable>
        {
            if (lhs.CompareTo(rhs) < 0)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsLessEqual<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TComparable : IComparable<TComparable>
        {
            if (lhs.CompareTo(rhs) > 0)
                ThrowException<TException, TArgs>(paramName, format, args);
        }

        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsEqual<TException, TArgs, TEquatable>(TEquatable lhs, TEquatable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TEquatable : IEquatable<TEquatable>
        {
            if (!lhs.Equals(rhs))
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void IsNotEqual<TException, TArgs, TEquatable>(TEquatable lhs, TEquatable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TEquatable : IEquatable<TEquatable>
        {
            if (lhs.Equals(rhs))
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        #endregion

        #region Helper
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowException<TException, TArgs>(string paramName, string format, TArgs args)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            string message = args.BuildMessage(format);
            var exception = BuildException(typeof(TException), paramName, message);
            throw exception;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Exception BuildException(Type exception, string paramName, string message) => exception switch
        {
            _ when exception == typeof(ArgumentException) => new ArgumentException(message, paramName),
            _ when exception == typeof(ArgumentNullException) => new ArgumentNullException(paramName, message),
            _ when exception == typeof(ArgumentOutOfRangeException) => new ArgumentOutOfRangeException(paramName, message),
            _ when exception == typeof(NullReferenceException) => new NullReferenceException(message),
            _ when exception == typeof(InvalidOperationException) => new InvalidOperationException(message),
            _ => new Exception(message),
        };
        #endregion
    }
}