using Neo4j.Driver;
using Neo4jClient;

namespace Cycles.Old
{
    public class NodeRelationsCache
    {
        private readonly IAsyncSession _session;
        private readonly Dictionary<long, List<CacheEntity>> _cache;

        public NodeRelationsCache(IAsyncSession session)
        {
            _session = session;
            _cache = new();
        }

        public async Task<long> GetRelationshipCount(long nodeId, string type, RelationshipDirection direction)
        {
            var cacheEntities = await GetCacheEntities(nodeId);
            var cacheEntity = cacheEntities.FirstOrDefault(x => x.Type == type && x.IsOut == (direction == RelationshipDirection.Outgoing));
            return cacheEntity?.Count ?? 0;
        }

        private async Task<List<CacheEntity>> GetCacheEntities(long nodeId)
        {
            if (!_cache.TryGetValue(nodeId, out var cacheEntities))
            {
                //"type(r) as type, (startNode(r) = n) as isOut, count(*) as count"
                var cursor = await _session.RunAsync("MATCH (n)-[r]-() WHERE ID(n) = $nodeId RETURN type(r), startNode(r) = n, count(*)", new Dictionary<string, object>() { ["nodeId"] = nodeId });
                cacheEntities = new List<CacheEntity>();
                var records = await cursor.ForEachAsync((record) =>
                {
                    cacheEntities.Add(new CacheEntity()
                    {
                        Type = (string)record["type(r)"],
                        IsOut = (bool)record["startNode(r) = n"],
                        Count = (long)record["count(*)"],
                    });
                });

                _cache.Add(nodeId, cacheEntities);
            }

            return cacheEntities;
        }
    }

    public class CacheEntity
    {
        public string Type { get; set; } = string.Empty;
        public bool IsOut { get; set; }
        public long Count { get; set; }
    }
}
