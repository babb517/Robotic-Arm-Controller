using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;                  // for SerialPort
using System.Diagnostics;               // for Debug

namespace ServoTester
{
    class Program
    {
        static SerialPort sp;

        static void Main(string[] args)
        {
            sp = new SerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            sp.Open();

            int servo = 2;
            int pos = 1500;
            int speed = 100;

            char key;

            do
            {
                key = (Console.ReadKey(true)).KeyChar;
                

                switch (key)
                {
                    case ' ':
                        pos = 1505;
                        break;
                    case '1':
                        pos += 50;
                        break;
                    case '2':
                        pos -= 50;
                        break;
                    case '3':
                        pos += 10;
                        break;
                    case '4':
                        pos -= 10;
                        break;
                    case '5':
                        pos += 1;
                        break;
                    case '6':
                        pos -= 1;
                        break;
                }

                Console.WriteLine("Position: " + pos);

                string command = "#" + servo + "P" + pos + "S" + speed + "\r\n";
                send(command);

            } while (key != 27);

            sp.Close();
        }

        static private void send(string command)
        {
            string tempcommand = command + "\r\n";

            try
            {
                if (!sp.IsOpen)
                    sp.Open();

                sp.Write(tempcommand);

            }
            catch (Exception)
            {
                Debug.WriteLine("Oh noes! something went wrong!");
            }
        }
    }
}

