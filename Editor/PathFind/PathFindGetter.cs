#if UNITY_EDITOR
using Eevee.PathFind;
using System;
using UnityEngine;
using CollSize = System.SByte;

namespace EeveeEditor.PathFind
{
    public interface IPathFindDrawProxy
    {
        Type GroupTypeEnum { get; } // 地表的枚举类型，null代表int类型
        Type MoveTypeEnum { get; } // 移动的枚举类型，null代表int类型
        Type CollTypeEnum { get; } // 碰撞体积的枚举类型，null代表int类型
        PathFindComponent Component { get; } // 获得寻路组件
        Vector2 MinBoundary { get; } // 左/下边界
        float GridSize { get; } // 寻路尺寸：Unity尺寸

        bool ValidColl(CollSize value);
        Vector2Int? GetCurrentPoint(int index);
        Vector2? GetMoveDirection(int index);
    }

    internal readonly struct PathFindGetter
    {
        private static IPathFindDrawProxy _proxy;
        internal static IPathFindDrawProxy Proxy
        {
            get
            {
                if (_proxy is { } proxy)
                {
                    return proxy;
                }

                var proxyType = FindType.GetType(typeof(IPathFindDrawProxy));
                if (proxyType is null)
                {
                    Debug.LogError("IPathFindDrawProxy 未被继承！");
                    return null;
                }

                var proxyInstance = Activator.CreateInstance(proxyType) as IPathFindDrawProxy;
                _proxy = proxyInstance;
                return proxyInstance;
            }
        }
    }
}
#endif