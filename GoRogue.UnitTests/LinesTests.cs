using System;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    public class LineTests
    {
        private static readonly (int, int) _end = (8, 6);
        private const int _mapHeight = 10;
        private const int _mapWidth = 10;
        private static readonly (int, int) _start = (1, 1);
        private readonly System.Random _random = new System.Random();
        List<Point> _hardCodedRange = new List<Point>();

        private readonly Point sw = new Point(3, 4);
        private readonly Point nw = new Point(1, 1);
        private readonly Point ne = new Point(5, 0);
        private readonly Point se = new Point(7, 3);

        public LineTests()
        {
            _hardCodedRange.AddRange(Lines.Get(nw, ne));
            _hardCodedRange.AddRange(Lines.Get(ne, se));
            _hardCodedRange.AddRange(Lines.Get(se, sw));
            _hardCodedRange.AddRange(Lines.Get(sw, nw));
        }
        private Point RandomPosition()
        {
            var x = _random.Next(0, _mapWidth);
            var y = _random.Next(0, _mapHeight);
            return (x, y);
        }

        private static void DrawLine(Point start, Point end, int width, int height, Lines.Algorithm type)
        {
            var myChars = new char[width, height];

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    myChars[x, y] = x == 0 || y == 0 || x == width - 1 || y == height - 1 ? '#' : '.';

            foreach (var point in Lines.Get(start.X, start.Y, end.X, end.Y, type))
                myChars[point.X, point.Y] = '*';

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    Console.Write(myChars[x, y]);

                Console.WriteLine();
            }
        }

        [Fact]
        public void ManualBresenhamTest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.Bresenham);

        [Fact]
        public void ManualDDATest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.DDA);

        [Fact]
        public void ManualOrthoTest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.Orthogonal);

        [Fact]
        public void OrderedBresenhamTest()
        {
            //Random. rand = Random.GlobalRandom.DefaultRNG;
            for (var i = 0; i < 100; i++)
            {
                var start = RandomPosition();
                var end = RandomPosition();

                var line = Lines.Get(start, end, Lines.Algorithm.BresenhamOrdered).ToList();
                Assert.Equal(start, line[0]);
            }
        }
        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(nw.X, _hardCodedRange.LeftAt(nw.Y));
            Assert.Equal(4, _hardCodedRange.LeftAt(0));
            Assert.Equal(3, _hardCodedRange.LeftAt(4));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(ne.X, _hardCodedRange.RightAt(ne.Y));
            Assert.Equal(se.X, _hardCodedRange.RightAt(se.Y));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(ne.Y, _hardCodedRange.TopAt(ne.X));
            Assert.Equal(nw.Y, _hardCodedRange.TopAt(nw.X));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(se.Y, _hardCodedRange.BottomAt(se.X));
            Assert.Equal(sw.Y, _hardCodedRange.BottomAt(sw.X));
        }
    }
}
