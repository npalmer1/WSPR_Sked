using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Type2
{
    public class Type2Encode
    {        
        public byte[] wsprMessage;

        public Type2Encode(string callsign, string locator, int powerLevel)
        {
           

            locator = locator.PadRight(6);
            // Generate WSPR Type 2 message
            wsprMessage = EncodeType2Message(callsign, locator, powerLevel);
          

        }

        



        public static byte[] EncodeType2Message(string callsign, string locator, int powerLevel)
        {
            // Validate inputs
            if (callsign.Length > 11 || locator.Length != 6 || powerLevel < 0 || powerLevel > 60)
            {
                throw new ArgumentException("Invalid inputs for WSPR Type 2 message.");
            }
            //callsign = callsign.PadRight(11);
            locator = locator.PadRight(6);




            byte[] data = System.Text.Encoding.ASCII.GetBytes(callsign);

            // Initialize hash with some value, typically zero
            uint initval = 0;

            // Compute the hash
            uint hash = Hash(data, initval);

            // Constrain to 15 bits for WSPR
            uint wsprHash = hash & 0x7FFF;




            int locatorBits = ConvertLocatorToBits(locator);



            // Encode power level (7 bits)
            int powerBits = powerLevel & 0x7F;

            // Combine into 50-bit payload
            //long payload = ((long)callsignHash << 35) | ((long)locatorBits << 7) | powerBits;
            long payload = ((long)wsprHash << 35) | ((long)locatorBits << 7) | powerBits;


            // Convert to byte array
            byte[] wsprMessage = new byte[7];
            for (int i = 0; i < 7; i++)
            {
                wsprMessage[i] = (byte)((payload >> (6 - i) * 8) & 0xFF);
            }
            string hexstring = BitConverter.ToString(wsprMessage).Replace("-", "");
            //MessageBox.Show(hexstring);

            return wsprMessage;
        }

        public static int ConvertLocatorToBits(string locator)
        {
            // Convert 6-character Maidenhead locator to 18-bit binary representation
            int field1 = locator[0] - 'A';
            int field2 = locator[1] - 'A';
            int square1 = locator[2] - '0';
            int square2 = locator[3] - '0';
            int sub1 = locator[4] - 'A';
            int sub2 = locator[5] - 'A';

            return (field1 << 14) | (field2 << 10) | (square1 << 6) | (square2 << 2) | (sub1 << 1) | sub2;
        }


        public static uint Hash(byte[] data, uint initval)
        {
            uint length = (uint)data.Length;
            uint a, b, c;
            a = b = c = 0xdeadbeef + length + initval; // Initialization

            int index = 0;

            // Process data in chunks of 12 bytes
            while (length > 12)
            {
                a += BitConverter.ToUInt32(data, index);
                b += BitConverter.ToUInt32(data, index + 4);
                c += BitConverter.ToUInt32(data, index + 8);

                Mix(ref a, ref b, ref c);

                length -= 12;
                index += 12;
            }

            // Process remaining bytes
            c += length; // Add the length of the remaining bytes
            switch (length)
            {
                case 12: c += (uint)data[index + 11] << 24; goto case 11;
                case 11: c += (uint)data[index + 10] << 16; goto case 10;
                case 10: c += (uint)data[index + 9] << 8; goto case 9;
                case 9: c += (uint)data[index + 8]; goto case 8;
                case 8: b += (uint)data[index + 7] << 24; goto case 7;
                case 7: b += (uint)data[index + 6] << 16; goto case 6;
                case 6: b += (uint)data[index + 5] << 8; goto case 5;
                case 5: b += (uint)data[index + 4]; goto case 4;
                case 4: a += (uint)data[index + 3] << 24; goto case 3;
                case 3: a += (uint)data[index + 2] << 16; goto case 2;
                case 2: a += (uint)data[index + 1] << 8; goto case 1;
                case 1: a += (uint)data[index]; break;
            }

            // Final mixing
            Mix(ref a, ref b, ref c);

            return c;
        }

        private static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= c; a ^= Rotate(c, 4); c += b;
            b -= a; b ^= Rotate(a, 6); a += c;
            c -= b; c ^= Rotate(b, 8); b += a;
            a -= c; a ^= Rotate(c, 16); c += b;
            b -= a; b ^= Rotate(a, 19); a += c;
            c -= b; c ^= Rotate(b, 4); b += a;
        }

        private static uint Rotate(uint value, int shift)
        {
            return (value << shift) | (value >> (32 - shift));
        }


    }


}

