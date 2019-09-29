using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index.Sweepline;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a <c>SweepLineIndex</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class SweeplineNestedRingTester
    {
        private readonly GeometryGraph graph;  // used to find non-node vertices
        private readonly List<LinearRing> rings = new List<LinearRing>();
        private Envelope totalEnv = new Envelope();
        private SweepLineIndex sweepLine;
        private Coordinate nestedPt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="graph"></param>
        public SweeplineNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate NestedPoint => nestedPt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="ring"></param>
        public void Add(LinearRing ring)
        {
            rings.Add(ring);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            BuildIndex();
            var action = new OverlapAction(this);
            sweepLine.ComputeOverlaps(action);
            return action.IsNonNested;
        }

        /// <summary>
        ///
        /// </summary>
        private void BuildIndex()
        {
            sweepLine = new SweepLineIndex();
            foreach (var ring in rings)
            {
                var env = ring.EnvelopeInternal;
                var sweepInt = new SweepLineInterval(env.MinX, env.MaxX, ring);
                sweepLine.Add(sweepInt);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="innerRing"></param>
        /// <param name="searchRing"></param>
        /// <returns></returns>
        private bool IsInside(LinearRing innerRing, LinearRing searchRing)
        {
            var innerRingPts = innerRing.Coordinates;
            var searchRingPts = searchRing.Coordinates;
            if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                return false;
            var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
            Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
            bool isInside = PointLocation.IsInRing(innerRingPt, searchRingPts);
            if (isInside)
            {
                nestedPt = innerRingPt;
                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public class OverlapAction : ISweepLineOverlapAction
        {
            private readonly SweeplineNestedRingTester container;
            bool isNonNested = true;

            /// <summary>
            ///
            /// </summary>
            public bool IsNonNested => isNonNested;

            /// <summary>
            ///
            /// </summary>
            /// <param name="container"></param>
            public OverlapAction(SweeplineNestedRingTester container)
            {
                this.container = container;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="s0"></param>
            /// <param name="s1"></param>
            public void Overlap(SweepLineInterval s0, SweepLineInterval s1)
            {
                var innerRing = (LinearRing) s0.Item;
                var searchRing = (LinearRing) s1.Item;
                if (innerRing == searchRing)
                    return;
                if (container.IsInside(innerRing, searchRing))
                    isNonNested = false;
            }
        }
    }
}
