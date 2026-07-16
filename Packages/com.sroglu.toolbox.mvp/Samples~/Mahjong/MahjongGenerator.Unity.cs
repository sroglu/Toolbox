namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;

    /// <summary>Unity-facing convenience over the engine-free generator core.</summary>
    public static partial class MahjongGenerator
    {
        /// <summary>
        /// Fills a layout's slots in place with generated faces drawn from the face set,
        /// using every face in the set as a candidate. The layout's slot count must be even.
        /// </summary>
        public static void Fill(MahjongLayout layout, TileFaceSet faces, int seed)
        {
            var candidates = new List<string>(faces.Faces.Count);
            foreach (var face in faces.Faces)
            {
                candidates.Add(face.Id);
            }

            var ids = Fill(layout.Slots.Count, candidates, seed);
            for (int i = 0; i < layout.Slots.Count; i++)
            {
                var slot = layout.Slots[i];
                slot.FaceId = ids[i];
                layout.Slots[i] = slot;
            }
        }
    }
}
