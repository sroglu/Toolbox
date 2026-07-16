namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System;

    /// <summary>
    /// One authored tile position in a layout, in half-tile grid units (a tile fills
    /// a 2x2 block of fine cells, so neighbouring tiles differ by 2 on an axis).
    ///
    /// A slot is either a <b>base</b> tile — the bottom row that carries explicit
    /// <see cref="X"/>/<see cref="Y"/> coordinates — or a <b>relative</b> tile that
    /// sits on 1, 2 or 4 tiles below it and has no stored coordinates: its position
    /// is derived from the tiles it <see cref="RestsOn"/>.
    /// </summary>
    [Serializable]
    public struct TileSlot
    {
        /// <summary>Which face this slot shows (set by the generator or authored).</summary>
        public string FaceId;

        /// <summary>True for a bottom tile that carries its own coordinates.</summary>
        public bool IsBase;

        /// <summary>Half-tile X coordinate. Used only when <see cref="IsBase"/> is true.</summary>
        public int X;

        /// <summary>Half-tile Y coordinate. Used only when <see cref="IsBase"/> is true.</summary>
        public int Y;

        /// <summary>
        /// Indices (into the layout's slot list) of the 1, 2 or 4 tiles this tile sits
        /// on. Used only when <see cref="IsBase"/> is false; the position is derived
        /// from these supports.
        /// </summary>
        public int[] RestsOn;
    }
}
