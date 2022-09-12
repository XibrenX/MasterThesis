using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cycles.Old
{
    public static class Extensions
    {
        public static char IncrAfter(this ref char c)
        {
            char old = c;
            c = (char)(Convert.ToUInt16(c) + 1);
            return old;
        }

        public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }

        public static IEnumerable<T> Intertwine<T>(params IEnumerable<T>[] set)
        {
            var es = set.Select(s => s.GetEnumerator()).ToList();
            while (es.Count > 0)
            {
                for (int i = 0; i < es.Count; i++)
                {
                    if (es[i].MoveNext())
                        yield return es[i].Current;
                    else
                        es.RemoveAt(i);
                }
            }
        }

        public static IEnumerable<T> FullOuterJoin<T>(HashSet<T> a, HashSet<T> b)
        {
            foreach (var node in a)
            {
                if (!b.Contains(node))
                    yield return node;
            }

            foreach (var node in b)
            {
                if (!a.Contains(node))
                    yield return node;
            }
        }
    }

    public class SequenceEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public static readonly SequenceEqualityComparer<T> Default = new SequenceEqualityComparer<T>();

        public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
        {
            return ReferenceEquals(x, y) || x != null && y != null && x.SequenceEqual(y);
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            // Will not throw an OverflowException
            unchecked
            {
                return obj.Where(e => e != null).Select(e => e.GetHashCode()).Aggregate(17, (a, b) => 23 * a + b);
            }
        }
    }
}
