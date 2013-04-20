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
    /// <summary>
    /// This class contains the logic used to control the robot arm with the use of data which is provided by
    /// the CyberGlove Module and the Kinect Module.
    /// </summary>
    class RobotArmModule : ArmController.Integration.Module
    {
        #region Private Constants
        /**********************************************************************/
        /* Private Constants */
        /**********************************************************************/

        #region Servo Positions

        /// <summary>
        /// The current position of the shoulder servo.
        /// </summary>
        int currentShoulderPosition = 0;

        /// <summary>
        /// The current position of the arm servo.
        /// </summary>
        int currentArmPosition = 0;

        /// <summary>
        /// The current position of the forearm servo.
        /// </summary>
        int currentForearmPosition = 0;

        /// <summary>
        /// The current position of the wrist servo.
        /// </summary>
        int currentWristPosition = 0;

        /// <summary>
        /// The current position of the hand servo.
        /// </summary>
        int currentHandPosition = 0;

        #endregion

        /// <summary>
        /// Minimum movement amount that the robot arm can move a servo.
        /// </summary>
        private const int positionDeltaThreshold = 20;

        /// <summary>
        /// Radians per degree.
        /// </summary>
        private const double RAD_PER_DEG = 0.0174532925;

        /// <summary>
        /// Milliseconds to wait between command updates.
        /// </summary>
        private const long MIN_UPDATE_INTERVAL = 100;

        #region Servo Channel Numbers

        /// <summary>
        ///  Base servo channel number on the robot arm.
        /// </summary>
        private const int SHOULDER = 0;

        /// <summary>
        /// Arm servo channel number on the robot arm.
        /// </summary>
        private const int ARM = 1;

        /// <summary>
        /// Forearm servo channel number on the robot arm.
        /// </summary>
        private const int FOREARM = 2;

        /// <summary>
        /// Wrist servo channel number on the robot arm.
        /// </summary>
        private const int WRIST = 3;

        /// <summary>
        /// Hand servo channel number on the robot arm.
        /// </summary>
        private const int HAND = 4;

        #endregion Servos

        #region Input Extrema

        /// <summary>
        /// The maximum amount of degrees that the forearm servo can move.
        /// </summary>
        private const int ELBOW_PITCH_MAX_DEG = 140;

        /// <summary>
        /// The minimum amount of degrees that the forearm servo can move.
        /// </summary>
        private const int ELBOW_PITCH_MIN_DEG = 0;

        /// <summary>
        /// The maximum amount of degrees that the arm servo can move.
        /// </summary>
        private const int SHOULDER_PITCH_MAX_DEG = 60;

        /// <summary>
        /// The minimum amount of degrees that the arm servo can move.
        /// </summary>
        private const int SHOULDER_PITCH_MIN_DEG = -45; // -30;

        /// <summary>
        /// The intended minimum shoulder pitch which corresponds to human physical limits.
        /// </summary>
        private const int SHOULDER_PITCH_MIN_DEG_PHYSICAL = -40;

        /// <summary>
        /// The maximum amount of degrees that the shoulder servo can move.
        /// </summary>
        private const int SHOULDER_YAW_MAX_DEG = 100;

        /// <summary>
        /// The minimum amount of degrees that the shoulder servo can move.
        /// </summary>
        private const int SHOULDER_YAW_MIN_DEG = 0;

        #endregion Input Extrema

        #endregion Private Constants

        #region Private Members
        /**********************************************************************/
        /* Private Members */
        /**********************************************************************/

        /// <summary>
        /// The serial port used to send commands to the robot arm.
        /// </summary>
        SerialPort _serialPort;

        /// <summary>
        /// The last time that a command was sent to the robot arm.
        /// </summary>
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

        /// <summary>
        /// Initialization function that is called when the thread is created. Opens the serial port used to 
        /// communicate with the robot arm, sets the initial servo positions, and subscribes to the appropriate 
        /// data nodes on the Virtual Bus.
        /// </summary>
        protected override void OnInitialize() {
            //Open the serial port to communicate with the robot arm.
            _serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open(); 
            _lastUpdateTime = 0;

            //Initialize the robot arm position.
            move(ARM, 1500,100);
            move(SHOULDER, 1500, 100);
            move(FOREARM, 1500,100);
            move(WRIST, 1500, 100);
            move(HAND, 1500, 100);
            
            //Subscribe to the nodes published by the Kinect Module.
            Bus.Subscribe(BusNode.POSITION_TICK, OnValuePublished);

            //Subscribe to the nodes published by the CyberGlove Module.
            Bus.Subscribe(BusNode.CLAW_OPEN_PERCENT, OnValuePublished);
            Bus.Subscribe(BusNode.ROBOT_ACTIVE, OnValuePublished);
            Bus.Subscribe(BusNode.WRIST_PERCENT, OnValuePublished);

       }

        /// <summary>
        /// An event handling method called when the controller has requested that the module finalize its state.
        /// Closes the serial port that is used to communicate with the robot arm.
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
            int finalArmPosition = 0;
            int finalForearmPosition = 0;
            int finalWristPosition = 0;
            int finalHandPosition = 0;
            int finalShoulderPosition = 0;

            // Let's limit the rate at which we send commands
            // TODO: Do this right!
            if (System.DateTime.Now.Ticks < _lastUpdateTime + MIN_UPDATE_INTERVAL * System.TimeSpan.TicksPerMillisecond) return;
            _lastUpdateTime = System.DateTime.Now.Millisecond;

            // What we want to do here is update our 'goal' position based on the data we can read from the virtual bus.
            // Since we've subscribed to the POSITION_TICK, the 'value' parameter has no meaning, we should get the value directly from the bus.
            Orientation arm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_UPPER_ARM);
            Orientation forearm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_LOWER_ARM);
            Orientation wrist = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_HAND);

            int hand = Bus.Get<int>(BusNode.CLAW_OPEN_PERCENT);
            bool armMoving = Bus.Get<bool>(BusNode.ROBOT_ACTIVE);
            int wrist_hand = Bus.Get<int>(BusNode.WRIST_PERCENT);
            
            //Hand servo nodes
            int handServoMinRange = Bus.Get<int>(BusNode.HAND_SERVO_MIN_RANGE);
            int handServoMaxRange = Bus.Get<int>(BusNode.HAND_SERVO_MAX_RANGE);
            int handServoSpeed = Bus.Get<int>(BusNode.HAND_SERVO_SPEED);
            bool handServoEnabled = Bus.Get<bool>(BusNode.HAND_SERVO_ENABLE);

            //Wrist servo nodes
            int wristServoMinRange = Bus.Get<int>(BusNode.WRIST_SERVO_MIN_RANGE);
            int wristServoMaxRange = Bus.Get<int>(BusNode.WRIST_SERVO_MAX_RANGE);
            int wristServoSpeed = Bus.Get<int>(BusNode.WRIST_SERVO_SPEED);
            bool wristServoEnabled = Bus.Get<bool>(BusNode.WRIST_SERVO_ENABLE);

            //Wrist rotate servo nodes
            int wristRotateServoMinRange = Bus.Get<int>(BusNode.WRIST_ROTATE_SERVO_MIN_RANGE);
            int wristRotateServoMaxRange = Bus.Get<int>(BusNode.WRIST_ROTATE_SERVO_MAX_RANGE);
            int wristRotateServoSpeed = Bus.Get<int>(BusNode.WRIST_ROTATE_SERVO_SPEED);
            bool wristRotateServoEnabled = Bus.Get<bool>(BusNode.WRIST_ROTATE_SERVO_ENABLE);

            //Forearm servo nodes
            int forearmServoMinRange = Bus.Get<int>(BusNode.FOREARM_SERVO_MIN_RANGE);
            int forearmServoMaxRange = Bus.Get<int>(BusNode.FOREARM_SERVO_MAX_RANGE);
            int forearmServoSpeed = Bus.Get<int>(BusNode.FOREARM_SERVO_SPEED);
            bool forearmServoEnabled = Bus.Get<bool>(BusNode.FOREARM_SERVO_ENABLE);

            //Arm servo nodes
            int armServoMinRange = Bus.Get<int>(BusNode.ARM_SERVO_MIN_RANGE);
            int armServoMaxRange = Bus.Get<int>(BusNode.ARM_SERVO_MAX_RANGE);
            int armServoSpeed = Bus.Get<int>(BusNode.ARM_SERVO_SPEED);
            bool armServoEnabled = Bus.Get<bool>(BusNode.ARM_SERVO_ENABLE);
            
            //Shoulder servo nodes
            int shoulderServoMinRange = Bus.Get<int>(BusNode.SHOULDER_SERVO_MIN_RANGE);
            int shoulderServoMaxRange = Bus.Get<int>(BusNode.SHOULDER_SERVO_MAX_RANGE);
            int shoulderServoSpeed = Bus.Get<int>(BusNode.SHOULDER_SERVO_SPEED);
            bool shoulderServoEnabled = Bus.Get<bool>(BusNode.SHOULDER_SERVO_ENABLE);


            // we should now convert these orientations to the values expected by the servo controller.

            if (armMoving)
            {
                if (node == BusNode.POSITION_TICK)
                {
                    // TODO: All the other joints
                    if (forearm != null && forearmServoEnabled)
                    {
                        //Debug.WriteLine("forearm: " + (forearm.Roll * 180) / Math.PI + " / " + (forearm.Pitch * 180) / Math.PI + " / " + (forearm.Yaw * 180) / Math.PI);
                        if (forearm.Pitch > ELBOW_PITCH_MAX_DEG * RAD_PER_DEG) forearm.Pitch = (float)(ELBOW_PITCH_MAX_DEG * RAD_PER_DEG);
                        if (forearm.Pitch < ELBOW_PITCH_MIN_DEG * RAD_PER_DEG) forearm.Pitch = (float)(ELBOW_PITCH_MIN_DEG * RAD_PER_DEG);

                        // scale to the valid range (1500 - 2200)
                        finalForearmPosition = 1500 + (int)((
                            (forearm.Pitch - ELBOW_PITCH_MIN_DEG * RAD_PER_DEG)
                                / ((ELBOW_PITCH_MAX_DEG - ELBOW_PITCH_MIN_DEG) * RAD_PER_DEG)
                            ) * 700);

                        // move the stuff!
                        // TODO: Do This correctly
                        if (Math.Abs(finalForearmPosition - currentForearmPosition) >= positionDeltaThreshold)
                        {
                            currentForearmPosition = finalForearmPosition;
                            move(FOREARM, currentForearmPosition, forearmServoSpeed);
                            Debug.WriteLine("Got " + currentForearmPosition);
                        }
                    }

                    if (arm != null && armServoEnabled)
                    {
                        if (arm.Pitch > SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG) arm.Pitch = (float)(SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG);
                        if (arm.Pitch < SHOULDER_PITCH_MIN_DEG * RAD_PER_DEG) arm.Pitch = (float)(SHOULDER_PITCH_MIN_DEG * RAD_PER_DEG);

                        // scale to the valid range (800 - 2200)
                        if (arm.Pitch > 0)
                        {
                            finalArmPosition = 1500 - (int)((
                                (arm.Pitch /
                                    (SHOULDER_PITCH_MAX_DEG * RAD_PER_DEG)
                                )
                                ) * 700);
                        }

                        else
                        {
                            finalArmPosition = 1500 + (int)((
                                (Math.Abs(arm.Pitch) /
                                    (Math.Abs(SHOULDER_PITCH_MIN_DEG) * RAD_PER_DEG)
                                )
                                ) * 700);
                        }

                        // move the stuff!
                        // TODO: Do This correctly
                        if (Math.Abs(finalArmPosition - currentForearmPosition) >= positionDeltaThreshold)
                        {
                            currentArmPosition = finalArmPosition;
                            move(ARM, currentArmPosition, armServoSpeed);
                            Debug.WriteLine("ARM: " + ARM + ", currentArmPosition: " + currentArmPosition);
                        }
                    }

                    if (arm != null && shoulderServoEnabled)
                    {
                        if (arm.Yaw > SHOULDER_YAW_MAX_DEG * RAD_PER_DEG) arm.Yaw = (float)(SHOULDER_YAW_MAX_DEG * RAD_PER_DEG);
                        if (arm.Yaw < SHOULDER_YAW_MIN_DEG * RAD_PER_DEG) arm.Yaw = (float)(SHOULDER_YAW_MIN_DEG * RAD_PER_DEG);

                        // scale to the valid range (800 - 2200)
                        if (arm.Yaw > 0)
                        {
                            finalShoulderPosition = 1500 - (int)((
                                (arm.Yaw /
                                    ((SHOULDER_YAW_MAX_DEG - SHOULDER_YAW_MIN_DEG) * RAD_PER_DEG)
                                ) //* (180 / ((float)Math.Abs(SHOULDER_PITC_MIN_DEG_PHYSICAL)))
                                ) * 700);
                        }
                        else
                        {
                            finalShoulderPosition = 1500 + (int)((
                                (Math.Abs(arm.Yaw) /
                                    ((SHOULDER_YAW_MAX_DEG - SHOULDER_YAW_MIN_DEG) * RAD_PER_DEG)
                                ) //* (180 / ((float)Math.Abs(SHOULDER_PITCH_MIN_DEG_PHYSICAL)))
                                ) * 700);
                        }

                        // move the stuff!
                        // TODO: Do This correctly
                        if (Math.Abs(finalShoulderPosition - currentShoulderPosition) >= positionDeltaThreshold)
                        {
                            currentShoulderPosition = finalShoulderPosition;
                            move(SHOULDER, finalShoulderPosition, shoulderServoSpeed);
                        }
                    }

                }

                 if (node == BusNode.CLAW_OPEN_PERCENT && handServoEnabled)
                {
                    // scale to the valid range (1000 - 2000)
                    finalHandPosition = 1500 + ((-hand + 50) * 10);

                    // move the stuff!
                    // TODO: Do This correctly
                    if (Math.Abs(finalHandPosition - currentHandPosition) >= positionDeltaThreshold)
                    {
                        currentHandPosition = finalHandPosition;
                        move(HAND, currentHandPosition, handServoSpeed);
                    }
                }

                 if (node == BusNode.WRIST_PERCENT && wristServoEnabled) // flexion/extension
                {
                    finalWristPosition = 1380 + (int)((-wrist_hand + 50) * 12.4f);

                    if (Math.Abs(finalWristPosition - currentWristPosition) >= positionDeltaThreshold)
                    {
                        currentWristPosition = finalWristPosition;
                        move(WRIST, currentWristPosition, wristServoSpeed);
                    }
                }
                

            }
        }

        /// <summary>
        /// This method is used to move a servo to a new position at a desired speed.
        /// </summary>
        /// <param name="servo"></param>
        /// <param name="pos"></param>
        /// <param name="speed"></param>
        private void move(int servo, int pos, int speed)
        {
            if (pos < 800 || pos > 2200)
            {
                Debug.WriteLine("ERROR: servo " + servo + " set to invalid position " + pos);
                pos = (pos < 800) ? 800 : 2200;
            }

            //Debug.WriteLine("Moving servo " + servo + " to position " + pos);

            string command = "#" + servo + "P" + pos + "S" + speed + "\r\n";
            send(command);
        }

        /// <summary>
        /// This helper method sends a command to the robot arm through a serial port.
        /// </summary>
        /// <param name="command"></param>
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
