using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public class DataPackageUtilities
    {
        #region CreateInstructions
        public static byte[] Handshake()
        {
            var handshake = DataPackageStart(SensorCodes.PID_COMMANDPACKET);
            
            handshake.AddRange(new byte[2] { 0x00, 0x04 });
            handshake.Add(SensorCodes.HANDSHAKE);
            //control code (only for handshake?)
            handshake.Add(0x0);
            return AddCheckSum(handshake);
        }

        public static byte[] GenerateImage()
        {
            var genImg = DataPackageStart(SensorCodes.PID_COMMANDPACKET);
            genImg.AddRange(new byte[] { 0x00, 0x03 });
            genImg.Add(SensorCodes.GETIMAGE);
            return AddCheckSum(genImg);
        }

        public static byte[] GenerateCharFileFromImgDataPackage(byte buffer)
        {
            var genChar = DataPackageStart();
            genChar.AddRange(new byte[] { 0x00, 0x04 });
            genChar.Add(SensorCodes.IMAGE2TZ);
            genChar.Add(buffer);
            return AddCheckSum(genChar);
        }

        public static byte[] GenerateTemplate()
        {
            var genTemplate = DataPackageStart();
            genTemplate.AddRange(new byte[] { 0x00, 0x03 });
            genTemplate.Add(SensorCodes.REGMODEL);
            return AddCheckSum(genTemplate);
        }

        public static byte[] StoreTemplate(byte bufferNumber, Int16 locationNumber)
        {
            var storeTemplate = DataPackageStart();
            storeTemplate.AddRange(new byte[] { 0x00, 0x06 });
            storeTemplate.Add(SensorCodes.STORE);
            storeTemplate.Add(bufferNumber);
            var loc = BitConverter.GetBytes(locationNumber);
            if (!BitConverter.IsLittleEndian)
            {
                loc = loc.Reverse().ToArray();
            }
            storeTemplate.AddRange(loc);
            return AddCheckSum(storeTemplate);            
        }
        #endregion
                
        /// <summary>
        /// Returns a byte array of data with a 2 byte checksum appended.
        /// </summary>
        /// <param name="data">
        /// byte list that needs a checksum
        /// </param>
        /// <returns>
        /// byte array of "data" with 2 byte checksum appended.
        /// </returns>
        public static byte[] AddCheckSum(List<byte> data)
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
        /// The package identifier. Default is command package.
        /// </param>
        /// <returns>
        /// List of bytes with Header, Address, and Package Identifier at the start.
        /// </returns>
        public static List<byte> DataPackageStart(byte packageIdentifier = 0x01)
        {
            var dataPackage = new List<byte>();

            dataPackage.AddRange(SensorCodes.HEADER_BYTEARRAY.Reverse());
            dataPackage.AddRange(SensorCodes.CHIP_ADDRESS_BYTEARRAY.Reverse());
            dataPackage.Add(packageIdentifier);

            return dataPackage;
        }

        #region Parsing
        public static int ParsePackageLength(byte[] buffer)
        {
            var subArray = buffer.Skip(7).Take(2);
            if (!BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(subArray.ToArray(), 0);
            }
            else
            {
                return BitConverter.ToInt16(subArray.Reverse().ToArray(), 0);
            }
        }

        public static bool ParseSuccess(byte[] buffer)
        {
            ValidateMinimumLength(buffer);

            if (buffer[9] > 0x00)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static byte ParsePackageConfirmationCode(byte[] buffer)
        {
            //Tenth byte is confirmation code
            ValidateMinimumLength(buffer);
            return buffer[9];
        }

        public static byte ParsePackageIdentifier(byte[] buffer)
        {
            //Seventh byte is PackageIdentifier
            ValidateMinimumLength(buffer);
            return buffer[6];
        }

        public static byte[] ParsePackageContents(byte[] buffer)
        {
            //First 9 bytes are other info, last 2 are checksum
            ValidateMinimumLength(buffer);
            return buffer.Skip(9).Take(buffer.Count() - 11).ToArray();
        }
        #endregion

        #region Validation
        public static bool ValidateCheckSum(byte[] buffer)
        {
            ValidateMinimumLength(buffer);
            var fullSum = BitConverter.GetBytes(buffer.Skip(6).Take(buffer.Length - 8).Sum(p => p));
            var subArray = new byte[2];

            if (BitConverter.IsLittleEndian)
            {
                subArray[0] = fullSum[1];
                subArray[1] = fullSum[0];
            }
            else
            {
                subArray[1] = fullSum[fullSum.Length - 1];
                subArray[0] = fullSum[fullSum.Length - 2];
            }

            if (subArray[0] == buffer[buffer.Length - 2] && subArray[1] == buffer[buffer.Length - 1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ValidateMinimumLength(byte[] buffer)
        {
            if (buffer.Length < 12)
            {
                throw new IndexOutOfRangeException("buffer must have a length of at least 12.");
            }
        }
        #endregion
    }
}
