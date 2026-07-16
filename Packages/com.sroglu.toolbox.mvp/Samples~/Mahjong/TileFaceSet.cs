namespace Sroglu.Toolbox.Mvp.Samples.Mahjong
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The set of faces a board can use, and the rule for when two faces match. Kept as a
    /// ScriptableObject so it is authored once in the editor and shared by layouts. The
    /// board core never sees this type — it is passed the <see cref="Matches"/> delegate.
    /// </summary>
    [CreateAssetMenu(menuName = "Sroglu/Mahjong/Tile Face Set", fileName = "TileFaceSet")]
    public class TileFaceSet : ScriptableObject
    {
        public List<TileFace> Faces = new List<TileFace>();

        private Dictionary<string, TileFace> index;

        /// <summary>Looks up a face by id, building the id-to-face map on first use.</summary>
        public TileFace Get(string id)
        {
            if (index == null)
            {
                index = new Dictionary<string, TileFace>();
                foreach (var face in Faces)
                {
                    index[face.Id] = face;
                }
            }

            return index[id];
        }

        /// <summary>
        /// True when two face ids form a matching pair: the same id, or two faces that
        /// share the same non-zero <see cref="TileFace.MatchGroup"/>.
        /// </summary>
        public bool Matches(string a, string b)
        {
            if (a == b)
            {
                return true;
            }

            var faceA = Get(a);
            var faceB = Get(b);
            return faceA.MatchGroup > 0 && faceA.MatchGroup == faceB.MatchGroup;
        }
    }
}
