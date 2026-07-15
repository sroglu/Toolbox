using System;
using System.Collections.Generic;

namespace Sroglu.Toolbox.Randomization
{
    /// <summary>
    /// Zero-dependency randomization helpers. Every method takes an optional
    /// <see cref="System.Random"/>; when none is supplied a shared static instance
    /// is used. Invalid inputs fail fast by throwing.
    /// </summary>
    public static class RandomUtils
    {
        private static readonly Random Shared = new Random();

        private static Random Resolve(Random rng)
        {
            return rng ?? Shared;
        }

        /// <summary>
        /// Returns a uniformly-chosen element from <paramref name="items"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="items">Source list. Must be non-null and non-empty.</param>
        /// <param name="rng">Optional generator; defaults to a shared instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty.</exception>
        public static T Pick<T>(IReadOnlyList<T> items, Random rng = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                throw new ArgumentException("Cannot pick from an empty list.", nameof(items));

            return items[Resolve(rng).Next(items.Count)];
        }

        /// <summary>
        /// Returns an element chosen with probability proportional to its weight.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="items">Source list. Must be non-null and non-empty.</param>
        /// <param name="weights">Per-element weights; must match <paramref name="items"/> in count.</param>
        /// <param name="rng">Optional generator; defaults to a shared instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when a list argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the lists differ in length, are empty, or the weights do not sum to a positive value.
        /// </exception>
        public static T PickWeighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights, Random rng = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));
            if (items.Count == 0)
                throw new ArgumentException("Cannot pick from an empty list.", nameof(items));
            if (items.Count != weights.Count)
                throw new ArgumentException("Items and weights must have the same length.", nameof(weights));

            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                float weight = weights[i];
                if (weight > 0f)
                    total += weight;
            }

            if (total <= 0f)
                throw new ArgumentException("At least one weight must be greater than zero.", nameof(weights));

            double roll = Resolve(rng).NextDouble() * total;
            float running = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                float weight = weights[i];
                if (weight <= 0f)
                    continue;

                running += weight;
                if (roll < running)
                    return items[i];
            }

            // Floating-point drift can leave the roll at the very top of the range;
            // fall back to the last positively-weighted element.
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (weights[i] > 0f)
                    return items[i];
            }

            throw new ArgumentException("At least one weight must be greater than zero.", nameof(weights));
        }

        /// <summary>
        /// Shuffles <paramref name="list"/> in place using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to shuffle. Must be non-null.</param>
        /// <param name="rng">Optional generator; defaults to a shared instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null.</exception>
        public static void Shuffle<T>(IList<T> list, Random rng = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Random generator = Resolve(rng);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = generator.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Returns a random integer in the half-open interval
        /// [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the maximum is below the minimum.</exception>
        public static int Range(int minInclusive, int maxExclusive, Random rng = null)
        {
            if (maxExclusive < minInclusive)
                throw new ArgumentException("maxExclusive must be greater than or equal to minInclusive.", nameof(maxExclusive));

            return Resolve(rng).Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a random float in the half-open interval
        /// [<paramref name="min"/>, <paramref name="max"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the maximum is below the minimum.</exception>
        public static float Range(float min, float max, Random rng = null)
        {
            if (max < min)
                throw new ArgumentException("max must be greater than or equal to min.", nameof(max));

            return min + (float)(Resolve(rng).NextDouble() * (max - min));
        }

        /// <summary>
        /// Returns true with the given probability. A probability of 0 (or below)
        /// never succeeds and 1 (or above) always succeeds.
        /// </summary>
        /// <param name="probability01">Success probability, clamped to the range 0..1.</param>
        /// <param name="rng">Optional generator; defaults to a shared instance.</param>
        public static bool Chance(float probability01, Random rng = null)
        {
            if (probability01 <= 0f)
                return false;
            if (probability01 >= 1f)
                return true;

            return Resolve(rng).NextDouble() < probability01;
        }
    }
}
