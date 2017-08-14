using System;
using System.Collections.Generic;
using System.Linq;

namespace Hades.StringExtension
{
    public static class Extensions
    {
        public static bool ContainsFromList(this string s, IEnumerable<string> source)
        {
            return source.Any(a => s.Contains(a,StringComparison.OrdinalIgnoreCase));
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string[] SplitToTwo(this string source, string delimiter, StringSplitOptions options)
        {
            var split = source.Split(new[] {delimiter}, options);

            if (split.Length == 2 || split.Length < 2)
            {
                return split;
            }

            var smallerArray = split.ToList().GetRange(1,split.Length-1);
            var secondPos = string.Empty;
            smallerArray.ForEach(a => secondPos += (delimiter + a));
            return new[]{split[1],secondPos.Substring(delimiter.Length)};
        }

        public static bool EqualsFromList(this string source, IEnumerable<string> list)
        {
            var equals = false;
            foreach (var s in list)
            {
                if (s == source)
                {
                    equals = true;
                }
            }
            return equals;
        }

        /// <summary>
        /// Splits data for method calls
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<string> StringSplit(this string source,char delimiter)
        {
            if (string.IsNullOrEmpty(source))
            {
                yield break;
            }

            var lastIndex = 0;
            var inQuot = false;

            for (var i = 0; i < source.Length; ++i)
            {
                var c = source[i];

                if (inQuot)
                {
                    if (c == '\'')
                    {
                        inQuot = false;
                    }
                }
                else if (c == '\'')
                {
                    inQuot = true;
                }
                else if (c == delimiter)
                {
                    yield return source.Substring(lastIndex, i - lastIndex);
                    lastIndex = i + 1;
                }
            }

            yield return source.Substring(lastIndex);
        }
    }
}
