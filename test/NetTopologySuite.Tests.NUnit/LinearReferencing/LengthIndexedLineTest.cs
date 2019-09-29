using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests the <see cref="LengthIndexedLine" /> class
    /// </summary>
    [TestFixture]
    public class LengthIndexedLineTest : AbstractIndexedLineTest
    {
        [Test]
        public void TestExtractLineBeyondRange()
        {
            CheckExtractLine("LINESTRING (0 0, 10 10)", -100, 100, "LINESTRING (0 0, 10 10)");
        }

        [Test]
        public void TestExtractLineReverse()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", 9, 1, "LINESTRING (9 0, 1 0)");
        }

        [Test]
        public void TestExtractLineReverseMulti()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                19, 1, "MULTILINESTRING ((29 0, 25 0, 20 0), (10 0, 1 0))");
        }

        [Test]
        public void TestExtractLineNegative()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", -9, -1, "LINESTRING (1 0, 9 0)");
        }

        [Test]
        public void TestExtractLineNegativeReverse()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", -1, -9, "LINESTRING (9 0, 1 0)");
        }

        [Test]
        public void TestExtractLineIndexAtEndpoint()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                10, -1, "LINESTRING (20 0, 25 0, 29 0)");
        }

        /**
         * Tests that leading and trailing zero-length sublines are trimmed in the computed result,
         * and that zero-length extracts return the lowest extracted zero-length line
         */

        [Test]
        public void TestExtractLineIndexAtEndpointWithZeroLenComponents()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, -1, "LINESTRING (20 0, 25 0, 29 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                5, 10, "LINESTRING (5 0, 10 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, 10, "LINESTRING (10 0, 10 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (10 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, -10, "LINESTRING (10 0, 10 0)");
        }

        [Test]
        public void TestExtractLineBothIndicesAtEndpoint()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                10, 10, "LINESTRING (10 0, 10 0)");
        }

        [Test]
        public void TestExtractLineBothIndicesAtEndpointNegative()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                -10, 10, "LINESTRING (10 0, 10 0)");
        }

        /**
         * From GEOS Ticket #323
         */
        [Test]
        public void TestProjectExtractPoint()
        {
            var linearGeom = Read("MULTILINESTRING ((0 2, 0 0), (-1 1, 1 1))");
            var indexedLine = new LengthIndexedLine(linearGeom);
            double index = indexedLine.Project(new Coordinate(1, 0));
            var pt = indexedLine.ExtractPoint(index);
            Assert.IsTrue(pt.Equals(new Coordinate(0, 0)));
        }

        [Test]
        public void TestExtractPointBeyondRange()
        {
            var linearGeom = Read("LINESTRING (0 0, 10 10)");
            var indexedLine = new LengthIndexedLine(linearGeom);
            var pt = indexedLine.ExtractPoint(100);
            Assert.IsTrue(pt.Equals(new Coordinate(10, 10)));

            var pt2 = indexedLine.ExtractPoint(0);
            Assert.IsTrue(pt2.Equals(new Coordinate(0, 0)));
        }

        [Test]
        public void TestProjectPointWithDuplicateCoords()
        {
            var linearGeom = Read("LINESTRING (0 0, 10 0, 10 0, 20 0)");
            var indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(10, 1));
            Assert.IsTrue(projIndex == 10.0);
        }

        /// <summary>
        /// These tests work for LengthIndexedLine, but not LocationIndexedLine
        /// </summary>
        [Test]
        public void TestOffsetStartPointRepeatedPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }

        /// <summary>
        /// Tests that z values are interpolated
        /// </summary>
        [Test]
        public void TestComputeZ()
        {
            var linearGeom = Read("LINESTRING (0 0 0, 10 10 10)");
            var indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(5, 5));
            var projPt = indexedLine.ExtractPoint(projIndex);
            //    System.out.println(projPt);
            Assert.That(projPt, Is.InstanceOf<CoordinateZ>());
            Assert.IsTrue(((CoordinateZ)projPt).Equals3D(new CoordinateZ(5, 5, 5)));
        }

        /// <summary>
        /// Tests that if the input does not have Z ordinates, neither does the output.
        /// </summary>
        [Test]
        public void TestComputeZNaN()
        {
            var linearGeom = Read("LINESTRING (0 0, 10 10 10)");
            var indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(5, 5));
            var projPt = indexedLine.ExtractPoint(projIndex);
            Assert.IsTrue(double.IsNaN(projPt.Z));
        }

        private void CheckExtractLine(string wkt, double start, double end, string expected)
        {
            var linearGeom = Read(wkt);
            var indexedLine = new LengthIndexedLine(linearGeom);
            var result = indexedLine.ExtractLine(start, end);
            CheckExpected(result, expected);
        }

        protected override Geometry IndicesOfThenExtract(Geometry linearGeom, Geometry subLine)
        {
            var indexedLine = new LengthIndexedLine(linearGeom);
            double[] loc = indexedLine.IndicesOf(subLine);
            var result = indexedLine.ExtractLine(loc[0], loc[1]);
            return result;
        }

        protected override bool IndexOfAfterCheck(Geometry linearGeom, Coordinate testPt)
        {
            var indexedLine = new LengthIndexedLine(linearGeom);

            // check locations are consecutive
            double loc1 = indexedLine.IndexOf(testPt);
            double loc2 = indexedLine.IndexOfAfter(testPt, loc1);
            if (loc2 <= loc1) return false;

            // check extracted points are the same as the input
            var pt1 = indexedLine.ExtractPoint(loc1);
            var pt2 = indexedLine.ExtractPoint(loc2);
            if (!pt1.Equals2D(testPt)) return false;
            if (!pt2.Equals2D(testPt)) return false;

            return true;
        }

        protected override Coordinate ExtractOffsetAt(Geometry linearGeom, Coordinate testPt, double offsetDistance)
        {
            var indexedLine = new LengthIndexedLine(linearGeom);
            double index = indexedLine.IndexOf(testPt);
            return indexedLine.ExtractPoint(index, offsetDistance);
        }
    }
}
