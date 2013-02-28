using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;                  // for SerialPort
using System.Threading;                 // for threads.
using System.IO;

using ArmController.Integration;

namespace ArmController.IMU_Module
{
    class IMUModule : Module
    {
        #region Constants
        /**************************************************************************************/
        /* Constants */
        /**************************************************************************************/

        /// <summary>
        /// The com port to use to communicate with the lily pad.
        /// </summary>
        private static readonly string LILY_COM = "COM1";

        /// <summary>
        /// The baud rate to communicate with the lily pad with.
        /// </summary>
        private static readonly int LILY_BAUD = 19200;


        /// <summary>
        /// (Absolute) Rotation matrix for the shoulder.
        /// </summary>
        private Matrix33 _rotationShoulder;


        /// <summary>
        /// (Absolute) Rotation matrix for the bicep.
        /// </summary>
        private Matrix33 _rotationBicep;

        /// <summary>
        /// (Absolute) Rotation matrix for the forearm.
        /// </summary>
        private Matrix33 _rotationForearm;


        #endregion Constants


        #region Private Members
        /**************************************************************************************/
        /* Private Members */
        /**************************************************************************************/
        /// <summary>
        /// The serial port that we are using to communicate with the lily pad.
        /// </summary>
        SerialPort _lilySerial;

        /// <summary>
        /// The thread which is responsible for reading from the lilypad and publishing results.
        /// </summary>
        Thread _readerThread;

        /// <summary>
        /// A flag used to indicate that we should be running the reader thread.
        /// Bringing this to false will result in the reader thread stopping.
        /// </summary>
        bool _running;

        #endregion Private Members

        #region Protected Methods
        /**************************************************************************************/
        /* Protected Methods */
        /**************************************************************************************/
        /// <summary>
        /// Inititialize's the module and sets up a connection to the Lilypad IMU controller
        /// </summary>
        protected override void OnInitialize()
        {
            _lilySerial = new SerialPort(LILY_COM, LILY_BAUD, Parity.None, 8, StopBits.One);
            _lilySerial.Open();

            _rotationBicep = new Matrix33();
            _rotationForearm = new Matrix33();
            _rotationShoulder = new Matrix33();

            _running = true;

            _readerThread = new Thread(new ThreadStart(readIMU));
            _readerThread.Start();

        }

        protected override void OnFinalize()
        {
            _lilySerial.Close();
            _running = false;
            _readerThread.Join();
        }

        #endregion Protected Methods


        #region Private Methods
        /**************************************************************************************/
        /* Private Methods */
        /**************************************************************************************/
        /// <summary>
        /// Method which continually reads from the IMU
        /// </summary>
        private void readIMU()
        {
            // the current line we're working with.
            string currentLine;

            // Whether everything is A-OK
            bool good = true;

            while (_running)
            {
                currentLine = _lilySerial.ReadLine();
                good = true;

                /*
                 * The format of each line is:
                 * XR,P,Y,\n
                 * where:
                 *    X - Indicates the IMU being reported.
                 *        W - Wrist
                 *        F - forearm
                 *        B - bicep
                 *        S - shoulder
                 *    R - Indicates the roll of the IMU in degrees.
                 *    P - Indicates the pitch of the IMU in degrees.
                 *    Y - Indicates the yaw of the IMU in degrees.
                 */


                switch (currentLine[0])
                {
                    case 'W':
                        // wrist
                        // we don't track the wrist.

                        break;
                    case 'F':
                        // forearm
                        good = updateMatrix(currentLine, _rotationForearm);

                        // update the position of the forearm
                        updateBusForearm();


                        break;
                    case 'B':
                        // bicep
                        good = updateMatrix(currentLine, _rotationBicep);

                        // update the position of the forearm and bicep
                        updateBusBicep();
                        updateBusForearm();

                        break;
                    case 'S':
                        // shoulder
                        good = updateMatrix(currentLine, _rotationShoulder);


                        // update the position of the forearm, bicep, and shoulder
                        updateBusShoulder();
                        updateBusBicep();
                        updateBusForearm();

                        break;
                    default:
                        // who knows what this is.
                        // The input is corrupt.
                        good = false;

                        break;

                }

                if (!good)
                {
                    // Crap.
                    Console.WriteLine("Error: The IMU input line \"" + currentLine + "\" appears to be corrupt.");
                }
                else
                {
                    // notify everybody we have new information.
                    Bus.Publish<object>(BusNode.POSITION_TICK, null);
                }



            }
        }


        /// <summary>
        /// Reads the input line provided and updates the provided matrix with the correct yaw, pitch, and roll.
        /// </summary>
        /// <param name="input">The string to read the yaw, pitch, and roll from. </param>
        /// <param name="outMatrix">The matrix to output the results to.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool updateMatrix(string input, Matrix33 outMatrix)
        {
            bool ret = true;

            /*
             * The format of each line is:
             * XR,P,Y\n
             * where:
             *    X - Indicates the IMU being reported.
             *        W - Wrist
             *        F - forearm
             *        B - bicep
             *        S - shoulder
             *    R - Indicates the roll of the IMU in degrees.
             *    P - Indicates the pitch of the IMU in degrees.
             *    Y - Indicates the yaw of the IMU in degrees.
             */

            float yaw = 0, pitch = 0, roll = 0;
            string[] arr = input.Substring(1).Split(',');

            if (arr.Length != 3)
                throw new FormatException("The string \"" + input + "\" is not a valid yaw/pitch/roll specifier.");



            ret = ret && float.TryParse(arr[0], out yaw);
            ret = ret && float.TryParse(arr[1], out pitch);
            ret = ret && float.TryParse(arr[2], out roll);

            if (ret) outMatrix.setRotation(yaw, pitch, roll);
            return ret;
        }

        /// <summary>
        /// Sends the new orientation of the shoulder to the bus.
        /// </summary>
        private void updateBusShoulder()
        {
            // NOTE: Shoulder position is unused.
        }

        /// <summary>
        /// Sends the new orientation of the bicep to the bus.
        /// </summary>
        private void updateBusBicep()
        {
            Matrix33 relative = _rotationBicep.RelativeFrame(_rotationShoulder);

            Bus.Publish(BusNode.DIR_RIGHT_UPPER_ARM, relative.YVector);
            Bus.Publish(BusNode.ORIENTATION_RIGHT_UPPER_ARM, new Orientation(relative.Roll, relative.Pitch, relative.Yaw));

        }

        /// <summary>
        /// Sends the new orientation of the forearm to the bus.
        /// </summary>
        private void updateBusForearm()
        {
            Matrix33 relative = _rotationForearm.RelativeFrame(_rotationBicep);   
            Bus.Publish(BusNode.DIR_RIGHT_LOWER_ARM, relative.YVector);
            Bus.Publish(BusNode.ORIENTATION_RIGHT_LOWER_ARM, new Orientation(relative.Roll, relative.Pitch, relative.Yaw));
        }



        #endregion Private Methods


    }
}
