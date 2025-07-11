using System;
using System.Linq;
using Org.BouncyCastle.Utilities;

namespace EncodeV2a
{
    public static class Encode2a
    {
        /// <summary>
        /// Number of bits in a WSPR transmission
        /// </summary>
        public const byte WSPR_BIT_COUNT = 162;

        /// <summary>
        /// Total number of bytes in a WSPR transmission
        /// </summary>
        public const byte WSPR_MESSAGE_SIZE = 11;

        /// <summary>
        /// Convert station information into a 7-byte array 
        /// using JTEncode's WSPR bit packing algorythm
        /// </summary>
        /// <param name="callsign">callsign of any length (will get padded automatically)</param>
        /// <param name="location">4-character location (grid square identifier)</param>
        /// <param name="power">power level in dB (will get rounded automatically)</param>
        /// <returns></returns>
        /// 
        private static string padCallsign(string call)
        {
            // If only the 2nd character is a digit, then pad with a space.
            // If this happens, then the callsign will be truncated if it is
            // longer than 6 characters.

            string c = " ";
            if (call.Length > 2)
            {
                if (char.IsDigit(call[1]) && char.IsUpper(call[2]))
                {
                    c = c + call;
                   
                }
                if (c.Length > 6)
                {
                    c = c.Substring(0, 6);
                }
                else
                {
                    c = c.PadRight(6);
                }
            }
            return c;
        }

        private static string getBaseCall(string callsign, int slash_pos)
        {
            string baseCall = "";
            string baseC = "";
            
            baseCall = callsign.Substring(0, slash_pos);
            int len = baseCall.Length;

            //check this - is  it the same for single and double char suffixes?
            for (int i = 0; i < len; i++)
            {
                // Convert to uppercase
                baseC = baseC + char.ToUpper(baseCall[i]);

                // Check conditions and replace invalid characters
                if (!(char.IsDigit(baseCall[i]) || char.IsUpper(baseCall[i])))
                {
                    baseC = baseC + ' ';
                }
            }            
            return baseC;
        }

public static byte[] GetMessageBytes(string callsign, string location, double power)
        {
            uint intA;
            uint intB;
            string baseCall;
            
            //for type 2 messages with 
            byte[] bytes = new byte[7];
            // sanitize inputs and perform error checking
            try
            {
                //callsign = SanitizeCallsign(callsign);
                location = SanitizeLocation(location);
                byte powerByte = SanitizePower(power);
                

                int slash_pos = callsign.IndexOf('/');
                if (slash_pos !=-1)
                {
                    callsign = callsign.PadRight(10);
                }
                if (slash_pos == -1)
                {
                    callsign = SanitizeCallsign(callsign);
                    //type 1 message with base callsign
                    char[] callChars = callsign.ToCharArray();

                    intA = WsprCode(callChars[0]);
                    intA = intA * 36 + WsprCode(callChars[1]);
                    intA = intA * 10 + WsprCode(callChars[2]);
                    intA = (uint)(intA * 27 + (WsprCode(callChars[3]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[4]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[5]) - 10));

                    // pack location and power into a 32-bit integer
                    char[] locChars = location.ToCharArray();

                    intB = (uint)(
                        180 * (179 - 10 * (locChars[0] - 'A') - (locChars[2] - '0')) +
                        10 * (locChars[1] - 'A') +
                        1 * (locChars[3] - '0')
                    );
                    intB = (intB * 128) + powerByte + 64;
                }
                else if (callsign[slash_pos + 2] == ' ')
                {
                    //callsign = SanitizeCallsign2(callsign);
                    
                    //Type 2 message with single character suffix
                    baseCall = getBaseCall(callsign, slash_pos);

                    //baseCall = padCallsign(baseCall);
                    baseCall = SanitizeCallsign(baseCall);

                    // pack callsign data into a 32-bit integer
                    char[] callChars = baseCall.ToCharArray();
                    //uint intA;
                    intA = WsprCode(callChars[0]);
                    intA = intA * 36 + WsprCode(callChars[1]);
                    intA = intA * 10 + WsprCode(callChars[2]);
                    intA = (uint)(intA * 27 + (WsprCode(callChars[3]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[4]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[5]) - 10));

                    char X = callsign[slash_pos + 1];
                    int x = (int)X;
                    if (X >= (48) && x <= 57)
                    {
                        x -= 48;
                    }
                    else if (x >= 65 && x <= 90)
                    {
                        x -= 55;
                    }
                    else
                    {
                        x = 38;
                    }
                    //X = (char)x;


                    intB = (uint)(60000 - 32768 + x);

                    intB = (uint)((intB * 128) + powerByte + 2 + 64);

                }
                else if (callsign[slash_pos + 3] == ' ' || slash_pos == callsign.Length)
                {
                    //callsign = SanitizeCallsign2(callsign);
                    
                    //Type 2 message with two-digit numerical suffix
                    baseCall = getBaseCall(callsign, slash_pos);

                    //baseCall = padCallsign(baseCall);
                    baseCall = SanitizeCallsign(baseCall);

                    // pack callsign data into a 32-bit integer
                    char[] callChars = baseCall.ToCharArray();

                    intA = WsprCode(callChars[0]);
                    intA = intA * 36 + WsprCode(callChars[1]);
                    intA = intA * 10 + WsprCode(callChars[2]);
                    intA = (uint)(intA * 27 + (WsprCode(callChars[3]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[4]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[5]) - 10));


                    intB = (uint)(10 * (callsign[slash_pos + 1] - 48) + callsign[slash_pos + 2] - 48);
                    intB = (uint)(60000 + 26 + intB);
                    intB = (intB * 128) + powerByte + 2 + 64;
                }
                else
                {
                    //callsign = SanitizeCallsign2(callsign);
                    
                    //Type 2 message with prefix rather than suffix
                    string prefix;
                    baseCall = callsign.Substring((slash_pos + 1), callsign.Length - (slash_pos + 1));
                    prefix = callsign.Substring(0, slash_pos);
                    string px = " ";
                    if (prefix[2] == ' ' || prefix.Length == 3)
                    {
                        // Right align prefix
                        px = " ";
                        px = px + prefix[1];
                        px = px + prefix[2];
                        //px = px + prefix[3];
                    }
                    baseCall = padCallsign(baseCall);
                    char[] callChars = baseCall.ToCharArray();

                    intA = WsprCode(callChars[0]);
                    intA = intA * 36 + WsprCode(callChars[1]);
                    intA = intA * 10 + WsprCode(callChars[2]);
                    intA = (uint)(intA * 27 + (WsprCode(callChars[3]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[4]) - 10));
                    intA = (uint)(intA * 27 + (WsprCode(callChars[5]) - 10));
                    intB = 0;
                    for (int i = 0; i < 3; ++i)
                    {
                        intB = 37 * intB + WsprCode(prefix[i]);
                    }
                    if (intB >= 32768)
                    {
                        intB = intB - 32768;
                        intB = (intB * 128) + powerByte + 2 + 64;
                    }
                    else
                    {
                        intB = (intB * 128) + powerByte + 1 + 64;
                    }

                }

                // translate the two integers into a 7-byte array
                bytes[3] = ((byte)((intA & 0xF) << 4));
                intA >>= 4;
                bytes[2] = ((byte)(intA & 0xFF));
                intA >>= 8;
                bytes[1] = ((byte)(intA & 0xFF));
                intA >>= 8;
                bytes[0] = ((byte)(intA & 0xFF));
                bytes[6] = ((byte)((intB & 0x3) << 6));
                intB >>= 2;
                bytes[5] = ((byte)(intB & 0xFF));
                intB >>= 8;
                bytes[4] = ((byte)(intB & 0xFF));
                intB >>= 8;
                bytes[3] = (byte)(bytes[3] | (intB & 0xF));
            }
            catch
            {
                bytes = null;
            }
            return bytes;
        }

        /// <summary>
        /// Clean-up a callsign in preparation for WSPR encoding.
        /// Throw an exception if it is not in an expected format.
        /// </summary>
        public static string SanitizeCallsign2(string callsign)
        {
            try
            {
                if (callsign is null)
                    throw new ArgumentException("callsign must contain at least one number");

                callsign = callsign.Trim().ToUpper();

                // Trim long callsigns to 6 characters
                //if (callsign.Length > 6)
                   // callsign = callsign.Substring(0, 6);

                int numbers = callsign.Where(x => char.IsDigit(x)).Count();
                if (numbers < 1)
                    throw new ArgumentException("callsign must contain at least one number");

                //if (numbers > 1)
                //    throw new ArgumentException("callsign may not contain multiple numbers");

                if (!char.IsLetter(callsign.First()))
                    throw new ArgumentException("callsign must start with a letter");

                //if (!char.IsLetter(callsign.Last()))
                  //  throw new ArgumentException("callsign must end with a letter");

                // If the 2nd character is a digit pad with a space
                //if (char.IsNumber(callsign[1]))
                //    callsign = " " + callsign;

                // The third character must now be a number
                //if (!char.IsNumber(callsign[2]))
                //    throw new ArgumentException("the callsign's second or third character must be a number");

                // Right-pad short callsigns with whitespace
                while (callsign.Length < 10)
                    callsign += " ";
            }
            catch
            {

            }

            return callsign;
        }

        public static string SanitizeCallsign(string callsign)
        {
            try
            {
                if (callsign is null)
                    throw new ArgumentException("callsign must contain at least one number");

                callsign = callsign.Trim().ToUpper();

                // Trim long callsigns to 6 characters
                if (callsign.Length > 6)
                    callsign = callsign.Substring(0, 6);

                int numbers = callsign.Where(x => char.IsDigit(x)).Count();
                if (numbers < 1)
                    throw new ArgumentException("callsign must contain at least one number");

                if (numbers > 1)
                    throw new ArgumentException("callsign may not contain multiple numbers");

                if (!char.IsLetter(callsign.First()))
                    throw new ArgumentException("callsign must start with a letter");

                if (!char.IsLetter(callsign.Last()))
                    throw new ArgumentException("callsign must end with a letter");

                // If the 2nd character is a digit pad with a space
                if (char.IsNumber(callsign[1]))
                    callsign = " " + callsign;

                // The third character must now be a number
                if (!char.IsNumber(callsign[2]))
                    throw new ArgumentException("the callsign's second or third character must be a number");

                // Right-pad short callsigns with whitespace
                while (callsign.Length < 6)
                    callsign += " ";
            }
            catch
            {

            }

            return callsign;
        }

        /// <summary>
        /// Clean-up a 4-character location in preparation for WSPR encoding.
        /// Throw an exception if it is not in an expected format.
        /// </summary>
        public static string SanitizeLocation(string location)
        {
            if (location is null)
                throw new ArgumentException("location contain exactly four characters");

            // All characters must be uppercase
            location = location.ToUpper();

            // Location must be exactly four characters long
            if (location.Length != 4)
                location.PadRight(4);
                //throw new ArgumentException("location contain exactly four characters");
            
            // First two characters must be A thru R
            foreach (char letter in location.ToCharArray().Take(2))
                if (letter < 'A' || letter > 'R')
                    throw new ArgumentException("location must start with two letters A-R");

            // Last two characters must be 0-9
            foreach (char letter in location.ToCharArray().Skip(2).Take(2))
                if (letter < '0' || letter > '9')
                    throw new ArgumentException("location must end with two digits 0-9");
            
            return location;
        }
        public static string SanitizeLocation2(string location)
        {
            if (location is null)
                throw new ArgumentException("location contain exactly four characters");

            // All characters must be uppercase
            location = location.ToUpper();

            // Location must be exactly four characters long
            if (location.Length != 4)
                location.PadRight(4);
            //throw new ArgumentException("location contain exactly four characters");
            /*
            // First two characters must be A thru R
            foreach (char letter in location.ToCharArray().Take(2))
                if (letter < 'A' || letter > 'R')
                    throw new ArgumentException("location must start with two letters A-R");

            // Last two characters must be 0-9
            foreach (char letter in location.ToCharArray().Skip(2).Take(2))
                if (letter < '0' || letter > '9')
                    throw new ArgumentException("location must end with two digits 0-9");
            */
            return location;
        }

        public static byte[] GetValidPowerLevels()
        {
            return new byte[] { 0, 3, 7, 10, 13, 17, 20, 23, 27, 30, 33, 37, 40, 43, 47, 50, 53, 57, 60 };
        }

        public static string GetPowerDescription(byte dB)
        {
            double mW = Math.Pow(10, dB / 10.0);
            string power = (mW < 1000)
                ? $"{dB} dB ({mW:#} mW)"
                : $"{dB} dB ({mW / 1000:#} W)";
            power = power.Replace("01 ", "00 ");
            return power;
        }

        /// <summary>
        /// Sanitize a power level in preparation for WSPR encoding.
        /// Only certain power levels are supported by the WSPR protocol.
        /// </summary>
        public static byte SanitizePower(double power)
        {
            if (double.IsNaN(power) || double.IsInfinity(power))
                throw new ArgumentException("power must me finite");

            byte powerLevel = 0;
            foreach (byte validPowerLevel in GetValidPowerLevels())
            {
                if (power >= validPowerLevel)
                    powerLevel = validPowerLevel;
            }

            return powerLevel;
        }

        /// <summary>
        /// Convolve a byte array using JTEncode's convolution method
        /// </summary>
        public static byte[] Convolve(byte[] data, int bitCount = WSPR_BIT_COUNT, int messageSize = WSPR_MESSAGE_SIZE)
        {
            byte[] paddedInput = new byte[messageSize];
            Array.Copy(data, 0, paddedInput, 0, data.Length);

            byte[] output = new byte[bitCount];
            UInt32 reg0 = 0;
            UInt32 reg1 = 0;
            byte inputBit;
            byte parityBit;
            byte bitIndex = 0;
            byte i, j, k;

            for (i = 0; i < WSPR_MESSAGE_SIZE; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    // Set input bit according the MSB of current element
                    inputBit = (byte)((((paddedInput[i] << j) & 0x80) == 0x80) ? 1 : 0);

                    // Shift both registers and put in the new input bit
                    reg0 <<= 1;
                    reg1 <<= 1;
                    reg0 |= inputBit;
                    reg1 |= inputBit;

                    // AND Register 0 with feedback taps, calculate parity
                    UInt32 regTemp = reg0 & 0xf2d05351;
                    parityBit = 0;
                    for (k = 0; k < 32; k++)
                    {
                        parityBit = (byte)(parityBit ^ (regTemp & 0x01));
                        regTemp >>= 1;
                    }
                    output[bitIndex] = parityBit;
                    bitIndex++;

                    // AND Register 1 with feedback taps, calculate parity
                    regTemp = reg1 & 0xe4613c47;
                    parityBit = 0;
                    for (k = 0; k < 32; k++)
                    {
                        parityBit = (byte)(parityBit ^ (regTemp & 0x01));
                        regTemp >>= 1;
                    }

                    output[bitIndex] = parityBit;
                    bitIndex++;
                    if (bitIndex >= bitCount)
                        break;
                }
            }

            return output;
        }

        /// <summary>
        /// Interleave a byte array according to JTEncode's WSPR standard
        /// </summary>
        public static byte[] Interleave(byte[] data)
        {
            byte[] d = new byte[WSPR_BIT_COUNT];
            byte rev, j2, j, k;
            byte i = 0;

            for (j = 0; j < 255; j++)
            {
                j2 = j;
                rev = 0;

                for (k = 0; k < 8; k++)
                {
                    if ((j2 & 0x01) > 0)
                        rev = (byte)(rev | (1 << (7 - k)));
                    j2 >>= 1;
                }

                if (rev < WSPR_BIT_COUNT)
                {
                    d[rev] = data[i];
                    i++;
                }

                if (i >= WSPR_BIT_COUNT)
                    break;
            }

            return d;
        }

        /// <summary>
        /// Combine data with a standard synchronization array that has good auto-correlation properties.
        /// </summary>
        public static byte[] IntegrateSyncValues(byte[] data)
        {
            byte[] sync =
            {
             1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0,
             1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0,
             0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 1,
             0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0,
             1, 1, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1,
             0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1,
             1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0,
             1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0
        };

            if (data.Length != sync.Length)
                throw new ArgumentException($"Input data must be the same size as the sync array ({sync.Length} elements)");

            return Enumerable
                .Range(0, WSPR_BIT_COUNT)
                .Select(i => data[i] * 2 + sync[i])
                .Select(x => (byte)x)
                .ToArray();
        }

        /// <summary>
        /// Encode a character as a number according to JTEncode's standard
        /// </summary>
        public static byte WsprCode(char c)
        {
            if (char.IsDigit(c))
                return (byte)(c - 48);
            else if (c == ' ')
                return 36;
            else if (c >= 'A' && c <= 'Z')
                return (byte)(c - 55);
            else
                return 36;
                //throw new InvalidOperationException($"character {c} is not allowed");
        }
    }
}