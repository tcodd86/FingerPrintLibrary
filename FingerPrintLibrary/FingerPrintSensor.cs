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
                Port = new SerialPort(address, baudRate, Parity.None, 8);
                Port.Open();

                Port.DataReceived += new SerialDataReceivedEventHandler(Sensor_DataReceived);
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

        public byte[] GenerateHandshakeInstruction()
        {
            var handshake = GenerateDataPackageStart(SensorAddresses.PID_COMMANDPACKET);

            var length = new byte[2] { 0x04, 0x00 };
            handshake.AddRange(length.Reverse());
            handshake.Add(SensorAddresses.INS_HANDSHAKE);
            handshake.Add(0x0);
            return AddCheckSum(handshake);
        }

        private void Sensor_DataReceived(object sender, SerialDataReceivedEventArgs args)
        {
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

            dataPackage.AddRange(SensorAddresses.HEADER_BYTEARRAY.Reverse());
            dataPackage.AddRange(SensorAddresses.CHIP_ADDRESS_BYTEARRAY.Reverse());
            dataPackage.Add(packageIdentifier);

            return dataPackage;
        }

        /// <summary>
        /// Closes the serial port if open and disposes it.
        /// </summary>
        public void Disposeserial()
        {
            if (Port.IsOpen)
            {
                Port.Close();
            }
            Port.Dispose();
        }
        
        ~FingerPrintSensor()
        {
            Disposeserial();
        }
    }
}
