using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using FingerPrintLibrary;
using System.Threading;

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

            if (success)
            {
                var readSucccess = ReadFingerprint(sensor, out handshakeConfirmationCode);
                if (!readSucccess)
                {
                    string message;
                    SensorCodes.ConfirmationCodes.TryGetValue(handshakeConfirmationCode, out message);
                    Console.WriteLine($"{message}");
                }
            }
            Console.ReadLine();
        }

        public bool EnrollFingerPrint(FingerPrintSensor sensor, short position = -1)
        {
            byte confirmationCode;
            string message;

            //1. Read fingerprint and store in ImageBuffer
            var success = ReadFingerprint(sensor, out confirmationCode);

            if (!success)
            {
                return success;
            }

            //2. Convert into Char file (Img2Tz) and store in Char Buffer 1
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 0x01);
            if (!success)
            {
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine("Failed to generate character file. Failure message: " + message);
                return success;
            }

            //3. Read fingerprint again and store in ImageBuffer
            success = ReadFingerprint(sensor, out confirmationCode);
            if (!success)
            {
                return success;
            }

            //4. Convert into Char file and store in Char Buffer 2
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 0x01);
            if (!success)
            {
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine("Failed to generate character file. Failure message: " + message);
                return success;
            }

            //5. Create template from Char buffer 1 & 2 which is stored in both Char buffers
            success = sensor.GenerateTemplate(out confirmationCode);
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 0x01);
            if (!success)
            {
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine("Failed to generate template from character buffers. Failure message: " + message);
                return success;
            }

            //6. Store template in next available template position (if position = -1) or specified location
            if (position == -1)
            {
                Int16 templateCount;
                success = sensor.GetNumberOfTemplates(out confirmationCode, out templateCount);
                if (!success)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to read number of templates. Failure message: " + message);
                    return success;
                }

                success = sensor.StoreTemplate(out confirmationCode, templateCount, 0x01);
                if (!success)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to read number of templates. Failure message: " + message);
                    return success;
                }
            }
            else
            {
                if (position > 255)
                {
                    throw new ArgumentOutOfRangeException("position cannot be greater than 255.");
                }
                success = sensor.StoreTemplate(out confirmationCode, position, 0x01);
                if (!success)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to read number of templates. Failure message: " + message);
                    return success;
                }
            }

            return true;
        }

        private static bool ReadFingerprint(FingerPrintSensor sensor, out byte confirmationCode)
        {
            string message;
            bool read = false;
            int count = 0;
            confirmationCode = SensorCodes.OK;
            Console.WriteLine("Please place your fingerprint on the sensor. Reading will begin in 1 second.");
            Thread.Sleep(1000);

            Console.WriteLine("Waiting to read fingerprint.");

            while (read == false)
            {
                read = sensor.ReadFingerprint(out confirmationCode);
                if (!read)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to read finger print. Error Message: " + message);
                    Console.WriteLine("Attempting to read again in 1 second.");
                    Thread.Sleep(1000);
                }

                count++;
                if (count > 3)
                {
                    Console.WriteLine("Failed to read fingerprint.");
                    break;
                }
            }

            return read;
        }

        public bool FingerPrintSearch(FingerPrintSensor sensor)
        {
            byte confirmationCode;
            throw new NotImplementedException();
            //1. Read fingerprint and store in ImageBuffer
            var readSuccess = sensor.ReadFingerprint(out confirmationCode);
            if (!readSuccess)
            {
                string message;
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine(message);
                return false;
            }
            //2. Convert into Char file and store in Char Buffer 1
            var charSuccess = sensor.GenerateCharacterFileFromImage(out confirmationCode, 1);
            charSuccess = sensor.GenerateCharacterFileFromImage(out confirmationCode, 2);
            //3. Search for fingerprint in library
            //still need to implement
        }
    }
}
