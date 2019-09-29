﻿/*
 * Copyright (c) 2017 Jia Yu.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * and Eclipse Distribution License v. 1.0 which accompanies this distribution.
 * The Eclipse Public License is available at http://www.eclipse.org/legal/epl-v10.html
 * and the Eclipse Distribution License is available at
 *
 * http://www.eclipse.org/org/documents/edl-v10.php.
 */

using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Index.Strtree
{

    /// <summary>
    /// The Class GeometryDistanceComparer.
    /// </summary>
    public class GeometryDistanceComparer : IComparer<Geometry>
    {

        /// <summary>The normal order.</summary>
        private readonly bool _normalOrder;

        /// <summary>The query center.</summary>
        private readonly Point _queryCenter;

        /// <summary>
        /// Instantiates a new Geometry distance comparator.
        /// </summary>
        /// <param name="queryCenter">The query center</param>
        /// <param name="normalOrder">A value of <c>true</c> means puts the least record at the head of this queue. peek() will get the least element. Vice versa.</param>
        public GeometryDistanceComparer(Point queryCenter, bool normalOrder)
        {
            _queryCenter = queryCenter;
            _normalOrder = normalOrder;
        }

         /// <inheritdoc cref="IComparer{T}.Compare"/>
        public int Compare(Geometry g1, Geometry g2)
        {
            double distance1 = g1.EnvelopeInternal.Distance(this._queryCenter.EnvelopeInternal);
            double distance2 = g2.EnvelopeInternal.Distance(this._queryCenter.EnvelopeInternal);
            if (_normalOrder)
            {
                if (distance1 > distance2)
                {
                    return 1;
                }
                if (distance1 == distance2)
                {
                    return 0;
                }

                return -1;
            }
            else
            {
                if (distance1 > distance2)
                {
                    return -1;
                }
                if (distance1 == distance2)
                {
                    return 0;
                }

                return 1;
            }

        }
    }
}