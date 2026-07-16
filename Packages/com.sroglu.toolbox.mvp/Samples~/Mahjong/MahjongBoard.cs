namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The live Mahjong Solitaire board and its rules. This is the engine-free core:
    /// it depends only on plain C# plus the MVP <see cref="ObservableModel"/> base, so
    /// it runs and is unit-tested outside Unity. The Unity-facing convenience overload
    /// that loads from a <c>MahjongLayout</c>/<c>TileFaceSet</c> lives in a partial file.
    ///
    /// The board is an observable model: it raises <see cref="ObservableModel.Changed"/>
    /// after a load or a successful match so a presenter can re-render.
    /// </summary>
    public partial class MahjongBoard : ObservableModel
    {
        private readonly List<TileNode> tiles = new List<TileNode>();
        private Func<string, string, bool> facesMatch;

        /// <summary>All tiles on the board, in slot order (removed ones stay in the list).</summary>
        public IReadOnlyList<TileNode> Tiles => tiles;

        /// <summary>The long axis tiles are pulled out along; drives the free rule.</summary>
        public SlideAxis SlideAxis { get; private set; }

        /// <summary>
        /// Builds the board from raw slot data. The face matcher is passed in as a
        /// delegate (<c>TileFaceSet.Matches</c> in Unity) so the core stays free of the
        /// ScriptableObjects and can be loaded with fake data in a test.
        /// </summary>
        /// <param name="slots">The authored slots; base tiles carry coordinates, relative tiles carry supports.</param>
        /// <param name="slideAxis">The board-global long axis.</param>
        /// <param name="facesMatch">Returns true when two face ids are considered a matching pair.</param>
        public void Load(IList<TileSlot> slots, SlideAxis slideAxis, Func<string, string, bool> facesMatch)
        {
            SlideAxis = slideAxis;
            this.facesMatch = facesMatch;

            tiles.Clear();

            foreach (var slot in slots)
            {
                tiles.Add(new TileNode { FaceId = slot.FaceId });
            }

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsBase)
                {
                    continue;
                }

                foreach (var supportIndex in slot.RestsOn)
                {
                    tiles[i].RestsOn.Add(tiles[supportIndex]);
                }
            }

            ResolveDerived(slots);
            BuildCoveredAndSides();
            RaiseChanged();
        }

        /// <summary>The tiles that can currently be taken.</summary>
        public IEnumerable<TileNode> FreeTiles()
        {
            foreach (var tile in tiles)
            {
                if (tile.IsFree)
                {
                    yield return tile;
                }
            }
        }

        /// <summary>
        /// Takes a matching pair off the board. Both tiles must be distinct, currently
        /// free, and their faces must match. On success both are removed, a change is
        /// raised, and true is returned; otherwise nothing changes and false is returned.
        /// </summary>
        public bool TryMatch(TileNode a, TileNode b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if (!a.IsFree || !b.IsFree)
            {
                return false;
            }

            if (!facesMatch(a.FaceId, b.FaceId))
            {
                return false;
            }

            a.Removed = true;
            b.Removed = true;
            RaiseChanged();
            return true;
        }

        // Base tiles take their authored coordinates at layer 0. Relative tiles resolve
        // once every tile they rest on is resolved: their centre is the average of the
        // supports' coordinates (an integer in half-units since the support count is
        // 1, 2 or 4) and their layer is one above the highest support.
        private void ResolveDerived(IList<TileSlot> slots)
        {
            var resolved = new HashSet<TileNode>();

            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsBase)
                {
                    continue;
                }

                var node = tiles[i];
                node.X = slots[i].X;
                node.Y = slots[i].Y;
                node.Layer = 0;
                resolved.Add(node);
            }

            bool progressed = true;
            while (progressed)
            {
                progressed = false;

                foreach (var node in tiles)
                {
                    if (resolved.Contains(node))
                    {
                        continue;
                    }

                    if (!AllResolved(node.RestsOn, resolved))
                    {
                        continue;
                    }

                    int sumX = 0;
                    int sumY = 0;
                    int highestLayer = 0;
                    foreach (var support in node.RestsOn)
                    {
                        sumX += support.X;
                        sumY += support.Y;
                        if (support.Layer > highestLayer)
                        {
                            highestLayer = support.Layer;
                        }
                    }

                    node.X = sumX / node.RestsOn.Count;
                    node.Y = sumY / node.RestsOn.Count;
                    node.Layer = highestLayer + 1;
                    resolved.Add(node);
                    progressed = true;
                }
            }
        }

        private static bool AllResolved(List<TileNode> supports, HashSet<TileNode> resolved)
        {
            foreach (var support in supports)
            {
                if (!resolved.Contains(support))
                {
                    return false;
                }
            }

            return true;
        }

        // CoveredBy is the reverse of RestsOn. Left/Right are same-layer neighbours one
        // tile away along the slide axis whose depth overlaps (|depth diff| < 2).
        private void BuildCoveredAndSides()
        {
            foreach (var node in tiles)
            {
                foreach (var support in node.RestsOn)
                {
                    support.CoveredBy.Add(node);
                }
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                var a = tiles[i];
                int slideA = SlideCoordinate(a);
                int depthA = DepthCoordinate(a);

                for (int j = 0; j < tiles.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var b = tiles[j];
                    if (a.Layer != b.Layer)
                    {
                        continue;
                    }

                    if (Math.Abs(depthA - DepthCoordinate(b)) >= 2)
                    {
                        continue;
                    }

                    int slideB = SlideCoordinate(b);
                    if (slideB == slideA - 2)
                    {
                        a.Left.Add(b);
                    }
                    else if (slideB == slideA + 2)
                    {
                        a.Right.Add(b);
                    }
                }
            }
        }

        private int SlideCoordinate(TileNode node) => SlideAxis == SlideAxis.X ? node.X : node.Y;

        private int DepthCoordinate(TileNode node) => SlideAxis == SlideAxis.X ? node.Y : node.X;
    }
}
