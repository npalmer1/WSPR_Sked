using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace JenkinsHash
{
    public class Hash
    {

        static uint Rot(uint x, int k)
        {
            return (x << k) | (x >> (32 - k)); // Rotate bits
        }
        static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= c; a ^= Rot(c, 4); c += b;
            b -= a; b ^= Rot(a, 6); a += c;
            c -= b; c ^= Rot(b, 8); b += a;
            a -= c; a ^= Rot(c, 16); c += b;
            b -= a; b ^= Rot(a, 19); a += c;
            c -= b; c ^= Rot(b, 4); b += a;
        }

        static void Final(ref uint a, ref uint b, ref uint c)
        {
            c ^= b; c -= Rot(b, 14);
            a ^= c; a -= Rot(c, 11);
            b ^= a; b -= Rot(a, 25);
            c ^= b; c -= Rot(b, 16);
            a ^= c; a -= Rot(c, 4);
            b ^= a; b -= Rot(a, 14);
            c ^= b; c -= Rot(b, 24);
        }

        //static unsafe void
        public unsafe uint nhash(byte[] key, int length0, uint initval0)
        {
            const int HASH_LITTLE_ENDIAN = 1;
            bool isLittleEndian = BitConverter.IsLittleEndian; // Check system endianness
            //uint32_t a,b,c; 
            uint a, b, c;  /* internal state */
            //size_t length;
            int length;
            //uint32_t initval;
            uint initval;
            //union { const void *ptr; size_t i; } u;     /* needed for Mac Powerbook G4 */

            //length=*length0;
            //initval=*initval0;
            length = length0;
            initval = initval0;

            /* Set up the internal state */
            a = b = c = (uint)(0xdeadbeef + (uint)length + initval);

            /*u.ptr = key;
            if (HASH_LITTLE_ENDIAN && ((u.i & 0x3) == 0)) {
              const uint32_t *k = (const uint32_t *)key;         /* read 32-bit chunks */
            //const uint8_t* k8; */

           
            if (isLittleEndian && (key[key.Length] & 0x3) == 0)
            {
                uint* k = null; // Pointer for reading 32-bit chunks
                byte* k8 = null; // Pointer for reading 8-bit chunks
                fixed (byte* ptr = key) // Pin the key array in memory //this is the bit of unsafe code
                {


                    // Check if the platform is little-endian and the pointer is 4-byte aligned
                    if (BitConverter.IsLittleEndian && ((long)ptr & 0x3) == 0)
                    {
                        k = (uint*)ptr; // Cast to 32-bit pointer                        

                    }
                    else
                    {
                        k8 = ptr; // Use 8-bit pointer

                    }
                }

                //k8 = 0;   
                //Silence compiler warning
                /*------ all but last block: aligned reads and affect 32 bits of (a,b,c) */
                while (length > 12)
                {
                    a += k[0];
                    b += k[1];
                    c += k[2];
                    Mix(ref a, ref b, ref c);
                    length -= 12;
                    k += 3;
                }



                /*----------------------------- handle the last (probably partial) block */
                /* 
                 * "k[2]&0xffffff" actually reads beyond the end of the string, but
                 * then masks off the part it's not allowed to read.  Because the
                 * string is aligned, the masked-off tail is in the same word as the
                 * rest of the string.  Every machine with memory protection I've seen
                 * does it on word boundaries, so is OK with this.  But VALGRIND will
                 * still catch it and complain.  The masking trick does make the hash
                 * noticably faster for short strings (like English words).
                 */
#if !VALGRIND
                switch (length)
                {
                    case 12: c += k[2]; b += k[1]; a += k[0]; break;
                    case 11: c += k[2] & 0xffffff; b += k[1]; a += k[0]; break;
                    case 10: c += k[2] & 0xffff; b += k[1]; a += k[0]; break;
                    case 9: c += k[2] & 0xff; b += k[1]; a += k[0]; break;
                    case 8: b += k[1]; a += k[0]; break;
                    case 7: b += k[1] & 0xffffff; a += k[0]; break;
                    case 6: b += k[1] & 0xffff; a += k[0]; break;
                    case 5: b += k[1] & 0xff; a += k[0]; break;
                    case 4: a += k[0]; break;
                    case 3: a += k[0] & 0xffffff; break;
                    case 2: a += k[0] & 0xffff; break;
                    case 1: a += k[0] & 0xff; break;
                    case 0: return c; // Zero-length case
                }


#else // make valgrind happy 
        k8 = (const uint8_t *)k;
        switch (length)
        {
            case 12: c += k[2]; b += k[1]; a += k[0]; break;
            case 11: c += ((uint)k8[10]) << 16; goto case 10;
            case 10: c += ((uint)k8[9]) << 8; goto case 9;
            case 9:  c += k8[8]; goto case 8;
            case 8:  b += k[1]; a += k[0]; break;
            case 7:  b += ((uint)k8[6]) << 16; goto case 6;
            case 6:  b += ((uint)k8[5]) << 8; goto case 5;
            case 5:  b += k8[4]; goto case 4;
            case 4:  a += k[0]; break;
            case 3:  a += ((uint)k8[2]) << 16; goto case 2;
            case 2:  a += ((uint)k8[1]) << 8; goto case 1;
            case 1:  a += k8[0]; break;
            case 0:  Console.WriteLine($"Result: {c}"); return;
        }
    

#endif //!valgrind 

                //else if (HASH_LITTLE_ENDIAN && ((u.i & 0x1) == 0)) 

            }
            else if (isLittleEndian && (key[key.Length] & 0x1) == 0)
            {
                //const uint16_t *k = (const uint16_t *)key;         /* read 16-bit chunks */
                //const uint8_t  *k8;

                ushort[] k = Enumerable.Range(0, key.Length / 2)
                                          .Select(i => BitConverter.ToUInt16(key, i * 2))
                                          .ToArray();

                // Read 8-bit chunks (equivalent to `uint8_t *k8`)
                byte[] k8 = key; // Directly use the byte array

                /*--------------- all but last block: aligned reads and different mixing */

                int index = 0;
                
                while (length > 12)
                {
                    a += (uint)(k[index] + (k[index + 1] << 16));
                    b += (uint)(k[index + 2] + (k[index + 3] << 16));
                    c += (uint)(k[index + 4] + (k[index + 5] << 16));

                    Mix(ref a, ref b, ref c); // Call the Mix method
                    length -= 12;
                    index += 6; // Move to the next chunk
                }

                /*----------------------------- handle the last (probably partial) block */
                //k8 = (const uint8_t *)k;
                k8 = k.SelectMany(BitConverter.GetBytes).ToArray();
                switch (length)
                {
                    case 12:
                        c += (uint)(k[4] + (k[5] << 16));
                        b += (uint)(k[2] + (k[3] << 16));
                        a += (uint)(k[0] + (k[1] << 16));
                        break;
                    case 11:
                        c += (uint)(k8[10] << 16);
                        goto case 10;
                    case 10:
                        c += k[4];
                        b += (uint)(k[2] + (k[3] << 16));
                        a += (uint)(k[0] + (k[1] << 16));
                        break;
                    case 9:
                        c += k8[8];
                        goto case 8;
                    case 8:
                        b += (uint)(k[2] + (k[3] << 16));
                        a += (uint)(k[0] + (k[1] << 16));
                        break;
                    case 7:
                        b += (uint)(k8[6] << 16);
                        goto case 6;
                    case 6:
                        b += k[2];
                        a += (uint)(k[0] + (k[1] << 16));
                        break;
                    case 5:
                        b += k8[4];
                        goto case 4;
                    case 4:
                        a += (uint)(k[0] + (k[1] << 16));
                        break;
                    case 3:
                        a += (uint)(k8[2] << 16);
                        goto case 2;
                    case 2:
                        a += k[0];
                        break;
                    case 1:
                        a += k8[0];
                        break;
                    case 0: return c;

                }
            }
            else
            {                        /* need to read the key one byte at a time */
                //const uint8_t *k = (const uint8_t *)key;
                //uint *k = 
                byte[] k = key;
                /*--------------- all but the last block: affect some 32 bits of (a,b,c) */
                int index = 0;
                
                while (length > 12)
                {
                    a += key[index];
                    a += (uint)(key[index + 1] << 8);
                    a += (uint)(key[index + 2] << 16);
                    a += (uint)(key[index + 3] << 24);

                    b += key[index + 4];
                    b += (uint)(key[index + 5] << 8);
                    b += (uint)(key[index + 6] << 16);
                    b += (uint)(key[index + 7] << 24);

                    c += key[index + 8];
                    c += (uint)(key[index + 9] << 8);
                    c += (uint)(key[index + 10] << 16);
                    c += (uint)(key[index + 11] << 24);

                    Mix(ref a, ref b, ref c); // Call the Mix method
                    length -= 12;
                    index += 12; // Move to the next chunk
                }

                /*-------------------------------- last block: affect all 32 bits of (c) */
                switch (length)
                {
                    case 12:
                        c += (uint)(k[11] << 24); // Fall through
                        goto case 11;
                    case 11:
                        c += (uint)(k[10] << 16); // Fall through
                        goto case 10;
                    case 10:
                        c += (uint)(k[9] << 8); // Fall through
                        goto case 9;
                    case 9:
                        c += k[8]; // Fall through
                        goto case 8;
                    case 8:
                        b += (uint)(k[7] << 24); // Fall through
                        goto case 7;
                    case 7:
                        b += (uint)(k[6] << 16); // Fall through
                        goto case 6;
                    case 6:
                        b += (uint)(k[5] << 8); // Fall through
                        goto case 5;
                    case 5:
                        b += k[4]; // Fall through
                        goto case 4;
                    case 4:
                        a += (uint)(k[3] << 24); // Fall through
                        goto case 3;
                    case 3:
                        a += (uint)(k[2] << 16); // Fall through
                        goto case 2;
                    case 2:
                        a += (uint)(k[1] << 8); // Fall through
                        goto case 1;
                    case 1:
                        a += k[0];
                        break;
                    case 0:

                        return c;
                }
            }

Final(ref a, ref b, ref c);
return c;
}
    }
}
