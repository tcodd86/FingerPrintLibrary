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
            byte confirmationCode;
            var success = fingerprintSensor.HandShake(out confirmationCode);
            return new SensorResponse(confirmationCode);
        }

        public SensorResponse EnrollFingerPrint(short position = -1)
        {
            byte confirmationCode;

            var response = ReadFingerprintAndGenerateTemplate();

            if (response.Success)
            {
                //Store template in next available template position (if position = -1) or specified location
                if (position == -1)
                {
                    var positions = fingerprintSensor.GetUsedLibraryPositions();
                    var success = FingerPrintSensor.DetermineNextAvailablePosition(out position, positions, fingerprintSensor.templateCapacity);

                    if (!success)
                    {
                        return new SensorResponse(false, "Failed to get available templates.");
                    }

                    success = fingerprintSensor.StoreTemplate(out confirmationCode, position, 0x01);
                    if (!success)
                    {
                        return new SensorResponse("Failed to store template. Failure message: ", confirmationCode);
                    }
                }
                else
                {
                    if (position > fingerprintSensor.templateCapacity - 1)
                    {
                        throw new ArgumentOutOfRangeException($"position cannot be greater than {fingerprintSensor.templateCapacity - 1}.");
                    }
                    var success = fingerprintSensor.StoreTemplate(out confirmationCode, position, 0x01);
                    if (!success)
                    {
                        return new SensorResponse("Failed to store template. Failure message: ", confirmationCode);
                    }
                }
            }

            return response;
        }
        
        private bool ReadFingerprint(out byte confirmationCode)
        {
            string message;
            bool read = false;
            int count = 0;
            confirmationCode = SensorCodes.OK;
            Thread.Sleep(1000);

            while (read == false)
            {
                read = fingerprintSensor.ReadFingerprint(out confirmationCode);
                if (!read)
                {
                    SensorCodes.ConfirmationCodes.TryGetValue(confirmationCode, out message);
                    Thread.Sleep(100);
                }

                count++;
                if (count > 50)
                {
                    break;
                }
            }

            return read;
        }

        public SensorResponse FingerPrintSearch()
        {
            byte confirmationCode;
            var response = ReadFingerprintAndGenerateTemplate();
            
            if (response.Success)
            {
                short matchLevel;
                var success = fingerprintSensor.PreciseMatchFingerprint(out confirmationCode, out matchLevel);
                response = new SensorResponse(confirmationCode);
            }
            return response;
        }

        public SensorResponse ReadFingerprintAndGenerateTemplate()
        {
            byte confirmationCode;

            //1. Read fingerprint and store in ImageBuffer
            var success = ReadFingerprint(out confirmationCode);
            if (!success)
            {
                return new SensorResponse("Failed to read fingerprint. Failure message: ", confirmationCode);
            }

            //2. Convert into Char file (Img2Tz) and store in Char Buffer 1
            success = fingerprintSensor.GenerateCharacterFileFromImage(out confirmationCode, 0x01);
            if (!success)
            {
                return new SensorResponse("Failed to generate character file. Failure message: ", confirmationCode);
            }

            //3. Read fingerprint again and store in ImageBuffer
            success = ReadFingerprint(out confirmationCode);
            if (!success)
            {
                return new SensorResponse("Failed to read fingerprint. Failure message: ", confirmationCode);
            }

            //4. Convert into Char file and store in Char Buffer 2
            success = fingerprintSensor.GenerateCharacterFileFromImage(out confirmationCode, 0x02);
            if (!success)
            {
                return new SensorResponse("Failed to generate character file. Failure message: ", confirmationCode);
            }

            //5. Create template from Char buffer 1 & 2 which is stored in both Char buffers
            success = fingerprintSensor.GenerateTemplate(out confirmationCode);
            if (!success)
            {
                return new SensorResponse("Failed to generate template from character buffers. Failure message: ", confirmationCode);
            }

            return new SensorResponse(true);
        }
    }
}
