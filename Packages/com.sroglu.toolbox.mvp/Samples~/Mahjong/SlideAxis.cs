namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    /// <summary>
    /// The single board-global long axis that tiles are pulled out along. It is the
    /// one source of truth the view orientation and the free-rule both read, so the
    /// two can never disagree about which direction is "long".
    /// </summary>
    public enum SlideAxis
    {
        /// <summary>Tiles slide out horizontally; X is the long axis, Y is the depth.</summary>
        X,

        /// <summary>Tiles slide out vertically; Y is the long axis, X is the depth.</summary>
        Y
    }
}
