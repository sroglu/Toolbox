namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    /// <summary>
    /// Drives the Mahjong board. It builds and refreshes the view from the board, and
    /// turns two tile clicks into a match attempt: the first click selects a tile, the
    /// second calls <see cref="MahjongBoard.TryMatch"/>. Either way the selection clears,
    /// and the board's Changed event re-renders the view (removing a matched pair).
    /// </summary>
    public class MahjongPresenter : Presenter<MahjongBoardView, MahjongBoard>
    {
        private TileNode selected;

        public MahjongPresenter(MahjongBoardView view, MahjongBoard model)
            : base(view, model)
        {
        }

        protected override void OnBind()
        {
            Model.Changed += OnBoardChanged;
            View.TileClicked += OnTileClicked;

            View.Build(Model.Tiles, Model.SlideAxis);
            View.Refresh();
        }

        protected override void OnUnbind()
        {
            Model.Changed -= OnBoardChanged;
            View.TileClicked -= OnTileClicked;
        }

        private void OnBoardChanged()
        {
            View.Refresh();
        }

        private void OnTileClicked(TileNode tile)
        {
            if (selected == null)
            {
                selected = tile;
                View.SetSelected(tile);
                return;
            }

            if (ReferenceEquals(selected, tile))
            {
                selected = null;
                View.ClearSelection();
                return;
            }

            Model.TryMatch(selected, tile);
            selected = null;
            View.ClearSelection();
        }
    }
}
