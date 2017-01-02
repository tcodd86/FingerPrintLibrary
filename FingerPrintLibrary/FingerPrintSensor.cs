using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FingerPrintLibrary
{
    public class FingerPrintSensor
    {
        private SerialWrapper Wrapper { get; set; }
        public int templateCapacity { get; private set; }
        public string ErroMessage { get; private set; }

        #region Constructors
        /// <summary>
        /// Initializes new fingerprint sensor.
        /// </summary>
        /// <param name="baudRate">Baud rate of sensor.</param>
        /// <param name="address">Address of sensor.</param>
        /// <param name="ABC">Model specifier indicating capacity. A, B, or C.</param>
        public FingerPrintSensor(int baudRate, string address, char ABC)
        {
            //Timeout to allow sensor to warm up per spec
            Thread.Sleep(500);
            Wrapper = new SerialWrapper(baudRate, address);
            switch (ABC)
            {
                case 'B':
                    templateCapacity = 375;
                    break;
                case 'C':
                    templateCapacity = 880;
                    break;
                case 'A':
                default:
                    templateCapacity = 120;
                    break;
            }
        }

        /// <summary>
        /// Uses default Baud Rate of 57600 and default of model 'A' with 120 fingerprint capacity.
        /// </summary>
        /// <param name="address">
        /// Address of fingerprint sensor.
        /// </param>
        public FingerPrintSensor(string address) : this(57600, address, 'A')
        { }

        /// <summary>
        /// Uses default model 'A' with 120 fingerprint capacity.
        /// </summary>
        /// <param name="address">Address of sensor.</param>
        /// <param name="baudRate">Baud rate.</param>
        public FingerPrintSensor(string address, int baudRate) : this(baudRate, address, 'A')
        { }

        /// <summary>
        /// Uses default Baud rate of 57600.
        /// </summary>
        /// <param name="address">Address of sensor.</param>
        /// <param name="ABC">Model of sensor to determine capacity. A, B, or C.</param>
        public FingerPrintSensor(string address, char ABC) : this(57600, address, ABC)
        { }
        #endregion
        
        public SensorResponse HandShake()
        {
            var send = DataPackageUtilities.Handshake();
            return SendPackageParseResults(send);
        }

        public SensorResponse ReadLibaryPosition(byte[] position)
        {
            var send = DataPackageUtilities.ReadTemplateAtLocation(position);
            return SendPackageParseResults(send);
        }

        /// <summary>
        /// Attempts to read fingerprint off of sensor.
        /// </summary>
        /// <param name="maxAttempts">
        /// Maximum number of reads attempted. Default value is 10.
        /// </param>
        /// <returns>
        /// True if fingerprint is successfully read.
        /// </returns>
        public SensorResponse ReadFingerprint(int maxAttempts = 100)
        {
            var send = DataPackageUtilities.GenerateImage();
            var success = false;
            byte[] result = new byte[12];
            int attempts = 0;
            SensorResponse response = new SensorResponse(false, "Failed to read fingerprint");
            while (!success && attempts < maxAttempts)
            {
                //wait 1/10 of a second between attempts
                Thread.Sleep(100);

                response = SendPackageParseResults(send);
                success = response.Success;
                attempts++;
            }
                     
            return response;
        }

        public SensorResponse GenerateCharacterFileFromImage(byte buffer = 0x01)
        {
            var send = DataPackageUtilities.GenerateCharFileFromImgDataPackage(buffer);
            return SendPackageParseResults(send);
        }

        public SensorResponse GenerateTemplate()
        {
            var send = DataPackageUtilities.GenerateTemplate();
            return SendPackageParseResults(send);
        }

        /// <summary>
        /// Finds the number of templates stored in the library.
        /// </summary>
        /// <param name="numberOfTemplates">
        /// Variable to have number of templates stored in it.
        /// </param>
        /// <returns></returns>
        public SensorResponse ReadValidTemplateNumber(out Int16 numberOfTemplates)
        {
            var send = DataPackageUtilities.ReadValidTemplateNumber();
            var result = Wrapper.SendAndReadSerial(send).Result;
            numberOfTemplates = DataPackageUtilities.ByteToShort(DataPackageUtilities.ParsePackageContents(result));
            return new SensorResponse(DataPackageUtilities.ParsePackageConfirmationCode(result));
        }

        public SensorResponse StoreTemplate(Int16 positionToStoreTemplate, byte charBufferToUse)
        {
            var positionAsBytes = DataPackageUtilities.ShortToByte(positionToStoreTemplate);
            var send = DataPackageUtilities.StoreTemplate(charBufferToUse, positionAsBytes);
            return SendPackageParseResults(send);
        }

        /// <summary>
        /// Does a precise match of the templates in buffer 1 and 2.
        /// </summary>
        /// <param name="matchingScore">Matching score.</param>
        /// <returns></returns>
        public SensorResponse PreciseMatchFingerprint(out short matchingScore)
        {
            var preciseMatching = DataPackageUtilities.MatchFingerPrint();
            var result = Wrapper.SendAndReadSerial(preciseMatching).Result;
            matchingScore = DataPackageUtilities.ByteToShort(DataPackageUtilities.ParsePackageContents(result));
            return new SensorResponse(DataPackageUtilities.ParsePackageConfirmationCode(result));
        }

        /// <summary>
        /// Quick search.
        /// </summary>
        /// <param name="bufferID">Buffer to search against. Defaults to buffer 0x01.</param>
        /// <returns></returns>
        public SearchResponse Search(byte bufferID = 0x01)
        {
            var search = DataPackageUtilities.Search(bufferID);
            var result = Wrapper.SendAndReadSerial(search).Result;
            byte confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            var data = DataPackageUtilities.ParsePackageContents(result);
            short pageNumber = DataPackageUtilities.ByteToShort(data.Take(2));
            short matchingScore = DataPackageUtilities.ByteToShort(data.Skip(2));
            return new SearchResponse(confirmationCode, pageNumber, matchingScore);
        }

        /// <summary>
        /// Sets the address of the module to the provided value.
        /// </summary>
        /// <param name="newAddress">The address to set the device to. Must be 4 bytes long. Will be set to current device address when returned.</param>
        /// <returns>Successfully changed address.</returns>
        /// <exception cref="ArgumentException">If newAddress is not Length = 4.</exception>
        public SensorResponse SetAddress(ref byte[] newAddress)
        {
            var send = DataPackageUtilities.SetAddress(newAddress);
            var result = Wrapper.SendAndReadSerial(send).Result;
            //pull address out of return packet. It will be the current address of the sensor whether it got changed or not.
            newAddress = result.Skip(2).Take(4).ToArray();
            return new SensorResponse(DataPackageUtilities.ParsePackageConfirmationCode(result));
        }

        public  ImageResponse UploadImageToComputer()
        {
            var send = DataPackageUtilities.UploadImageToComputer();
            var result = Wrapper.SendAndReadSerial(send).Result;
            var confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);

            if (DataPackageUtilities.ParseSuccess(result))
            {
                var imageBytes = DataPackageUtilities.ParseImage(result);
                //var stream = new MemoryStream(imageBytes);
                //IntPtr pointer = new IntPtr();
                //return new ImageResponse(confirmationCode, new Bitmap(256, 288, 256, PixelFormat.Format8bppIndexed, pointer));
                return new ImageResponse(confirmationCode, CopyDataToBitmap(imageBytes));
            }
            else
            {
                return new ImageResponse(confirmationCode);
            }
        }

        /// <summary>
        /// function CopyDataToBitmap
        /// Purpose: Given the pixel data return a bitmap of size [352,288],PixelFormat=24RGB 
        /// </summary>
        /// <param name="data">Byte array with pixel data</param>
        private Bitmap CopyDataToBitmap(byte[] data)
        {
            //Here create the Bitmap to the know height, width and format
            //Bitmap bmp = new Bitmap(256, 288);
            Bitmap bmp = new Bitmap(Image.FromStream(new MemoryStream(data)), new Size(256, 288));
            //Create a BitmapData and Lock all pixels to be written 
            BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.WriteOnly, bmp.PixelFormat);

            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            //Unlock the pixels
            bmp.UnlockBits(bmpData);

            //Return the bitmap 
            return bmp;
        }

        #region SystemParameters
        public bool GetStatusRegister(out byte[] statusRegister)
        {
            return ReadSystemParameter(out statusRegister, 0, 2);
        }

        public bool GetSystemIdentifierCode(out byte[] systemIdentifier)
        {
            return ReadSystemParameter(out systemIdentifier, 2, 2);
        }

        public bool GetFingerLibrarySize(out byte[] fingerLibrarySize)
        {
            return ReadSystemParameter(out fingerLibrarySize, 4, 2);
        }

        public bool GetSecurityLevel(out byte[] securityLevel)
        {
            return ReadSystemParameter(out securityLevel, 6, 2);
        }

        public bool GetDeviceAddress(out byte[] deviceAddress)
        {
            return ReadSystemParameter(out deviceAddress, 8, 4);
        }

        public bool GetDataPacketSize(out byte[] dataPacketSize)
        {
            return ReadSystemParameter(out dataPacketSize, 12, 2);
        }

        public bool GetBaudSettings(out byte[] baudSettings)
        {
            return ReadSystemParameter(out baudSettings, 14, 2);
        }

        private bool ReadSystemParameter(out byte[] statusRegister, int skip, int take)
        {
            byte confirmationCode;
            bool success;
            var result = GetSystemParameters(out confirmationCode, out success);
            if (!success)
            {
                statusRegister = new byte[] { };

            }
            else
            {
                statusRegister = result.Skip(skip).Take(take).ToArray();
            }
            return success;
        }
        
        private byte[] GetSystemParameters(out byte confirmationCode, out bool success)
        {
            var readSysParam = DataPackageUtilities.ReadSystemParameters();
            var result = Wrapper.SendAndReadSerial(readSysParam).Result;
            confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            success = DataPackageUtilities.ParseSuccess(result);
            return result;
        }
#endregion

        public string GetConfirmationCodeMessage(byte confirmationCode)
        {
            string message = "";
            SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
            return message;
        }

        /// <summary>
        /// Transmits send and parses results.
        /// </summary>
        /// <param name="send">Byte array to send to sensor.</param>
        /// <returns>
        /// True if fingerprint sensor returns success. False if failed.
        /// </returns>
        private SensorResponse SendPackageParseResults(byte[] send)
        {
            try
            {
                var result = Wrapper.SendAndReadSerial(send).Result;
                return new SensorResponse(DataPackageUtilities.ParsePackageConfirmationCode(result));
            }
            catch (Exception ex)
            {
                return new SensorResponse(false, ex.Message);
            }            
        }

        /// <summary>
        /// Generates a list of used library positions
        /// </summary>
        /// <returns>
        /// List with addresses of any position in library that already has a template stored.
        /// </returns>
        public List<int> GetUsedLibraryPositions()
        {
            var positions = new List<int>();
            short count;

            ReadValidTemplateNumber(out count);

            for (short i = 0; i < templateCapacity - 1; i++)
            {
                var position = DataPackageUtilities.ShortToByte(i);
                var result = ReadLibaryPosition(position);
                if (result.Success)
                {
                    positions.Add((int)i);
                }

                if (positions.Count >= count)
                {
                    break;
                }
            }

            return positions;
        }

        public static bool DetermineNextAvailablePosition(out short position, List<int> positions, int templateCapacity)
        {
            position = -1;

            if (positions.Count == 0)
            {
                position = 0;
                return true;
            }
            else if (positions.Count == 1)
            {
                if (positions[0] == 0)
                {
                    position = 1;
                }
                else
                {
                    position = 0;
                }
                return true;
            }
            else
            {
                if (positions[0] != 0)
                {
                    position = 0;
                    return true;
                }
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    if (positions[i + 1] - positions[i] != 1)
                    {
                        position = (short)(i + 1);
                        if (position < templateCapacity)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (position == -1)
                {
                    position = (short)(positions[positions.Count - 1] + 1);
                    if (position >= templateCapacity)
                    {
                        position = -1;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Closes the serial port if open and disposes it.
        /// </summary>
        public void Disposeserial()
        {
            Wrapper.Disposeserial();
        }
    }
}
