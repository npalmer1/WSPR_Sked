using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessagePack.Formatters;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Ocsp;
using JenkinsHash;

namespace WsprEncoders
{

    
    public class WsprEncoder
    {
        const int WSPR_SYMBOL_COUNT = 162;
        const int WSPR_BIT_COUNT = 162;
        public char[] callsign;
     
       
        public char[] locator;
        public byte[] c = new byte[11];
        public byte[] symbols;
        public byte power;


        public WsprEncoder(string cs, string lc, int dbm)
        {
            char[] call = new char[12];
            char[] loc = new char[6];
            if (cs.Length > 12)
            {
                cs = cs.Substring(0, 12);
            }
            for (int i=0;i<cs.Length;i++)
            {
                call[i] = cs[i];
            }
            if (lc.Length > 6)
            {
                lc = lc.Substring(0, 6);
            }
            for (int i=0;i<loc.Length;i++)
            {
                loc[i] = lc[i];
            }

            symbols = wspr_encode(call, loc, (byte)dbm);
        }

        

        /*
     * wspr_encode(const char * call, const char * loc, const uint8_t dbm, uint8_t * symbols)
     *
     * Takes a callsign, grid locator, and power level and returns a WSPR symbol
     * table for a Type 1, 2, or 3 message.
     *
     * call - Callsign (12 characters maximum).
     * loc - Maidenhead grid locator (6 characters maximum).
     * dbm - Output power in dBm.
     * symbols - Array of channel symbols to transmit returned by the method.
     *  Ensure that you pass a uint8_t array of at least size WSPR_SYMBOL_COUNT to the method.
     *
     */
        //void JTEncode::wspr_encode(const char * call, const char * loc, const int8_t dbm, uint8_t * symbols)
        //private void wspr_encode(string[12] call, string[6] loc, sbyte dbm, sbyte symbols)
        private byte[] wspr_encode(char[] call, char[] loc, byte dbm)
        {
            int i;
            //char call_[13];
            char[] call_ = new char[12]; //strings have zero terminator in c, but not in c#
                                         //char loc_[7];
            char[] loc_ = new char[6];
            //uint8_t dbm_ = dbm;
            byte dbm_ = dbm;
            //strcpy(call_, call);
            for (i = 0; i < 12; i++)
            {
                call_[i] = call[i];
            }
            //strcpy(loc_, loc);
            for (i = 0; i < 6; i++)
            {
                loc_[i] = loc[i];
            }

            // Ensure that the message text conforms to standards
            // --------------------------------------------------
            //wspr_message_prep(call_, loc_, dbm_);
            
            wspr_message_prep(call_, loc_, dbm_);

            // Bit packing
            // -----------
            //uint8_t c[11];
           
        //wspr_bit_packing(c);
        
        wspr_bit_packing(ref c); //pass c by pointer or reference or 'out c' instead of ref c

        // Convolutional Encoding
        // ---------------------
        //uint8_t s[WSPR_SYMBOL_COUNT];
        byte[] s = new byte[WSPR_SYMBOL_COUNT];
        //convolve(c, s, 11, WSPR_BIT_COUNT);
        convolve(ref c, ref s, 11, WSPR_BIT_COUNT);

        // Interleaving
        // ------------
        //wspr_interleave(s);
        wspr_interleave(ref s);

        // Merge with sync vector
        // ----------------------
        //wspr_merge_sync_vector(s, symbols);
        wspr_merge_sync_vector(ref s, ref symbols);
            return symbols;
    }


/*
 
/*
 * latlon_to_grid(float lat, float lon, char* ret_grid)
 *
 * Takes a station latitude and longitude provided in decimal degrees format and
 * returns a string with the 6-digit Maidenhead grid designator.
 *
 * lat - Latitude in decimal degrees format.
 * lon - Longitude in decimal degrees format.
 * ret_grid - Derived Maidenhead grid square. A pointer to a character array of
 *   at least 7 bytes must be provided here for the function return value.
 *
 */
    //void JTEncode::latlon_to_grid(float lat, float lon, char* ret_grid)private 
        private void latlon_to_grid(float lat, float lon, ref char[] ret_grid)
        {
            //char grid[7];
            char[] grid = new char[6];
            //memset(grid, 0, 7);
            //Array.Fill(grid, 0, 0, 6);
            for (int i = 0; i < 6; i++)
            {
                grid[i] = '0';
            }


            // Bounds checks
            if (lat < -90.0)
            {
                lat = -90.0f;
            }
            if (lat > 90.0)
            {
                lat = 90.0f;
            }
            if (lon < -180.0)
            {
                lon = -180.0f;
            }
            if (lon > 180.0)
            {
                lon = 180.0f;
            }

            // Normalize lat and lon
            lon += 180.0f;
            lat += 90.0f;

            // Derive first coordinate pair
            //grid[0] = (char)((uint8_t)(lon / 20) + 'A');
            grid[0] = Convert.ToChar((int)(lon / 20) + 'A');
            //grid[1] = (char)((uint8_t)(lat / 10) + 'A');
            grid[1] = Convert.ToChar((int)(lon / 10) + 'A');

            // Derive second coordinate pair
            //lon = lon - ((uint8_t)(lon / 20) * 20);
            lon = lon - (int)((lon / 20) * 20);
            //lat = lat - ((uint8_t)(lat / 10) * 10);
            lat = lat - (int)((lat / 10) * 10);
            //grid[2] = (char)((uint8_t)(lon / 2) + '0');
            grid[2] = Convert.ToChar((int)(lon / 2) + '0');
            //grid[3] = (char)((uint8_t)(lat) + '0');
            grid[2] = Convert.ToChar((int)(lat) + '0');

            // Derive third coordinate pair
            //lon = lon - ((uint8_t)(lon / 2) * 2);
            lon = lon - (int)((lon / 2) * 2);
            //lat = lat - ((uint8_t)(lat));
            lat = lat - (int)(lat);
            //grid[4] = (char)((uint8_t)(lon * 12) + 'a');
            grid[4] = Convert.ToChar((int)((lon * 12) + 'a'));
            //grid[5] = (char)((uint8_t)(lat * 24) + 'a');
            grid[5] = Convert.ToChar((int)((lat * 24) + 'a'));

            //strncpy(ret_grid, grid, 6);
            for (int i = 0; i < 6; i++)
            {
                ret_grid[i] = grid[i];
            }
        }



        //uint8_t JTEncode::wspr_code(char c)
        private byte wspr_code(char c)
        {
            // Validate the input then return the proper integer code.
            // Change character to a space if the char is not allowed.

            //if(isdigit(c))           

            if (char.IsDigit(c))
            {
                //return (uint8_t)(c - 48);
                return (byte)(c - 48);
            }
            else if (c == ' ')
            {
                return 36;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                //return (uint8_t)(c - 55);
                return (byte)(c - 55);
            }
            else
            {
                return 36;
            }
            return 36;
        }




        //void JTEncode::wspr_message_prep(char * call, char * loc, int8_t dbm)
        private void wspr_message_prep(char[] call, char[] loc, byte dbm)
{
    // Callsign validation and padding
    // -------------------------------

    // Ensure that the only allowed characters are digits, uppercase letters, slash, and angle brackets
    //uint8_t i;
    byte i;

    for (i = 0; i < 12; i++)
    {
        if (call[i] != '/' && call[i] != '<' && call[i] != '>')
        {
            //call[i] = toupper(call[i]);			
            call[i] = char.ToUpper(call[i]);
            //if(!(isdigit(call[i]) || isupper(call[i])))
            if (!char.IsDigit(call[i]) || char.IsUpper(call[i]))
            {
                call[i] = ' ';
            }
        }
    }
    //call[12] = 0;

    //strncpy(callsign, call, 12);
    //callsign is an array of 12 characters - could juyst be a char[12] in c#
    //copies 12 characters from call to array callsign
    for (i = 0; i < 12; i++)
    {
        callsign[i] = call[i];
    }

    // Grid locator validation
    //if(strlen(loc) == 4 || strlen(loc) == 6)
    if (loc.Length == 4 || loc.Length == 6)
    {
        for (i = 0; i <= 1; i++)
        {
            //loc[i] = toupper(loc[i]);
            loc[i] = char.ToUpper(loc[i]);
            if ((loc[i] < 'A' || loc[i] > 'R'))
            {
                        //strncpy(loc, "AA00AA", 7);
                        loc[0] = 'A';
                        loc[1] = 'A';
                        loc[2] = '0';
                        loc[3] = '0';
                        loc[4] = 'A';
                        loc[5] = 'A';                       
            }
        }
        for (i = 2; i <= 3; i++)
        {
            //if(!(isdigit(loc[i])))
            if (char.IsDigit(loc[i]))
            {
                        //strncpy(loc, "AA00AA", 7);
                        loc[0] = 'A';
                        loc[1] = 'A';
                        loc[2] = '0';
                        loc[3] = '0';
                        loc[4] = 'A';
                        loc[5] = 'A';
             }
        }
    }
    else
    {
                //strncpy(loc, "AA00AA", 7);
                loc[0] = 'A';
                loc[1] = 'A';
                loc[2] = '0';
                loc[3] = '0';
                loc[4] = 'A';
                loc[5] = 'A';                
    }

    //if(strlen(loc) == 6)
    if (loc.Length == 6)
    {
        for (i = 4; i <= 5; i++)
        {
            //loc[i] = toupper(loc[i]);
            loc[i] = char.ToUpper(loc[i]);
            if ((loc[i] < 'A' || loc[i] > 'X'))
            {
                //stncpy(loc, "AA00AA", 7);              
                        loc[0] = 'A';
                        loc[1] = 'A';
                        loc[2] = '0';
                        loc[3] = '0';
                        loc[4] = 'A';
                        loc[5] = 'A';
            }
        }
    }

    //strncpy(locator, loc, 7);
    for (i = 0; i < 6; i++)
    {
        locator[i] = loc[i];
    }


    // Power level validation
    // Only certain increments are allowed
    if (dbm > 60)
    {
        dbm = 60;
    }
    //const uint8_t VALID_DBM_SIZE = 28;
    const byte VALID_DBM_SIZE = 28;
            //const int8_t valid_dbm[VALID_DBM_SIZE] =
            //byte[] valid = new byte[VALID_DBM_SIZE];
            //valid = {-30, -27, -23, -20, -17, -13, -10, -7, -3,0, 3, 7, 10, 13, 17, 20, 23, 27, 30, 33, 37, 40,43, 47, 50, 53, 57, 60};
            int[] valid_dbm = { -30, -27, -23, -20, -17, -13, -10, -7, -3, 0, 3, 7, 10, 13, 17, 20, 23, 27, 30, 33, 37, 40, 43, 47, 50, 53, 57, 60 };
          
            for (i = 0; i < VALID_DBM_SIZE; i++)  //28
            {
                if (dbm == valid_dbm[i])
                {
                        power = dbm;
            }
    }
    // If we got this far, we have an invalid power level, so we'll round down
    for (i = 1; i < VALID_DBM_SIZE; i++)
    {
        if (dbm < valid_dbm[i] && dbm >= valid_dbm[i - 1])
        {
            power = (byte)valid_dbm[i - 1];
        }
    }
}


        //void JTEncode::wspr_bit_packing(uint8_t * c)
        private void wspr_bit_packing(ref byte[] c)
        {
            //uint32_t n, m;  
            int n, m;

            // Determine if type 1, 2 or 3 message
            //char* slash_avail = strchr(callsign, (int)'/'); //slash_avail is a character pointer
            string cs = callsign.ToString();
            int slash_avail = cs.IndexOf('/');
            if (callsign[0] == '<') //callsign starts with < and is to be hashed - type 3
            {
                // Type 3 message
                //char base_call[13];
                char[] base_call = new char[12]; //should his be 12 as c# strings are no nul terminated
                                                 //memset(base_call, 0, 13);  //used to initialize or reset a block of memory to a specific value. - sets 13 bytes of the memory pointed to by base_call to 0 (c# equiv?)
                for (int a = 0; a < 12; a++)
                {
                    base_call[a] = '0';
                }
                //uint32_t init_val = 146;
                uint init_val = 146;

                //char* bracket_avail = strchr(callsign, (int)'>');

                int bracket_avail = cs.IndexOf('>');

                //int call_len = bracket_avail - callsign - 1;
                int call_len = bracket_avail - 1;

                //strncpy(base_call, callsign + 1, call_len);
                for (int a = 0; a < call_len; a++)
                {
                    base_call[a] = callsign[a];
                }
               
                //uint32_t hash = nhash_(base_call, &call_len, &init_val);
                Hash Nhash = new Hash();
                byte[] byteArray = Encoding.UTF8.GetBytes(base_call);
                uint hash = Nhash.nhash(byteArray, call_len, init_val);  //note: init_val is 146
                                                                   //hash &= 32767;
                hash &= 32767; //32767 is 0x7FFF - that is 15 bits and this ANDs hash with it

                // Convert 6 char grid square to "callsign" format for transmission
                // by putting the first character at the end
                char temp_loc = locator[0];
                locator[0] = locator[1];
                locator[1] = locator[2];
                locator[2] = locator[3];
                locator[3] = locator[4];
                locator[4] = locator[5];
                locator[5] = temp_loc;

                n = wspr_code(locator[0]);
                n = (byte)(n * 36 + wspr_code(locator[1]));
                n = (byte)(n * 10 + wspr_code(locator[2]));
                n = (byte)(n * 27 + (wspr_code(locator[3]) - 10));
                n = (byte)(n * 27 + (wspr_code(locator[4]) - 10));
                n = (byte)(n * 27 + (wspr_code(locator[5]) - 10));

                m = (byte)((hash * 128) - (power + 1) + 64);
            }
            //else if(slash_avail == (void *)0)  //doesn't include a / so type 1
            else if (slash_avail == -1)
            {
                // Type 1 message
                pad_callsign(ref callsign);
                n = wspr_code(callsign[0]);
                n = (byte)(n * 36 + wspr_code(callsign[1]));
                n = (byte)(n * 10 + wspr_code(callsign[2]));
                n = (byte)(n * 27 + (wspr_code(callsign[3]) - 10));
                n = (byte)(n * 27 + (wspr_code(callsign[4]) - 10));
                n = (byte)(n * 27 + (wspr_code(callsign[5]) - 10));

                m = (byte)(((179 - 10 * (locator[0] - 'A') - (locator[2] - '0')) * 180) +
                    (10 * (locator[1] - 'A')) + (locator[3] - '0'));
                m = (byte)((m * 128) + power + 64);
            }
            //else if(slash_avail) //?????
            else if (slash_avail >= 0)
            {
                // Type 2 message
                //int slash_pos = slash_avail - callsign;
                int slash_pos = slash_avail;
                //uint8_t i;
                byte i;

                // Determine prefix or suffix
                //if(callsign[slash_pos + 2] == ' ' || callsign[slash_pos + 2] == 0)  //the 0 is lookign  for a null terminator - not available in c#
                if (callsign[slash_pos + 2] == ' ' || (slash_pos + 2) == callsign.Length)
                {
                    // Single character suffix
                    //char base_call[7];
                    char[] base_call = new char[6];
                    //memset(base_call, 0, 7);
                    for (int a = 0; a < 6; a++)
                    {
                        base_call[a] = '0';
                    }

                    //strncpy(base_call, callsign, slash_pos);
                    for (int a = 0; a < slash_pos; a++)
                    {
                        base_call[a] = callsign[a];
                    }

                    for (i = 0; i < 6; i++)
                    {
                        //base_call[i] = toupper(base_call[i]);
                        base_call[i] = char.ToUpper(base_call[i]);
                        //if(!(isdigit(base_call[i]) || isupper(base_call[i])))
                        if (!(char.IsDigit(base_call[i]) || char.IsUpper(base_call[i])))
                        {
                            base_call[i] = ' ';
                        }
                    }
                    pad_callsign(ref base_call);

                    n = wspr_code(base_call[0]);
                    n = (byte)(n * 36 + wspr_code(base_call[1]));
                    n = (byte)(n * 10 + wspr_code(base_call[2]));
                    n = (byte)(n * 27 + (wspr_code(base_call[3]) - 10));
                    n = (byte)(n * 27 + (wspr_code(base_call[4]) - 10));
                    n = (byte)(n * 27 + (wspr_code(base_call[5]) - 10));

                    char x = callsign[slash_pos + 1]; /////////- should this be a char?
                    if (x >= 48 && x <= 57)
                    {
                        x -= Convert.ToChar(48);
                    }
                    else if (x >= 65 && x <= 90)
                    {
                        x -= Convert.ToChar(55);
                    }
                    else
                    {
                        x = Convert.ToChar(38);
                    }

                    m = (byte)(60000 - 32768 + x);

                    m = (byte)((m * 128) + power + 2 + 64);
                }
                //else if(callsign[slash_pos + 3] == ' ' || callsign[slash_pos + 3] == 0)
                else if (callsign[slash_pos + 3] == ' ' || (slash_pos + 3) == callsign.Length)
                {
                    // Two-digit numerical suffix
                    //char base_call[7];
                    char[] base_call = new char[6];
                    //memset(base_call, 0, 7);
                    for (int a = 0; a < 6; a++)
                    {
                        base_call[a] = '0';
                    }
                    //strncpy(base_call, callsign, slash_pos); ////////////////////////////////////////////////////////////////////
                    for (i = 0; i < slash_pos; i++)
                    {
                        base_call[i] = callsign[i];
                    }

                    for (i = 0; i < 6; i++)
                    {
                        //base_call[i] = toupper(base_call[i]);
                        base_call[i] = char.ToUpper(base_call[i]);
                        //if(!(isdigit(base_call[i]) || isupper(base_call[i])))
                        if (char.IsDigit(base_call[i]) || char.IsUpper(base_call[i]))
                        {
                            base_call[i] = ' ';
                        }
                    }
                    pad_callsign(ref base_call);

                    n = wspr_code(base_call[0]);
                    n = (byte)(n * 36 + wspr_code(base_call[1]));
                    n = (byte)(n * 10 + wspr_code(base_call[2]));
                    n = (byte)(n * 27 + (wspr_code(base_call[3]) - 10));
                    n = (byte)(n * 27 + (wspr_code(base_call[4]) - 10));
                    n = (byte)(n * 27 + (wspr_code(base_call[5]) - 10));

                    // TODO: needs validation of digit
                    m = (byte)(10 * (callsign[slash_pos + 1] - 48) + callsign[slash_pos + 2] - 48);
                    m = (byte)(60000 + 26 + m);
                    m = (byte)((m * 128) + power + 2 + 64);
                }
                else
                {
                    // Prefix
                    //char prefix[4];
                    char[] prefix = new char[3];
                    //char base_call[7];
                    char[] base_call = new char[6];
                    //memset(prefix, 0, 4);
                    for (int a = 0; a < 3; a++)
                    {
                        prefix[a] = '0';
                    }
                    //memset(base_call, 0, 7);
                    for (int a = 0; a < 6; a++)
                    {
                        base_call[a] = '0';
                    }
                    //strncpy(prefix, callsign, slash_pos);	

                    string cs2;
                    cs2 = cs.Substring(0, slash_pos);
                    for (i = 0; i < cs2.Length; i++)
                    {
                        prefix[i] = cs2[i];
                    }

                    //strncpy(base_call, callsign + slash_pos + 1, 7);
                    cs2 = cs.Substring(slash_pos + 1, cs.Length - (slash_pos + 1));
                    for (i = 0; i < cs.Length; i++)
                    {
                        base_call[i] = cs[i];
                    }

                    //if(prefix[2] == ' ') || prefix[2] == 0) //strings not zero terminated in c# 
                    if (prefix[2] == ' ' || prefix[2] == 0)
                    {
                        // Right align prefix
                        //prefix[3] = 0;
                        prefix[2] = prefix[1];
                        prefix[1] = prefix[0];
                        prefix[0] = ' ';
                    }

                    for (i = 0; i < 6; i++)
                    {
                        //base_call[i] = toupper(base_call[i]);
                        base_call[i] = char.ToUpper(base_call[i]);
                        //if(!(isdigit(base_call[i]) || isupper(base_call[i])))
                        if (!(char.IsDigit(base_call[i]) || char.IsUpper(base_call[i])))
                        {
                            base_call[i] = ' ';
                        }
                    }
                    pad_callsign(ref base_call);

                    n = wspr_code(base_call[0]);
                    n = (n * 36 + wspr_code(base_call[1]));
                    n = (n * 10 + wspr_code(base_call[2]));
                    n = (n * 27 + (wspr_code(base_call[3]) - 10));
                    n = (n * 27 + (wspr_code(base_call[4]) - 10));
                    n = (n * 27 + (wspr_code(base_call[5]) - 10));

                    m = 0;
                    //for(uint8_t i = 0; i < 3; ++i)
                    for (i = 0; i < 3; i++)
                    {
                        m = (37 * m + wspr_code(prefix[i]));
                    }

                    if (m >= 32768)
                    {
                        m -= 32768;
                        m = (m * 128) + power + 2 + 64;
                    }
                    else
                    {
                        m = (m * 128) + power + 1 + 64;
                    }
                }


                // Callsign is 28 bits, locator/power is 22 bits.
                // A little less work to start with the least-significant bits
                //c[3] = (uint8_t)((n & 0x0f) << 4);	
                c[3] = (byte)((n & 0x0F) << 4);
                n = (n >> 4);
                //c[2] = (uint8_t)(n & 0xff);
                c[2] = (byte)(n & 0x0FF);
                n = n >> 8;
                //c[1] = (uint8_t)(n & 0xff);
                c[1] = (byte)(n & 0x0FF);
                n = n >> 8;
                //c[0] = (uint8_t)(n & 0xff);
                c[0] = (byte)(n & 0x0FF);

                //c[6] = (uint8_t)((m & 0x03) << 6);
                c[6] = (byte)((m & 0x03) << 6);
                m = m >> 2;
                //c[5] = (uint8_t)(m & 0xff);
                c[5] = (byte)(m & 0xFF);
                m = m >> 8;
                //c[4] = (uint8_t)(m & 0xff);
                c[4] = (byte)(m & 0xFF);
                m = m >> 8;
                //c[3] |= (uint8_t)(m & 0x0f);
                c[3] = (byte)(m & 0xFF);
                c[7] = 0;
                c[8] = 0;
                c[9] = 0;
                c[10] = 0;
            }
        }

//void JTEncode::wspr_interleave(uint8_t * s)
private void wspr_interleave(ref byte[] s)
{
    //uint8_t d[WSPR_BIT_COUNT];
    byte[] d = new byte[WSPR_BIT_COUNT];
    //uint8_t rev, index_temp, i, j, k;
    byte rev, index_temp, i, j, k;

    i = 0;

    for (j = 0; j < 255; j++)
    {
        // Bit reverse the index
        index_temp = j;
        rev = 0;

        for (k = 0; k < 8; k++)
        {
            //if (index_temp & 0x01)
            //{
            //       rev = rev | (1 << (7 - k));
            //}

                    if ((index_temp & 0x01) == 0x01) // Check if LSB of index_temp is 1
                    {
                        rev = (byte)(rev | (1 << (7 - k))); // Set the (7 - k)th bit of rev to 1
                    }
                    index_temp = (byte)(index_temp >> 1);
        }

        if (rev < WSPR_BIT_COUNT)
        {
            d[rev] = s[i];
            i++;
        }

        if (i >= WSPR_BIT_COUNT)
        {
            break;
        }
    }

    //memcpy(s, d, WSPR_BIT_COUNT);
    //Buffer.BlockCopy(s, 0, d, 0, WSPR_BIT_COUNT);
    //OR:
    Array.Copy(s, d, WSPR_BIT_COUNT);
}


//void JTEncode::wspr_merge_sync_vector(uint8_t * g, uint8_t * symbols)
private void wspr_merge_sync_vector(ref byte[] g, ref byte[] symbols)
{
    //uint8_t i;
    byte i;
    //const uint8_t sync_vector[WSPR_SYMBOL_COUNT] =
   
        
        byte[] sync_vector = {1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0,1, 0, 1, 1, 1, 1, 0, 0, 
            0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0,0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 1,
            0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0,1, 1, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0,
            1, 0, 0, 0, 0, 0, 1,0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1,1, 1, 0, 0, 0, 0, 0,
            1, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0,1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0};

    for (i = 0; i < WSPR_SYMBOL_COUNT; i++)
    {
        symbols[i] = (byte)(sync_vector[i] + (2 * g[i]));
    }
}


//void JTEncode::convolve(uint8_t * c, uint8_t * s, uint8_t message_size, uint8_t bit_size)
private void convolve(ref byte[] c, ref byte[] s, byte message_size, byte bit_size)
{
    //uint32_t reg_0 = 0;
    int reg_0 = 0;
    //uint32_t reg_1 = 0;
    int reg_1 = 0;
    //uint32_t reg_temp = 0;
    int reg_temp = 0;
    //uint8_t input_bit, parity_bit;
   int input_bit, parity_bit;
    //uint8_t bit_count = 0;
    byte bit_count = 0;
    //uint8_t i, j, k;
    byte i, j, k;

    for (i = 0; i < message_size; i++)
    {
        for (j = 0; j < 8; j++)
        {
            // Set input bit according the MSB of current element
            input_bit = (((c[i] << j) & 0x80) == 0x80) ? 1 : 0;

            // Shift both registers and put in the new input bit
            reg_0 = reg_0 << 1;
            reg_1 = reg_1 << 1;
            //reg_0 |= (uint32_t)input_bit;
            reg_0 |= (int)input_bit;
            //reg_1 |= (uint32_t)input_bit;
            reg_1 |= (int)input_bit;

                    // AND Register 0 with feedback taps, calculate parity
                    reg_temp = (int)(reg_0 & 0xf2d05351);
            parity_bit = 0;
            for (k = 0; k < 32; k++)
            {
                //parity_bit = parity_bit ^ (reg_temp & 0x01);
                parity_bit = (byte)(parity_bit ^ (reg_temp & 0x01));
                //reg_temp = reg_temp >> 1;
                reg_temp = (byte)(reg_temp >> 1);
            }
            s[bit_count] = (byte)parity_bit;
            bit_count++;

                    // AND Register 1 with feedback taps, calculate parity
                    reg_temp = (int)(reg_1 & 0xe4613c47);
            parity_bit = 0;
            for (k = 0; k < 32; k++)
            {
                //parity_bit = parity_bit ^ (reg_temp & 0x01);
                parity_bit = (byte)(parity_bit ^ (reg_temp & 0x01));
                //reg_temp = reg_temp >> 1;
                reg_temp = (byte)(reg_temp >> 1);
            }
            s[bit_count] = (byte)parity_bit;
            bit_count++;
            if (bit_count >= bit_size)
            {
                break;
            }
        }
    }
}



//void JTEncode::pad_callsign(char * call)
private void pad_callsign(ref char[] call)
{
    // If only the 2nd character is a digit, then pad with a space.
    // If this happens, then the callsign will be truncated if it is
    // longer than 6 characters.
    //if(isdigit(call[1]) && isupper(call[2]))
    if (char.IsDigit(call[1]) && char.IsUpper(call[2]))
    {
        // memmove(call + 1, call, 6);
        call[5] = call[4];
        call[4] = call[3];
        call[3] = call[2];
        call[2] = call[1];
        call[1] = call[0];
        call[0] = ' ';
    }

    // Now the 3rd charcter in the callsign must be a digit
    // if(call[2] < '0' || call[2] > '9')
    // {
    // 	// return 1;
    // }
}
}
}
