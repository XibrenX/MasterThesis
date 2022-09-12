using Cycles;
using System.Text;

namespace Cycles
{
    public static class Extensions
    {
        public static char IncrAfter(this ref char c)
        {
            char old = c;
            c = (char)(Convert.ToUInt16(c) + 1);
            return old;
        }

        public static string ToPascalCase(this string snake_case)
        {
            var sb = new StringBuilder(snake_case.Length);

            var e = snake_case.GetEnumerator();
            if (e.MoveNext()) { sb.Append(char.ToUpperInvariant(e.Current)); }
            while (e.MoveNext())
            {
                if (e.Current != '_')
                {
                    sb.Append(e.Current);
                }
                else if (e.MoveNext())
                {
                    sb.Append(char.ToUpperInvariant(e.Current));
                }
            }

            return sb.ToString();
        }

        public static string to_snake_case(this string PascalCase)
        {
            var sb = new StringBuilder(PascalCase.Length + PascalCase.Length / 5);

            var e = PascalCase.GetEnumerator();
            if (e.MoveNext()) { sb.Append(char.ToLowerInvariant(e.Current)); }

            while (e.MoveNext())
            {
                if (char.IsUpper(e.Current))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(e.Current));
                }
                else
                {
                    sb.Append(e.Current);
                }
            }

            return sb.ToString();
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> t, params T[] additional)
        {
            return t.Concat((IEnumerable<T>)additional);
        }
    }
}
