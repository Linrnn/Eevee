namespace Eevee.QuadTree
{
    /// <summary>
    /// 预处理缓存
    /// </summary>
    public readonly struct QuadPreCache
    {
        public readonly QuadElement PreEle;
        public readonly QuadElement TarEle;
        public readonly QuadNode PreNode;
        public readonly QuadNode TarNode;
        public readonly int PreIndex;
        public readonly int TreeId;

        public QuadPreCache(in QuadElement preEle, in QuadElement tarEle, QuadNode preNode, QuadNode tarNode, int preIndex, int treeId)
        {
            PreEle = preEle;
            TarEle = tarEle;
            PreNode = preNode;
            TarNode = tarNode;
            PreIndex = preIndex;
            TreeId = treeId;
        }
    }
}