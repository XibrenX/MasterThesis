namespace Cycles.Algorithms
{
    public class BFSNodeInfo
    {
        public uint Distance { get; private set; } = uint.MaxValue;
        public bool Visisted { get; private set; } = false;

        public ShortestRecursivePaths ShortestRecursivePaths { get; }

        public BFSNodeInfo(Node node)
        {
            ShortestRecursivePaths = new(node);
        }

        public void SetDistance(uint value)
        {
            Distance = Math.Min(value, Distance);
        }

        public void SetVisited()
        {
            Visisted = true;
        }
    }
}
