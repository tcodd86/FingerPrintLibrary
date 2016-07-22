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
            var send = GenerateHandshakeInstruction();

            var result = Wrapper.SendAndReadSerial(send).Result;
            
            var success = ParseSuccess(result);

            return success;
        }

        public byte[] GenerateHandshakeInstruction()
        {
            var handshake = GenerateDataPackageStart(SensorCodes.PID_COMMANDPACKET);

            var length = new byte[2] { 0x04, 0x00 };
            handshake.AddRange(length.Reverse());
            handshake.Add(SensorCodes.HANDSHAKE);
            handshake.Add(0x0);
            return AddCheckSum(handshake);
        }
        
        public int ParseLength(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public bool ParseSuccess(byte[] buffer)
        {
            if (buffer.Length < 10)
            {
                throw new IndexOutOfRangeException("buffer must have a length of 12 or greater.");
            }

            if (buffer[9] > 0x00)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public byte ParseReturn(byte[] buffer)
        {
            if (buffer.Length < 7)
            {
                throw new IndexOutOfRangeException("buffer must have a length of 12 or greater.");
            }

            return buffer[6];
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
            //get sum of everything but the header and chip address
            var fullSum = BitConverter.GetBytes(data.Skip(6).Sum(p => (int)p));

            //transposition to checkSum relies on it being little endian format
            if (!BitConverter.IsLittleEndian)
            {
                fullSum = fullSum.Reverse().ToArray();
            }

            var checkSum = new byte[2];

            if (fullSum.Length >= 2)
            {
                checkSum[0] = fullSum[0];
                checkSum[1] = fullSum[1];
            }
            else if (fullSum.Length == 1)
            {
                checkSum[0] = fullSum[0];
                checkSum[1] = 0x00;                
            }
            else
            {
                throw new ArgumentOutOfRangeException("data", "data byte list must return a sum >= 0");
            }

            if (BitConverter.IsLittleEndian)
            {
                checkSum = checkSum.Reverse().ToArray();
            }
            data.AddRange(checkSum);

            return data.ToArray();
        }

        /// <summary>
        /// Generates the start of the datapackage to send including header, adddress, and package identifier.
        /// </summary>
        /// <param name="packageIdentifier">
        /// The package identifier.
        /// </param>
        /// <returns>
        /// List of bytes with Header, Address, and Package Identifier at the start.
        /// </returns>
        public List<byte> GenerateDataPackageStart(byte packageIdentifier)
        {
            var dataPackage = new List<byte>();

            dataPackage.AddRange(SensorCodes.HEADER_BYTEARRAY.Reverse());
            dataPackage.AddRange(SensorCodes.CHIP_ADDRESS_BYTEARRAY.Reverse());
            dataPackage.Add(packageIdentifier);

            return dataPackage;
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
