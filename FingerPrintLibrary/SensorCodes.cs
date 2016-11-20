using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public static class SensorCodes
    {
        #region Confirmation Codes
        public static byte OK = 0x00;
        public static byte PACKETRECIEVEERR = 0x01;
        public static byte NOFINGER = 0x02;
        public static byte IMAGEFAIL = 0x03;
        public static byte IMAGEMESS = 0x06;
        public static byte FEATUREFAIL = 0x07;
        public static byte NOMATCH = 0x08;
        public static byte NOTFOUND = 0x09;
        public static byte ENROLLMISMATCH = 0x0A;
        public static byte BADLOCATION = 0x0B;
        public static byte DBRANGEFAIL = 0x0C;
        public static byte UPLOADFEATUREFAIL = 0x0D;
        public static byte PACKETRESPONSEFAIL = 0x0E;
        public static byte UPLOADFAIL = 0x0F;
        public static byte DELETEFAIL = 0x10;
        public static byte DBCLEARFAIL = 0x11;        
        public static byte INVALIDIMAGE = 0x15;
        public static byte FLASHERR = 0x18;
        public static byte NODEFERR = 0x19;
        public static byte INVALIDREG = 0x1A;
        public static byte INCORRECTREGCONFIG = 0x1B;
        public static byte WRONGNOTEPADPAGE = 0x1C;
        public static byte COMMPORTFAILED = 0x1D;
        #endregion
        //public static byte PASSFAIL = 0x13;
        //public static byte ADDRCODE = 0x20;
        //public static byte PASSVERIFY = 0x21;


        #region Data Package Types
        public static byte PID_COMMANDPACKET = 0x1;
        public static byte PID_DATAPACKET = 0x2;
        public static byte PID_ACKPACKET = 0x7;
        public static byte PID_ENDDATAPACKET = 0x8;
        #endregion

        #region Error Responses
        public static byte TIMEOUT = 0xFF;
        public static byte BADPACKET = 0xFE;
        #endregion

        #region Instruction Codes
        public static byte GETIMAGE = 0x01;
        public static byte IMAGE2TZ = 0x02;
        public static byte MATCH = 0x03;
        public static byte SEARCH = 0x04;
        public static byte REGMODEL = 0x05;
        public static byte STORE = 0x06;
        public static byte LOADCHAR = 0x07;
        public static byte UPLOAD_TEMPLATE = 0x08;
        public static byte DOWN_TEMPLATE = 0x09;
        public static byte UP_IMAGE = 0x0A;
        public static byte DOWN_IMAGE = 0x0B;
        public static byte DELETE = 0x0C;
        public static byte EMPTY = 0x0D;
        public static byte SET_SYS = 0x0E;
        public static byte READ_SYS = 0x0F;
        //public static byte VERIFYPASSWORD = 0x13;
        public static byte GET_RANDOM_CODE = 0x014;
        public static byte SET_ADDRESS = 0x15;
        public static byte HANDSHAKE = 0x17;
        public static byte WRITE_NOTEPAD = 0x18;
        public static byte READ_NOTEPAD = 0x19;
        //public static byte HISPEEDSEARCH = 0x1B;
        public static byte TEMPLATECOUNT = 0x1D;
        #endregion

        #region Header and Address
        public static int HEADER = 0xEF01;
        public static byte[] HEADER_BYTEARRAY
        {
            get
            {
                return new byte[2] { 0x01, 0xEF };
            }
        }

        public static uint CHIP_ADDRESS = 0xFFFFFFFF;
        public static byte[] CHIP_ADDRESS_BYTEARRAY
        {
            get
            {
                return BitConverter.GetBytes(CHIP_ADDRESS);
            }
        }
        #endregion

        #region Confirmation Codes Definitions
        public static Dictionary<byte, string> ConfirmationCodes = new Dictionary<byte, string>()
        {
            {0x00, "Command execution complete." }
            , {0x01, "Error when receiving data package." }
            , {0x02, "No finger on the sensor." }
            , {0x03, "Failed to enroll finger." }
            , {0x06, "Failed to generate character file due to the over-disorderly fingerprint image." }
            , {0x07, "Failed to generate character file due to lackness of character point or over-smallness of fingerprint image." }
            , {0x08, "Finger doesn't match" }
            , {0x09, "Fail to find the matching finger." }
            , {0x0A, "Failed to combine the character files." }
            , {0x0B, "Addressing PageID is beyond the finger library." }
            , {0x0C, "Error when reading template from library or the template is invalid." }
            , {0x0D, "Error when uploading template." }
            , {0x0E, "Module can't receive the following data packages." }
            , {0x0F, "Error when uploading image." }
            , {0x10, "Failed to delete the template." }
            , {0x11, "Failed to clear the finger library." }
            , {0x15, "Failed to generate the image for the lackness of primary image." }
            , {0x18, "Error when writing flash." }
            , {0x19, "No definition error." }
            , {0x1A, "Invalid register number." }
            , {0x1B, "Incorrect configuration of register." }
            , {0x1C, "Wrong notepad page number." }
            , {0x1D, "Failed to operate the communication port." }
        };
        #endregion
    }
}
