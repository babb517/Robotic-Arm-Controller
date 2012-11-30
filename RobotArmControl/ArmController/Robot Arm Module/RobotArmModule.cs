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
            move(1, 1500);
            move(2, 1500);

            // Subscribe to the kinect data tick
            Bus.Subscribe(BusNode.POSITION_TICK, OnValuePublished);
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
            
            string data = _serialPort.ReadLine();

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
            //if (System.DateTime.Now.Millisecond < _lastUpdateTime + MIN_UPDATE_INTERVAL) return;
            _lastUpdateTime = System.DateTime.Now.Millisecond;

            // What we want to do here is update our 'goal' position based on the data we can read from the virtual bus.
            // Since we've subscribed to the POSITION_TICK, the 'value' parameter has no meaning, we should get the value directly from the bus.
            Orientation arm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_UPPER_ARM);
            Orientation forearm = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_LOWER_ARM);
            Orientation wrist = Bus.Get<Orientation>(BusNode.ORIENTATION_RIGHT_HAND);

            // we should now convert these orientations to the values expected by the servo controller.

            

            // WRIST:
            if (wrist != null)
            {
                //Debug.WriteLine("Wrist: " + (wrist.Roll * 180) / Math.PI + " / " + (wrist.Pitch * 180) / Math.PI + " / " + (wrist.Yaw * 180) / Math.PI );

                // clip to +- 30 degrees
                if (wrist.Yaw > 30 * RAD_PER_DEG) wrist.Yaw = (float)(30 * RAD_PER_DEG);
                if (wrist.Yaw < -30 * RAD_PER_DEG) wrist.Yaw = (float)(-30 * RAD_PER_DEG);

                // scale to the valid range (1000 - 2000)
                int wrist_pos = 1000 + (int)(((-wrist.Yaw + (30 * RAD_PER_DEG)) / (60 * RAD_PER_DEG)) * 1000);

                // move the stuff!
                // TODO: Do This correctly
                move(3, wrist_pos);
            }



            // TODO: All the other joints
            if (forearm != null)
            {
                //Debug.WriteLine("forearm: " + (forearm.Roll * 180) / Math.PI + " / " + (forearm.Pitch * 180) / Math.PI + " / " + (forearm.Yaw * 180) / Math.PI);
                if (forearm.Pitch > 90 * RAD_PER_DEG) forearm.Pitch = (float)(90 * RAD_PER_DEG);
                if (forearm.Pitch < 0 * RAD_PER_DEG) forearm.Pitch = (float)(0 * RAD_PER_DEG);

                // scale to the valid range (1000 - 2000)
                int forearm_pos = 1000 + (int)(((-forearm.Pitch + (120 * RAD_PER_DEG)) / (180 * RAD_PER_DEG)) * 1000);

                // move the stuff!
                // TODO: Do This correctly
                move(2, forearm_pos);
            }
            
            if (arm != null)
            {
                Debug.WriteLine("arm: " + (arm.Roll * 180) / Math.PI + " / " + (arm.Pitch * 180) / Math.PI + " / " + (arm.Yaw * 180) / Math.PI);
                if (arm.Pitch > 40 * RAD_PER_DEG) arm.Pitch = (float)(40 * RAD_PER_DEG);
                if (arm.Pitch < -80 * RAD_PER_DEG) arm.Pitch = (float)(-80 * RAD_PER_DEG);

                // scale to the valid range (1000 - 2000)
                int arm_pos = 1000 + (int)(((-arm.Pitch + (40 * RAD_PER_DEG)) / (80 * RAD_PER_DEG)) * 1000);

                // move the stuff!
                // TODO: Do This correctly
                move(1, arm_pos);
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
            catch (Exception ex)
            {
               Debug.WriteLine("Oh noes! something went wrong!");
            }

        }

        #endregion Private Methods









    }
}
