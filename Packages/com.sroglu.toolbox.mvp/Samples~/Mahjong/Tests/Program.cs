namespace Sroglu.Toolbox.Mvp.Samples.Mahjong.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Standalone test runner for the engine-free Mahjong core. It needs no Unity or
    /// dotnet — build and run with mono:
    ///
    ///   csc -nologo -warn:0 -out:/tmp/mahjong.exe \
    ///       Packages/com.sroglu.toolbox.mvp/Runtime/IModel.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Runtime/IObservableModel.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Runtime/ObservableModel.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/SlideAxis.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/TileSlot.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/TileNode.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongBoard.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongValidator.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongGenerator.cs \
    ///       Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/Tests/Program.cs \
    ///   && mono /tmp/mahjong.exe
    /// </summary>
    public static class Program
    {
        private static int passed;
        private static int failed;

        // Match by exact id — enough for the core rule tests.
        private static readonly Func<string, string, bool> MatchById = (a, b) => a == b;

        public static int Main()
        {
            CoverStackFreesBasesWhenTopRemoved();
            LeftRightBlocksMiddleUntilNeighbourRemoved();
            SlideAxisChoosesWhichSideBlocks();
            ValidateCatchesFloatingTile();
            ValidateCatchesThreeSupports();
            ValidateCatchesOddFaceCount();
            TryMatchRemovesFreePairAndRejectsCovered();
            GeneratorIsEvenAndDeterministic();

            Console.WriteLine($"\n{passed} passed, {failed} failed.");
            return failed == 0 ? 0 : 1;
        }

        // A 2x2 base with one tile resting on all four: the top is not covered and is
        // free; the four bases are covered and not free until the top is taken.
        private static void CoverStackFreesBasesWhenTopRemoved()
        {
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("a", 2, 0),
                Base("b", 0, 2),
                Base("b", 2, 2),
                Relative("c", 0, 1, 2, 3),
            };

            var board = new MahjongBoard();
            board.Load(slots, SlideAxis.X, MatchById);

            var top = board.Tiles[4];
            var base00 = board.Tiles[0];

            Check("top rests on 4", top.RestsOn.Count == 4);
            Check("top derived centre (1,1)", top.X == 1 && top.Y == 1 && top.Layer == 1);
            Check("top is not covered", !top.IsCovered);
            Check("top is free", top.IsFree);
            Check("bases are covered", board.Tiles.Take(4).All(t => t.IsCovered));
            Check("bases are not free", board.Tiles.Take(4).All(t => !t.IsFree));

            top.Removed = true;
            Check("a base becomes free once the top is gone", base00.IsFree);
        }

        // Three tiles in a row along the slide axis: the middle is blocked by both
        // neighbours until one is removed.
        private static void LeftRightBlocksMiddleUntilNeighbourRemoved()
        {
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("b", 2, 0),
                Base("a", 4, 0),
            };

            var board = new MahjongBoard();
            board.Load(slots, SlideAxis.X, MatchById);

            var middle = board.Tiles[1];
            Check("middle has a left and a right neighbour", middle.Left.Count == 1 && middle.Right.Count == 1);
            Check("middle blocked on both sides is not free", !middle.IsFree);

            board.Tiles[0].Removed = true;
            Check("middle is free once its left side opens", middle.IsFree);
        }

        // The same row of three, but with the slide axis turned 90 degrees, is no longer a
        // blocking line: along Y the row sits on one slide-coordinate with too much depth
        // gap, so nothing is a left/right neighbour and every tile is free.
        private static void SlideAxisChoosesWhichSideBlocks()
        {
            var rowAlongX = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("b", 2, 0),
                Base("a", 4, 0),
            };

            var boardY = new MahjongBoard();
            boardY.Load(rowAlongX, SlideAxis.Y, MatchById);
            Check("row along X has no Y-neighbours", boardY.Tiles[1].Left.Count == 0 && boardY.Tiles[1].Right.Count == 0);
            Check("with slide axis Y the row's middle is free", boardY.Tiles[1].IsFree);

            var columnAlongY = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("b", 0, 2),
                Base("a", 0, 4),
            };

            var boardYColumn = new MahjongBoard();
            boardYColumn.Load(columnAlongY, SlideAxis.Y, MatchById);
            var middle = boardYColumn.Tiles[1];
            Check("along Y the column's middle is blocked", !middle.IsFree);
            boardYColumn.Tiles[0].Removed = true;
            Check("along Y the column's middle frees when a neighbour goes", middle.IsFree);
        }

        private static void ValidateCatchesFloatingTile()
        {
            // The relative tile rests on a single base whose footprint does not overlap it
            // in a legal way — build a clearly floating case: rest on two far-apart bases.
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("a", 100, 0),
                Relative("b", 0, 1),
            };

            var problems = MahjongValidator.Validate(slots);
            Check("validate flags a floating tile", problems.Any(p => p.Contains("floats")));
        }

        private static void ValidateCatchesThreeSupports()
        {
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("a", 2, 0),
                Base("b", 4, 0),
                Relative("b", 0, 1, 2),
            };

            var problems = MahjongValidator.Validate(slots);
            Check("validate flags a 3-support tile", problems.Any(p => p.Contains("rests on 3")));
        }

        private static void ValidateCatchesOddFaceCount()
        {
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("a", 2, 0),
                Base("b", 4, 0),
                Base("c", 6, 0),
            };

            var problems = MahjongValidator.Validate(slots);
            Check("validate flags odd face 'b'", problems.Any(p => p.Contains("Face 'b'")));
            Check("validate flags odd face 'c'", problems.Any(p => p.Contains("Face 'c'")));
        }

        private static void TryMatchRemovesFreePairAndRejectsCovered()
        {
            var slots = new List<TileSlot>
            {
                Base("a", 0, 0),
                Base("a", 10, 0),   // far apart, both free, matching faces
                Base("b", 20, 0),
                Base("b", 22, 0),   // adjacent -> a blocking pair along X
                Relative("c", 2, 3),
            };

            var board = new MahjongBoard();
            board.Load(slots, SlideAxis.X, MatchById);

            var freeA = board.Tiles[0];
            var farA = board.Tiles[1];
            var covered = board.Tiles[2];

            Check("two distant matching tiles are both free", freeA.IsFree && farA.IsFree);
            Check("try-match removes a free matching pair", board.TryMatch(freeA, farA));
            Check("both are now removed", freeA.Removed && farA.Removed);

            Check("covered tile is not free", !covered.IsFree);
            Check("try-match rejects a covered tile", !board.TryMatch(covered, board.Tiles[3]));
        }

        private static void GeneratorIsEvenAndDeterministic()
        {
            var faces = new List<string> { "a", "b", "c" };
            var first = MahjongGenerator.Fill(6, faces, 12345);
            var again = MahjongGenerator.Fill(6, faces, 12345);
            var other = MahjongGenerator.Fill(6, faces, 999);

            var counts = first.GroupBy(f => f).ToDictionary(g => g.Key, g => g.Count());
            Check("every generated face count is even", counts.Values.All(c => c % 2 == 0));
            Check("same seed gives the same board", first.SequenceEqual(again));
            Check("a different seed shuffles differently", !first.SequenceEqual(other));
        }

        private static TileSlot Base(string faceId, int x, int y) =>
            new TileSlot { FaceId = faceId, IsBase = true, X = x, Y = y };

        private static TileSlot Relative(string faceId, params int[] restsOn) =>
            new TileSlot { FaceId = faceId, IsBase = false, RestsOn = restsOn };

        private static void Check(string label, bool condition)
        {
            if (condition)
            {
                passed++;
                Console.WriteLine($"  PASS  {label}");
            }
            else
            {
                failed++;
                Console.WriteLine($"  FAIL  {label}");
            }
        }
    }
}
