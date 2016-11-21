﻿using System;
using System.Collections.Generic;

using FingerPrintLibrary;
using System.Threading;

namespace FingerPrintTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            bool success = false;
            FingerPrintSensor sensor = null;

            while (!success)
            {
                Console.WriteLine("Available ports are:");
                var ports = SerialWrapper.GetPorts();
                foreach (string name in ports)
                {
                    Console.WriteLine(name);
                }
                Console.WriteLine();
                Console.WriteLine("Enter the COM port the fingerprint sensor is on and press Enter.");

                var comPort = Console.ReadLine();

                Console.WriteLine($"Looking for fingerprint sensor on port {comPort}.");

                sensor = new FingerPrintSensor(comPort);

                byte handshakeConfirmationCode;
                success = sensor.HandShake(out handshakeConfirmationCode);

                Console.WriteLine(success ? "Successfully connected to the fingerprint sensor." : "Failed to connect");
            }

            var read = string.Empty;

            while(!string.Equals(read, "exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Enter command (Search, Enroll, Library, Exit)");
                read = Console.ReadLine();
                switch (read)
                {
                    case "Search":
                        success = FingerPrintSearch(sensor);
                        Console.WriteLine(success ? "Matched!" : "Failed to match.");
                        Console.ReadLine();
                        break;
                    case "Enroll":
                        success = EnrollFingerPrint(sensor);
                        Console.WriteLine(success ? "Successfully enrolled fingerprint!" : "Failed to enroll.");
                        break;
                    case "Library":
                        ReadLibraryPositions(sensor);
                        break;
                    case "Exit":
                        read = "exit";
                        break;
                    default:
                        break;
                }
            }
        }

        public static bool EnrollFingerPrint(FingerPrintSensor sensor, short position = -1)
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
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 0x02);
            if (!success)
            {
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine("Failed to generate character file. Failure message: " + message);
                return success;
            }

            //5. Create template from Char buffer 1 & 2 which is stored in both Char buffers
            success = sensor.GenerateTemplate(out confirmationCode);
            //success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 0x01);
            if (!success)
            {
                SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                Console.WriteLine("Failed to generate template from character buffers. Failure message: " + message);
                return success;
            }

            //6. Store template in next available template position (if position = -1) or specified location
            if (position == -1)
            {
                //success = sensor.ReadValidTemplateNumber(out confirmationCode, out templateCount);
                var positions = sensor.GetUsedLibraryPositions();
                success = SensorFunctions.DetermineNextAvailablePosition(out position, positions, sensor.templateCapacity);

                if (!success)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to get available templates.");
                    return success;
                }

                success = sensor.StoreTemplate(out confirmationCode, position, 0x01);
                if (!success)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Console.WriteLine("Failed to store template. Failure message: " + message);
                    return success;
                }
            }
            else
            {
                if (position > sensor.templateCapacity - 1)
                {
                    throw new ArgumentOutOfRangeException($"position cannot be greater than {sensor.templateCapacity - 1}.");
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

        public static bool FingerPrintSearch(FingerPrintSensor sensor)
        {
            byte confirmationCode;
            bool success;
            //1. Read fingerprint twice, store in ImageBuffers
            success = sensor.ReadFingerprint(out confirmationCode);
            if (!success)
            {
                PrintFailureMessage(confirmationCode);
                return success;
            }
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 1);
            success = sensor.ReadFingerprint(out confirmationCode);
            if (!success)
            {
                PrintFailureMessage(confirmationCode);
                return false;
            }
            success = sensor.GenerateCharacterFileFromImage(out confirmationCode, 2);

            //3. Need to generate template first
            success = sensor.GenerateTemplate(out confirmationCode);
            if (!success)
            {
                PrintFailureMessage(confirmationCode);
                return false;
            }

            //4. Search for fingerprint in library
            short matchLevel;
            success = sensor.PreciseMatchFingerprint(out confirmationCode, out matchLevel);

            return success;
        }

        private static void PrintFailureMessage(byte confirmationCode)
        {
            string message;
            SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
            Console.WriteLine(message);
        }

        public static void ReadLibraryPositions(FingerPrintSensor sensor)
        {
            List<int> positions = sensor.GetUsedLibraryPositions();

            Console.WriteLine($"{positions.Count} templates are stored in the library in positions:");
            foreach (var pos in positions)
            {
                Console.WriteLine(pos);
            }
            Console.ReadLine();
        }

    }
}
