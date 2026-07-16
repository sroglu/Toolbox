namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An authored board shape: the list of tile slots and the board-global slide axis.
    /// Faces are usually filled in by the generator, so a layout only needs to describe
    /// where tiles sit (base coordinates) and how they stack (relative supports).
    /// </summary>
    [CreateAssetMenu(menuName = "Sroglu/Mahjong/Layout", fileName = "MahjongLayout")]
    public class MahjongLayout : ScriptableObject
    {
        public List<TileSlot> Slots = new List<TileSlot>();

        public SlideAxis SlideAxis = SlideAxis.X;
    }
}
