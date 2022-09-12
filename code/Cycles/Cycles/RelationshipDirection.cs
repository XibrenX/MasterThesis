using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cycles
{
    public enum RelationshipDirection : byte
    {
        In,
        Out
    }

    public static class RelationshipDirectionExtensions
    {
        public static RelationshipDirection Inverse(this RelationshipDirection direction)
        {
            return (RelationshipDirection)((int)direction ^ 1);
        }

        public static string GetChar(this RelationshipDirection direction)
        {
            return direction switch
            {
                RelationshipDirection.In => "<",
                RelationshipDirection.Out => ">",
                _ => string.Empty
            };
        }
    }
}
