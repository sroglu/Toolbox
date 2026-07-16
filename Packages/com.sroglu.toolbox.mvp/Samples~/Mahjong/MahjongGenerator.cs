namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;

    /// <summary>
    /// Fills a layout with faces so every tile has a partner. Faces are added to a bag in
    /// pairs (so each face always appears an even number of times) and then shuffled with
    /// a small deterministic generator seeded by an int, so the same seed always yields
    /// the same board and it can be reproduced in a test — no <c>System.Random</c> or
    /// clock is used. The Unity-facing overload that reads a layout/face set lives in a
    /// partial file.
    /// </summary>
    public static partial class MahjongGenerator
    {
        /// <summary>
        /// Returns one face id per slot. <paramref name="slotCount"/> must be even.
        /// Candidate faces are cycled through in pairs to fill the bag, then the bag is
        /// shuffled deterministically from <paramref name="seed"/>.
        /// </summary>
        public static string[] Fill(int slotCount, IReadOnlyList<string> candidateFaceIds, int seed)
        {
            var bag = new string[slotCount];

            int pairs = slotCount / 2;
            int written = 0;
            for (int pair = 0; pair < pairs; pair++)
            {
                var face = candidateFaceIds[pair % candidateFaceIds.Count];
                bag[written++] = face;
                bag[written++] = face;
            }

            Shuffle(bag, seed);
            return bag;
        }

        // Fisher-Yates driven by a tiny xorshift generator so the order is fully
        // determined by the seed and reproducible outside Unity.
        private static void Shuffle(string[] items, int seed)
        {
            uint state = (uint)seed;
            if (state == 0)
            {
                state = 0x9E3779B9;
            }

            for (int i = items.Length - 1; i > 0; i--)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;

                int j = (int)(state % (uint)(i + 1));
                var swap = items[i];
                items[i] = items[j];
                items[j] = swap;
            }
        }
    }
}
