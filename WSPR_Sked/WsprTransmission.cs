using MathNet.Numerics;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wspr_Encode;
using WsprSharp;

/*
MIT License

Copyright (c) 2021 Scott W Harden

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Modifications by N. Palmer 2025
 */

namespace WsprSharp
{
    public class WsprTransmission
    {
        public  string Callsign;
        public  string Location;
        public int Power;
        public  byte[] Message = new byte[7];
        
        public string MessageString;

        public  byte[] Levels;
        public  string LevelsString = string.Empty;
        public bool IsValid = false; //=> string.IsNullOrEmpty(ErrorMessage);
        public  string ErrorMessage;
        public  string ErrorMessageDetails = string.Empty;
        public string wsprmsgPath = "";


        /// Create a new single-packet WSPR transmission     
        /// <param name="callsign">standard length (6 character max) callsign</param>
        /// <param name="location">4-character grid square</param>
        /// <param name="power">transmission power (dB units)</param>
        ///  /// <summary>
       
       
        public byte[] WsprTxn(string callsign, string location, double power, int slotNo, int msgType, bool onetx, int opsys) //, byte[] type2Message)
        {
            bool normalCall = true;
            if (callsign.Length > 6 || callsign.Contains("/"))
            {
                normalCall = false;
            }
            if (slotNo == 1 && msgType == 1)  //check call, locator, power
            {
                try
                {
                    Callsign = Encode.SanitizeCallsign(callsign);
                    Location = Encode.SanitizeLocation(location);
                    Power = Encode.SanitizePower(power);
                    ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    ErrorMessageDetails = ex.ToString();
                    Message = Array.Empty<byte>();
                    Levels = Array.Empty<byte>();
                    IsValid = false;
                    return null;
                }
            }
            else //slotNo == 1 or2 and msgType ==2 or 3
            {              
              
                try
                {
                    Callsign = Encode.SanitizeCallsign2(callsign);
                    Location = Encode.SanitizeLocation2(location);
                    Power = Encode.SanitizePower(power);
                    ErrorMessage = string.Empty;
                    IsValid = true;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    ErrorMessageDetails = ex.ToString();
                    Message = Array.Empty<byte>();
                    Levels = Array.Empty<byte>();
                    IsValid = false;
                    return null;
                }
            }
            try
            {
               
               
                if (slotNo == 1 && msgType == 1 )
                {
                    location = location.Substring(0, 4);
                    Message = Encode.GetMessageBytes(callsign, location, power);
                    MessageString = string.Join(" ", Message.Select(x => x.ToString("X2")));
                    byte[] convolved = Encode.Convolve(Message);
                    byte[] interleaved = Encode.Interleave(convolved);
                    Levels = Encode.IntegrateSyncValues(interleaved);
                    LevelsString = GetLevelsString(",");
                }
                else //if msgNo == 2 or 3 and slotNo = 1 or 2
                {              
                    if ((msgType == 2 || msgType == 3)  && slotNo == 1)
                    {
                        if (onetx)
                        {
                            //allow type2/3  message to be sent in one txn
                            location = location.PadRight(6, '*');
                        }
                        else
                        {
                            location = location.Substring(0, 4);    //prevent wspr_enc.exe from interpreting this as a type 3 message
                        }
                    }
                    if (msgType == 2 && slotNo == 2)
                    {
                        location = location.PadRight(6,'*');
                    }     
                    if (msgType == 3 && slotNo ==2)
                    {                        
                        location = location.PadRight(6, '*');  //this will truncate the locator to 4
                    }
                  
                    WsprMsg wspr = new WsprMsg();
                    Levels = wspr.getWsprLevels(wsprmsgPath, callsign, location, power,slotNo,opsys);
                                       
                  
                    LevelsString = GetLevelsString(",");
                    if (Levels == null)
                    {
                        IsValid = false;
                        return null;
                    }
                }
                IsValid = true;
                return Levels;
                 
                
            }
            catch
            {
                Levels = Array.Empty<byte>();
                Message = null;
                ErrorMessage = "Encoding error";
                IsValid = false;
            }
            return Levels;
        }

        public string GetLevelsString(string separator = ",", int valuesPerLine = 30)
        {
            StringBuilder sb = new StringBuilder();
            if (Levels != null)
            {
                for (int i = 0; i < Levels.Length; i++)
                {
                    sb.Append(Levels[i]);
                    sb.Append(separator);
                    if (i % valuesPerLine == valuesPerLine - 1)
                        sb.Append(Environment.NewLine);
                }
                return sb.ToString().Trim(new char[] { '\r', '\n', ',' });
            }
            else { return null; }
        }
    }
}
