namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;

    /// <summary>
    /// One tile on the live board. Plain C# with no engine dependency, so the whole
    /// rule engine can run and be tested outside Unity.
    ///
    /// The stacking relation is stored in one direction — <see cref="RestsOn"/> ("I am
    /// on top of these") — and the reverse edge <see cref="CoveredBy"/> is built once at
    /// load. <see cref="Left"/>/<see cref="Right"/> are the same-layer neighbours along
    /// the board's slide axis.
    /// </summary>
    public class TileNode
    {
        /// <summary>Which face this tile shows; two tiles match when their faces match.</summary>
        public string FaceId;

        /// <summary>True once the tile has been taken off the board.</summary>
        public bool Removed;

        /// <summary>The 1, 2 or 4 tiles directly under this one that hold it up.</summary>
        public readonly List<TileNode> RestsOn = new List<TileNode>();

        /// <summary>The tiles that rest on this one (reverse of <see cref="RestsOn"/>).</summary>
        public readonly List<TileNode> CoveredBy = new List<TileNode>();

        /// <summary>Same-layer neighbours immediately to the left along the slide axis.</summary>
        public readonly List<TileNode> Left = new List<TileNode>();

        /// <summary>Same-layer neighbours immediately to the right along the slide axis.</summary>
        public readonly List<TileNode> Right = new List<TileNode>();

        /// <summary>Derived half-tile X coordinate.</summary>
        public int X;

        /// <summary>Derived half-tile Y coordinate.</summary>
        public int Y;

        /// <summary>Derived stack layer; base tiles are layer 0.</summary>
        public int Layer;

        /// <summary>True while any tile still resting on this one is on the board.</summary>
        public bool IsCovered
        {
            get
            {
                foreach (var tile in CoveredBy)
                {
                    if (!tile.Removed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>True when no tile still on the board sits to the left.</summary>
        public bool LeftOpen
        {
            get
            {
                foreach (var tile in Left)
                {
                    if (!tile.Removed)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>True when no tile still on the board sits to the right.</summary>
        public bool RightOpen
        {
            get
            {
                foreach (var tile in Right)
                {
                    if (!tile.Removed)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// The Mahjong free rule: a tile can be taken when it is still on the board,
        /// nothing is stacked on it, and at least one of its long sides is open.
        /// </summary>
        public bool IsFree => !Removed && !IsCovered && (LeftOpen || RightOpen);
    }
}
