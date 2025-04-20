namespace Eevee.Pool
{
    public interface IObjectCreate
    {
        /// <summary>
        /// 创建后，初始化数据
        /// </summary>
        void OnCreate();
    }
}