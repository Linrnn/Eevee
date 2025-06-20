using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 内存在栈分配的元素集合<br/>
    /// 不支持以下情况：结构体存在引用类型
    /// </summary>
    internal readonly ref struct StackAllocSpan<T>
    {
        private readonly bool _referenceType;
        private readonly int _size;
        private readonly Span<byte> _span;

        internal StackAllocSpan(int size, in Span<byte> span)
        {
            var type = typeof(T);
            _referenceType = type.IsClass || type.IsInterface;
            _size = size;
            _span = span;
        }

        internal T Get(ref int index)
        {
            var element = Get(index);
            index += _size;
            return element;
        }
        internal unsafe T Get(int index)
        {
            fixed (void* ptr = &_span[index])
            {
                if (_referenceType)
                {
                    var handle = Unsafe.Read<GCHandle>(ptr);
                    try
                    {
                        return (T)handle.Target;
                    }
                    catch (Exception exception)
                    {
                        LogRelay.Fail(exception);
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                else
                {
                    return Unsafe.Read<T>(ptr);
                }
            }

            return default;
        }

        internal void Set(ref int index, T element)
        {
            Set(index, element);
            index += _size;
        }
        internal unsafe void Set(int index, T element)
        {
            fixed (void* ptr = &_span[index])
                if (_referenceType)
                    Unsafe.Write(ptr, GCHandle.Alloc(element, GCHandleType.Pinned));
                else
                    Unsafe.Write(ptr, element);
        }
    }
}