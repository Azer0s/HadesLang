using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StringExtension
{
    public static class Extensions
    {
        public static bool ContainsFromList(this string s, IEnumerable<string> source)
        {
            return source.Any(a => s.ToLower().Contains(a.ToLower()));
        }

        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (var index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        public static bool EndsWithFromList(this string s, List<string> list)
        {
            foreach (var s1 in list)
            {
                if (s.EndsWith(s1))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Closes(this string str,char open, char close)
        {
            var buff = 0;
            foreach (var c in str)
            {
                if (c == open)
                {
                    buff++;
                }

                if (c == close)
                {
                    buff--;
                }
            }

            return buff == 0;
        }

        public static string GetIntValue(this string source)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(source);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("D"));
            }

            var groups = sb.ToString().SplitByLength(8).ToArray();

            for (var i = 0; i < groups.Length; i++)
            {
                groups[i] = (Convert.ToInt32(groups[i]) % 21 /*Becaus first two digits of int.MaxValue are 21*/).ToString();
            }

            return string.Join("",groups);
        }

        public static bool NestedFunction(this string source,Regex function)
        {
            var repls = function.Replace(source,
                match =>
                    match.Groups[1].Value + ":#");
            return repls.Split(':').Length == 2 && repls.EndsWith("#");
        }

        public static bool IsValidFunction(this string source)
        {
            return source.StringSplit(':',new []{'\'','[',']'}).Count() == 2;
        }

        public static string Remainder(this string source, Regex check)
        {
            var matches = check.Matches(source);
            return matches.Cast<Match>().Aggregate(source, (current, match) => current.Replace(match.Value, ""));
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
        public static IEnumerable<string> StringSplit(this string source,char delimiter, char[] escape = null)
        {
            if (escape == null)
            {
                escape = new[] {'\''};
            }

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
                    if (escape.Contains(c))
                    {
                        inQuot = false;
                    }
                }
                else if (escape.Contains(c))
                {
                    inQuot = true;
                }
                else if (c == delimiter)
                {
                    var val = source.Substring(lastIndex, i - lastIndex);

                    if (!string.IsNullOrEmpty(val))
                    {
                        yield return val;
                    }
                    lastIndex = i + 1;
                }
            }

            yield return source.Substring(lastIndex);
        }
    }
}
