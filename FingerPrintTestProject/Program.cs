using System;
using System.Collections.Generic;

using FingerPrintLibrary;
using System.Threading;
using System.IO;
using System.Drawing;

namespace FingerPrintTestProject
{
    class Program
    {
        static void Main(string[] args)
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

            var sensor = new SimpleSensor(comPort);
                
            var response = sensor.HandShake();

            Console.WriteLine(response.Success ? "Successfully connected to the fingerprint sensor." : "Failed to connect");
            if (!response.Success)
            {
                Console.WriteLine("Failed to connect to Fingerprint Sensor. Please check connections and try again.");
                Console.WriteLine("Press enter to end the application.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var read = string.Empty;

            while(!string.Equals(read, "exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Enter command (Search, Enroll, Library, Image, Exit)");
                read = Console.ReadLine();
                switch (read)
                {
                    case "Search":
                        var search = sensor.FingerPrintSearch();
                        Console.WriteLine(search.Success ? $"Matched! Found on page {search.PageNumber} with match score of {search.MatchLevel}." : $"Failed to match. Error message {response.ErrorMessage}.");
                        Console.ReadLine();
                        break;
                    case "Enroll":
                        response = sensor.EnrollFingerPrint();
                        Console.WriteLine(response.Success ? "Successfully enrolled fingerprint!" : $"Failed to enroll. Error message: {response.ErrorMessage}");
                        break;
                    case "Library":
                        ReadLibraryPositions(sensor.fingerprintSensor);
                        break;
                    case "Image":
                        Console.WriteLine("Enter filename;");
                        var fileName = Console.ReadLine();
                        GetImage(sensor.fingerprintSensor, fileName);
                        break;
                    case "Exit":
                        read = "exit";
                        break;
                    default:
                        break;
                }
            }
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

        public static void GetImage(FingerPrintSensor sensor, string pictureName)
        {
            var read = sensor.ReadFingerprint();
            if (!read.Success)
            {
                Console.WriteLine("Failed to read fingerprint.");
                return;
            }

            var response = sensor.UploadImageToComputer();

            if (response.Success)
            {
                var filePath = @"C:\Users\Public\Pictures\" + (pictureName) + ".bmp";
                response.Picture.Save(filePath);
            }
            else
            {
                Console.WriteLine("Failed to load image from sensor.");
                Console.ReadLine();
            }
        }
    }
}
