namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System;
    using UnityEngine;

    /// <summary>
    /// One face a tile can show. Faces normally match by <see cref="Id"/>, but a face can
    /// also belong to a <see cref="MatchGroup"/> (any non-zero value) so that any two
    /// tiles in the same group match — the classic flowers-and-seasons rule where a group
    /// of four different tiles all pair with each other.
    /// </summary>
    [Serializable]
    public class TileFace
    {
        [Tooltip("Unique id for this face, e.g. \"redDragon\" or \"pin5\".")]
        public string Id;

        [Tooltip("Sprite shown for this face. May be left empty; the demo falls back to a label.")]
        public Sprite Sprite;

        [Tooltip("0 = match by id. Any non-zero value = match any face sharing this group (flowers/seasons).")]
        public int MatchGroup;
    }
}
