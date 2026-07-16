using Sroglu.Toolbox.Assets;
using UnityEngine;

namespace Sroglu.Toolbox.Spawning.Samples
{
    /// <summary>
    /// A marker component to put on the demo prefab's root. The prefab is registered
    /// in an <see cref="AssetCatalog"/> under an id (for example "enemy.goblin"); the
    /// typed spawn path finds it by this component on the root.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
    }

    /// <summary>
    /// A code-only walkthrough of the Spawner API showing that the string-id path and
    /// the typed <see cref="Spawner.Spawn{T}"/> path resolve to the SAME prefab and
    /// therefore share ONE pool.
    ///
    /// In a real project the <see cref="catalog"/> is authored in the inspector: an
    /// entry maps the id (here "enemy.goblin") to a prefab whose root carries an
    /// <see cref="Enemy"/> component. This demo spawns by id, despawns, then spawns by
    /// type — because pools are keyed by prefab reference, the typed spawn hands back
    /// the very instance the id spawn just recycled.
    /// </summary>
    public class SpawnerDemo : MonoBehaviour
    {
        [SerializeField]
        private AssetCatalog catalog;

        [SerializeField]
        private string enemyId = "enemy.goblin";

        private void Start()
        {
            var registry = new AssetRegistry(catalog);
            var spawner = new Spawner(registry, poolRoot: transform);

            // Spawn by string id.
            GameObject byId = spawner.Spawn(enemyId, Vector3.zero, Quaternion.identity);
            Debug.Log($"Spawned by id '{enemyId}': {byId.name}");

            // Recycle it back into the prefab's pool.
            spawner.Despawn(byId);

            // Spawn the same prefab by the component on its root. Same prefab => same
            // pool, so this reuses the instance just despawned.
            Enemy byType = spawner.Spawn<Enemy>(new Vector3(2f, 0f, 0f), Quaternion.identity);
            Debug.Log($"Spawned by type Enemy: {byType.name} " +
                      $"(same instance as the id spawn: {ReferenceEquals(byId, byType.gameObject)})");

            // Despawn via the typed convenience overload.
            spawner.Despawn(byType);

            // Tear everything down.
            spawner.Clear();
        }
    }
}
