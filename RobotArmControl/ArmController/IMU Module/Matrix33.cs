using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

using ArmController.Integration;

namespace ArmController.IMU_Module
{
    /// <summary>
    /// A simple matrix class used to perform some basic 3D operations.
    /// </summary>
    [Serializable]
    class Matrix33
    {
        #region Private Members
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/
        /// <summary>
        /// Our actual matrix that we're wrapping.
        /// </summary>
        private Matrix _matrix;

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
            _matrix = Matrix.CreateFromYawPitchRoll(0, 0, 0);
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
            Matrix.CreateFromYawPitchRoll(yaw * (((float)Math.PI) / 180), pitch * (((float)Math.PI) / 180), roll * (((float)Math.PI) / 180));
        }

        #endregion Constructors


        #region Public Methods
        /**************************************************************************************/
        /* Public Methods */
        /**************************************************************************************/
    


        /// <summary>
        /// Multiplies two matrices and returns the result.
        /// </summary>
        /// <param name="A">The first matrix.</param>
        /// <param name="B">The second matrix.</param>
        /// <returns>The resulting matrix (A * B).</returns>
        public static Matrix33 operator*(Matrix33 A, Matrix33 B)
        {
            Matrix33 ret = new Matrix33();
            ret._matrix = Matrix.Multiply(A._matrix, B._matrix);
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
            ret._matrix = Matrix.Subtract(A._matrix, B._matrix);
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
            ret._matrix = Matrix.Add(A._matrix, B._matrix);
            return ret;

        }


        /// <summary>
        /// Gets the yaw represented by the rotational matrix.
        /// </summary>
        public float Yaw
        {
            get
            {
                return (float)Math.Atan2(_matrix.M12, _matrix.M11);

            }
        }

        /// <summary>
        /// Gets the pitch represented by the rotational matrix.
        /// </summary>
        public float Pitch
        {
            get
            {
                return (float)Math.Atan2(_matrix.M23, _matrix.M33);
            }
        }


        /// <summary>
        /// Gets the roll represented by the rotational matrix.
        /// </summary>
        public float Roll
        {
            get
            {
                return (float)Math.Atan2(-_matrix.M13, Math.Sqrt(_matrix.M23 * _matrix.M23 + _matrix.M33 * _matrix.M33));
            }
        }

        /// <summary>
        /// Gets the orientation of the matrix.
        /// </summary>
        public Orientation Orientation
        {
            get
            {
                return new Orientation(Roll, Pitch, Yaw);
            }
        }


        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 XVector
        {
            get
            {
                return _matrix.Right;
            }
        }

        /// <summary>
        /// Gets the point corresponding to the X axis from this matrix.
        /// </summary>
        public SkeletonPoint XPoint
        {
            get
            {
                SkeletonPoint ret = new SkeletonPoint();
                Vector3 vec = XVector;
                ret.X = vec.X;
                ret.Y = vec.Y;
                ret.Z = vec.Z;
                return ret;
            }
        }

        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 YVector
        {
            get
            {
                return _matrix.Forward;
            }
        }

        /// <summary>
        /// Gets the point corresponding to the Y axis from this matrix.
        /// </summary>
        public SkeletonPoint YPoint
        {
            get
            {
                SkeletonPoint ret = new SkeletonPoint();
                Vector3 vec = YVector;
                ret.X = vec.X;
                ret.Y = vec.Y;
                ret.Z = vec.Z;
                return ret;
            }
        }

        /// <summary>
        /// Gets the vector corresponding to the X axis from this matrix.
        /// </summary>
        public Vector3 ZVector
        {
            get
            {
                return _matrix.Up;
            }
        }

        /// <summary>
        /// Gets the point corresponding to the Z axis from this matrix.
        /// </summary>
        public SkeletonPoint ZPoint
        {
            get
            {
                SkeletonPoint ret = new SkeletonPoint();
                Vector3 vec = ZVector;
                ret.X = vec.X;
                ret.Y = vec.Y;
                ret.Z = vec.Z;
                return ret;
            }
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
                result._matrix = Matrix.Transpose(_matrix);
                return result;
            }
        }

        /// <summary>
        /// Calculates and returns the inverse matrix.
        /// </summary>
        /// <returns>The inverse of this matrix. <returns>
        public Matrix33 Invert
        {
            get
            {
                Matrix33 result = new Matrix33();
                result._matrix = Matrix.Invert(_matrix);
                return result;
            }
        }

        /// <summary>
        /// Rotates the matrix by the provided yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">The yaw (in degrees) to rotate by.</param>
        /// <param name="pitch">The pitch (in degrees) to rotate by.</param>
        /// <param name="roll">The roll (in degrees) to rotate by.</param>
        /// <returns>The result of the rotation.</returns>
        public Matrix33 Rotate(float yaw, float pitch, float roll)
        {
            
            Matrix33 rotationMatrix = new Matrix33(yaw, pitch, roll);

            // TODO: Is this the correct order, or is it rotationMatrix * this.
            return rotationMatrix * this;
        }


        /// <summary>
        /// Calculates the relative frame of reference given a parent frame based on the same global frame.
        /// </summary>
        /// <param name="parent">A parent frame based within the same frame of reference as this frame.</param>
        /// <returns>A new frame based within the parent frame of reference.</returns>
        public Matrix33 RelativeFrame(Matrix33 parent)
        {
            // TODO: Is this the correect order?
            return parent.Invert * this;
        }

        /// <summary>
        /// Sets the rotation of this matrix to match the provided yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">The new yaw.</param>
        /// <param name="pitch">The new pitch.</param>
        /// <param name="roll">The new roll.</param>
        public void setRotation(float yaw, float pitch, float roll)
        {
            Matrix.CreateFromYawPitchRoll(yaw * (((float)Math.PI) / 180), pitch * (((float)Math.PI) / 180), roll * (((float)Math.PI) / 180), out _matrix);
        }

        #endregion Public Methods


    }
}
