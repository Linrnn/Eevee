namespace Eevee.Pool
{
    public interface IRecyclable
    {
        /// <summary>
        /// 回收到对象池
        /// </summary>
        void Recycle();
    }
}