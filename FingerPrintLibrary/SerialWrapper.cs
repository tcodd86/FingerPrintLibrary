using System;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FingerPrintLibrary
{
    public class SerialWrapper
    {
        private SerialPort Port;
        
        private TaskCompletionSource<byte[]> TCS = new TaskCompletionSource<byte[]>();

        public string Address { get; set; }

        public int BaudRate { get; set; }

        public SerialWrapper(int baudRate, string address)
        {
            BaudRate = baudRate;

            Address = address;

            SomeEvent += ReadFinished;

            if (baudRate % 9600 != 0 || baudRate < 9600 || baudRate > 115200)
            {
                throw new ArgumentOutOfRangeException("baudRate", "Baud rate must be a multiple of 9600 between 9600 and 115200");
            }

            try
            {
                Port = new SerialPort(address, baudRate, Parity.None, 8);
                Port.Open();

                Port.DataReceived += new SerialDataReceivedEventHandler(Sensor_DataReceived);
                //Minimum size of acknowledge packet is 12 bytes. Only trigger when 12 bytes have been received
                Port.ReceivedBytesThreshold = 12;
            }
            catch (Exception ex)
            {
                var error = new Exception("Error while initializing serial port.", ex);
                throw error;
            }
        }

        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        private event EventHandler<ReadFinishedEventArgs> SomeEvent;

        private void OnReadBufferFinished(ReadFinishedEventArgs e)
        {
            SomeEvent?.Invoke(this, e);
        }

        private class ReadFinishedEventArgs
        {
            public byte[] ReadBuffer { get; set; }

            public ReadFinishedEventArgs(byte[] readBuffer)
            {
                ReadBuffer = readBuffer;
            }
        }

        private void ReadFinished(object sender, ReadFinishedEventArgs e)
        {
            TCS.SetResult(e.ReadBuffer);
        }

        public async Task<byte[]> SendAndReadSerial(byte[] sendData)
        {
            //start fresh
            TCS = new TaskCompletionSource<byte[]>();

            //send data to FingerPrint sensor
            WriteByteArray(sendData);

            await TCS.Task;

            return TCS.Task.Result;
        }
        
        private void Sensor_DataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            //max buffer length is 256. Pad a little.
            int bufferSize = 300;
            var buffer = new byte[bufferSize];

            //maybe hardcode delay in here to make sure all bytes have been received
            Port.Read(buffer, 0, Port.BytesToRead);

            //Raise OnBufferFinished event
            OnReadBufferFinished(new ReadFinishedEventArgs(buffer));
        }
        
        private void WriteByteArray(byte[] write)
        {
            Port.Write(write, 0, write.Length);
        }

        private void WriteByteArray(byte[] write, int offset)
        {
            Port.Write(write, offset, write.Length - offset);
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
    }
}
