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

        public bool HandShake()
        {
            var send = new List<byte>();

            throw new NotImplementedException();
        }

        public bool ReceiveSuccess(byte[] received)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a byte array of data with a 2 byte checksum appended.
        /// </summary>
        /// <param name="data">
        /// byte list that needs a checksum
        /// </param>
        /// <returns>
        /// byte array of "data" with 2 byte checksum appended.
        /// </returns>
        public byte[] AddCheckSum(List<byte> data)
        {
            var fullSum = BitConverter.GetBytes(data.Sum(p => (int)p));

            var checkSum = new byte[2];

            if (fullSum.Length >= 2)
            {
                checkSum[0] = fullSum[0];
                checkSum[1] = fullSum[1];
            }
            else if (fullSum.Length == 1)
            {
                checkSum[0] = fullSum[0];
                checkSum[1] = 0x0;
            }
            else
            {
                throw new ArgumentOutOfRangeException("data", "data byte list must return a sum >= 0");
            }

            data.AddRange(checkSum);

            return data.ToArray();
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
