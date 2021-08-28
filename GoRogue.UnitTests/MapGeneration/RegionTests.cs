﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.MapGeneration
{
    public sealed class RegionTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        /*   0 1 2 3 4 5 6 7
         * 0        /--*
         * 1   *+--+    \
         * 2     \       \
         * 3      \    +--+*
         * 4       *--/
         * 5
         */
        private readonly Point _sw = new Point(3, 4);
        private readonly Point _nw = new Point(1, 1);
        private readonly Point _ne = new Point(5, 0);
        private readonly Point _se = new Point(7, 3);
        private readonly Region _area;

        public RegionTests(ITestOutputHelper output)
        {
            _output = output;
            _area = new Region(_nw, _ne, _se, _sw);

            _output.WriteLine("Test Region:");
            _output.WriteLine(GetRegionString(_area));
        }

        [Fact(Skip = "Corners exist in two lines, and are therefore added twice. Some lines may intersect on more than one square.")]
        public void RegionFromRectangle()
        {
            var rect = new Rectangle(0, 0, 15, 10);
            var region = Region.Rectangle(rect);

            //Each Corner is included in two boundaries, so total number of points is offset by 4
            Assert.Equal(rect.Area, region.Count);
            Assert.Equal(rect.PerimeterPositions().Count(), region.OuterPoints.Count);
            Assert.Equal(new HashSet<Point>(rect.Expand(-1, -1).Positions()), new HashSet<Point>(region.InnerPoints));
        }

        [Fact]
        public void RegionFromParallelogramTopCorner()
        {
            var region = Region.ParallelogramFromTopCorner((0, 0), 15, 10);

            // TODO: Assert something useful
            _output.WriteLine("\nRegion from Parallelogram Top Corner:");
            _output.WriteLine(GetRegionString(region));
        }

        [Fact]
        public void RegionFromParallelogramBottomCorner()
        {
            var region = Region.ParallelogramFromBottomCorner((15, 10), 15, 10);

            // TODO: Assert something useful
            _output.WriteLine("\nRegion from Parallelogram Bottom Corner:");
            _output.WriteLine(GetRegionString(region));
        }

        [Fact]
        public void RegionTest()
        {
            Assert.Equal(18, _area.OuterPoints.Count);
            Assert.Equal(8, _area.InnerPoints.Count);
            Assert.Equal(5, _area.NorthBoundary.Count);
            Assert.Equal(4, _area.WestBoundary.Count);
            Assert.Equal(5, _area.SouthBoundary.Count);
            Assert.Equal(4, _area.EastBoundary.Count);
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            Assert.Equal("Region: NW(1,1)=> NE(5,0)=> SE(7,3)=> SW(3,4)", _area.ToString());
        }
        [Fact]
        public void ContainsTest()
        {
            Assert.False(_area.Contains(new Point(-5, -5)));
            Assert.False(_area.Contains(new Point(1, 2)));
            Assert.False(_area.Contains(new Point(9, 8)));
            Assert.False(_area.Contains(new Point(6, 15)));
            Assert.True(_area.Contains(new Point(2, 1)));
            Assert.True(_area.Contains(new Point(4, 1)));
            Assert.True(_area.Contains(new Point(6, 3)));
            Assert.True(_area.Contains(new Point(3, 3)));
        }
        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(_nw.X, _area.LeftAt(_nw.Y));
            Assert.Equal(4, _area.LeftAt(_area.Top));
            Assert.Equal(3, _area.LeftAt(_area.Bottom));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(_ne.X, _area.RightAt(_ne.Y));
            Assert.Equal(5, _area.RightAt(_area.Top));
            Assert.Equal(5, _area.RightAt(_area.Bottom));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(_ne.Y, _area.TopAt(_ne.X));
            Assert.Equal(_nw.Y, _area.TopAt(_nw.X));
            Assert.Equal(0, _area.TopAt(5));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(_se.Y, _area.BottomAt(_se.X));
            Assert.Equal(_sw.Y, _area.BottomAt(_sw.X));
            Assert.Equal(4, _area.BottomAt(5));
        }
        [Fact]
        public void TopTest()
        {
            Assert.Equal(0, _area.Top);
        }
        [Fact]
        public void BottomTest()
        {
            Assert.Equal(4, _area.Bottom);
        }
        [Fact]
        public void LeftTest()
        {
            Assert.Equal(1, _area.Left);
        }
        [Fact]
        public void RightTest()
        {
            Assert.Equal(7, _area.Right);
        }
        [Fact]
        public void RotateTest()
        {
            /* (0,0 & 0,1)
             * ###
             * #  ##
             * #    ##
             *  #     ##
             *  #       ##
             *   #        ##
             *   #          ## (14, 6)
             *    #         #
             *    #        #
             *     #      #
             *     #     #
             *      #   #
             *      #  #
             *       ##
             *       # (6, 14)
             */
            float degrees = 45.0f;
            Point centerOfRotation = new Point(6,14);
            Region prior = new Region(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            Region copyOfPrior = new Region(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            Region post = prior.Rotate(degrees, centerOfRotation);

            _output.WriteLine("\nRotated Region:");
            _output.WriteLine(GetRegionString(post));

            Assert.Equal(prior.Bottom, post.Bottom);
            Assert.Equal(prior.SouthWestCorner, post.SouthWestCorner);
            Assert.True(prior.Left < post.Left);
            Assert.True(prior.Right < post.Right);
            Assert.True(prior.SouthEastCorner.X < post.SouthEastCorner.X);
            Assert.True(prior.SouthEastCorner.Y < post.SouthEastCorner.Y);

            Assert.True(copyOfPrior.Matches(prior));
        }

        [Fact]
        public void InnerFromOuterPointsTest()
        {
            Assert.Equal(8, _area.InnerPoints.Count);
        }

        [Fact]
        public void FlipVerticalTest()
        {
            var newArea = _area.FlipVertical(0);
            _output.WriteLine("\nVertically Flipped Region:");
            _output.WriteLine(GetRegionString(newArea));

            //north-south values have flipped and became negative
            Assert.Equal(_sw.X, newArea.NorthWestCorner.X);
            Assert.Equal(-_sw.Y, newArea.NorthWestCorner.Y);
            Assert.Equal(_se.X, newArea.NorthEastCorner.X);
            Assert.Equal(-_se.Y, newArea.NorthEastCorner.Y);
            Assert.Equal(_ne.X, newArea.SouthEastCorner.X);
            Assert.Equal(-_ne.Y, newArea.SouthEastCorner.Y);
            Assert.Equal(_nw.X, newArea.SouthWestCorner.X);
            Assert.Equal(-_nw.Y, newArea.SouthWestCorner.Y);
        }

        [Fact]
        public void FlipHorizontalTest()
        {
            var newArea = _area.FlipHorizontal(0);
            _output.WriteLine("\nHorizontally Flipped Region:");
            _output.WriteLine(GetRegionString(newArea));

            //east-west values should have reversed and became negative
            Assert.Equal(-_ne.X, newArea.NorthWestCorner.X);
            Assert.Equal(_ne.Y, newArea.NorthWestCorner.Y);
            Assert.Equal(-_nw.X, newArea.NorthEastCorner.X);
            Assert.Equal(_nw.Y, newArea.NorthEastCorner.Y);
            Assert.Equal(-_sw.X, newArea.SouthEastCorner.X);
            Assert.Equal(_sw.Y, newArea.SouthEastCorner.Y);
            Assert.Equal(-_se.X, newArea.SouthWestCorner.X);
            Assert.Equal(_se.Y, newArea.SouthWestCorner.Y);
        }

        [Fact]
        public void TransposeTest()
        {
            var newArea = _area.Transpose(0,0);
            _output.WriteLine("\nTransposed Region:");
            _output.WriteLine(GetRegionString(newArea));

            //northeast and southwest corners should have inverted
            Assert.Equal(_nw.Y, newArea.NorthWestCorner.X);
            Assert.Equal(_nw.X, newArea.NorthWestCorner.Y);
            Assert.Equal(_sw.Y, newArea.NorthEastCorner.X);
            Assert.Equal(_sw.X, newArea.NorthEastCorner.Y);
            Assert.Equal(_se.Y, newArea.SouthEastCorner.X);
            Assert.Equal(_se.X, newArea.SouthEastCorner.Y);
            Assert.Equal(_ne.Y, newArea.SouthWestCorner.X);
            Assert.Equal(_ne.X, newArea.SouthWestCorner.Y);
        }

        [Fact]
        public void TranslateTest()
        {
            var newArea = _area.Translate(2, 3);
            _output.WriteLine("\nTranslated Region:");
            _output.WriteLine(GetRegionString(newArea));

            Assert.Equal(_ne + (2,3), newArea.NorthEastCorner);
            Assert.Equal(_nw + (2,3), newArea.NorthWestCorner);
            Assert.Equal(_se + (2,3), newArea.SouthEastCorner);
            Assert.Equal(_sw + (2,3), newArea.SouthWestCorner);
        }

        public void Dispose()
        {

        }

        private string GetRegionString(Region region)
        {
            var bounds = region.Bounds;
            var gv = new LambdaGridView<char>(region.Width, region.Height, pos =>
            {
                // Offset to grid view coords
                pos += region.Bounds.Position;

                // Print proper char
                if (region.IsCorner(pos)) return 'C';
                if (region.OuterPoints.Contains(pos)) return 'O';
                if (region.InnerPoints.Contains(pos)) return 'I';

                return '.';
            });

            var lines = gv.ToString().Split('\n');

            var final = new StringBuilder();

            // Generate x scale
            final.Append(' ');
            for (int i = 0; i < bounds.Width; i++)
                final.Append($" {i + bounds.MinExtentX}");
            final.Append('\n');

            // Add each line with y-scale value
            for (int i = 0; i < lines.Length; i++)
                final.Append($"{i + bounds.MinExtentY} {lines[i]}\n");

            return final.ToString();

        }
    }
}
