using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;                  // for SerialPort
using System.Threading;                 // for threads.
using System.IO;
using Microsoft.Kinect;
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
        private static readonly string LILY_COM = "COM5";

        /// <summary>
        /// The baud rate to communicate with the lily pad with.
        /// </summary>
        private static readonly int LILY_BAUD = 19200;
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
        /// (Absolute) Rotation matrix for the shoulder.
        /// </summary>
      //  private Orientation _rotationShoulder;
        private Matrix33 _rotationShoulder;

        /// <summary>
        /// (Absolute) Rotation matrix for the bicep.
        /// </summary>
       // private Orientation _rotationBicep;
        private Matrix33 _rotationBicep;

        /// <summary>
        /// (Absolute) Rotation matrix for the forearm.
        /// </summary>
      //  private Orientation _rotationForearm;
        private Matrix33 _rotationForearm;

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
            String COMPort = Bus.Get<String>(BusNode.IMU_COM_PORT);
            int baudRate = Bus.Get<int>(BusNode.IMU_BAUD_RATE);
            bool IMUEnabled = Bus.Get<bool>(BusNode.IMU_ENABLE);

            if (IMUEnabled)
            {
                _lilySerial = new SerialPort(COMPort, baudRate, Parity.None, 8, StopBits.One);
                _lilySerial.Open();

                //    _rotationBicep = new Orientation(0,0,0);
                //    _rotationForearm = new Orientation(0,0,0);
                //    _rotationShoulder = new Orientation(0,0,0);

                _rotationBicep = new Matrix33();
                _rotationForearm = new Matrix33();
                _rotationShoulder = new Matrix33();

                _running = true;

                _readerThread = new Thread(new ThreadStart(readIMU));
                _readerThread.Start();
            }
        }

        protected override void OnFinalize()
        {
            _running = false;
            Thread.Sleep(1);
            _lilySerial.Close();
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

            
           /*
            while (true)
            {
                Thread.Sleep(100);
                Matrix33 tmp = new Matrix33((float)(-00.0 * Math.PI / 180.0), (float)(00.0 * Math.PI / 180.0), (float)(-0.0 * Math.PI / 180.0));
                tmp.setRotation((float)(30 * Math.PI / 180), (float)(40 * Math.PI / 180), (float)(-50 * Math.PI / 180));
                Bus.Publish(BusNode.ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM, tmp.Orientation);
                Bus.Publish<object>(BusNode.POSITION_TICK, null);
            } */
            

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

                Console.Write("Reading IMU input: " + currentLine);


                switch (currentLine[0])
                {
                    case 'F':
                        // forearm
                        Console.WriteLine("Got forearm.");
                        good = updateMatrix(currentLine, ref _rotationForearm);
                      //  good = updateOrientation(currentLine, ref _rotationForearm);


                        // update the position of the forearm
                        updateBusForearm();


                        break;
                    case 'B':
                        // bicep
                        Console.WriteLine("Got bicep.");
                        good = updateMatrix(currentLine, ref _rotationBicep);
                     //   good = updateOrientation(currentLine, ref _rotationBicep);

                        // update the position of the forearm and bicep
                        updateBusBicep();
                        updateBusForearm();

                        break;
                    case 'W':   
                        // wrist
                        // we don't track the wrist.

                        // TODO: This is a hack because we're using the 'wrist' IMU for the shoulder.
                        //  break;
                    case 'S':
                        // shoulder
                        Console.WriteLine("Got shoulder.");
                        good = updateMatrix(currentLine, ref  _rotationShoulder);
                     //   good = updateOrientation(currentLine, ref _rotationShoulder);


                        // update the position of the forearm, bicep, and shoulder
                        updateBusShoulder();
                        updateBusBicep();
                        updateBusForearm();

                        break;
                    default:
                          Console.WriteLine("Got unknown.");
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
        private bool updateMatrix(string input, ref Matrix33 outMatrix)
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

            if (arr.Length != 3) ret = false;

            ret = ret && float.TryParse(arr[0], out yaw);
            ret = ret && float.TryParse(arr[1], out pitch);
            ret = ret && float.TryParse(arr[2], out roll);

            if (ret) outMatrix.setRotation(yaw * (float)(Math.PI / 180.0), pitch * (float)(Math.PI / 180.0), roll * (float)(Math.PI / 180.0));
            return ret;
        }

        /// <summary>
        /// Reads the input line provided and updates the provided matrix with the correct yaw, pitch, and roll.
        /// </summary>
        /// <param name="input">The string to read the yaw, pitch, and roll from. </param>
        /// <param name="outOr">The orientation to output the results to.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool updateOrientation(string input, ref Orientation outOr)
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

            if (arr.Length != 3) ret = false;

            ret = ret && float.TryParse(arr[0], out yaw);
            ret = ret && float.TryParse(arr[1], out pitch);
            ret = ret && float.TryParse(arr[2], out roll);

            outOr.Yaw = yaw * (float)(Math.PI / 180.0);
            outOr.Pitch = pitch * (float)(Math.PI / 180.0);
            outOr.Roll = roll * (float)(Math.PI / 180.0);

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
            Matrix33 tmp = new Matrix33();
            tmp.setRotation( _rotationShoulder.Yaw, _rotationShoulder.Pitch, _rotationShoulder.Roll);
          //  Orientation relOrientation = new Orientation(_rotationBicep.Roll - _rotationShoulder.Roll + (float)(Math.PI /2), 
          //      _rotationBicep.Pitch - _rotationShoulder.Pitch, 
          //      _rotationBicep.Yaw - _rotationShoulder.Yaw
          //  );
          //  Orientation absOrientation = _rotationBicep;


            Matrix33 relative = _rotationBicep.RelativeFrame(_rotationShoulder);
            Orientation relOrientation = relative.Orientation;
            Orientation absOrientation = _rotationBicep.Orientation;

            Bus.Publish(BusNode.DIR_RIGHT_UPPER_ARM, relative.YPoint);
            Bus.Publish(BusNode.ORIENTATION_RIGHT_UPPER_ARM, new Orientation(relOrientation.Roll, relOrientation.Pitch, relOrientation.Yaw));
            Bus.Publish(BusNode.ABSOLUTE_ORIENTATION_RIGHT_UPPER_ARM, new Orientation(absOrientation.Roll, absOrientation.Pitch,absOrientation.Yaw));
        }

        /// <summary>
        /// Sends the new orientation of the forearm to the bus.
        /// </summary>
        private void updateBusForearm()
        {
      //      Orientation relOrientation = new Orientation(_rotationForearm.Roll - _rotationBicep.Roll,
      //          _rotationForearm.Pitch - _rotationBicep.Pitch,
      //          _rotationForearm.Yaw - _rotationBicep.Yaw
      //      );
      //      Orientation absOrientation = _rotationForearm;


            Matrix33 relative = _rotationForearm.RelativeFrame(_rotationBicep);
            Orientation relOrientation = relative.Orientation;
            Orientation absOrientation = _rotationForearm.Orientation;


            Bus.Publish(BusNode.DIR_RIGHT_LOWER_ARM, relative.YPoint);
            Bus.Publish(BusNode.ORIENTATION_RIGHT_LOWER_ARM, new Orientation(relOrientation.Roll, relOrientation.Yaw, relOrientation.Pitch));
            Bus.Publish(BusNode.ABSOLUTE_ORIENTATION_RIGHT_LOWER_ARM, new Orientation(absOrientation.Roll, absOrientation.Yaw, absOrientation.Pitch));
        }




        #endregion Private Methods


    }
}
