using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCallsign;
class ComputeHashCall
{
    public int ComputeHash(string callsign) //return hashed c/s as int
    {
        // Convert the callsign to a byte array
        byte[] data = System.Text.Encoding.ASCII.GetBytes(callsign);

        // Initialize hash variables
        uint a, b, c;
        a = b = 0xdeadbeef + (uint)data.Length; // Arbitrary seed
        c = a;

        // Hashing logic
        int i = 0;
        while (i + 12 <= data.Length)
        {
            a += BitConverter.ToUInt32(data, i);
            b += BitConverter.ToUInt32(data, i + 4);
            c += BitConverter.ToUInt32(data, i + 8);

            a -= c; a ^= (c << 4) | (c >> (32 - 4)); c += b;
            b -= a; b ^= (a << 6) | (a >> (32 - 6)); a += c;
            c -= b; c ^= (b << 8) | (b >> (32 - 8)); b += a;
            a -= c; a ^= (c << 16) | (c >> (32 - 16)); c += b;
            b -= a; b ^= (a << 19) | (a >> (32 - 19)); a += c;
            c -= b; c ^= (b << 4) | (b >> (32 - 4)); b += a;

            i += 12;
        }

        c += (uint)data.Length;

        if (i < data.Length)
        {
            int remaining = data.Length - i;
            switch (remaining)
            {
                case 11: c += (uint)data[i + 10] << 24; goto case 10;
                case 10: c += (uint)data[i + 9] << 16; goto case 9;
                case 9: c += (uint)data[i + 8] << 8; goto case 8;
                case 8: b += (uint)data[i + 7] << 24; goto case 7;
                case 7: b += (uint)data[i + 6] << 16; goto case 6;
                case 6: b += (uint)data[i + 5] << 8; goto case 5;
                case 5: b += (uint)data[i + 4]; goto case 4;
                case 4: a += (uint)data[i + 3] << 24; goto case 3;
                case 3: a += (uint)data[i + 2] << 16; goto case 2;
                case 2: a += (uint)data[i + 1] << 8; goto case 1;
                case 1: a += (uint)data[i]; break;
            }
        }

        // Final mixing of hash values
        a -= c; a ^= (c << 4) | (c >> (32 - 4)); c += b;
        b -= a; b ^= (a << 6) | (a >> (32 - 6)); a += c;
        c -= b; c ^= (b << 8) | (b >> (32 - 8)); b += a;
        a -= c; a ^= (c << 16) | (c >> (32 - 16)); c += b;
        b -= a; b ^= (a << 19) | (a >> (32 - 19)); a += c;
        c -= b; c ^= (b << 4) | (b >> (32 - 4)); b += a;

        return (int)(c & 0x7FFF); // Mask to 15 bits
    }
   
}

