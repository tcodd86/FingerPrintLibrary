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
        public int BaudRate { get; }

        public string Address { get; }

        private SerialPort Port { get; set; }

        #region Constructors
        /// <summary>
        /// Initializes new fingerprint sensor.
        /// </summary>
        /// <param name="baudRate">Baud rate of sensor.</param>
        /// <param name="address">Address of sensor.</param>
        public FingerPrintSensor(int baudRate, string address)
        {
            if (baudRate % 9600 != 0 || baudRate < 9600 || baudRate > 115200)
            {
                throw new ArgumentOutOfRangeException("baudRate", "Baud rate must be a multiple of 9600 between 9600 and 115200");
            }

            BaudRate = baudRate;
            Address = address;
            
            try
            {
                Port = new SerialPort(address, baudRate);
                Port.Open();
            }
            catch (Exception ex)
            {
                var error = new Exception("Error while initializing serial port.", ex);                
                throw error;
            }
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

        private void WriteByteArray(byte[] write)
        {
            Port.Write(write, 0, write.Length);
        }

        private void WriteByteArray(byte[] write, int offset)
        {
            Port.Write(write, offset, write.Length - offset);
        }



        #region Destructor
        ~FingerPrintSensor()
        {
            if (Port.IsOpen)
            {
                Port.Close();                
            }
            Port.Dispose();
        }
        #endregion
    }
}
