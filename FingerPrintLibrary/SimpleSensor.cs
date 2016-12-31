using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public class SimpleSensor
    {
        public FingerPrintSensor fingerprintSensor { get; private set; }

        #region Constructors
        public SimpleSensor(string address)
        {
            fingerprintSensor = new FingerPrintSensor(address);
        }

        public SimpleSensor(string address, char ABC)
        {
            fingerprintSensor = new FingerPrintSensor(address, ABC);
        }

        public SimpleSensor(string address, int baudRate)
        {
            fingerprintSensor = new FingerPrintSensor(address, baudRate);
        }
        #endregion

        public SensorResponse HandShake()
        {
            return fingerprintSensor.HandShake();
        }

        public SensorResponse EnrollFingerPrint(short position = -1)
        {
            var positions = fingerprintSensor.GetUsedLibraryPositions();
            var response = ReadFingerprintAndGenerateTemplate();

            if (response.Success)
            {
                //Store template in next available template position (if position = -1) or specified location
                if (position == -1)
                {
                    var success = FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, fingerprintSensor.templateCapacity);

                    if (!success)
                    {
                        return new SensorResponse(false, "Failed to get available templates.");
                    }

                    response = fingerprintSensor.StoreTemplate(position, 0x01);
                }
                else
                {
                    if (position > fingerprintSensor.templateCapacity - 1)
                    {
                        throw new ArgumentOutOfRangeException($"position cannot be greater than {fingerprintSensor.templateCapacity - 1}.");
                    }
                    response = fingerprintSensor.StoreTemplate(position, 0x01);
                }
            }

            return response;
        }
        
        private SensorResponse ReadFingerprint(int numberOfTries = 200)
        {
            bool read = false;
            int count = 0;
            Thread.Sleep(1000);
            var response = new SensorResponse(false);

            while (read == false)
            {
                response = fingerprintSensor.ReadFingerprint();
                read = response.Success;
                if (!read)
                {
                    Thread.Sleep(50);
                }
                count++;
                if (count > numberOfTries)
                {
                    break;
                }
            }

            return response;
        }

        public SearchResponse FingerPrintSearch()
        {
            byte confirmationCode;
            var response = ReadFingerprintGenerateCharacter();
            
            if (response.Success)
            {
                short matchLevel;
                short pageNumber;
                var success = fingerprintSensor.Search(out pageNumber, out confirmationCode, out matchLevel);
                return new SearchResponse(confirmationCode, pageNumber, matchLevel);
            }
            else
            {
                return new SearchResponse(response);
            }            
        }

        public SensorResponse ReadFingerprintAndGenerateTemplate()
        {
            var response = ReadFingerprintGenerateCharacter(0x01);
            if (!response.Success)
            {
                return response;
            }

            response = ReadFingerprintGenerateCharacter(0x02);
            if (!response.Success)
            {
                return response;
            }

            //Create template from Char buffer 1 & 2 which is stored in both Char buffers
            return fingerprintSensor.GenerateTemplate();
        }

        public SensorResponse ReadFingerprintGenerateCharacter(byte buffer = 0x01)
        {
            //1. Read fingerprint and store in ImageBuffer
            var success = ReadFingerprint();
            if (!success.Success)
            {
                return new SensorResponse("Failed to read fingerprint. Failure message: ", success.ResponseCode);
            }

            //2. Convert into Char file (Img2Tz) and store in Char Buffer
            return fingerprintSensor.GenerateCharacterFileFromImage(buffer);            
        }
    }
}
