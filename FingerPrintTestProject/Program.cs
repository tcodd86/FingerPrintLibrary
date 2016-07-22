using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using FingerPrintLibrary;

namespace FingerPrintTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the COM port the fingerprint sensor is on and press Enter.");

            var comPort = Console.ReadLine();

            Console.WriteLine("Looking for fingerprint sensor on port " + comPort + ".");

            var sensor = new FingerPrintSensor(comPort);

            var success = sensor.HandShake();

            Console.WriteLine(success ? "Successfully connected to the fingerprint sensor." : "Failed to connect");

            Console.ReadLine();
        }
    }
}
