using System;
using System.Collections.Generic;
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable UnusedMethodReturnValue.Global

namespace Hades.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<K> Map<T, K>(this IEnumerable<T> source, Func<T, K> action)
        {
            foreach (var x in source)
            {
                yield return action(x);
            }
        }

        public static S Filter<T, S>(this IEnumerable<T> source, Func<S, T, S> accumulator, S initialAccumulator)
        {
            var init = initialAccumulator;
            foreach (var x in source)
            {
                init = accumulator(init, x);
            }

            return init;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var x in source)
            {
                action(x);
            }
        }
    }
}