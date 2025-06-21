using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 内存在栈分配的数组<br/>
    /// 不支持以下情况：结构体存在引用类型
    /// </summary>
    internal readonly ref struct StackAllocArray<T>
    {
        private readonly bool _referenceType;
        private readonly int _scale;
        private readonly Span<byte> _span;

        internal StackAllocArray(in Span<byte> span)
        {
            _referenceType = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            _scale = Unsafe.SizeOf<T>();
            _span = span;
        }
        internal StackAllocArray(int scale, in Span<byte> span)
        {
            _referenceType = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            _scale = scale;
            _span = span;
        }
        internal static void GetSize(int length, out int scale, out int capacity)
        {
            int size = Unsafe.SizeOf<T>();
            scale = size;
            capacity = length * size;
        }

        internal T Get(ref int offset)
        {
            var element = Get(offset);
            offset += _scale;
            return element;
        }
        internal unsafe T Get(int offset)
        {
            fixed (void* ptr = &_span[offset])
                if (_referenceType)
                    return (T)Unsafe.Read<GCHandle>(ptr).Target;
                else
                    return Unsafe.Read<T>(ptr);
        }

        internal void Set(ref int offset, T element)
        {
            Set(offset, element);
            offset += _scale;
        }
        internal unsafe void Set(int offset, T element)
        {
            fixed (void* ptr = &_span[offset])
                if (_referenceType)
                    Unsafe.Write(ptr, GCHandle.Alloc(element, GCHandleType.Pinned));
                else
                    Unsafe.Write(ptr, element);
        }

        internal unsafe void Dispose()
        {
            if (_referenceType)
                for (int length = _span.Length, offset = 0; offset < length; offset += _scale)
                    fixed (void* ptr = &_span[offset])
                        if (Unsafe.Read<GCHandle>(ptr) is { IsAllocated: true } handle)
                            handle.Free();
            _span.Clear();
        }
    }
}