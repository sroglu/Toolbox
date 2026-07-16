namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The passive view of the whole board. The presenter calls <see cref="Build"/> to
    /// spawn one <see cref="MahjongTileView"/> per tile, <see cref="Refresh"/> to repaint
    /// them after a change, and the selection methods to highlight a picked tile. It
    /// raises <see cref="TileClicked"/> with the tile the user clicked. It holds no board.
    /// </summary>
    public class MahjongBoardView : MonoBehaviour, IView
    {
        [Tooltip("World size of one half-tile grid step.")]
        [SerializeField] private float halfWorld = 0.5f;

        [Tooltip("How far each stack layer is nudged up-right so higher tiles are visible.")]
        [SerializeField] private float layerLift = 0.12f;

        /// <summary>Raised with the tile the user clicked.</summary>
        public event Action<TileNode> TileClicked;

        private TileFaceSet faces;
        private SlideAxis slideAxis;
        private readonly List<MahjongTileView> tileViews = new List<MahjongTileView>();
        private readonly Dictionary<TileNode, MahjongTileView> viewByNode = new Dictionary<TileNode, MahjongTileView>();

        /// <summary>Supplies the face art used when spawning tile views.</summary>
        public void SetFaceSet(TileFaceSet faceSet)
        {
            faces = faceSet;
        }

        /// <summary>Spawns a tile view per node, positioned by the slide-axis-aware layout.</summary>
        public void Build(IReadOnlyList<TileNode> tiles, SlideAxis axis)
        {
            slideAxis = axis;

            foreach (var tileView in tileViews)
            {
                Destroy(tileView.gameObject);
            }

            tileViews.Clear();
            viewByNode.Clear();

            foreach (var node in tiles)
            {
                var go = new GameObject($"Tile_{node.FaceId}_L{node.Layer}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = WorldPosition(node);

                var tileView = go.AddComponent<MahjongTileView>();
                tileView.Bind(node, faces.Get(node.FaceId).Sprite);

                var clicked = node;
                tileView.Clicked += () => TileClicked?.Invoke(clicked);

                tileViews.Add(tileView);
                viewByNode[node] = tileView;
            }
        }

        /// <summary>Repaints every tile from the model's current state.</summary>
        public void Refresh()
        {
            foreach (var tileView in tileViews)
            {
                tileView.RenderState();
            }
        }

        /// <summary>Highlights one tile as the current selection.</summary>
        public void SetSelected(TileNode node)
        {
            viewByNode[node].SetSelected(true);
        }

        /// <summary>Clears any selection highlight.</summary>
        public void ClearSelection()
        {
            foreach (var tileView in tileViews)
            {
                tileView.SetSelected(false);
            }
        }

        // The slide axis becomes the horizontal world axis, so the long direction always
        // reads left-to-right. Higher layers are nudged up-right and pulled forward.
        private Vector3 WorldPosition(TileNode node)
        {
            float slide = SlideCoordinate(node) * halfWorld;
            float depth = DepthCoordinate(node) * halfWorld;
            return new Vector3(slide + node.Layer * layerLift, depth + node.Layer * layerLift, -node.Layer);
        }

        private int SlideCoordinate(TileNode node) => slideAxis == SlideAxis.X ? node.X : node.Y;

        private int DepthCoordinate(TileNode node) => slideAxis == SlideAxis.X ? node.Y : node.X;
    }
}
