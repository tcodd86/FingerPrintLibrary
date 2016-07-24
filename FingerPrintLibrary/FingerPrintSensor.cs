using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace FingerPrintLibrary
{
    public class FingerPrintSensor
    {
        public SerialWrapper Wrapper;

        public string FailureReason;

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
        
        public bool HandShake()
        {
            var send = DataPackageUtilities.GenerateHandshakeDataPackage();

            var result = Wrapper.SendAndReadSerial(send).Result;
            
            var success = DataPackageUtilities.ParseSuccess(result);

            return success;
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
        public bool ReadFingerprint(int maxAttempts = 10)
        {
            var send = DataPackageUtilities.GenerateGenImageDataPackage();
            var success = false;

            int attempts = 0;
            while (!success && attempts < maxAttempts)
            {
                var result = Wrapper.SendAndReadSerial(send).Result;
                success = DataPackageUtilities.ParseSuccess(result);
                attempts++;
            }

            //set FailurReason here based on response
            return success;
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
