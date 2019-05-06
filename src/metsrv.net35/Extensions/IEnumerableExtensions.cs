using System.Collections.Generic;
using System.Linq;

namespace Met.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        // [ [ 1, 2, 3 ], [ 4, 5, 6 ] ] => [ 1, 2, 3, 4, 5, 6 ]
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> elements)
        {
            return elements.SelectMany(x => x);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<List<T>> elements)
        {
            return elements.SelectMany(x => x);
        }
    }
}
