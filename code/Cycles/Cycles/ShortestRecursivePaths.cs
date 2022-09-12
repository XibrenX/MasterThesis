using System.Collections;

namespace Cycles
{
    public class ShortestRecursivePaths : IReadOnlyCollection<RecursivePath>
    {
        private readonly Dictionary<PathPattern, RecursivePath> _dict = new();

        public Node StartNode { get; }

        public ShortestRecursivePaths(Node startNode)
        {
            StartNode = startNode;
        }

        public int Count => _dict.Count;

        public void Add(RecursivePathSegment segment)
        {
            if (segment.StartNode != StartNode)
                throw new ArgumentException($"{nameof(segment)}.{nameof(segment.StartNode)} must be equal to {nameof(StartNode)}");

            if (_dict.TryGetValue(segment.Pattern, out var recursivePath))
            {
                recursivePath.Add(segment);
            }
            else
            {
                recursivePath = new RecursivePath(segment);
                _dict.Add(recursivePath.Pattern, recursivePath);
            }
        }

        public IEnumerator<RecursivePath> GetEnumerator() => _dict.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.Values.GetEnumerator();
    }
}
