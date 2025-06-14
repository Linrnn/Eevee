namespace Eevee.Pool
{
    public interface IObjectPool<T> where T : class
    {
        T Alloc();
        void Release(T element);
    }
}