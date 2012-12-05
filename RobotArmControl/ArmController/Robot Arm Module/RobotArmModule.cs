using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

using ArmController.Integration;
using ArmController.Kinect_Module;

namespace ArmController.Robot_Arm_Module
{
    class RobotArmModule : ArmController.Integration.Module
    {
        #region Private Constants
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        /// <summary>
        /// Radians per degree...
        /// </summary>
        private const double RAD_PER_DEG = 0.0174532925;

        /// <summary>
        /// milliseconds to wait between command updates.
        /// </summary>
        private const long MIN_UPDATE_INTERVAL = 500;

        /// <summary>
        ///  Servo constants
        /// </summary>
        #region Servos
        private const int SHOULDER_ROLL = 0;
        private const int SHOULDER_PITCH = 1;
        private const int ELBOW_JOINT = 2;
        private const int WRIST_JOINT = 3;
        private const int FINGERS = 4;
        #endregion Servos

        #region Input Extrema
        private const int ELBOW_PITCH_MAX_DEG = 140;
        private const int ELBOW_PITCH_MIN_DEG = 0;

        private const int SHOULDER_PITCH_MAX_DEG = 60;
        private const int SHOULDER_PITCH_MIN_DEG = -30;
        private const int SHOULDER_PITCH_MIN_DEG_PHYSICAL = -40;        // The intended minimum shoulder pitch, corresponds to human physical limits.
        #endregion Input Extrema




        #endregion Private Constants

        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        SerialPort _serialPort;

        long _lastUpdateTime;

        #endregion Private Members

        #region Constructors
        /**********************************************************************/
        /* Constructors */
        /**********************************************************************/
        public RobotArmModule() { /* Intentionally Left Blank */ }

        #endregion Constructors

        #region Event Handling

        /**********************************************************************/
        /* Event Handling */
        /**********************************************************************/

        protected override void OnInitialize() {
            _serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);

            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();

            _lastUpdateTime = 0;

            //Initialize the robot arm position
            move(SHOULDER_PITCH, 1500);
            move(ELBOW_JOINT, 700);
            move(WRIST_JOINT, 1500);

          //  // Subscribe to the kinect data tick
          Bus.Subscribe(BusNode.POSITION_TICK, OnValuePublished);
          //Bus.Subscribe(BusNode.CLAW_OPEN_PERCENT, OnValuePublished);
          //Bus.Subscribe(BusNode.ROBOT_ACTIVE, OnValuePublished);
          //Bus.Subscribe(BusNode.WRIST_PERCENT, OnValuePublished);

       }

        /// <summary>
        /// An event handling method called when the controller has requested that the module finalize its state.
        /// </summary>
        protected override void OnFinalize() { _serialPort.Close(); }

        #endregion EventHandling


        #region Private Methods
        /**********************************************************************/
        /* Private Methods */
        /**********************************************************************/

        /// <summary>
        /// This is called when data is received from the robot arm via serial bus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            /*
            try
            {
                string data = _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                // something is wrong....
            }
            */
            //this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

        /// <summary>
        /// This is called when a subscribed value is published on the virtual bus.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        private void OnValuePublished(BusNode node, object value)
        {
            // Let's limit the rate at which we send commands
            // TODO: Do this right!


            if (System.DateTime.Now.Ticks < _lastUpdateTime + MIN_UPDATE_INTERVAL) return;
            _lastUpdateTime = System.DateTime.Now.Millisecond;

            // What we want to do here is update our 'goal' position based on the data we can read from the virtual bus.
            // Since we've subscribed to the POSITION_TICK, the 'value' parameter has no meaning, we should get the value directly from the bus.
            Orientation arm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_UPPER_ARM);
            Orientation forearm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_LOWER_ARM);
            Orientation wrist = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_HAND);
            int hand = Bus.Get<int>(BusNode.CLAW_OPEN_PERCENT);
            bool armMoving = Bus.Get<bool>(BusNode.ROBOT_ACTIVE);

            int wrist_hand = Bus.Get<int>(BusNode.WRIST_PERCENT);


            // we should now convert these orientations to the values expected by the servo controller.

            if (armMoving)
            {

                if (node == BusNode.POSITION_TICK)
                {
                    // TODO: All the other joints
                    if (forearm != null)
                    {
                        //Debug.WriteLine("forearm: " + (forearm.Roll * 180) / Math.PI + " / " + (forearm.Pitch * 180) / Math.PI + " / " + (forearm.Yaw * 180) / Math.PI);
                        if (forearm.Pitch > ELBOW_PITCH_MAX_DEG * RAD_PER_DEG) forearm.Pitch = (float)(ELBOW_PITCH_MAX_DEG * RAD_PER_DEG);
                        if (forearm.Pitch < ELBOW_PITCH_MIN_DEG * RAD_PER_DEG) forearm.Pitch = (float)(ELBOW_PITCH_MIN_DEG * RAD_PER_DEG);

                        // scale to the valid range (700 - 2000)
                        int forearm_pos = 700 + (int)((
                            (forearm.Pitch - ELBOW_PITCH_MIN_DEG * RAD_PER_DEG)
                                / ((ELBOW_PITCH_MAX_DEG - ELBOW_PITCH_MIN_DEG) * RAD_PER_DEG)
                            ) * 1300);

                        // move the stuff!
                        // TODO: Do This correctly
                        move(ELBOW_JOINT, forearm_pos);
                    }

                    if (arm != null)
                    {
                        if (arm.Pitch > SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG) arm.Pitch = (float)(SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG);
                        if (arm.Pitch < SHOULDER_PITCH_MIN_DEG * RAD_PER_DEG) arm.Pitch = (float)(SHOULDER_PITCH_MIN_DEG * RAD_PER_DEG);

                        int arm_pos;

                        // scale to the valid range (1000 - 2000)
                        if (arm.Pitch > 0)
                        {
                            arm_pos = 1500 - (int)((
                                arm.Pitch /
                                    (SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG)
                                ) * 500);
                        }
                        else
                        {
                            arm_pos = 1500 + (int)((
                                (Math.Abs(arm.Pitch) /
                                    (Math.Abs(SHOULDER_PITCH_MIN_DEG) * RAD_PER_DEG)
                                ) * (((float)Math.Abs(SHOULDER_PITCH_MIN_DEG)) / 90)
                                ) * 500);
                        }

                        //Debug.WriteLine("Arm pos: " + arm_pos);

                        // move the stuff!
                        // TODO: Do This correctly
                        move(SHOULDER_PITCH, arm_pos);
                    }
                }
                else if (node == BusNode.CLAW_OPEN_PERCENT)
                {
                    // scale to the valid range (1000 - 2000)
                    int FINGER_DEGREE = 2000 + (-hand * 10);

                    // move the stuff!
                    // TODO: Do This correctly
                    move(FINGERS, FINGER_DEGREE);
                }
                else if (node == BusNode.WRIST_PERCENT)
                {
                    int temp = 1380 + ((-wrist_hand + 50) * 20);

                    move(WRIST_JOINT, temp);
                }
                

            }
        }

        private void move(int servo, int pos)
        {
            // TODO: Don't hardcode this
            int speed = 500;
            //Debug.WriteLine("Moving servo " + servo + " to position " + pos);

            string command = "#" + servo + "P" + pos + "S" + speed + "\r\n";
            send(command);
        }


        private void send(string command)
        {
            string tempcommand = command + "\r\n";

            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                _serialPort.Write(tempcommand);

            }
            catch (Exception)
            {
               Debug.WriteLine("Oh noes! something went wrong!");
            }

        }

        #endregion Private Methods









    }
}
