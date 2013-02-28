using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmController.IMU_Module
{
    /// <summary>
    /// A simple vector class.
    /// </summary>
    class Vector3
    {
        #region Private Members
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/
        /// <summary>
        /// Our actual matrix that we're wrapping.
        /// </summary>
        private float[] _vector;

        #endregion Private Members

       
         #region Constructors
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/
        /// <summary>
        /// Basic Constructor.
        /// Creates a vector initialized to all zeros.
        /// </summary>
        public Vector3()
        {
            /// initialize vector
            _vector = new float[3];

            for (int i = 0; i < 3; i++)
            {
                _vector[i] = 0;
            }
        }

        /// <summary>
        /// Full constructor. Creates a vector with the provided values.
        /// </summary>
        /// <param name="zero">this[0]</param>
        /// <param name="one">this[1]</param>
        /// <param name="two">this[2]</param>
        public Vector3(float zero, float one, float two)
        {
            /// initialize vector
            _vector = new float[3];
            _vector[0] = zero;
            _vector[1] = one;
            _vector[2] = two;
        }

         #endregion Constructors


        #region Public Methods
        /**************************************************************************************/
        /* Public Methods */
        /**************************************************************************************/

        /// <summary>
        /// Indexing operator for working with the underlying array.
        /// </summary>
        /// <param name="i">The index to access.</param>
        /// <returns>The value at that index.</returns>
        public float this[int i]
        {
            get 
            {
                return _vector[i];
            }

            set 
            {
                _vector[i] = value;
            }

        }

        /// <summary>
        /// Produces the dot product of two vectors.
        /// </summary>
        /// <param name="A">The first vector.</param>
        /// <param name="B">The second vector.</param>
        /// <returns>The scalar dot product.</returns>
        public static float operator*(Vector3 A, Vector3 B)
        {
            float ret = 0;

            for (int i = 0; i < 3; i++) {
                ret += A[i] * B[i];
            }

            return ret;

        }

        /// <summary>
        /// Element-wise subtraction between two vectors.
        /// </summary>
        /// <param name="A">The first vectors.</param>
        /// <param name="B">The second vectors.</param>
        /// <returns>The resulting vector (A - B).</returns>
        public static Vector3 operator-(Vector3 A, Vector3 B)
        {
            Vector3 ret = new Vector3();

            for (int i = 0; i < 3; i++) {
                ret[i] = A[i] - B[i];
            }

            return ret;
        }

        /// <summary>
        /// Element-wise addition between two vectors.
        /// </summary>
        /// <param name="A">The first vectors.</param>
        /// <param name="B">The second vectors.</param>
        /// <returns>The resulting vector (A + B).</returns>
        public static Vector3 operator+(Vector3 A, Vector3 B)
        {
            Vector3 ret = new Vector3();

            for (int i = 0; i < 3; i++)
            {
                ret[i] = A[i] + B[i];
            }

            return ret;
        }

        /// <summary>
        /// Calculates the magnitude of the vector.
        /// </summary>
        /// <returns>The scalar magnitude of the vector.</returns>
        public float Magnitude
        {
            get
            {
                float tmp = 0;

                for (int i = 0; i < 3; i++)
                {
                    tmp = this[i] * this[i];
                }

                return (float)Math.Sqrt(tmp);
            }
        }

        /// <summary>
        /// Calculates the angle between this vector and another.
        /// </summary>
        /// <param name="other">The intersecting vector.</param>
        /// <returns>The angle between this and other (in radians).</returns>
        public float Angle(Vector3 other)
        {
            return (float)Math.Acos((this * other) / (this.Magnitude * other.Magnitude));

        }

        #endregion Public Methods


    }
}
