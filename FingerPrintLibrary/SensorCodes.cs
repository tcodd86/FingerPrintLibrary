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

        #region 
        public static byte GETIMAGE = 0x01;
        public static byte IMAGE2TZ = 0x02;
        public static byte REGMODEL = 0x05;
        public static byte STORE = 0x06;
        public static byte LOAD = 0x07;
        public static byte UPLOAD = 0x08;
        public static byte DELETE = 0x0C;
        public static byte EMPTY = 0x0D;
        public static byte VERIFYPASSWORD = 0x13;
        public static byte HANDSHAKE = 0x17;
        public static byte HISPEEDSEARCH = 0x1B;
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
    }
}
