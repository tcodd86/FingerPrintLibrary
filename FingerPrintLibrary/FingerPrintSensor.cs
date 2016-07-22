using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

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
        /// Closes the serial port if open and disposes it.
        /// </summary>
        public void Disposeserial()
        {
            Wrapper.Disposeserial();
        }
    }
}
