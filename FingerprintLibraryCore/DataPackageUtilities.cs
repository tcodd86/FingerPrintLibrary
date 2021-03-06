﻿using System;
using System.Collections.Generic;
using System.Linq;

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
            //control code (only for handshake)
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

        public static byte[] UploadImageToComputer()
        {
            var upImage = DataPackageStart();
            upImage.AddRange(new byte[] { 0x00, 0x03 });
            upImage.Add(SensorCodes.UP_IMAGE);
            return AddCheckSum(upImage);
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

        public static byte[] StoreTemplate(byte bufferNumber, byte[] locationNumber)
        {
            var storeTemplate = DataPackageStart();
            storeTemplate.AddRange(new byte[] { 0x00, 0x06 });
            storeTemplate.Add(SensorCodes.STORE);
            storeTemplate.Add(bufferNumber);         
            storeTemplate.AddRange(locationNumber);
            return AddCheckSum(storeTemplate);            
        }
                
        public static byte[] ReadValidTemplateNumber()
        {
            var getTemplates = DataPackageStart();
            getTemplates.AddRange(new byte[] { 0x00, 0x03 });
            getTemplates.Add(SensorCodes.TEMPLATECOUNT);
            return AddCheckSum(getTemplates);
        }

        public static byte[] ReadTemplateAtLocation(byte[] location, byte buffer = 0x01)
        {
            var readTemplate = DataPackageStart();
            readTemplate.AddRange(new byte[] { 0x00, 0x06 });
            readTemplate.Add(SensorCodes.LOADCHAR);
            readTemplate.Add(buffer);
            readTemplate.AddRange(location);
            return AddCheckSum(readTemplate);
        }

        public static byte[] MatchFingerPrint()
        {
            var preciseMatch = DataPackageStart();
            preciseMatch.AddRange(new byte[] { 0x00, 0x03 });
            preciseMatch.Add(SensorCodes.MATCH);
            return AddCheckSum(preciseMatch);
        }

        public static byte[] Search(byte bufferNumber = 0x01)
        {
            var search = DataPackageStart();
            search.AddRange(new byte[] { 0x00, 0x08 });
            search.Add(SensorCodes.SEARCH);
            search.Add(bufferNumber);
            search.AddRange(new byte[] { 0x00, 0x00 });//start page
            search.AddRange(new byte[] { 0x03, 0xE9 });//"page num" this is what the manufacturers sample program sends through.
            return AddCheckSum(search);
        }

        public static byte[] ReadSystemParameters()
        {
            var readSysParam = DataPackageStart();
            readSysParam.AddRange(new byte[] { 0x00, 0x03 });
            readSysParam.Add(SensorCodes.READ_SYS);
            return AddCheckSum(readSysParam);
        }

        public static byte[] SetAddress(byte[] newAddress)
        {
            if (newAddress.Length != 4)
            {
                throw new ArgumentException("New address must be 4 bytes.", "newAddress");
            }

            var setAddress = DataPackageStart();
            setAddress.AddRange(new byte[] { 0x00, 0x07 });
            setAddress.Add(SensorCodes.SET_ADDRESS);
            setAddress.AddRange(newAddress);
            return AddCheckSum(setAddress);
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
        #endregion

        #region Parsing
        public static int ParsePackageLength(byte[] buffer)
        {
            var subArray = buffer.Skip(7).Take(2);
            return ByteToShort(subArray);
        }

        public static Int16 ByteToShort(IEnumerable<byte> subArray)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(subArray.ToArray(), 0);
            }
            else
            {
                return BitConverter.ToInt16(subArray.Reverse().ToArray(), 0);
            }
        }

        public static byte[] ShortToByte(Int16 count)
        {
            var array = BitConverter.GetBytes(count);
            if (BitConverter.IsLittleEndian)
            {
                return array.Reverse().ToArray();
            }
            else
            {
                return array;
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
            //First 10 bytes are other info, last 2 are checksum
            ValidateMinimumLength(buffer);
            var dataLength = ParsePackageLength(buffer);
            //take dataLength - 3 because 1 is confirmation code and 2 are checksum
            return buffer.Skip(10).Take(dataLength - 3).ToArray();
        }

        public static byte[] ParseImage(byte[] buffer)
        {
            ValidateMinimumLength(buffer);
            return buffer.Skip(12).Take(4608).ToArray();
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
