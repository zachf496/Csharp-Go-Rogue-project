﻿using Troschuetz.Random;
using System.Collections.Generic;
using SadRogue.Primitives;
using System.Runtime.CompilerServices;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.Random;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Generates a maze in the wall areas of a map, using crawlers that walk the map carving tunnels.
    ///
    /// Context Components Required:
    ///     - None
    ///
    /// Context Components Added/Used:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ContextComponents.ItemList{Area}"/></term>
    /// <description>"Tunnels"</description>
    /// </item>
    /// <item>
    /// <term><see cref="ISettableMapView{T}"/> where T is bool</term>
    /// <description>"WallFloor"</description>
    /// </item>
    /// </list>
    ///
    /// In the case of both components, existing components are used if they are present; new ones are added if not.
    /// </summary>
    /// <remarks>
    /// This generation steps generates mazes, and adds the tunnels made to the <see cref="ContextComponents.ItemList{Area}"/> context component with
    /// the proper tag (if one is specified) on the <see cref="GenerationContext"/>.  If no such component exists,
    /// one is created.  It also sets the all locations inside the tunnels to true in the map's "WallFloor" map view context component.  If the
    /// GenerationContext has an existing "WallFloor" context component, that component is used.  If not, an <see cref="ArrayMap{T}"/> where T is bool is
    /// created and added to the map context, whose width/height match <see cref="GenerationContext.Width"/>/<see cref="GenerationContext.Height"/>.
    /// </remarks>
    [PublicAPI]
    public class MazeGeneration : GenerationStep
    {
        /// <summary>
        /// RNG to use for maze generation.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Out of 100, how much to increase the chance of a crawler changing direction each step.  Once it changes direction, it resets to 0 and increases
        /// by this amount.  Defaults to 10.
        /// </summary>
        public ushort CrawlerChangeDirectionImprovement = 10;

        /// <summary>
        /// Optional tag that must be associated with the component used to store tunnels generated by this algorithm.
        /// </summary>
        public readonly string? TunnelsComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// Creates a new maze generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="MazeGeneration"/>.</param>
        /// /// <param name="tunnelsComponentTag">Optional tag that must be associated with the component used to store tunnels/mazes created by the algorithm.  Defaults to "Tunnels".</param>
        /// <param name="wallFloorComponentTag">Optional tag that must be associated with the map view component used to store/set floor/wall status.  Defaults to "WallFloor".</param>
        public MazeGeneration(string? name = null, string? tunnelsComponentTag = "Tunnels", string? wallFloorComponentTag = "WallFloor")
            : base(name)
        {
            TunnelsComponentTag = tunnelsComponentTag;
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (CrawlerChangeDirectionImprovement > 100)
                throw new InvalidConfigurationException(this, nameof(CrawlerChangeDirectionImprovement), "The value must be a valid percent (between 0 and 100).");

            // Logic implemented from http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/

            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetComponentOrNew<ISettableMapView<bool>>(
                newFunc: () => new ArrayMap<bool>(context.Width, context.Height),
                tag: WallFloorComponentTag
            );

            // Get or create/add a tunnel list context component
            var tunnelList = context.GetComponentOrNew(
                newFunc: () => new ContextComponents.ItemList<Area>(),
                tag: TunnelsComponentTag
            );


            var crawlers = new List<Crawler>();
            Point empty = FindEmptySquare(wallFloorContext, RNG);
            
            while (empty != Point.None)
            {
                var crawler = new Crawler();
                crawlers.Add(crawler);
                crawler.MoveTo(empty);
                var startedCrawler = true;
                ushort percentChangeDirection = 0;

                while (crawler.Path.Count != 0)
                {
                    // Dig this position
                    wallFloorContext[crawler.CurrentPosition] = true;

                    // Get valid directions (basically is any position outside the map or not?
                    var points = AdjacencyRule.Cardinals.NeighborsClockwise(crawler.CurrentPosition).ToArray();
                    var directions = AdjacencyRule.Cardinals.DirectionsOfNeighborsClockwise(Direction.None).ToList();

                    var validDirections = new bool[4];

                    // Rule out any valid directions based on their position. Only process cardinals, do not use diagonals
                    for (var i = 0; i < 4; i++)
                        validDirections[i] = IsPointWallsExceptSource(wallFloorContext, points[i], directions[i] + 4);

                    // If not a new crawler, exclude where we came from
                    if (!startedCrawler)
                        validDirections[directions.IndexOf(crawler.Facing + 4)] = false;

                    // Do we have any valid direction to go?
                    if (validDirections[0] || validDirections[1] || validDirections[2] || validDirections[3])
                    {
                        int index;

                        // Are we just starting this crawler? OR Is the current crawler facing
                        // direction invalid?
                        if (startedCrawler || validDirections[directions.IndexOf(crawler.Facing)] == false)
                        {
                            // Just get anything
                            index = GetDirectionIndex(validDirections, RNG);
                            crawler.Facing = directions[index];
                            percentChangeDirection = 0;
                            startedCrawler = false;
                        }
                        else
                        {
                            // Increase probability we change direction
                            percentChangeDirection += CrawlerChangeDirectionImprovement;

                            if (RNG.PercentageCheck(percentChangeDirection))
                            {
                                index = GetDirectionIndex(validDirections, RNG);
                                crawler.Facing = directions[index];
                                percentChangeDirection = 0;
                            }
                            else
                                index = directions.IndexOf(crawler.Facing);
                        }

                        crawler.MoveTo(points[index]);
                    }
                    else
                        crawler.Backtrack();
                }

                empty = FindEmptySquare(wallFloorContext, RNG);
            }

            // Add appropriate items to the tunnels list
            tunnelList.AddItems(crawlers.Select(c => c.AllPositions).Where(a => a.Count != 0), Name);
        }

        private static Point FindEmptySquare(IMapView<bool> map, IGenerator rng)
        {
            // Try random positions first
            for (int i = 0; i < 100; i++)
            {
                var location = map.RandomPosition(false, rng);

                if (IsPointConsideredEmpty(map, location))
                    return location;
            }

            // Start looping through every single one
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                var location = Point.FromIndex(i, map.Width);

                if (IsPointConsideredEmpty(map, location))
                    return location;
            }

            return Point.None;
        }

        private static int GetDirectionIndex(bool[] validDirections, IGenerator rng)
        {
            // 10 tries to find random ok valid
            bool randomSuccess = false;
            int tempDirectionIndex = 0;

            for (int randomCounter = 0; randomCounter < 10; randomCounter++)
            {
                tempDirectionIndex = rng.Next(4);
                if (!validDirections[tempDirectionIndex]) continue;

                randomSuccess = true;
                break;
            }

            if (randomSuccess) return tempDirectionIndex;

            // Couldn't find an active valid, so just run through each one
            if (validDirections[0])
                tempDirectionIndex = 0;
            else if (validDirections[1])
                tempDirectionIndex = 1;
            else if (validDirections[2])
                tempDirectionIndex = 2;
            else
                tempDirectionIndex = 3;

            return tempDirectionIndex;
        }

        // TODO: Create random position function that has a fallback for if random fails after max retries
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPointConsideredEmpty(IMapView<bool> map, Point location) => !IsPointMapEdge(map, location) &&  // exclude outer ridge of map
                   location.X % 2 != 0 && location.Y % 2 != 0 && // check is odd number position
                   IsPointSurroundedByWall(map, location) && // make sure is surrounded by a wall.
                   !map[location]; // The location is a wall

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPointMapEdge(IMapView<bool> map, Point location, bool onlyEdgeTest = false)
        {
            if (onlyEdgeTest)
                return location.X == 0 || location.X == map.Width - 1 || location.Y == 0 || location.Y == map.Height - 1;
            return location.X <= 0 || location.X >= map.Width - 1 || location.Y <= 0 || location.Y >= map.Height - 1;
        }

        private static bool IsPointSurroundedByWall(IMapView<bool> map, Point location)
        {
            var points = AdjacencyRule.EightWay.Neighbors(location);

            var mapBounds = map.Bounds();
            foreach (var point in points)
            {
                if (!mapBounds.Contains(point))
                    return false;

                if (map[point])
                    return false;
            }

            return true;
        }

        private static bool IsPointWallsExceptSource(IMapView<bool> map, Point location, Direction sourceDirection)
        {
            // exclude the outside of the map
            var mapInner = map.Bounds().Expand(-1, -1);

            if (!mapInner.Contains(location))
                // Shortcut out if this location is part of the map edge.
                return false;

            // Get map indexes for all surrounding locations
            var index = AdjacencyRule.EightWay.DirectionsOfNeighborsClockwise().ToArray();

            Direction[] skipped;

            if (sourceDirection == Direction.Right)
                skipped = new[] { sourceDirection, Direction.UpRight, Direction.DownRight };
            else if (sourceDirection == Direction.Left)
                skipped = new[] { sourceDirection, Direction.UpLeft, Direction.DownLeft };
            else if (sourceDirection == Direction.Up)
                skipped = new[] { sourceDirection, Direction.UpRight, Direction.UpLeft };
            else
                skipped = new[] { sourceDirection, Direction.DownRight, Direction.DownLeft };

            foreach (var direction in index)
            {
                if (skipped[0] == direction || skipped[1] == direction || skipped[2] == direction)
                    continue;

                if (!map.Bounds().Contains(location + direction) || map[location + direction])
                    return false;
            }

            return true;
        }

        private class Crawler
        {
            public readonly Area AllPositions = new Area();
            public Point CurrentPosition = new Point(0, 0);
            public Direction Facing = Direction.Up;
            public readonly Stack<Point> Path = new Stack<Point>();

            public void Backtrack()
            {
                if (Path.Count != 0)
                    CurrentPosition = Path.Pop();
            }

            public void MoveTo(Point position)
            {
                Path.Push(position);
                AllPositions.Add(position);
                CurrentPosition = position;
            }
        }
    }
}
