namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    /// <summary>
    /// Unity-facing convenience over the engine-free <see cref="MahjongBoard"/> core. It
    /// forwards a layout and face set into the core <c>Load</c>, passing the face set's
    /// <c>Matches</c> as the matcher so the core stays free of the ScriptableObjects.
    /// </summary>
    public partial class MahjongBoard
    {
        /// <summary>Loads the board from an authored layout and its face set.</summary>
        public void Load(MahjongLayout layout, TileFaceSet faces)
        {
            Load(layout.Slots, layout.SlideAxis, faces.Matches);
        }
    }
}
