using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerPrintLibrary
{
    public static class SensorAddresses
    {
        public static byte FINGERPRINT_OK = 0x00;
        public static byte FINGERPRINT_PACKETRECIEVEERR = 0x01;
        public static byte FINGERPRINT_NOFINGER = 0x02;
        public static byte FINGERPRINT_IMAGEFAIL = 0x03;
        public static byte FINGERPRINT_IMAGEMESS = 0x06;
        public static byte FINGERPRINT_FEATUREFAIL = 0x07;
        public static byte FINGERPRINT_NOMATCH = 0x08;
        public static byte FINGERPRINT_NOTFOUND = 0x09;
        public static byte FINGERPRINT_ENROLLMISMATCH = 0x0A;
        public static byte FINGERPRINT_BADLOCATION = 0x0B;
        public static byte FINGERPRINT_DBRANGEFAIL = 0x0C;
        public static byte FINGERPRINT_UPLOADFEATUREFAIL = 0x0D;
        public static byte FINGERPRINT_PACKETRESPONSEFAIL = 0x0E;
        public static byte FINGERPRINT_UPLOADFAIL = 0x0F;
        public static byte FINGERPRINT_DELETEFAIL = 0x10;
        public static byte FINGERPRINT_DBCLEARFAIL = 0x11;
        public static byte FINGERPRINT_PASSFAIL = 0x13;
        public static byte FINGERPRINT_INVALIDIMAGE = 0x15;
        public static byte INS_HANDSHAKE = 0x17;
        public static byte FINGERPRINT_FLASHERR = 0x18;
        public static byte FINGERPRINT_INVALIDREG = 0x1A;
        public static byte FINGERPRINT_ADDRCODE = 0x20;
        public static byte FINGERPRINT_PASSVERIFY = 0x21;

        public static byte PID_COMMANDPACKET = 0x1;
        public static byte PID_DATAPACKET = 0x2;
        public static byte PID_ACKPACKET = 0x7;
        public static byte PID_ENDDATAPACKET = 0x8;

        public static byte FINGERPRINT_TIMEOUT = 0xFF;
        public static byte FINGERPRINT_BADPACKET = 0xFE;

        public static byte FINGERPRINT_GETIMAGE = 0x01;
        public static byte FINGERPRINT_IMAGE2TZ = 0x02;
        public static byte FINGERPRINT_REGMODEL = 0x05;
        public static byte FINGERPRINT_STORE = 0x06;
        public static byte FINGERPRINT_LOAD = 0x07;
        public static byte FINGERPRINT_UPLOAD = 0x08;
        public static byte FINGERPRINT_DELETE = 0x0C;
        public static byte FINGERPRINT_EMPTY = 0x0D;
        public static byte FINGERPRINT_VERIFYPASSWORD = 0x13;
        public static byte FINGERPRINT_HISPEEDSEARCH = 0x1B;
        public static byte FINGERPRINT_TEMPLATECOUNT = 0x1D;

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
    }
}
