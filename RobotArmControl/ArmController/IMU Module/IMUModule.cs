using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;                  // for SerialPort
using System.Threading;                 // for threads.

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


        private Matrix33

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
            string currentLine;

            while (_running)
            {
                currentLine = _lilySerial.ReadLine();

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


                        break;
                    case 'F':
                        // forearm

                        break;
                    case 'B':
                        // bicep

                        break;
                    case 'S':
                        // shoulder
                        break;
                    default:
                        // who knows what this is.
                        break;

                }



            }
        }




        #endregion Private Methods


    }
}
