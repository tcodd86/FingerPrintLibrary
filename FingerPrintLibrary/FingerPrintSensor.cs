using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        
        public bool HandShake(out byte confirmationCode)
        {
            var send = DataPackageUtilities.Handshake();
            return SendPackageParseResults(send, out confirmationCode);
        }

        public bool ReadLibaryPosition(byte[] position, out byte confirmationCode)
        {
            var send = DataPackageUtilities.ReadTemplateAtLocation(position);
            return SendPackageParseResults(send, out confirmationCode);
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
        public bool ReadFingerprint(out byte confirmationCode, int maxAttempts = 100)
        {
            var send = DataPackageUtilities.GenerateImage();
            var success = false;
            confirmationCode = 0x01;
            byte[] result = new byte[12];
            int attempts = 0;
            while (!success && attempts < maxAttempts)
            {
                //wait 1/10 of a second between attempts
                Thread.Sleep(100);

                success = SendPackageParseResults(send, out confirmationCode);
                attempts++;
            }
                     
            return success;
        }

        public bool GenerateCharacterFileFromImage(out byte confirmationCode, byte buffer = 0x01)
        {
            var send = DataPackageUtilities.GenerateCharFileFromImgDataPackage(buffer);
            return SendPackageParseResults(send, out confirmationCode);
        }

        public bool GenerateTemplate(out byte confirmationCode)
        {
            var send = DataPackageUtilities.GenerateTemplate();
            return SendPackageParseResults(send, out confirmationCode);
        }

        public bool ReadValidTemplateNumber(out byte confirmationCode, out Int16 numberOfTemplates)
        {
            var send = DataPackageUtilities.ReadValidTemplateNumber();
            var result = Wrapper.SendAndReadSerial(send).Result;
            confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            numberOfTemplates = DataPackageUtilities.ByteToShort(DataPackageUtilities.ParsePackageContents(result));
            return DataPackageUtilities.ParseSuccess(result);
        }

        public bool StoreTemplate(out byte confirmationCode, Int16 positionToStoreTemplate, byte charBufferToUse)
        {
            var positionAsBytes = DataPackageUtilities.ShortToByte(positionToStoreTemplate);
            var send = DataPackageUtilities.StoreTemplate(charBufferToUse, positionAsBytes);
            return SendPackageParseResults(send, out confirmationCode);
        }

        public bool PreciseMatchFingerprint(out byte confirmationCode, out short matchingScore)
        {
            var preciseMatching = DataPackageUtilities.MatchFingerPrint();
            var result = Wrapper.SendAndReadSerial(preciseMatching).Result;
            confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            matchingScore = DataPackageUtilities.ByteToShort(DataPackageUtilities.ParsePackageContents(result));
            return DataPackageUtilities.ParseSuccess(result);
        }

        public bool Search(out short pageNumber, out byte confirmationCode, out short matchingScore, byte bufferID = 0x01)
        {
            var search = DataPackageUtilities.Search(bufferID);
            var result = Wrapper.SendAndReadSerial(search).Result;
            confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            var data = DataPackageUtilities.ParsePackageContents(result);
            pageNumber = DataPackageUtilities.ByteToShort(data.Take(2));
            matchingScore = DataPackageUtilities.ByteToShort(data.Skip(2));
            return DataPackageUtilities.ParseSuccess(result);
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
        /// <param name="confirmationCode">byte that will contain confirmation code.</param>
        /// <returns>
        /// True if fingerprint sensor returns success. False if failed.
        /// </returns>
        private bool SendPackageParseResults(byte[] send, out byte confirmationCode)
        {
            var result = Wrapper.SendAndReadSerial(send).Result;
            confirmationCode = DataPackageUtilities.ParsePackageConfirmationCode(result);
            return DataPackageUtilities.ParseSuccess(result);
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
            byte confirmationCode;

            for (short i = 0; i < templateCapacity - 1; i++)
            {
                var position = DataPackageUtilities.ShortToByte(i);
                var result = ReadLibaryPosition(position, out confirmationCode);
                if (result)
                {
                    positions.Add((int)i);
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
