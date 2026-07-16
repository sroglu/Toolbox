namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Zero-setup bootstrap for the Mahjong sample. Drop it on an empty GameObject and
    /// press play — it builds a small layout in code (a 2x2 base with one tile resting on
    /// all four, plus a few spaced-out free tiles), fills faces with the deterministic
    /// generator, and wires up the board, view, and presenter. No scene asset or prefab
    /// is needed, so it runs in this source-only repo. Click two matching free tiles to
    /// take them off; enable Gizmos to see the free/blocked state and the slide axis.
    /// </summary>
    public class MahjongDemo : MonoBehaviour
    {
        [Tooltip("Seed for the deterministic face generator; the same seed gives the same board.")]
        [SerializeField] private int seed = 12345;

        [Tooltip("Board-global long axis that tiles slide out along.")]
        [SerializeField] private SlideAxis slideAxis = SlideAxis.X;

        private MahjongPresenter presenter;

        private void Start()
        {
            var layout = BuildDemoLayout(slideAxis);
            var faces = BuildDemoFaceSet();

            MahjongGenerator.Fill(layout, faces, seed);

            var problems = MahjongValidator.Validate(layout.Slots);
            foreach (var problem in problems)
            {
                Debug.LogWarning($"[Mahjong] layout problem: {problem}");
            }

            var board = new MahjongBoard();
            board.Load(layout, faces);

            var view = gameObject.AddComponent<MahjongBoardView>();
            view.SetFaceSet(faces);

            var gizmo = gameObject.AddComponent<MahjongBoardGizmo>();
            gizmo.Board = board;

            presenter = new MahjongPresenter(view, board);
            presenter.Initialize();

            var free = board.FreeTiles().ToList();
            Debug.Log($"[Mahjong] {board.Tiles.Count} tiles, {free.Count} free to start: " +
                      string.Join(", ", free.Select(t => $"{t.FaceId}@({t.X},{t.Y},L{t.Layer})")));
        }

        private void OnDestroy()
        {
            presenter?.Dispose();
        }

        // A 2x2 base with one tile resting on all four (exercises stacking, cover and the
        // free rule), plus three well-spaced base tiles that all stay free (so a match can
        // be made by clicking). Eight tiles total, so the generator can pair them all.
        private static MahjongLayout BuildDemoLayout(SlideAxis slideAxis)
        {
            var layout = ScriptableObject.CreateInstance<MahjongLayout>();
            layout.SlideAxis = slideAxis;
            layout.Slots = new List<TileSlot>
            {
                Base(0, 0),
                Base(2, 0),
                Base(0, 2),
                Base(2, 2),
                Relative(0, 1, 2, 3),
                Base(10, 0),
                Base(20, 0),
                Base(30, 0),
            };
            return layout;
        }

        private static TileFaceSet BuildDemoFaceSet()
        {
            var faces = ScriptableObject.CreateInstance<TileFaceSet>();
            faces.Faces = new List<TileFace>
            {
                new TileFace { Id = "bamboo1" },
                new TileFace { Id = "circle5" },
                new TileFace { Id = "redDragon" },
                new TileFace { Id = "greenDragon" },
            };
            return faces;
        }

        private static TileSlot Base(int x, int y) =>
            new TileSlot { IsBase = true, X = x, Y = y };

        private static TileSlot Relative(params int[] restsOn) =>
            new TileSlot { IsBase = false, RestsOn = restsOn };
    }
}
