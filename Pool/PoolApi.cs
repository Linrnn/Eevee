namespace Eevee.Pool
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPool<T> where T : class
    {
        T Alloc();
        void Release(T element);
    }

    public interface IRecyclable
    {
        /// <summary>
        /// 回收到对象池
        /// </summary>
        void Recycle();
    }

    #region “ObjectInterPool”依赖的接口
    public interface IObjectCreate
    {
        /// <summary>
        /// 创建后，初始化数据
        /// </summary>
        void OnCreate();
    }

    public interface IObjectAlloc
    {
        /// <summary>
        /// 初始化数据
        /// </summary>
        void OnAlloc();
    }

    public interface IObjectRelease
    {
        /// <summary>
        /// 回归后，释放数据
        /// </summary>
        void OnRelease();
    }

    public interface IObjectDestroy
    {
        /// <summary>
        /// 销毁数据
        /// </summary>
        void OnDestroy();
    }
    #endregion
}