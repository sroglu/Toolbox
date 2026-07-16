namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;

    /// <summary>
    /// Author-time lint for a layout. Unlike the runtime, this deliberately inspects the
    /// data for mistakes and returns clear English messages naming each offending slot —
    /// it is the one place defensive checking is wanted. An empty result means the layout
    /// is valid and solvable in principle.
    /// </summary>
    public static class MahjongValidator
    {
        /// <summary>
        /// Checks a layout and returns one message per problem found (empty when valid):
        /// out-of-range supports, an illegal support count, a floating tile that does not
        /// sit on a layer directly below it, two tiles overlapping on the same layer, an
        /// odd tile total, or any face id that ends up with an odd (unmatchable) count.
        /// </summary>
        public static List<string> Validate(IList<TileSlot> slots)
        {
            var problems = new List<string>();

            var x = new int[slots.Count];
            var y = new int[slots.Count];
            var layer = new int[slots.Count];
            var resolved = new bool[slots.Count];

            CheckSupportsAndResolve(slots, problems, x, y, layer, resolved);
            CheckFootprints(slots, problems, x, y, layer, resolved);
            CheckFaceCounts(slots, problems);

            return problems;
        }

        private static void CheckSupportsAndResolve(
            IList<TileSlot> slots, List<string> problems, int[] x, int[] y, int[] layer, bool[] resolved)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsBase)
                {
                    x[i] = slot.X;
                    y[i] = slot.Y;
                    layer[i] = 0;
                    resolved[i] = true;
                    continue;
                }

                var supports = slot.RestsOn;
                if (supports == null || supports.Length == 0)
                {
                    problems.Add($"Slot {i}: relative tile has no supports; it must rest on 1, 2 or 4 tiles.");
                    continue;
                }

                if (supports.Length != 1 && supports.Length != 2 && supports.Length != 4)
                {
                    problems.Add($"Slot {i}: rests on {supports.Length} tiles; a tile can only rest on 1, 2 or 4 (3 has no integer centre).");
                }

                foreach (var supportIndex in supports)
                {
                    if (supportIndex < 0 || supportIndex >= slots.Count)
                    {
                        problems.Add($"Slot {i}: support index {supportIndex} is out of range.");
                    }
                    else if (supportIndex == i)
                    {
                        problems.Add($"Slot {i}: rests on itself.");
                    }
                }
            }

            // Resolve positions iteratively for every slot whose supports are all in range
            // and already resolved, mirroring the runtime derivation.
            bool progressed = true;
            while (progressed)
            {
                progressed = false;

                for (int i = 0; i < slots.Count; i++)
                {
                    if (resolved[i] || slots[i].IsBase)
                    {
                        continue;
                    }

                    var supports = slots[i].RestsOn;
                    if (supports == null || supports.Length == 0 || !SupportsResolvable(supports, slots.Count, resolved))
                    {
                        continue;
                    }

                    int sumX = 0;
                    int sumY = 0;
                    int highestLayer = 0;
                    foreach (var supportIndex in supports)
                    {
                        sumX += x[supportIndex];
                        sumY += y[supportIndex];
                        if (layer[supportIndex] > highestLayer)
                        {
                            highestLayer = layer[supportIndex];
                        }
                    }

                    x[i] = sumX / supports.Length;
                    y[i] = sumY / supports.Length;
                    layer[i] = highestLayer + 1;
                    resolved[i] = true;
                    progressed = true;
                }
            }
        }

        private static bool SupportsResolvable(int[] supports, int slotCount, bool[] resolved)
        {
            foreach (var supportIndex in supports)
            {
                if (supportIndex < 0 || supportIndex >= slotCount || !resolved[supportIndex])
                {
                    return false;
                }
            }

            return true;
        }

        private static void CheckFootprints(
            IList<TileSlot> slots, List<string> problems, int[] x, int[] y, int[] layer, bool[] resolved)
        {
            // Every support must sit exactly one layer below and overlap this tile's footprint.
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsBase || !resolved[i])
                {
                    continue;
                }

                var supports = slots[i].RestsOn;
                foreach (var supportIndex in supports)
                {
                    if (supportIndex < 0 || supportIndex >= slots.Count || !resolved[supportIndex])
                    {
                        continue;
                    }

                    bool oneLayerBelow = layer[supportIndex] == layer[i] - 1;
                    bool overlaps = System.Math.Abs(x[supportIndex] - x[i]) < 2 && System.Math.Abs(y[supportIndex] - y[i]) < 2;
                    if (!oneLayerBelow || !overlaps)
                    {
                        problems.Add($"Slot {i}: floats over slot {supportIndex} (not the layer directly below with an overlapping footprint).");
                    }
                }
            }

            // No two tiles may overlap on the same layer.
            for (int i = 0; i < slots.Count; i++)
            {
                if (!resolved[i])
                {
                    continue;
                }

                for (int j = i + 1; j < slots.Count; j++)
                {
                    if (!resolved[j] || layer[i] != layer[j])
                    {
                        continue;
                    }

                    if (System.Math.Abs(x[i] - x[j]) < 2 && System.Math.Abs(y[i] - y[j]) < 2)
                    {
                        problems.Add($"Slot {i} and slot {j}: overlap on the same layer.");
                    }
                }
            }
        }

        private static void CheckFaceCounts(IList<TileSlot> slots, List<string> problems)
        {
            if (slots.Count % 2 != 0)
            {
                problems.Add($"The layout has {slots.Count} tiles; the total must be even so every tile can be paired.");
            }

            var counts = new Dictionary<string, int>();
            foreach (var slot in slots)
            {
                var id = slot.FaceId ?? string.Empty;
                counts.TryGetValue(id, out int current);
                counts[id] = current + 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value % 2 != 0)
                {
                    problems.Add($"Face '{pair.Key}' appears {pair.Value} times (odd); every face must appear an even number of times.");
                }
            }
        }
    }
}
