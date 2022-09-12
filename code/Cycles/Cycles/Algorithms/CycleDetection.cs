using Cycles;
using System.Diagnostics;

namespace Cycles.Algorithms
{
    public static class CycleDetection
    {
        public static Dictionary<PathPattern, List<RecursiveCycle>> GroupCycles(Stopwatch stopwatch, List<RecursiveCycle> cycles)
        {
            stopwatch.Restart();

            var dict = cycles.GroupBy(p => p.Pattern).ToDictionary(g => g.Key, g => g.ToList());

            stopwatch.Stop();

            return dict;
        }

        public static void OrderCycles(Stopwatch stopwatch, List<RecursiveCycle> cycles)
        {
            stopwatch.Restart();

            foreach (var cycle in cycles)
            {
                if (cycle.Pattern > cycle.Pattern.Inverse())
                {
                    cycle.Mirror();
                }
            }

            stopwatch.Stop();
        }

        public static List<RecursiveCycle> PerformBFSCycleDetection(Stopwatch stopwatch, Dictionary<long, Node> nodes_dir, CycleFilter p)
        {
            stopwatch.Restart();

            var cycles = new List<RecursiveCycle>();

            void AddCycle(RecursiveCycle cycle)
            {
                if (cycle.PathA != cycle.PathB)
                {
                    var rA = cycle.PathA.Segments.Select(s => s.Relationship.Id).ToHashSet();
                    var rB = cycle.PathB.Segments.Select(s => s.Relationship.Id).ToHashSet();
                    var c = new HashSet<long>(rA);

                    c.IntersectWith(rB);
                    if (c.Count == 1 && rA.Count == 1 && rB.Count == 1)
                    {
                        //Cycle uses same way back and should not exists, and their are not other paths that could make it valid
                        return;
                    }
                }

                if (p.IsAllowedCycle(cycle))
                {
                    cycles.Add(cycle);
                }
            }

            foreach (var node in nodes_dir.Values.OrderBy(n => n.BFSInfo.Distance))
            {
                if (node.Id == 14330547)
                {

                }

                var bfs = node.BFSInfo;
                bfs.SetVisited();

                foreach (var r in node.Relations)
                {
                    var pr = r.GetPathRelationship(node);
                    var otherNode = pr.RightNode;
                    var otherBfs = otherNode.BFSInfo;

                    if (otherBfs.Visisted && otherBfs.Distance < bfs.Distance)
                    {
                        if (otherBfs.ShortestRecursivePaths.Count == 0 && otherBfs.Distance == 0)
                        {
                            bfs.ShortestRecursivePaths.Add(new RecursivePathSegment(pr));
                        }

                        foreach (var ShortestRecursivePaths in otherBfs.ShortestRecursivePaths)
                        {
                            bfs.ShortestRecursivePaths.Add(new RecursivePathSegment(pr, ShortestRecursivePaths));
                        }
                    }
                }

                foreach (var recursiveShortestPath in bfs.ShortestRecursivePaths)
                {
                    if (recursiveShortestPath.Segments.Count > 1)
                    {
                        AddCycle(new MirroredRecursiveCycle(recursiveShortestPath));
                    }
                }

                if (bfs.ShortestRecursivePaths.Count > 1)
                {
                    var srp = bfs.ShortestRecursivePaths.ToList();
                    for (var i = srp.Count - 1; i >= 0; i--)
                    {
                        var rPathA = srp[i];
                        srp.RemoveAt(i);
                        foreach (var rPathB in srp)
                        {
                            AddCycle(new RecursiveCycle(rPathA, rPathB));
                        }
                    }
                }

                var srp_o = new ShortestRecursivePaths(node);
                foreach (var r in node.Relations)
                {
                    var pr = r.GetPathRelationship(node);
                    var otherNode = pr.RightNode;
                    var otherBfs = otherNode.BFSInfo;

                    if (otherBfs.Visisted && otherBfs.Distance == bfs.Distance)
                    {
                        foreach (var ShortestRecursivePaths in otherBfs.ShortestRecursivePaths)
                        {
                            srp_o.Add(new RecursivePathSegment(pr, ShortestRecursivePaths));
                        }
                    }
                }

                foreach (var rPathA in srp_o)
                {
                    foreach (var rPathB in bfs.ShortestRecursivePaths)
                    {
                        AddCycle(new RecursiveCycle(rPathA, rPathB));
                    }
                }
            }

            stopwatch.Stop();
            return cycles;
        }
    }
}