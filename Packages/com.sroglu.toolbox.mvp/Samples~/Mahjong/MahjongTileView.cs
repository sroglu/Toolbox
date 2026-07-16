namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The passive view of a single tile. It shows the face (a sprite when the face has
    /// one, otherwise a plain coloured quad plus a text label of the id, so the demo runs
    /// with zero imported art) and raises <see cref="Clicked"/> when the tile is clicked.
    /// It colours itself by board state on <see cref="RenderState"/> — green when free,
    /// red when blocked, and hides itself once removed.
    /// </summary>
    public class MahjongTileView : MonoBehaviour, IView
    {
        private static readonly Color FreeColor = new Color(0.45f, 0.8f, 0.45f);
        private static readonly Color BlockedColor = new Color(0.8f, 0.4f, 0.4f);
        private static readonly Color SelectedColor = new Color(0.95f, 0.9f, 0.4f);

        /// <summary>Raised when the user clicks this tile.</summary>
        public event Action Clicked;

        private TileNode node;
        private SpriteRenderer spriteRenderer;
        private MeshRenderer quadRenderer;
        private bool selected;

        /// <summary>Binds the view to its tile and builds its visuals once.</summary>
        public void Bind(TileNode tile, Sprite sprite)
        {
            node = tile;

            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.95f, 0.95f, 0.2f);

            if (sprite != null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
            }
            else
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "Face";
                quad.transform.SetParent(transform, false);
                quad.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
                DestroyImmediate(quad.GetComponent<Collider>());
                quadRenderer = quad.GetComponent<MeshRenderer>();

                var text = new GameObject("Label").AddComponent<TextMesh>();
                text.transform.SetParent(transform, false);
                text.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                text.transform.localScale = Vector3.one * 0.12f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.color = Color.black;
                text.text = node.FaceId;
            }

            RenderState();
        }

        /// <summary>Refreshes colour and visibility from the tile's current state.</summary>
        public void RenderState()
        {
            if (node.Removed)
            {
                gameObject.SetActive(false);
                return;
            }

            var color = selected ? SelectedColor : node.IsFree ? FreeColor : BlockedColor;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
            else
            {
                quadRenderer.material.color = color;
            }
        }

        /// <summary>Marks the tile as the current selection and repaints.</summary>
        public void SetSelected(bool value)
        {
            selected = value;
            RenderState();
        }

        private void OnMouseDown()
        {
            Clicked?.Invoke();
        }
    }
}
