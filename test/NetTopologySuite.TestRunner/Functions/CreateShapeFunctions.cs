﻿using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class CreateShapeFunctions
    {
        private static readonly int DEFAULT_POINTSIZE = 100;

        public static Geometry Grid(Geometry g, int nCells)
        {
            var geoms = new List<Geometry>();

            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            int nCellsOnSide = (int) Math.Sqrt(nCells) + 1;
            double cellSizeX = env.Width/nCellsOnSide;
            double cellSizeY = env.Height/nCellsOnSide;

            for (int i = 0; i < nCellsOnSide; i++)
            {
                for (int j = 0; j < nCellsOnSide; j++)
                {
                    double x1 = env.MinX + i * cellSizeX;
                    double y1 = env.MinY + j * cellSizeY;
                    double x2 = env.MinX + (i+1) * cellSizeX;
                    double y2 = env.MinY + (j+1) * cellSizeY;
                    var cellEnv = new Envelope(x1, x2, y1, y2);

                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }
            return geomFact.BuildGeometry(geoms);
        }

        public static Geometry Supercircle3(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 3);
        }

        public static Geometry Squircle(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 4);
        }

        public static Geometry Supercircle5(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 5);
        }

        public static Geometry SupercirclePoint5(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 0.5);
        }

        public static Geometry Supercircle(Geometry g, int nPts, double pow)
        {
            var gsf = new GeometricShapeFactory();
            gsf.NumPoints = nPts;
            if (g != null)
                gsf.Envelope = g.EnvelopeInternal;
            else
                gsf.Envelope = new Envelope(0, 1, 0, 1);
            return gsf.CreateSupercircle(pow);
        }
    }
}