using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.PathFinding
{
    /// <summary>
    /// Grid A* pathfinding over a rectangular <c>width x height</c> cell grid.
    /// Self-contained: uses its own internal binary min-heap so importing this
    /// package alone is enough (no dependency on any collections package).
    /// </summary>
    public static class AStar
    {
        /// <summary>
        /// Finds the shortest walkable path from <paramref name="start"/> to
        /// <paramref name="goal"/> on a grid.
        /// </summary>
        /// <param name="start">Start cell (inclusive in the result).</param>
        /// <param name="goal">Goal cell (inclusive in the result).</param>
        /// <param name="width">Grid width in cells; valid x is 0..width-1.</param>
        /// <param name="height">Grid height in cells; valid y is 0..height-1.</param>
        /// <param name="isWalkable">Predicate deciding whether a cell can be entered.</param>
        /// <param name="allowDiagonal">
        /// When false (default) only the 4 orthogonal neighbors are considered and the
        /// heuristic is Manhattan distance. When true the 8 neighbors are considered and
        /// the heuristic is octile distance.
        /// </param>
        /// <returns>
        /// The path from <paramref name="start"/> to <paramref name="goal"/> inclusive,
        /// or an empty list if no path exists (or either endpoint is out of bounds /
        /// not walkable).
        /// </returns>
        public static List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int goal,
            int width,
            int height,
            Func<Vector2Int, bool> isWalkable,
            bool allowDiagonal = false)
        {
            var path = new List<Vector2Int>();

            if (isWalkable == null || width <= 0 || height <= 0)
            {
                return path;
            }

            if (!InBounds(start, width, height) || !InBounds(goal, width, height))
            {
                return path;
            }

            if (!isWalkable(start) || !isWalkable(goal))
            {
                return path;
            }

            if (start == goal)
            {
                path.Add(start);
                return path;
            }

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
            var closed = new HashSet<Vector2Int>();
            var open = new MinHeap();
            open.Push(start, Heuristic(start, goal, allowDiagonal));

            while (open.Count > 0)
            {
                Vector2Int current = open.Pop();

                if (current == goal)
                {
                    return Reconstruct(cameFrom, current);
                }

                if (!closed.Add(current))
                {
                    continue;
                }

                float currentG = gScore[current];

                foreach (Vector2Int neighbor in Neighbors(current, width, height, allowDiagonal, isWalkable))
                {
                    if (closed.Contains(neighbor))
                    {
                        continue;
                    }

                    float stepCost = (neighbor.x != current.x && neighbor.y != current.y) ? 1.41421356f : 1f;
                    float tentativeG = currentG + stepCost;

                    if (gScore.TryGetValue(neighbor, out float knownG) && tentativeG >= knownG)
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    open.Push(neighbor, tentativeG + Heuristic(neighbor, goal, allowDiagonal));
                }
            }

            return path;
        }

        private static bool InBounds(Vector2Int cell, int width, int height)
        {
            return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b, bool allowDiagonal)
        {
            int dx = Math.Abs(a.x - b.x);
            int dy = Math.Abs(a.y - b.y);

            if (!allowDiagonal)
            {
                // Manhattan distance for 4-neighbor movement.
                return dx + dy;
            }

            // Octile distance for 8-neighbor movement.
            int min = Math.Min(dx, dy);
            int max = Math.Max(dx, dy);
            return (max - min) + 1.41421356f * min;
        }

        private static IEnumerable<Vector2Int> Neighbors(
            Vector2Int cell,
            int width,
            int height,
            bool allowDiagonal,
            Func<Vector2Int, bool> isWalkable)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal && !allowDiagonal)
                    {
                        continue;
                    }

                    var neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (!InBounds(neighbor, width, height) || !isWalkable(neighbor))
                    {
                        continue;
                    }

                    yield return neighbor;
                }
            }
        }

        private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };

            while (cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Self-contained binary min-heap keyed by an f-score. Kept private to this
        /// package so pathfinding has no external collection dependency.
        /// </summary>
        private sealed class MinHeap
        {
            private readonly List<Vector2Int> _cells = new List<Vector2Int>();
            private readonly List<float> _priorities = new List<float>();

            public int Count => _cells.Count;

            public void Push(Vector2Int cell, float priority)
            {
                _cells.Add(cell);
                _priorities.Add(priority);
                SiftUp(_cells.Count - 1);
            }

            public Vector2Int Pop()
            {
                Vector2Int top = _cells[0];
                int last = _cells.Count - 1;

                _cells[0] = _cells[last];
                _priorities[0] = _priorities[last];
                _cells.RemoveAt(last);
                _priorities.RemoveAt(last);

                if (_cells.Count > 0)
                {
                    SiftDown(0);
                }

                return top;
            }

            private void SiftUp(int index)
            {
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (_priorities[index] >= _priorities[parent])
                    {
                        break;
                    }

                    Swap(index, parent);
                    index = parent;
                }
            }

            private void SiftDown(int index)
            {
                int count = _cells.Count;

                while (true)
                {
                    int left = (2 * index) + 1;
                    int right = left + 1;
                    int smallest = index;

                    if (left < count && _priorities[left] < _priorities[smallest])
                    {
                        smallest = left;
                    }

                    if (right < count && _priorities[right] < _priorities[smallest])
                    {
                        smallest = right;
                    }

                    if (smallest == index)
                    {
                        break;
                    }

                    Swap(index, smallest);
                    index = smallest;
                }
            }

            private void Swap(int a, int b)
            {
                (_cells[a], _cells[b]) = (_cells[b], _cells[a]);
                (_priorities[a], _priorities[b]) = (_priorities[b], _priorities[a]);
            }
        }
    }
}
