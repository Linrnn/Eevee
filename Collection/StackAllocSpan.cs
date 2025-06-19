using Eevee.Diagnosis;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Eevee.Collection
{
    /// <summary>
    /// 内存在栈分配的元素集合
    /// </summary>
    internal readonly ref struct StackAllocSpan<T>
    {
        private readonly bool _referenceType;
        private readonly int _scaleSize;
        private readonly Span<byte> _span;

        internal StackAllocSpan(int scaleSize, in Span<byte> span)
        {
            var type = typeof(T);
            _referenceType = type.IsClass || type.IsInterface;
            _scaleSize = scaleSize;
            _span = span;
        }

        internal T Get(ref int index)
        {
            var element = Get(index);
            index += _scaleSize;
            return element;
        }
        internal T Get(int index)
        {
            unsafe
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
            }

            return default;
        }

        internal void Set(ref int index, T element)
        {
            Set(index, element);
            index += _scaleSize;
        }
        internal void Set(int index, T element)
        {
            unsafe
            {
                fixed (void* ptr = &_span[index])
                    if (_referenceType)
                        Unsafe.Write(ptr, GCHandle.Alloc(element, GCHandleType.Pinned));
                    else
                        Unsafe.Write(ptr, element);
            }
        }
    }
}