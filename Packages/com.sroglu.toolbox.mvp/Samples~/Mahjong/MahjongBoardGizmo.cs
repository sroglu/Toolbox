namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using UnityEngine;

    /// <summary>
    /// Editor author-feedback for a board. For every tile it draws a wire cube at the tile's
    /// derived world position, coloured by state (green = free, red = blocked, grey =
    /// removed), plus a short arrow along the board's slide axis so the long direction is
    /// visible at a glance. <c>OnDrawGizmos</c> runs only in the editor, so this never costs
    /// anything at runtime. Assign <see cref="Board"/> from your bootstrap (the demo does).
    /// </summary>
    public class MahjongBoardGizmo : MonoBehaviour
    {
        [Tooltip("World size of one half-tile grid step; keep in sync with the board view.")]
        public float halfWorld = 0.5f;

        [Tooltip("How far each stack layer is nudged; keep in sync with the board view.")]
        public float layerLift = 0.12f;

        /// <summary>The board to draw. Set at runtime by the bootstrap; not serialized.</summary>
        [System.NonSerialized] public MahjongBoard Board;

        private void OnDrawGizmos()
        {
            // The board only exists once the bootstrap has loaded it; nothing to draw before then.
            if (Board == null)
            {
                return;
            }

            Vector3 slideDirection = Board.SlideAxis == SlideAxis.X ? Vector3.right : Vector3.up;

            foreach (var node in Board.Tiles)
            {
                Vector3 position = transform.TransformPoint(WorldPosition(node));

                Gizmos.color = node.Removed ? Color.gray : node.IsFree ? Color.green : Color.red;
                Gizmos.DrawWireCube(position, new Vector3(halfWorld * 2f * 0.9f, halfWorld * 2f * 0.9f, 0.1f));
                Gizmos.DrawLine(position, position + slideDirection * halfWorld);
            }
        }

        private Vector3 WorldPosition(TileNode node)
        {
            int slide = Board.SlideAxis == SlideAxis.X ? node.X : node.Y;
            int depth = Board.SlideAxis == SlideAxis.X ? node.Y : node.X;
            return new Vector3(slide * halfWorld + node.Layer * layerLift, depth * halfWorld + node.Layer * layerLift, -node.Layer);
        }
    }
}
