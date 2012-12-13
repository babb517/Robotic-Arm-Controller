using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;                  // for SerialPort
using System.Text.RegularExpressions;   // 
using System.Threading;

using ArmController.Integration;
using System.Diagnostics;

namespace ArmController.CyberGloveLibrary
{
    class GloveModule : ArmController.Integration.Module
    {

        //static string[] sensorNames = 
        //{
        //    "(nothing)",
        //    "Thumb Rotation",
        //    "Thumb MPJ",
        //    "Thumb IJ",
        //    "Thumb abduction"
        //};

        Thread gloveThread;         // primary thread, started in OnInitialize() and joined in OnFinalize()
        bool running;               // flag set by OnFinalize() to let thread know to gracefully finish
        SerialPort sp;              // serial port used to communicate with the CyberGlove

        /// <summary>
        /// Open CyberGlove serial port and start data collection thread.
        /// </summary>
        protected override void OnInitialize()
        {
            // //Console.WriteLine("Hi");

            //Declare the serial port that the glove is using and open it.
            sp = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            sp.Open();

            Bus.Publish(BusNode.ROBOT_ACTIVE, false);

            gloveThread = new Thread(new ThreadStart(readGlove));
            gloveThread.Start();

        } // end OnInitialize()

        /// <summary>
        /// Stop streaming CyberGlove and close the data collection thread.
        /// </summary>
        protected override void OnFinalize()
        {
            running = false;

            gloveThread.Join();
        }

        /// <summary>
        /// Method to endlessly stream data from a CyberGlove.  Should be threaded.
        /// </summary>
        protected void readGlove()
        {
            string msg = "";
            bool moving = false;
            bool movingGesture = false;
            bool pinkyGesture = false;

            double index_percent = 0, wrist_percent = 0;
            double indexFinger = 0, wrist = 0;

            running = true;

            //Endless loop to always listen for commands.
            while (running)
            {
                //This is sent to the glove to begin receiving commands.

                //sp.Write("G");  // tell glove to start streaming
                sp.Write("g");  // request a single sample

                try
                {
                    Debug.WriteLine("Reading...");
                    msg = sp.ReadLine();
                    Debug.WriteLine("Yay!");
                    //Console.WriteLine(msg);

                    /* Sensor 5 - Index MPJ - byte 4 in manual - clamp control
                     *  125 (open) to 180 (closed) (craig)
                     *  125 (open) to 175 (closed) (Michael)
                       Sensors 8 & 11 - Middle and Ring first joint - (byte indices 9 and 13)
                        >145 bent for 8 & 11 (Craig)
                        >145 (Michael) 
                       Sensor 14 - Pinkie first joint - Pinkie PIJ (Byte index 17 in manual)
                        >145 consider bent (Craig)
                     *  >120 bent (Michael) 
                    */

                    string[] sensors = Regex.Split(msg, "\\s+");

                    if (sensors.Length >= 18)
                    {
                        int pinky = Convert.ToInt32(sensors[14]);
                        int ring = Convert.ToInt32(sensors[11]);
                        int middle = Convert.ToInt32(sensors[8]);
                        indexFinger = Convert.ToInt32(sensors[5]);
                        wrist = Convert.ToInt32(sensors[17]);

                        //Starting Postion
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

                        //start and stop
                        if (middle >= 145 && ring >= 145)
                        {
                            if (!movingGesture)
                            {
                                if (moving == true)
                                {
                                    moving = false;
                                    //Console.WriteLine("Not Moving");
                                    Bus.Publish(BusNode.ROBOT_ACTIVE, false);
                                }
                                else
                                {
                                    moving = true;
                                    //Console.WriteLine("Moving");
                                    Bus.Publish(BusNode.ROBOT_ACTIVE, true);
                                }
                                movingGesture = true;
                            }
                        }
                        else
                        {
                            if (movingGesture)
                                movingGesture = false;
                        }


                        //Claw
                        //Debug.WriteLine("The index finger is at " + indexFinger);
                        // range is 125 to 180
                        /*
                         * Index finger calculations
                         */
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
                        if (wrist >= 170)
                            wrist_percent = (wrist - 170) / 45 * 50 + 50;
                        else
                            wrist_percent = (wrist - 75) / 95 * 50;

                        //wrist_percent = ((wrist - 75) / 140) * 100;
                        if (wrist_percent < 0)
                            wrist_percent = 0;
                        else if (wrist_percent > 100)
                            wrist_percent = 100;

                        /*
                         * Send data to robot arm
                         */
                        //if (moving == true)
                        //{
                            //Console.WriteLine("The claw is open " + percent + " percent");
                            //Debug.WriteLine("The index finger is at " + (100 - (int)index_percent));
                            //Debug.WriteLine("Raw wrist: " + wrist + ", percent: " + (int)wrist_percent);
                            Debug.WriteLine("Moving" + movingGesture);
                            Bus.Publish(BusNode.CLAW_OPEN_PERCENT, 100 - (int)index_percent);
                            Bus.Publish(BusNode.WRIST_PERCENT, (int)wrist_percent);
                        //}
                    }

                    /*
                  for (int i = 0; i < sensors.Length; i++)
                  {
                      if(i == 5 ||i == 8||i == 11||i == 14)
                      //Console.WriteLine(i + ": " + sensors[i]);
                  }
                  */


                    /*while (sp.BytesToRead < 20) { }
                    msg = sp.ReadExisting();

                    //Console.WriteLine(msg);*/

                    /*while (sp.BytesToRead < 20) { }
                    c = sp.Read(data, 0, 20);
                    ////Console.WriteLine("Got " + c + " chars");

                    uint j;
                    for (int i = 0; i < c; i++)
                    {
                        j = data[i];
                        //Console.Write(j + " ");
                    }
                    //Console.Write("\n");

                    Array.Clear(data, 0, 20);
                     */


                    /*// ***This does NOT frame the data, where we check for "G" at the start and null at the end and
                    // ensure we got 20 bytes.
                    while (sp.BytesToRead < 20) { }
                    msg = sp.ReadExisting();

                    ////Console.WriteLine(BitConverter.ToString(Encoding.ASCII.GetBytes(msg)));
                    for (int i = 0; i < 20; i++)
                        //Console.Write(" " + System.Convert.ToInt32(msg[i]));
                    //Console.Write("\n");*/


                }
                catch (TimeoutException)
                {
                    // NO
                }
                //break;
            } // end endless loop reading from CyberGlove

            ////Console.ReadKey();
        } // end of readGlove()

    } // end class Program
} // end namespace CyberGloveLibrary
