namespace Hades.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Gets that <see cref="char"/> at a given <paramref name="index"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to search.</param>
        /// <param name="index">The index of <paramref name="str"/> to return.</param>
        /// <returns>
        /// The <see cref="char"/> at the given <paramref cref="index"/>.  If <paramref name="index"/> is less than 0
        /// or greater than the length of the string, returns an ASCII NULL (\0).
        /// </returns>
        public static char CharAt(this string str, int index)
        {
            if (index > str.Length - 1 || index < 0)
            {
                return '\0';
            }

            return str[index];
        }
    }
}