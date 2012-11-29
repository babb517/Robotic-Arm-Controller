using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;                  // for SerialPort
using System.Text.RegularExpressions;   // 

namespace CyberGloveLibrary
{
    class Program
    {
        static string[] sensorNames = 
        {
            "(nothing)",
            "Thumb Rotation",
            "Thumb MPJ",
            "Thumb IJ",
            "Thumb abduction"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Hi");

            SerialPort sp = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);

            sp.Open();
            //sp.Write("G");  // tell glove to start streaming

            string msg = "";
            
            while (true)
            {
                sp.Write("g");
                
                try
                {
                    msg = sp.ReadLine();
                    Console.WriteLine(msg);

                    string[] sensors = Regex.Split(msg, "\\s+");
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

                    /*
                    for (int i = 0; i < sensors.Length; i++)
                    {
                        if(i == 5 ||i == 8||i == 11||i == 14)
                        Console.WriteLine(i + ": " + sensors[i]);
                    }
                    */
                    int pinky = Convert.ToInt32(sensors[14]);
                    int ring = Convert.ToInt32(sensors[11]);
                    int middle = Convert.ToInt32(sensors[8]);
                    int index = Convert.ToInt32(sensors[5]);

                    //Starting Postion
                    if (pinky >= 120)
                    {

                    }

                    //start and stop
                    if (middle >= 145 && ring >= 145)
                    {

                    }

                    //Claw
                    int percent;
                    percent = ((index - 125) / 50) * 100;





                    /*while (sp.BytesToRead < 20) { }
                    msg = sp.ReadExisting();

                    Console.WriteLine(msg);*/

                    /*while (sp.BytesToRead < 20) { }
                    c = sp.Read(data, 0, 20);
                    //Console.WriteLine("Got " + c + " chars");

                    uint j;
                    for (int i = 0; i < c; i++)
                    {
                        j = data[i];
                        Console.Write(j + " ");
                    }
                    Console.Write("\n");

                    Array.Clear(data, 0, 20);
                     */

                    
                    /*// ***This does NOT frame the data, where we check for "G" at the start and null at the end and
                    // ensure we got 20 bytes.
                    while (sp.BytesToRead < 20) { }
                    msg = sp.ReadExisting();

                    //Console.WriteLine(BitConverter.ToString(Encoding.ASCII.GetBytes(msg)));
                    for (int i = 0; i < 20; i++)
                        Console.Write(" " + System.Convert.ToInt32(msg[i]));
                    Console.Write("\n");*/

                    
                }
                catch (TimeoutException)
                {
                    // NO
                }
                //break;
            }

            Console.ReadKey();

        } // end Main()
    } // end class Program
} // end namespace CyberGloveLibrary
