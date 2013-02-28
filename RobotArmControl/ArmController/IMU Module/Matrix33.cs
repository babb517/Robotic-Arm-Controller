using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmController.Integration;

namespace ArmController.IMU_Module
{
    /// <summary>
    /// A simple matrix class used to perform some basic 3D operations.
    /// </summary>
    class Matrix33
    {
        #region Private Members
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/
        /// <summary>
        /// Our actual matrix that we're wrapping.
        /// </summary>
        private float[,] _matrix;

        #endregion Private Members

       
         #region Constructors
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/
        /// <summary>
        /// Basic Constructor.
        /// Creates a 3x3 identity matrix.
        /// </summary>
        public Matrix33()
        {
            /// initialize matrix
            _matrix = new float[3,3];

            // Default to the identity matrix
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (i == j) _matrix[i,j] = 1;
                    else _matrix[i,j] = 0;
                }
            }
        }


        /// <summary>
        /// Full Constructor.
        /// Creates a 3x3 rotational matrix.
        /// </summary>
        /// <param name="yaw">The initial yaw of the matrix.</param>
        /// <param name="pitch">The initial pitch of the matrix.</param>
        /// <param name="roll">The initial roll of the matrix.</param>
        public Matrix33(float yaw, float pitch, float roll)
        {
            /// initialize matrix
            _matrix = new float[3,3];

            // calculate the rotational matrix
            // see http://en.wikipedia.org/wiki/Rotation_matrix

            setRotation(yaw, pitch, roll);
        }

        #endregion Constructors


        #region Public Methods
        /**************************************************************************************/
        /* Public Methods */
        /**************************************************************************************/
        
        /// <summary>
        /// Indexing operator for working with the underlying array.
        /// </summary>
        /// <param name="i">The row to access.</param>
        /// <param name="j">The column to access.</param>
        /// <returns>The value at that row/column.</returns>
        public float this[int i,int j]
        {
            get 
            {
                return _matrix[i,j];
            }

            set 
            {
                _matrix[i,j] = value;
            }

        }


        /// <summary>
        /// Multiplies two matrices and returns the result.
        /// </summary>
        /// <param name="A">The first matrix.</param>
        /// <param name="B">The second matrix.</param>
        /// <returns>The resulting matrix (A * B).</returns>
        public static Matrix33 operator*(Matrix33 A, Matrix33 B)
        {
            Matrix33 ret = new Matrix33();

            // The result A*B is defined s.t. for each i,j
            //   AB_ij = \sum_{k=0,k<3} A_ik * B_kj

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    // initialize the result.
                    ret[i,j] = 0;

                    // sum
                    for (int k = 0; k < 3; k++) {
                        ret[i,j] += A[i,k] * B[k,j];
                    }
                }
            }

            return ret;

        }

        /// <summary>
        /// Element-wise subtraction between two matrices.
        /// </summary>
        /// <param name="A">The first matrix.</param>
        /// <param name="B">The second matrix.</param>
        /// <returns>The resulting matrix (A - B).</returns>
        public static Matrix33 operator-(Matrix33 A, Matrix33 B)
        {
            Matrix33 ret = new Matrix33();

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                     ret[i,j] = A[i,j] - B[i,j];
                }
            }

            return ret;

        }

        /// <summary>
        /// Element-wise addition between two matrices.
        /// </summary>
        /// <param name="A">The first matrix.</param>
        /// <param name="B">The second matrix.</param>
        /// <returns>The resulting matrix (A + B).</returns>
        public static Matrix33 operator+(Matrix33 A, Matrix33 B)
        {
            Matrix33 ret = new Matrix33();

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                     ret[i,j] = A[i,j] + B[i,j];
                }
            }

            return ret;

        }


        /// <summary>
        /// Gets the yaw represented by the rotational matrix.
        /// </summary>
        public float Yaw
        {
            get
            {
                return (float)Math.Atan2(this[0,1], this[0,0]);
            }
        }

        /// <summary>
        /// Gets the pitch represented by the rotational matrix.
        /// </summary>
        public float Pitch
        {
            get
            {
                return (float)Math.Atan2(this[2,1], this[2,2]);
            }
        }

        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 XVector
        {
            get
            {
                return Col(0);
            }
        }

        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 YVector
        {
            get
            {
                return Col(1);
            }
        }

        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 ZVector
        {
            get
            {
                return Col(2);
            }
        }

        /// <summary>
        /// Gets the roll represented by the rotational matrix.
        /// </summary>
        public float Roll
        {
            get
            {
                return (float)Math.Atan2(-this[2,0], Math.Sqrt(this[2,1] * this[2,1] + this[2,2] * this[2,2]));
            }
        }

        /// <summary>
        /// Accesses a single row in the matrix.
        /// </summary>
        /// <param name="i">The matrix to access.</param>
        /// <returns>A copy of the data from that row.</returns>
        public Vector3 Row(int i)
        {
            Vector3 ret = new Vector3();

            for (int j = 0; j < 3; j++)
                ret[j] = this[i,j];

            return ret;

        }

        /// <summary>
        /// Accesses a single column in the matrix.
        /// </summary>
        /// <param name="j">The column to access.</param>
        /// <returns>A copy of the data from the column.</returns>
        public Vector3 Col(int j)
        {
            Vector3 ret = new Vector3();

            for (int i = 0; i < 3; i++)
                ret[j] = this[i,j];

            return ret;
        }


        /// <summary>
        /// Calculates and returns the transposed matrix, obtained by swapping the row/column of each
        /// matrix element.
        /// </summary>
        /// <returns>The transpose of this matrix. <returns>
        public Matrix33 Transpose
        {
            get
            {
                Matrix33 result = new Matrix33();

                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        result[j,i] = this[i,j];
                    }
                }

                return result;
            }
        }


        /// <summary>
        /// Rotates the matrix by the provided yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">The yaw (in radians) to rotate by.</param>
        /// <param name="pitch">The pitch (in radians) to rotate by.</param>
        /// <param name="roll">The roll (in radians) to rotate by.</param>
        /// <returns>The result of the rotation.</returns>
        public Matrix33 Rotate(float yaw, float pitch, float roll)
        {
            Matrix33 rotationMatrix = new Matrix33(yaw, pitch, roll);

            // TODO: Is this the correct order, or is it rotationMatrix * this.
            return this * rotationMatrix;
        }


        /// <summary>
        /// Calculates the relative frame of reference given a parent frame based on the same global frame.
        /// </summary>
        /// <param name="parent">A parent frame based within the same frame of reference as this frame.</param>
        /// <returns>A new frame based within the parent frame of reference.</returns>
        public Matrix33 RelativeFrame(Matrix33 parent)
        {
            // TODO: Is this the correect order?
            return this * parent.Transpose;
        }

        /// <summary>
        /// Sets the rotation of this matrix to match the provided yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">The new yaw.</param>
        /// <param name="pitch">The new pitch.</param>
        /// <param name="roll">The new roll.</param>
        public void setRotation(float yaw, float pitch, float roll)
        {
            float sin_yaw = (float)Math.Sin(yaw);
            float cos_yaw = (float)Math.Cos(yaw);
            float sin_pitch = (float)Math.Sin(pitch);
            float cos_pitch = (float)Math.Cos(pitch);
            float sin_roll = (float)Math.Sin(roll);
            float cos_roll = (float)Math.Cos(roll);


            this[0, 0] = cos_roll * cos_yaw;
            this[1, 0] = sin_pitch * sin_roll * cos_yaw - cos_pitch * sin_yaw;
            this[2, 0] = sin_pitch * sin_yaw + cos_pitch * sin_roll * sin_yaw;

            this[0, 1] = cos_roll * sin_yaw;
            this[1, 1] = cos_pitch * cos_yaw + sin_pitch * sin_roll * sin_yaw;
            this[2, 1] = cos_pitch * sin_roll * sin_yaw - sin_pitch * cos_yaw;

            this[0, 2] = -sin_roll;
            this[1, 2] = sin_pitch * cos_roll;
            this[2, 2] = cos_pitch * cos_roll;

        }


        #endregion Public Methods


    }
}
