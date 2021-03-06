using System;
using System.Runtime.Serialization;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// </summary>
    [Serializable]
    public abstract class PackedCoordinateSequence : CoordinateSequence
    {
        /// <summary>
        /// A soft reference to the Coordinate[] representation of this sequence.
        /// Makes repeated coordinate array accesses more efficient.
        /// </summary>
        [NonSerialized]
        protected WeakReference CoordRef;

        protected PackedCoordinateSequence(int count, int dimension, int measures)
            : base(count, dimension, measures)
        {
        }

        /// <summary>
        /// Returns (possibly a copy of) the ith Coordinate in this collection.
        /// Whether or not the Coordinate returned is the actual underlying
        /// Coordinate or merely a copy depends on the implementation.
        /// Note that in the future the semantics of this method may change
        /// to guarantee that the Coordinate returned is always a copy. Callers are
        /// advised not to assume that they can modify a CoordinateSequence by
        /// modifying the Coordinate returned by this method.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public sealed override Coordinate GetCoordinate(int i)
        {
            var arr = GetCachedCoords();
            if(arr != null)
                 return arr[i];
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Returns a copy of the i'th coordinate in this sequence.
        /// This method optimizes the situation where the caller is
        /// going to make a copy anyway - if the implementation
        /// has already created a new Coordinate object, no further copy is needed.
        /// </summary>
        /// <param name="i">The index of the coordinate to retrieve.</param>
        /// <returns>
        /// A copy of the i'th coordinate in the sequence
        /// </returns>
        public sealed override Coordinate GetCoordinateCopy(int i)
        {
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="i">The index of the coordinate to copy.</param>
        /// <param name="c">A Coordinate to receive the value.</param>
        public sealed override void GetCoordinate(int i, Coordinate c)
        {
            c.X = GetOrdinate(i, 0);
            c.Y = GetOrdinate(i, 1);
            if (HasZ)
            {
                c.Z = GetZ(i);
            }

            if (HasM)
            {
                c.M = GetM(i);
            }
        }

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation.
        /// Note that if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        /// <returns></returns>
        public sealed override Coordinate[] ToCoordinateArray()
        {
            var arr = GetCachedCoords();
            // testing - never cache
            if (arr != null)
                return arr;

            arr = new Coordinate[Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = GetCoordinateInternal(i);

            CoordRef = new WeakReference(arr);
            return arr;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Coordinate[] GetCachedCoords()
        {
            var localCoordRef = CoordRef;
            if (localCoordRef != null)
            {
                var arr = (Coordinate[]) localCoordRef.Target;
                if (arr != null)
                    return arr;

                CoordRef = null;
                return null;
            }
            return null;
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public sealed override double GetX(int index)
        {
            return GetOrdinate(index, 0);
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public sealed override double GetY(int index)
        {
            return GetOrdinate(index, 1);
        }

        /// <summary>
        /// Returns ordinate Z of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Z ordinate in the index'th coordinate, or Double.NaN if not defined.
        /// </returns>
        public sealed override double GetZ(int index)
        {
            if (HasZ)
            {
                return GetOrdinate(index, 2);
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Returns ordinate M of the specified coordinate if available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the M ordinate in the index'th coordinate, or Double.NaN if not defined.
        /// </returns>
        public sealed override double GetM(int index)
        {
            if (HasM)
            {
                int mIndex = Dimension - Measures;
                return GetOrdinate(index, mIndex);
            }
            else
            {
                return Coordinate.NullOrdinate;
            }
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return CoordinateSequences.ToString(this);
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract Coordinate GetCoordinateInternal(int index);

        [OnDeserialized]
        private void OnDeserialization(StreamingContext context)
        {
            CoordRef = null;
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on doubles.
    /// </summary>
    [Serializable]
    public class PackedDoubleCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly double[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedDoubleCoordinateSequence(double[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords.Length % dimension != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedDoubleCoordinateSequence(float[] coordinates, int dimension, int measures)
            : base(coordinates?.Length / dimension ?? 0, dimension, measures)
        {
            _coords = new double[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
                _coords[i] = coordinates[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedDoubleCoordinateSequence(Coordinate[] coordinates, int dimension)
            : this(coordinates, dimension, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedDoubleCoordinateSequence(Coordinate[] coordinates, int dimension, int measures)
            : base(coordinates?.Length ?? 0, dimension, measures)
        {
            if (coordinates == null)
                coordinates = new Coordinate[0];

            _coords = new double[coordinates.Length * Dimension];
            for (int i = 0; i < coordinates.Length; i++)
            {
                _coords[i * Dimension] = coordinates[i].X;
                if (Dimension >= 2)
                    _coords[i * Dimension + 1] = coordinates[i].Y;
                if (Dimension >= 3)
                    _coords[i * Dimension + 2] = coordinates[i][2]; // Z or M
                if (Dimension >= 4)
                    _coords[i * Dimension + 3] = coordinates[i][3]; // M
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        public PackedDoubleCoordinateSequence(Coordinate[] coordinates) : this(coordinates, 3, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedDoubleCoordinateSequence(int size, int dimension, int measures)
            : base(size, dimension, measures)
        {
            _coords = new double[size * Dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Coordinate GetCoordinateInternal(int index)
        {
            double x = _coords[index * Dimension];
            double y = _coords[index * Dimension + 1];
            if (Dimension == 2 && Measures == 0)
            {
                return new Coordinate(x, y);
            }
            else if (Dimension == 3 && Measures == 0)
            {
                double z = _coords[index * Dimension + 2];
                return new CoordinateZ(x, y, z);
            }
            else if (Dimension == 3 && Measures == 1)
            {
                double m = _coords[index * Dimension + 2];
                return new CoordinateM(x, y, m);
            }
            else if (Dimension == 4 && Measures == 1)
            {
                double z = _coords[index * Dimension + 2];
                double m = _coords[index * Dimension + 3];
                return new CoordinateZM(x, y, z, m);
            }

            // note: JTS's "Coordinate" is our "CoordinateZ".
            return new CoordinateZ(x, y);
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public double[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new PackedDoubleCoordinateSequence((double[])_coords.Clone(), Dimension, Measures);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <remarks>
        /// Beware, for performance reasons the ordinate index is not checked, if
        /// it's over dimensions you may not get an exception but a meaningless
        /// value.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            return _coords[index * Dimension + ordinateIndex];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked.
        /// If it is larger than the dimension a meaningless value may be returned.
        /// </remarks>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + ordinateIndex] = value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override Envelope ExpandEnvelope(Envelope env)
        {
            int dim = Dimension;
            for (int i = 0; i < _coords.Length; i += dim)
                env.ExpandToInclude(_coords[i], _coords[i + 1]);
            return env;
        }

        public override CoordinateSequence Reversed()
        {
            int dim = Dimension;
            double[] coords = new double[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(double), coords, --j * dim * sizeof(double), dim * sizeof(double));
            }
            return new PackedDoubleCoordinateSequence(coords, dim, Measures);
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on floats.
    /// </summary>
    [Serializable]
    public class PackedFloatCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private readonly float[] _coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedFloatCoordinateSequence(float[] coords, int dimension, int measures)
            : base(coords?.Length / dimension ?? 0, dimension, measures)
        {
            if (coords.Length % dimension != 0)
                throw new ArgumentException("Packed array does not contain " +
                    "an integral number of coordinates");

            _coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedFloatCoordinateSequence(double[] coordinates, int dimension, int measures)
            : base(coordinates?.Length / dimension ?? 0, dimension, measures)
        {
            _coords = new float[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
                _coords[i] = (float) coordinates[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedFloatCoordinateSequence(Coordinate[] coordinates, int dimension)
            : this(coordinates, dimension, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedFloatCoordinateSequence(Coordinate[] coordinates, int dimension, int measures)
            : base(coordinates?.Length ?? 0, dimension, measures)
        {
            if (coordinates == null)
                coordinates = new Coordinate[0];

            _coords = new float[coordinates.Length * Dimension];
            for (int i = 0; i < coordinates.Length; i++)
            {
                _coords[i * Dimension] = (float) coordinates[i].X;
                if (Dimension >= 2)
                    _coords[i * Dimension + 1] = (float) coordinates[i].Y;
                if (Dimension >= 3)
                _coords[i * Dimension + 2] = (float) coordinates[i].Z;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        public PackedFloatCoordinateSequence(int size, int dimension, int measures)
            : base(size, dimension, measures)
        {
            _coords = new float[size * Dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Coordinate GetCoordinateInternal(int index)
        {
            double x = _coords[index * Dimension];
            double y = _coords[index * Dimension + 1];
            if (Dimension == 2 && Measures == 0)
            {
                return new Coordinate(x, y);
            }
            else if (Dimension == 3 && Measures == 0)
            {
                double z = _coords[index * Dimension + 2];
                return new CoordinateZ(x, y, z);
            }
            else if (Dimension == 3 && Measures == 1)
            {
                double m = _coords[index * Dimension + 2];
                return new CoordinateM(x, y, m);
            }
            else if (Dimension == 4 && Measures == 1)
            {
                double z = _coords[index * Dimension + 2];
                double m = _coords[index * Dimension + 3];
                return new CoordinateZM(x, y, z, m);
            }

            // note: JTS's "Coordinate" is our "CoordinateZ".
            return new CoordinateZ(x, y);
        }

        /// <summary>
        /// Gets the underlying array containing the coordinate values.
        /// </summary>
        /// <returns>The array of coordinate values</returns>
        public float[] GetRawCoordinates()
        {
            return _coords;
        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new PackedFloatCoordinateSequence((float[])_coords.Clone(), Dimension, Measures);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinate indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <remarks>
        /// Beware, for performance reasons the ordinate index is not checked, if
        /// it's over dimensions you may not get an exception but a meaningless
        /// value.
        /// </remarks>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <returns></returns>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            return _coords[index * Dimension + ordinateIndex];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinateIndex">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            CoordRef = null;
            _coords[index * Dimension + ordinateIndex] = (float) value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override Envelope ExpandEnvelope(Envelope env)
        {
        for (int i = 0; i < _coords.Length; i += Dimension )
            env.ExpandToInclude(_coords[i], _coords[i + 1]);
        return env;
        }

        public override CoordinateSequence Reversed()
        {
            int dim = Dimension;
            float[] coords = new float[_coords.Length];
            int j = Count;
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(_coords, i * dim * sizeof(float), coords, --j * dim * sizeof(float), dim * sizeof(float));
            }
            return new PackedDoubleCoordinateSequence(coords, dim, Measures);
        }

    }
}
