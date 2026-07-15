using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.Grids
{
    /// <summary>
    /// A rectangular grid of <typeparamref name="T"/> cells addressed by integer
    /// (x, y) coordinates, with bounds checks, neighbor enumeration, and an optional
    /// mapping to world space via a cell size and origin.
    /// </summary>
    /// <typeparam name="T">The value stored in each cell.</typeparam>
    public class Grid<T>
    {
        private readonly T[,] _cells;
        private readonly float _cellSize;
        private readonly Vector3 _origin;

        /// <summary>Number of columns (x extent).</summary>
        public int Width { get; }

        /// <summary>Number of rows (y extent).</summary>
        public int Height { get; }

        /// <summary>
        /// Creates a grid of the given size with a default cell size of 1 and origin at zero.
        /// </summary>
        /// <param name="factory">Optional per-cell initializer called with (x, y).</param>
        /// <exception cref="ArgumentOutOfRangeException">If width or height is not positive.</exception>
        public Grid(int width, int height, Func<int, int, T> factory = null)
            : this(width, height, 1f, Vector3.zero, factory) { }

        /// <summary>
        /// Creates a grid with an explicit world mapping.
        /// </summary>
        /// <param name="cellSize">Side length of a cell in world units.</param>
        /// <param name="origin">World position of cell (0, 0)'s lower-left corner.</param>
        /// <param name="factory">Optional per-cell initializer called with (x, y).</param>
        /// <exception cref="ArgumentOutOfRangeException">If width or height is not positive.</exception>
        public Grid(int width, int height, float cellSize, Vector3 origin, Func<int, int, T> factory = null)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _cellSize = cellSize;
            _origin = origin;
            _cells = new T[width, height];

            if (factory != null)
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        _cells[x, y] = factory(x, y);
        }

        /// <summary>Gets or sets the value at (x, y).</summary>
        /// <exception cref="IndexOutOfRangeException">If the coordinate is out of bounds.</exception>
        public T this[int x, int y]
        {
            get { RequireInBounds(x, y); return _cells[x, y]; }
            set { RequireInBounds(x, y); _cells[x, y] = value; }
        }

        /// <summary>Gets or sets the value at the given cell coordinate.</summary>
        /// <exception cref="IndexOutOfRangeException">If the coordinate is out of bounds.</exception>
        public T this[Vector2Int c]
        {
            get => this[c.x, c.y];
            set => this[c.x, c.y] = value;
        }

        /// <summary>Returns whether (x, y) is inside the grid.</summary>
        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>Returns whether the cell coordinate is inside the grid.</summary>
        public bool InBounds(Vector2Int c) => InBounds(c.x, c.y);

        /// <summary>
        /// Reads the value at (x, y). Returns <c>false</c> with <paramref name="value"/> set to
        /// <c>default</c> if the coordinate is out of bounds.
        /// </summary>
        public bool TryGet(int x, int y, out T value)
        {
            if (!InBounds(x, y)) { value = default; return false; }
            value = _cells[x, y];
            return true;
        }

        /// <summary>
        /// Enumerates the in-bounds neighbor coordinates of (x, y): the 4 orthogonal cells,
        /// or all 8 surrounding cells when <paramref name="diagonal"/> is <c>true</c>.
        /// </summary>
        public IEnumerable<Vector2Int> Neighbors(int x, int y, bool diagonal = false)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (!diagonal && dx != 0 && dy != 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (InBounds(nx, ny)) yield return new Vector2Int(nx, ny);
                }
            }
        }

        /// <summary>Enumerates the in-bounds neighbors of a cell coordinate.</summary>
        public IEnumerable<Vector2Int> Neighbors(Vector2Int c, bool diagonal = false)
            => Neighbors(c.x, c.y, diagonal);

        /// <summary>Invokes <paramref name="action"/> for every cell with its (x, y, value).</summary>
        public void ForEach(Action<int, int, T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    action(x, y, _cells[x, y]);
        }

        /// <summary>Returns the world position of the center of cell (x, y).</summary>
        public Vector3 CellToWorld(int x, int y)
            => _origin + new Vector3((x + 0.5f) * _cellSize, (y + 0.5f) * _cellSize, 0f);

        /// <summary>Returns the cell coordinate containing the given world position.</summary>
        public Vector2Int WorldToCell(Vector3 world)
        {
            Vector3 local = world - _origin;
            return new Vector2Int(
                Mathf.FloorToInt(local.x / _cellSize),
                Mathf.FloorToInt(local.y / _cellSize));
        }

        private void RequireInBounds(int x, int y)
        {
            if (!InBounds(x, y))
                throw new IndexOutOfRangeException($"Cell ({x}, {y}) is outside the {Width}x{Height} grid.");
        }
    }
}
