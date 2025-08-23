using Eevee.Diagnosis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Eevee.Utils
{
    public readonly struct SubString : IEquatable<SubString>, IComparable<SubString>, IReadOnlyList<char>
    {
        #region 类型
        public struct Enumerator : IEnumerator<char>
        {
            private readonly SubString _enumerator;
            private int _index;
            private char _current;

            internal Enumerator(in SubString enumerator)
            {
                _enumerator = enumerator;
                _index = 0;
                _current = default;
            }

            #region IEnumerator`1
            public readonly char Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
            #endregion

            #region IEnumerator
            readonly object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _enumerator.Length)
                {
                    _index = _enumerator.Length + 1;
                    _current = default;
                    return false;
                }

                _current = _enumerator[_index];
                ++_index;
                return true;
            }
            public void Reset() => Dispose();
            #endregion

            #region IDisposable
            public void Dispose()
            {
                _index = 0;
                _current = default;
            }
            #endregion
        }
        #endregion

        #region 字段/构造函数
        public readonly object Obj;
        public readonly int Start;
        public readonly int Length;

        public SubString(string str)
        {
            Obj = str;
            Start = 0;
            Length = str.Length;
        }
        public SubString(string str, int start)
        {
            Obj = str;
            Start = start;
            Length = str.Length - start;
            Assert.LessEqual<IndexOutOfRangeException, DiagnosisArgs<string, int, int>, int>(start, str.Length, nameof(str), "Sub异常长度！str：{0}，start：{1}，totalLength：{2}", new DiagnosisArgs<string, int, int>(str, start, str.Length));
        }
        public SubString(string str, int start, int length)
        {
            Obj = str;
            Start = start;
            Length = length;
            Assert.LessEqual<IndexOutOfRangeException, DiagnosisArgs<string, int, int, int>, int>(start + length, str.Length, nameof(str), "Sub异常长度！str：{0}，start：{1}，length：{2}，totalLength：{3}", new DiagnosisArgs<string, int, int, int>(str, start, length, str.Length));
        }
        public SubString(StringBuilder sb)
        {
            Obj = sb;
            Start = 0;
            Length = sb.Length;
        }
        public SubString(StringBuilder sb, int start)
        {
            Obj = sb;
            Start = start;
            Length = sb.Length - start;
            Assert.LessEqual<IndexOutOfRangeException, DiagnosisArgs<StringBuilder, int, int>, int>(start, sb.Length, nameof(sb), "Sub异常长度！sb：{0}，start：{1}，totalLength：{2}", new DiagnosisArgs<StringBuilder, int, int>(sb, start, sb.Length));
        }
        public SubString(StringBuilder sb, int start, int length)
        {
            Obj = sb;
            Start = start;
            Length = length;
            Assert.LessEqual<IndexOutOfRangeException, DiagnosisArgs<StringBuilder, int, int, int>, int>(start + length, sb.Length, nameof(sb), "Sub异常长度！str：{0}，start：{1}，length：{2}，totalLength：{3}", new DiagnosisArgs<StringBuilder, int, int, int>(sb, start, length, sb.Length));
        }
        public SubString(in SubString sub, int start, int length)
        {
            Obj = sub.Obj;
            Start = start;
            Length = length;
            Assert.LessEqual<IndexOutOfRangeException, DiagnosisArgs<object, int, int, int>, int>(start + length, sub.GetTotalLength(), nameof(sub), "Sub异常长度！str：{0}，start：{1}，length：{2}，totalLength：{3}", new DiagnosisArgs<object, int, int, int>(sub.Obj, start, length, sub.GetTotalLength()));
        }
        #endregion

        #region 基础方法
        public char this[int index]
        {
            get
            {
                Assert.Range<IndexOutOfRangeException, DiagnosisArgs<object, int, int, int>, int>(index, 0, Length - 1, nameof(index), "Obj:{0}, index:{1}, start:{2}, length:{3}, Index Out!", new DiagnosisArgs<object, int, int, int>(Obj, index, Start, Length));
                return Obj switch
                {
                    string str => str[Start + index],
                    StringBuilder sb => sb[Start + index],
                    _ => throw new ArgumentException($"Obj is {Obj?.GetType().FullName}", nameof(Obj)),
                };
            }
        }
        public int Count => GetTotalLength();

        public bool IsNullOrEmpty() => Obj is null || Length == 0;
        public int GetTotalLength() => Obj switch
        {
            string str => str.Length,
            StringBuilder sb => sb.Length,
            _ => 0,
        };

        public bool TryParse(out int result) => TryParse(in this, 0, Length, out result);
        public bool TryParse(out float result) => TryParse(in this, 0, Length, out result);
        public static bool TryParse(in SubString sub, int start, int count, out int result)
        {
            result = 0;

            int end = start + count - 1;
            while (start < end && char.IsWhiteSpace(sub[start])) // 跳过前导空格
                ++start;
            while (end >= start && char.IsWhiteSpace(sub[end])) // 跳过尾部空格
                --end;
            if (start > end) // 如果全是空格，返回 false
                return false;

            int i = start;
            if (sub[start] is '+' or '-') // 跳过正负号
                ++i;

            int integerPart = 0;
            for (; i <= end; ++i)
            {
                char ch = sub[i];
                if (char.IsNumber(ch)) // 处理数字
                {
                    int digit = ch - '0';
                    integerPart = integerPart * 10 + digit;
                }
                else
                {
                    return false; // 非法字符
                }
            }

            result = sub[start] == '-' ? -integerPart : integerPart;
            return true;
        }
        public static bool TryParse(in SubString sub, int start, int count, out float result)
        {
            result = 0;

            int end = start + count - 1;
            while (start < end && char.IsWhiteSpace(sub[start])) // 跳过前导空格
                ++start;
            while (end >= start && char.IsWhiteSpace(sub[end])) // 跳过尾部空格
                --end;
            if (start > end) // 如果全是空格，返回 false
                return false;

            int i = start;
            if (sub[start] is '+' or '-') // 跳过正负号
                ++i;

            bool hasFractional = false;
            int integerPart = 0;
            int fractionalPart = 0;
            int fractionalDivisor = 1;
            for (; i <= end; ++i)
            {
                char ch = sub[i];
                if (char.IsNumber(ch)) // 处理数字
                {
                    int digit = ch - '0';
                    if (hasFractional)
                    {
                        fractionalPart = fractionalPart * 10 + digit;
                        fractionalDivisor *= 10;
                    }
                    else
                    {
                        integerPart = integerPart * 10 + digit;
                    }
                }
                else if (ch == '.' && !hasFractional) // 处理小数点
                {
                    hasFractional = true;
                }
                else
                {
                    return false; // 非法字符
                }
            }

            if (sub[start] == '-')
                result = -integerPart - fractionalPart / (float)fractionalDivisor;
            else
                result = integerPart + fractionalPart / (float)fractionalDivisor;
            return true;
        }

        public bool BeContains(in SubString other) => Contains(in other, in this);
        public bool Contains(in SubString other) => Contains(in this, in other);

        public void Split(char separator, ICollection<SubString> splits)
        {
            for (int start = 0, i = 0; i < Length; ++i)
            {
                char ch = this[i];
                if (ch == separator)
                {
                    if (this[i > 0 ? i - 1 : 0] == separator)
                        continue;

                    splits.Add(new SubString(in this, start, i - start));
                    start = i + 1;
                }
                else if (i == Length - 1) // 结尾
                {
                    splits.Add(new SubString(in this, start, Length - start));
                }
            }
        }
        public void Split(string separator, ICollection<SubString> splits)
        {
            if (IsNullOrEmpty())
            {
                return;
            }

            if (string.IsNullOrEmpty(separator))
            {
                splits.Add(this);
                return;
            }

            int start = 0;
            for (int i = 0; i <= Length - separator.Length; ++i)
            {
                bool match = true;
                for (int j = 0; j < separator.Length; ++j)
                {
                    if (this[i + j] != separator[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    splits.Add(new SubString(in this, start, i - start));
                    start = i + separator.Length;
                    i += separator.Length - 1;
                }
            }

            if (start < Length)
            {
                splits.Add(new SubString(in this, start, Length - start));
            }
        }
        #endregion

        #region 迭代器
        public Enumerator GetEnumerator() => new(in this);
        IEnumerator<char> IEnumerable<char>.GetEnumerator() => new Enumerator(in this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(in this);
        #endregion

        #region 隐式转换/显示转换/运算符重载
        public static implicit operator SubString(string value) => new(value);
        public static implicit operator SubString(StringBuilder value) => new(value);

        public static bool operator ==(in SubString lhs, in SubString rhs)
        {
            if (lhs.Length != rhs.Length)
                return false;

            for (int length = Math.Min(lhs.Length, rhs.Length), i = 0; i < length; ++i)
                if (lhs[i] != rhs[i])
                    return false;

            return true;
        }
        public static bool operator !=(in SubString lhs, in SubString rhs)
        {
            if (lhs.Length != rhs.Length)
                return true;

            for (int length = Math.Min(lhs.Length, rhs.Length), i = 0; i < length; ++i)
                if (lhs[i] != rhs[i])
                    return true;

            return false;
        }
        #endregion

        #region 继承/重载
        public override bool Equals(object obj) => Obj switch
        {
            string str => this == str,
            StringBuilder sb => this == sb,
            SubString sub => this == sub,
            _ => throw new ArgumentException($"Obj is {Obj?.GetType().FullName}", nameof(Obj)),
        };
        public override int GetHashCode()
        {
            int hashCode = 0;

            switch (Obj)
            {
                case string:
                case StringBuilder:
                    for (int i = 0; i < Length; ++i)
                        hashCode = hashCode * 37 + this[i];
                    break;
            }

            return hashCode;
        }
        public override string ToString() => Obj switch
        {
            string str => str.Substring(Start, Length),
            StringBuilder sb => sb.ToString(Start, Length),
            _ => throw new ArgumentException($"Obj is {Obj?.GetType().FullName}", nameof(Obj)),
        };

        public bool Equals(SubString other) => this == other;
        public int CompareTo(SubString other)
        {
            for (int length = Math.Min(Length, other.Length), i = 0; i < length; ++i)
                if (this[i].CompareTo(other[i]) is var match && match != 0)
                    return match;

            return Length.CompareTo(other.Length);
        }
        #endregion

        #region 私有方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Contains(in SubString mainStr, in SubString subStr)
        {
            int mainLength = mainStr.Length;
            int subLength = subStr.Length;

            if (subLength == 0)
                return true;

            if (mainLength < subLength)
                return false;

            for (int checkLength = mainLength - subLength, i = 0; i <= checkLength; ++i)
            {
                int j;

                for (j = 0; j < subLength; ++j)
                    if (mainStr[i + j] != subStr[j])
                        break;

                if (j == subLength)
                    return true;
            }

            return false;
        }
        #endregion
    }
}