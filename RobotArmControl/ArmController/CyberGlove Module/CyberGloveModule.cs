using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;                  // for SerialPort
using System.Text.RegularExpressions;   // for tokenization
using System.Threading;

using ArmController.Integration;        // for the inter-module bus
using System.Diagnostics;               // for Debug.*

namespace ArmController.CyberGloveLibrary
{
    /// <summary>
    /// A simple class to constantly read data from a CyberGlove.
    /// </summary>
    class GloveModule : ArmController.Integration.Module
    {

        Thread gloveThread;         // primary thread, started in OnInitialize() and joined in OnFinalize()
        bool running;               // flag set by OnFinalize() to let thread know to gracefully finish
        SerialPort sp;              // serial port used to communicate with the CyberGlove

        /// <summary>
        /// Open CyberGlove serial port and start data collection thread.
        /// </summary>
        protected override void OnInitialize()
        {

            Bus.Publish(BusNode.ROBOT_ACTIVE, false);       // ensure that the robot arm isn't moving (the robot arm should also default to not tracking)

            // Declare the serial port that the glove is using and open it.
            sp = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            sp.Open();

            gloveThread = new Thread(new ThreadStart(readGlove));   // create instance of main thread that collects data
            gloveThread.Start();                                    // start it

        } // end OnInitialize()

        /// <summary>
        /// Stop streaming CyberGlove and close the data collection thread.
        /// </summary>
        protected override void OnFinalize()
        {
            running = false;        // set flag to end main loop of gloveThread
            gloveThread.Join();     // wait for gloveThread to join (gracefully exit)
        }

        /// <summary>
        /// Method to endlessly stream data from a CyberGlove.  Should be threaded.
        /// </summary>
        protected void readGlove()
        {
            string msg = "";                // store line we read from the CyberGlove
            bool moving = false;            // local flag to store if robot arm is enabled.  We never READ this from the robot, but use it to control our toggle before sending to the robot arm module (via the bus).
            // Flags to store if we're currently in a gesture, so we don't repeatedly tell the bus a gesture is occurring, but rather only on change.
            bool movingGesture = false;     // gesture to toggle robot arm motion
            bool pinkyGesture = false;      // gesture to move to home position (not currently used -- see below)

            double indexFinger = 0, wrist = 0;              // store raw index first joint and wrist flexion/abduction values read from CyberGlove
            double index_percent = 0, wrist_percent = 0;    // Store calculated claw open amount and wrist flexion/abduction, respctively, to send to bus.

            running = true;                 // flag meaning that we should keep collecting data.  Set to false by OnFinalize(), which is called when the program is to shut down.

            // Endless loop to always listen for command (until 'running' is set false by OnFinalize())
            while (running)
            {
                //sp.Write("G");  // tell glove to start streaming
                sp.Write("g");  // request a single sample from the CyberGlove

                try
                {
                    Debug.WriteLine("Reading from CyberGlove...");
                    msg = sp.ReadLine();
                    Debug.WriteLine("Yay!");

                    /* Calibration ranges:
                     * 
                     * Sensor 5 - Index MPJ - byte 4 in manual - clamp control
                     *  125 (open) to 180 (closed) (craig)
                     *  125 (open) to 175 (closed) (Michael)
                       Sensors 8 & 11 - Middle and Ring first joint - (byte indices 9 and 13)
                        >145 bent for 8 & 11 (Craig)
                        >145 (Michael) 
                       Sensor 14 - Pinkie first joint - Pinkie PIJ (Byte index 17 in manual)
                        >145 consider bent (Craig)
                     *  >120 bent (Michael) 
                    */

                    // Tokenize the line read from the CyberGlove on whitespace.  Using defaults, it should be 'g' (the character sent to request the data),
                    // followed by a space and then 18 numeric values (followed by spaces or a null for the last).  The 18 values are the 18
                    // sensors on the glove.  See the manual for their names.
                    string[] sensors = Regex.Split(msg, "\\s+");    

                    if (sensors.Length >= 18)   // If we got at least 18 tokens (we should get 19)
                    {
                        // Note that values are same for both right and left hand gloves.
                        int pinky = Convert.ToInt32(sensors[14]);       // get flex of pinky's first joint
                        int ring = Convert.ToInt32(sensors[11]);        // get flex of ring finger's first joint
                        int middle = Convert.ToInt32(sensors[8]);       // get flex of middle finger's first joint
                        indexFinger = Convert.ToInt32(sensors[5]);      // get flex of index finger's first joint
                        wrist = Convert.ToInt32(sensors[17]);           // get flex/extension of wrist

                        // Detect "Starting Postion" gesture
                        // Currently we do NOT publish this gesture to the bus
                        if (pinky >= 120)
                        {
                            if (!pinkyGesture)
                            {
                                //Console.WriteLine("Moving to starting position");
                                pinkyGesture = true;
                               // Bus.Publish(BusNode.ROBOT_RESET, (object)null);
                            }
                        }
                        else
                        {
                            if (pinkyGesture)
                                pinkyGesture = false;
                        }

                        // Detect start/stop (robot arm) gesture
                        if (middle >= 145 && ring >= 145)   // If both middle and ring fingers are significantly flexed...
                        {
                            if (!movingGesture)             // If we're not already in the "moving" gesture...
                            {
                                if (moving == true)         // If the robot arm is moving, stop it.
                                {
                                    moving = false;
                                    Console.WriteLine("Telling robot arm to stop");
                                    Bus.Publish(BusNode.ROBOT_ACTIVE, false);
                                }
                                else                        // If the robot arm is not moving, enable it.
                                {
                                    moving = true;
                                    Console.WriteLine("Telling robot arm to begin tracking");
                                    Bus.Publish(BusNode.ROBOT_ACTIVE, true);
                                }
                                movingGesture = true;       // Record that we're in the gesture, so that we only update the bus if we re-enter the gesture.
                            }
                        }
                        else                                // If we're not in the moving gesture...
                        {
                            if (movingGesture)              // ... and we were, record the change, so we can re-enter the gesture later.
                                movingGesture = false;
                        }


                        /*
                         * Calculate Claw openness based on index finger's first joint
                         * range is 125 to 180
                         */
                        //Debug.WriteLine("The index finger is at " + indexFinger);
                        index_percent = ((indexFinger - 125) / 55) * 100;
                        if (index_percent < 0)
                            index_percent = 0;
                        else if (index_percent > 100)
                            index_percent = 100;

                        /*
                         * Wrist flexion/extension calculations
                         */
                        // Craig: extension (hand up): 75 (roughly 100 deg from top of arm)
                        // Craig centered (hand straight): 170 (180 degrees)
                        // Craig flexion (hand down): 215 (roughly 120 deg from bottom of arm)
                        
                        // Calculation is piecewise so centered one hand is actually centered on gripper, but we can scale each half separately.
                        if (wrist >= 170)
                            wrist_percent = (wrist - 170) / 45 * 50 + 50;
                        else
                            wrist_percent = (wrist - 75) / 95 * 50;

                        if (wrist_percent < 0)
                            wrist_percent = 0;
                        else if (wrist_percent > 100)
                            wrist_percent = 100;

                        /*
                         * Send data to robot arm (publish our data to the bus)
                         */
                        //Console.WriteLine("The claw is open " + percent + " percent");
                        //Debug.WriteLine("The index finger is at " + (100 - (int)index_percent));
                        //Debug.WriteLine("Raw wrist: " + wrist + ", percent: " + (int)wrist_percent);
                        Debug.WriteLine("Moving gesture? " + movingGesture);
                        Bus.Publish(BusNode.CLAW_OPEN_PERCENT, 100 - (int)index_percent);
                        Bus.Publish(BusNode.WRIST_PERCENT, (int)wrist_percent);
                    }


                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("Timed out waiting for CyberGlove data!");
                }
            } // end endless loop reading from CyberGlove

        } // end of readGlove()

    } // end class GloveModule

} // end namespace CyberGloveLibrary
