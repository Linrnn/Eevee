﻿using Eevee.Define;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Eevee.Diagnosis
{
    /// <summary>
    /// 0GC断言
    /// </summary>
    internal readonly struct Assert
    {
        #region null, not null
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Null<TException, TArgs>(object obj, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (obj != null)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void NotNull<TException, TArgs>(object obj, string paramName, string format, TArgs args = default)
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
        internal static void True<TException, TArgs>(bool condition, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (!condition)
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void False<TException, TArgs>(bool condition, string paramName, string format, TArgs args = default)
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
        internal static void Greater<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
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
        internal static void Less<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
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
        internal static void GreaterEqual<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
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
        internal static void LessEqual<TException, TArgs, TComparable>(TComparable lhs, TComparable rhs, string paramName, string format, TArgs args = default)
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
        internal static void Equal<TException, TArgs, TEquatable>(TEquatable lhs, TEquatable rhs, string paramName, string format, TArgs args = default)
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
        internal static void NotEqual<TException, TArgs, TEquatable>(TEquatable lhs, TEquatable rhs, string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
            where TEquatable : IEquatable<TEquatable>
        {
            if (lhs.Equals(rhs))
                ThrowException<TException, TArgs>(paramName, format, args);
        }
        #endregion

        #region (T), as T, is T
        [Conditional(Macro.Debug)]
        [Conditional(Macro.Editor)]
        [Conditional(Macro.Assert)]
        internal static void Convert<TException, TArgs, TConvert>(object obj , string paramName, string format, TArgs args = default)
            where TException : Exception
            where TArgs : struct, IAssertArgs
        {
            if (obj == null && typeof(TConvert).IsValueType)
                ThrowException<TException, TArgs>(paramName, format, args);
            if (obj != null && obj is not TConvert)
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