﻿using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.MapAreaFinders.Implementations
{
    /// <summary>
    /// Variation of <see cref="OriginalMapAreaFinder"/> that uses the areas themselves in place of map-sized array for
    /// contains operations; using the known-size Point hashing algorithm.
    /// </summary>
    public class AreaContainsSizeHashMapAreaFinder
    {
        private MultiArea _foundAreas = null!;

        public AdjacencyRule AdjacencyMethod;

        public IGridView<bool> AreasView;

        public AreaContainsSizeHashMapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod)
        {
            AreasView = areasView;
            AdjacencyMethod = adjacencyMethod;
        }

        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod)
        {
            var areaFinder = new AreaContainsSizeHashMapAreaFinder(map, adjacencyMethod);
            return areaFinder.MapAreas();
        }

        public IEnumerable<Area> MapAreas()
        {
            _foundAreas = new MultiArea();

            for (var x = 0; x < AreasView.Width; x++)
                for (var y = 0; y < AreasView.Height; y++)
                {
                    var area = Visit(new Point(x, y));

                    if (!ReferenceEquals(null, area) && area.Count != 0)
                        yield return area;
                }
        }

        private Area? Visit(Point position)
        {
            // Don't bother allocating a MapArea, because the starting point isn't valid.
            if (!AreasView[position])
                return null;

            // Can't be in multiple areas
            if (_foundAreas.Contains(position))
                return null;

            var stack = new Stack<Point>();
            var area = new Area(new KnownSizeHasher(AreasView.Width));
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                // Already visited, or not part of any mapArea.  Also only called from functions that have allocated
                // visited
                if (area.Contains(position) || !AreasView[position])
                    continue;

                area.Add(position);

                foreach (var c in AdjacencyMethod.Neighbors(position))
                {
                    // Out of bounds, thus not actually a neighbor
                    if (c.X < 0 || c.Y < 0 || c.X >= AreasView.Width || c.Y >= AreasView.Height)
                        continue;

                    if (AreasView[c] && !area.Contains(c))
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
