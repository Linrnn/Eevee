namespace Eevee.Pool
{
    public interface IObjectRelease
    {
        /// <summary>
        /// 回归后，释放数据
        /// </summary>
        void OnRelease();
    }
}