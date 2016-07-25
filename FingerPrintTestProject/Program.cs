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

            byte handshakeConfirmationCode;
            var success = sensor.HandShake(out handshakeConfirmationCode);

            Console.WriteLine(success ? "Successfully connected to the fingerprint sensor." : "Failed to connect");

            Console.ReadLine();
        }

        public bool EnrollFingerPrint(int position = -1)
        {
            throw new NotImplementedException();
            //1. Read fingerprint and store in ImageBuffer
            //2. Convert into Char file (Img2Tz) and store in Char Buffer 1
            //3. Read fingerprint again and store in ImageBuffer
            //4. Convert into Char file and store in Char Buffer 2
            //5. Create template from Char buffer 1 & 2 which is stored in both Char buffers
            //6. Store template in next available template position (if position = -1) or specified location
        }

        public bool FingerPrintSearch()
        {
            throw new NotImplementedException();
            //1. Read fingerprint and store in ImageBuffer
            //2. Convert into Char file and store in Char Buffer 1
            //3. Search for fingerprint in library
        }
    }
}
