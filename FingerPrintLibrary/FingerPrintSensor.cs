using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FingerPrintLibrary
{
    public class FingerPrintSensor
    {
        public SerialWrapper Wrapper;

        #region Constructors
        /// <summary>
        /// Initializes new fingerprint sensor.
        /// </summary>
        /// <param name="baudRate">Baud rate of sensor.</param>
        /// <param name="address">Address of sensor.</param>
        public FingerPrintSensor(int baudRate, string address)
        {
            //Timeout to allow sensor to warm up per spec
            Thread.Sleep(500);
            Wrapper = new SerialWrapper(baudRate, address);
        }

        /// <summary>
        /// Uses default Baud Rate of 57600
        /// </summary>
        /// <param name="address">
        /// Address of fingerprint sensor.
        /// </param>
        public FingerPrintSensor(string address) : this(57600, address)
        { }
        #endregion
        
        public bool HandShake(out byte confirmationCode)
        {
            var send = DataPackageUtilities.Handshake();
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
        public bool ReadFingerprint(out byte confirmationCode, int maxAttempts = 10)
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

        public bool GetNumberOfTemplates(out byte confirmationCode, out Int16 numberOfTemplates)
        {
            var send = DataPackageUtilities.GetNumberOfTemplates();
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
        /// Closes the serial port if open and disposes it.
        /// </summary>
        public void Disposeserial()
        {
            Wrapper.Disposeserial();
        }
    }
}
