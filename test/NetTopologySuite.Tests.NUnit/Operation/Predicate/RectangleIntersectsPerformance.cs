﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Predicate
{
    [TestFixture]
    public class RectangleIntersectsPerformanceTest
    {
        private const int MaxIter = 10;

        private static readonly PrecisionModel Pm = new PrecisionModel();
        private static readonly GeometryFactory Fact = new GeometryFactory(Pm, 0);

        [Test]
        public void Test()
        {
            Test(500);
            Test(1000);
            Test(2000);
            Test(100000);
        }

        private static void Test(int nPts)
        {
            const double size = 100;
            var origin = new Coordinate(0, 0);
            var sinePoly = CreateSineStar(origin, size, nPts).Boundary;
            /**
             * Make the geometry "crinkly" by rounding off the points.
             * This defeats the  MonotoneChain optimization in the full relate
             * algorithm, and provides a more realistic test.
             */
            var sinePolyCrinkly = GeometryPrecisionReducer.Reduce(sinePoly,
                                                                  new PrecisionModel(size/10));
            var target = sinePolyCrinkly;

            TestRectangles(target, 100);
        }

        private static void TestRectangles(Geometry target, int nRect)
        {
            var rects = CreateRectangles(target.EnvelopeInternal, nRect);
            Test(rects, target);
        }

        private static void Test(ICollection<Geometry> rect, Geometry g)
        {
            Console.WriteLine("Target # pts: " + g.NumPoints
                              + "  -- # Rectangles: " + rect.Count
                );

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < MaxIter; i++)
            {
                foreach (var t in rect)
                {
                    t.Intersects(g);
                }
            }
            sw.Stop();
            Console.WriteLine("Finished in " + sw.Elapsed);
            Console.WriteLine();
        }

        /// <summary>
        /// Creates a set of rectangular Polygons which
        /// cover the given envelope.
        /// The rectangles
        /// At least nRect rectangles are created.
        /// </summary>
        private static Geometry[] CreateRectangles(Envelope env, int nRect)
        {
            int nSide = 1 + (int) Math.Sqrt(nRect);
            double dx = env.Width/nSide;
            double dy = env.Height/nSide;

            var rectList = new List<Geometry>();
            for (int i = 0; i < nSide; i++)
            {
                for (int j = 0; j < nSide; j++)
                {
                    double baseX = env.MinX + i*dx;
                    double baseY = env.MinY + j*dy;
                    var envRect = new Envelope(
                        baseX, baseX + dx,
                        baseY, baseY + dy);
                    var rect = Fact.ToGeometry(envRect);
                    rectList.Add(rect);
                }
            }
            return GeometryFactory.ToGeometryArray(rectList);
        }

        private static Geometry CreateSineStar(Coordinate origin, double size, int nPts)
        {
            var gsf = new SineStarFactory
                          {Centre = origin, Size = size, NumPoints = nPts, ArmLengthRatio = 2, NumArms = 20};
            var poly = gsf.CreateSineStar();
            return poly;
        }

    }
}