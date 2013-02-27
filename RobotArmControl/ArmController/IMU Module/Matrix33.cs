using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Vector

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

        #endregion Constructors


        #region Getters/Setters
        /**************************************************************************************/
        /* Getters/Setters */
        /**************************************************************************************/
        

        #endregion Constructors


    }
}
