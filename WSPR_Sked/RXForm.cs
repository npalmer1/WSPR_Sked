using Google.Protobuf.WellKnownTypes;
//using WSPRlive;
//using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
//using static Mysqlx.Expect.Open.Types.Condition.Types;
//using static System.Net.Mime.MediaTypeNames;
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Maidenhead;
using MathNet.Numerics;
//using Microsoft.VisualBasic.ApplicationServices;
using MySql.Data.MySqlClient;
//using Mysqlx.Crud;
//using NAudio.Gui;
using NAudio.Wave;
//using Org.BouncyCastle.Tls;
using System;
//using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
//using System.Diagnostics.Eventing.Reader;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
//using System.Net.NetworkInformation;
//using System.Reflection.Metadata;
//using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
//using System.Transactions;
using System.Windows.Forms;

namespace WSPR_Sked
{
    public partial class RXForm : Form
    {

        public bool RXblock = false;
        private bool started = false;
        public int wavno = 2;
        public int prevwav = 1;

        public string version = "";


        public string output;
        public string results;
        public struct decoded_data
        {
            public DateTime datetime;
            public Int16 band;
            public string tx_sign;
            public string tx_loc;
            public double frequency;
            public Int16 power;
            public int snr;
            public Int16 drift;
            public int distance;
            public Int16 azimuth;
            public string reporter;
            public string reporter_loc;
            public float dt;
        }
        decoded_data DX = new decoded_data();
        DateTime startTime;

        string[] cells = new string[10];
        private static readonly object _lock = new object();
        int maxrows = 2000;

        public string my_loc;

        public bool blockDecodes = false;


        private string server;
        private string user;
        private string pass;
        private string Callsign = "";
        bool endSlot = false;

        public static string Frequency;
        public string prevFreq;
        public string postStatus = "";

        public string wsprfilepath;

        public string wsprdfilepath;

        public bool useDeep;
        public bool useQuick;
        public int OSD; // use Ordered Statistics Decoder if >0

        public int audioDevice = -1;
        public float gain = 1;
        public bool finished = false;

        public bool stopUrl = false;

        string slash = "\\";
        string userdir = "";
        string exeDir = "";
        

        DateTime nextDT;


        MessageClass Msg = new MessageClass();

        public RXForm()
        {
            InitializeComponent();
            OSDlistBox.SelectedIndex = 0;
        }

        //private WaveInEvent waveIn = new WaveInEvent();

        private void RXForm_Load(object sender, EventArgs e)
        {

        }

        public void set_header(string call, string serverName, string db_user, string db_pass, string loc, int audioDev, string wsprdpath, string ver, int opsys)
        {
            this.Text = "Transmissions received by this station";
            Callsign = call;
            server = serverName;
            user = db_user;
            pass = db_pass;
            my_loc = loc;
            userdir = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string exePath = Assembly.GetExecutingAssembly().Location;
            exeDir = Path.GetDirectoryName(exePath);
            if (opsys !=0) //if not windows
            {
                slash = "/";
            }
            version = "WS-" + ver;
            audioDevice = audioDev;
            wsprdfilepath = wsprdpath;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm";
            //dateTimePicker1.ShowUpDown = true;
            bandlistBox.SelectedIndex = 0;
            myloclabel.Text = my_loc;
            Read_Config();
            try
            {
                for (int i = 1; i <= 3; i++)    //delete all existimg wav files 
                {
                    string outpath = wsprdfilepath + slash+"temp" + i + ".wav";
                    if (File.Exists(outpath))
                    {
                        try
                        {
                            File.Delete(outpath);
                        }
                        catch { }
                    }
                }
            }
            catch
            {

            }

        }

        public async Task set_frequency(string Freq)
        {
            Frequency = Freq;
            RXFlabel.Text = Freq;
            myloclabel.Text = my_loc;
        }

        public async Task set_time(string time)
        {
            Timelabel.Text = time;

        }

        public async Task set_prev_frequency()  //remember frequency before decoding begins
        {
            prevFreq = Frequency;
        }



        private async void Record_Decode(int opsys)
        {
            string wsprdir = "C:\\WSPR_Sked";
            if (!Directory.Exists(wsprdir))
            {
                // Create the folder
                Directory.CreateDirectory(wsprdir);
            }
                if (prevwav == wavno)
            {
                return;
            }
            if (!File.Exists(wsprdfilepath + slash +"wsprd.exe"))
            {
                Msg.TMessageBox("Error: wsprd.exe not found! - see RX & Sound", "WSPR daemon error", 2000);
                return;
            }
            if (finished)
            {
                finished = false;
            }

        

            string outpath = wsprdir + slash +"temp" + wavno + ".wav";
            if (File.Exists(outpath))
            {
                try
                {
                    File.Delete(outpath);
                }
                catch { }
            }


            await Task.Delay(200);
            statuslabel.Text = "receiving";
            Msg.TMessageBox("Recording: " + wsprdir + slash+ "temp" + wavno + ".wav", "", 3000);

            string args = "";
            int mS = 110000;

            DateTime now = DateTime.Now.ToUniversalTime();
            int s = now.Second;
            int m = now.Minute;

            if (now.Minute % 2 == 1)
            {
                nextDT = now.AddMinutes(-1); //about odd min+51 so make it start time of last spot
            }
            else
            {
                nextDT = now.AddMinutes(-2); //should never get here unles a delay
            }

            startTime = now;

            int even = m % 2;
            if (s == 0 && even == 0)
            {
                mS = 110000; //if starting at zero count for 110S
            }
            else if (s == 59 && even == 1)  //if starting at 59 sec count for 1 sec longer
            {
                mS = 111000;
            }
            else if (even == 0 && s == 1)
            {
                mS = 109000;
                //mS = mS -(s * 1000); //if not starting at 0s deduct seconds
            }
            else
            {
                //mS = 0;
            }


            await RecordLineInAsync_Gain(outpath, mS);
            DateTime originalDT = DateTime.Now.ToUniversalTime();
            if (originalDT.Minute % 2 == 1)
            {
                originalDT = originalDT.AddMinutes(-1); //about odd min+51 so make it start time of last spot
            }
            else
            {
                originalDT = originalDT.AddMinutes(-2); //should never get here unles a delay
            }

            //string content = outpath;

            await Task.Delay(100);
            statuslabel.Text = "decoding";
            string d = "";
            if (useDeep)
            {
                d = " -d";
            }
            else if (useQuick)
            {
                d = " -q";
            }
            else //== normal depth
            {
                d = "";
            }
            string o = "";
            if (OSD > 0)
            {
                o = " -o " + OSD.ToString();
            }
            if (finished)
            {
                //prevwav = wavno;
            }
            results = "";


            if (wavno == 1)
            {
                prevwav = 1;
                wavno = 2;
            }
            else if (wavno == 2)
            {
                prevwav = 2;
                wavno = 3;
            }
            else
            {
                prevwav = 3;
                wavno = 1;
            }
            string wavfile = wsprdir + slash + "temp" + prevwav + ".wav";

            string c = "";
            string cmd = "";

            if (opsys == 0)  //windows
            {
                c = "/c ";
                cmd = "cmd.exe";
                //args = c + wsprdfilepath + slash + "wsprd.exe -a " + userdir + " -f " + Frequency + d + o + " " + wavfile;
                args = c + wsprdfilepath + slash + "wsprd.exe -a "+wsprdir+" -f " + Frequency + d + o + " " + wavfile;
            }
            else
            {
                //Linux etc.
                cmd = "/bin/bash";
                c = "-c ";
                //args = c + wsprdfilepath + slash + "wsprd -a " + userdir + " -f " + Frequency + d + o + " " + wavfile;
                args = c + wsprdfilepath + slash + "wsprd -a "+wsprdir+" -f " + Frequency + d + o + " " + wavfile;

            }
                var fileInfo = new FileInfo(wavfile);

                if (fileInfo.Exists && fileInfo.Length == 0)
                {
                    return;
                }
                output = "";
                if (!blockDecodes)    //block decodes whilst transmitting - from Wspr_transmit on form1
                {
                    Msg.TMessageBox("Decoding: " + wsprdir + slash + "temp" + prevwav + ".wav", "", 3000);
                    await Task.Run(() =>
                    {
                        runDecoder(cmd, args);

                    });

                    results = output;

                    statuslabel.Text = "saving";
                    await SaveReceived(originalDT);
                }


                //statuslabel.Text = "idle";
                finished = true;
                RXblock = false;
                started = false;
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.AllowUserToAddRows = false;
                }
            
        }

        public async Task SaveReceived(DateTime originalDT)
        {
            if (stopRXcheckBox.Checked)
            {
                return;
            }


            DateTime startT = new DateTime(originalDT.Year, originalDT.Month, originalDT.Day,
                                            originalDT.Hour, originalDT.Minute, 0); //set seconds to zero

            try
            {   
                await save_result_lines(startT);
                await Task.Delay(200);
                int rows = table_count(server, user, pass);
                if (rows > 0)
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
                    find_reported(rows);

                }
            }
            catch { }

        }




        public async Task Start_Receive(int opsys)
        {
            if (stopRXcheckBox.Checked)
            {
                return;
            }
            useDeep = DeepcheckBox.Checked;
            useQuick = QuickcheckBox.Checked;
            OSD = OSDlistBox.SelectedIndex;
            if (OSD == -1)
            {
                OSD = 0;
            }
            Record_Decode(opsys);
        }

        private async Task process_decoded(string data, double TXf, DateTime startT, bool containsData)
        {
            DX.frequency = 0;
            DX.snr = 0;
            DX.dt = 0;
            DX.drift = 0;
            DX.band = 0;
            DX.tx_sign = "";
            DX.tx_loc = "";
            DX.power = 0;
            DX.distance = 0;
            DX.azimuth = 0;
            DX.reporter = "";
            DX.reporter_loc = "";
            if (nextDT == startT)
            {
                return; //avoid duplication of entries
            }
            try
            {
                DX.datetime = startT;
                int off = 0;
                if (containsData)
                {

                    string[] R = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);


                    DX.snr = Convert.ToInt16(R[1]);
                    DX.dt = (float)Convert.ToDouble(R[2]);
                    if (R[3].Trim() == "")
                    {
                        off = 1;
                    }
                    else
                    {
                        off = 0;
                    }
                    DX.frequency = Convert.ToDouble(R[3 + off]);

                    DX.frequency = Math.Round(DX.frequency, 6); //round to 6 decimal places
                    DX.band = (Int16)convertMHz(DX.frequency);
                    DX.drift = Convert.ToInt16(R[4 + off]);


                    DX.power = Convert.ToInt16(R[7 + off]);

                    DX.tx_sign = R[5 + off];
                    DX.tx_loc = R[6 + off];
                    int km;
                    int az;
                    (km, az) = Calculate_km_az(DX.tx_loc, my_loc);

                    DX.distance = km;
                    DX.azimuth = (Int16)az;
                    DX.reporter = Callsign;
                    DX.reporter_loc = my_loc;

                }
                else
                {
                    if (!containsData)
                    {
                        DX.frequency = TXf;
                        DX.band = (Int16)convertMHz(DX.frequency);
                        DX.snr = 0;
                        DX.dt = 0;
                        DX.drift = 0;
                        DX.tx_sign = "nil rcvd";
                        DX.tx_loc = "";
                        DX.power = 0;
                        DX.distance = 0;
                        DX.azimuth = 0;
                        DX.reporter = "";
                        DX.reporter_loc = "";
                    }
                }

            }
            catch
            {

                DX.frequency = TXf;
                DX.band = (Int16)convertMHz(DX.frequency);
                DX.snr = 0;
                DX.dt = 0;
                DX.drift = 0;
                DX.tx_sign = "error";
                DX.tx_loc = "";
                DX.power = 0;
                DX.distance = 0;
                DX.azimuth = 0;
                DX.reporter = "";
                DX.reporter_loc = "";
            }
        }

        private int convertMHz(double f)
        {
            int b = -2;
            string mhz = f.ToString("F6");
            if (mhz.StartsWith("0.13"))
            {
                b = -1;
            }
            else if (mhz.StartsWith("0.47"))
            { b = 0; }
            else if (mhz.StartsWith("1.8"))
            { b = 1; }
            else if (mhz.StartsWith("3.5"))
            { b = 3; }
            else if (mhz.StartsWith("5."))
            { b = 5; }
            else if (mhz.StartsWith("7."))
            { b = 7; }
            else if (mhz.StartsWith("10"))
            { b = 10; }
            else if (mhz.StartsWith("14"))
            { b = 14; }
            else if (mhz.StartsWith("18"))
            { b = 18; }
            else if (mhz.StartsWith("21"))
            { b = 21; }
            else if (mhz.StartsWith("24"))
            { b = 24; }
            else if (mhz.StartsWith("28"))
            { b = 28; }
            else if (mhz.StartsWith("50"))
            { b = 50; }
            else if (mhz.StartsWith("70"))
            { b = 70; }
            else if (mhz.StartsWith("144"))
            { b = 15; }
            else if (mhz.StartsWith("432"))
            { b = 16; }

            else if (mhz.StartsWith("1296"))
            { b = 17; }

            return b;
        }

        private void update_grid() //add rows to the datagridview
        {

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            for (int i = 0; i < 10; i++)
            {

                row.Cells[i].Value = cells[i];
            }

            dataGridView1.Rows.Add(row);
        }
        private async Task save_result_lines(DateTime startT)
        {
            //startT is current time minuis 2 mins
            if (results == null || results == "")
            {
                return;
            }
            bool end = false;

            bool containsData = false;
            using var reader = new StringReader(results);

            string line = "";
           
            bool append = false;
            bool stop = false;
            string date = startT.ToString("yyMMdd");
            string time = startT.ToString("HHmm");

            try
            {
                while (!end)
                {
                    line = "";
                    line = reader.ReadLine().Trim();
                    if (line == null || line == "")
                    {
                        //end = true;
                        //stop = true;
                        DX.tx_sign = "nil rcvd";
                        line = "<DecodeFinished>";

                    }
                    if (line.Contains("<DecodeFinished>"))
                    {
                        end = true;
                        if (containsData)
                        {
                            stop = true;
                        }

                    }
                    else
                    {
                        containsData = true;
                        stop = false;
                    }


                    if (stop == false)
                    {
                        try
                        {
                            double f = 0;
                            if (prevFreq != "")
                            {
                                f = Convert.ToDouble(prevFreq);
                            }
                            await process_decoded(line, f, startT, containsData);
                            if (DX.tx_sign != null)
                            {
                                if (!DX.tx_sign.Contains("error"))
                                {
                                    if (DX.tx_sign.Contains("nil rcvd") && containsData)
                                    {
                                        end = true;
                                        stop = true;
                                    }
                                    else
                                    {
                                       
                                        append = true;
                                        await Save_Received_DB(server, user, pass);
                                        if (!DX.tx_sign.Contains("nil rcvd") || DX.tx_sign != "")
                                        {
                                            await Post_wsprdata(date, time);
                                        }

                                    }
                                }
                            }
                            else
                            {
                                end = true;
                                stop = true;
                            }

                            if (end)
                            {
                                stop = true;
                            }
                        }
                        catch { stop = true; end = true; }

                    }
                    //update_grid();


                }
            }
            catch
            {
                MessageBox.Show("Error");

            }
            results = ""; //nullG results
        }

        private async Task Save_WSPR_Textfile(DateTime startT, string filepath, bool append)
        {
            StreamWriter writer;
            writer = new StreamWriter(filepath, append);
            string wspr_line;
            string date = startT.ToString("yyyy-MM-dd");
            string time = startT.ToString("HH:mm");
            wspr_line = date + "\t" + time;
            wspr_line = wspr_line + "\t" + DX.snr.ToString();
            wspr_line += "\t" + DX.drift.ToString();
            wspr_line += "\t" + DX.frequency.ToString("F6");
            wspr_line += "\t" + DX.tx_sign;
            wspr_line += "\t" + DX.tx_loc;
            wspr_line += "\t" + DX.power.ToString();
            wspr_line += "\t" + DX.reporter;
            wspr_line += "\t" + DX.reporter_loc; // + Environment.NewLine;
            string date1 = startT.ToString("yyMMdd");
            string time1 = startT.ToString("HHmm");
            try
            {
                //Post_wsprdata(date1, time1);
                using (writer)
                {
                    writer.WriteLine(wspr_line);
                }
                writer.Close();
            }
            catch (Exception ex)
            {
                writer.Close();
                return;
            }
        }


        private async Task Post_wsprdata(string date, string time)  //uploads single spot to the new wsprnet database
        {
            string url = "http://wsprnet.org/post/";
            if (!await Msg.IsUrlReachable(url) || stopUrl)
            {
                return;
            }
            var formData = new Dictionary<string, string>
            {
                { "function", "wspr" },
                { "date",  date },
                { "time", time },
                { "sig", DX.snr.ToString() },
                { "dt", DX.dt.ToString("f1") },
                { "drift", DX.drift.ToString() },
                { "tqrg", DX.frequency.ToString("F6") },
                { "tcall", DX.tx_sign },
                { "tgrid", DX.tx_loc },
                { "dbm", DX.power.ToString() },
                { "version", version },
                 //{ "version", "2.7.0" },
                { "rcall", Callsign },
                { "rgrid", my_loc },
                { "rqrg", Frequency },
                { "mode", "2" }
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(Callsign, my_loc);

            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WS/0.1.10");

            var content = new FormUrlEncodedContent(formData);

            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
        }

        private async Task Post_wspr(string filepath) //this will upload a file to the old database    - not used!                                         
        {                                               //- the ALL_WSPR.TXT file needs modification first
            if (File.Exists(filepath))
            {

                //var uploadUrl = "http://wsprnet.org/post";
                var uploadUrl = "http://wsprnet.org/meptspots.php";

                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    // Add the file content
                    var fileStream = File.OpenRead(filepath);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
                    form.Add(fileContent, "allmept", Path.GetFileName(filepath));

                    // Add the form fields
                    form.Add(new StringContent(Callsign), "call");
                    form.Add(new StringContent(my_loc), "grid");

                    // Send the POST request
                    HttpResponseMessage response = await client.PostAsync(uploadUrl, form);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Status: {response.StatusCode}");
                    MessageBox.Show(responseBody);
                }
            }
        }

        public async Task runDecoder(string cmd, string args)
        {
            output = "";
           
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo()
                {
                    FileName = cmd, // Command to run
                                          //Arguments = args, // Arguments for the command
                    Arguments = args,
                    RedirectStandardOutput = true, // Redirect output if needed
                    RedirectStandardError = false,  // Redirect error stream if needed
                    UseShellExecute = false,       // Necessary for redirection
                    CreateNoWindow = true,          // Run without a visible window
                                                    //WorkingDirectory
                };


                Process process = new Process
                {
                    StartInfo = processInfo
                };

                process.Start();
                Task.Delay(200);
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

            }
            catch (Exception ex)
            {
                string error = "";
            }

        }



        public async Task RecordLineInAsync_Gain(string outputPath, int durationMs)
        {
            //float gain = 2.0f; // 2x gain, adjust as needed
            if (audioDevice < 0)
            {
                Msg.TMessageBox("Invalid audio device - check audio settings", "Audio error", 2000);
                return;
            }
            WaveInEvent waveIn = new WaveInEvent();
            WaveFileWriter writer;
            try
            {

                waveIn.WaveFormat = new WaveFormat(12000, 1); // 44.1kHz mono

                waveIn.DeviceNumber = audioDevice;

                writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);
                try
                {
                    waveIn.DataAvailable += (s, e) =>
                    {
                        byte[] buffer = e.Buffer;
                        int bytes = e.BytesRecorded;

                        for (int i = 0; i < bytes; i += 2)
                        {
                            short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
                            float sample32 = sample / 32768f;
                            sample32 *= gain;
                            sample32 = Math.Max(-1.0f, Math.Min(1.0f, sample32)); // Clamp to avoid clipping
                            short processedSample = (short)(sample32 * 32768f);
                            buffer[i] = (byte)(processedSample & 0xFF);
                            buffer[i + 1] = (byte)((processedSample >> 8) & 0xFF);
                        }
                        writer.Write(e.Buffer, 0, e.BytesRecorded);

                    };
                }
                catch
                {

                }

                waveIn.RecordingStopped += (s, e) =>
                {
                    writer?.Dispose();
                    waveIn.Dispose();
                };

                waveIn.StartRecording();
                await Task.Delay(durationMs);
                waveIn.StopRecording();
            }
            catch
            {
                waveIn.StopRecording();
            }
        }




        private void button1_Click(object sender, EventArgs e)
        {
            int rows = table_count(server, user, pass);
            if (rows > 0)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
                find_reported(rows);

            }
        }


        private int table_count(string server, string user, string pass)
        {
            int count;
            string connectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rpt";
            var connection = new MySqlConnection(connectionString);
            try
            {
                //string connectionString = "Server=server;Port=3306;Database=wspr;User ID=user;Password=pass;";

                using (connection)
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM received", connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                    connection.Close();
                }
                return count;

            }
            catch
            {
                connection.Close();
                return 0;
            }
        }

        public async Task Save_Received_DB(string serverName, string db_user, string db_pass)
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_rpt";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            DateTime date = new DateTime();
            if (DX.tx_sign == "")
            {
                return;
            }
            lock (_lock)
            {
                MySqlCommand command = connection.CreateCommand();
                try
                {

                   
                    command.CommandText = "INSERT IGNORE INTO received(datetime,band,tx_sign,tx_loc,frequency,power,snr,drift, distance,azimuth,reporter,reporter_loc,dt) ";
                    command.CommandText += "VALUES(@datetime,@band,@tx_sign,@tx_loc,@frequency,@power,@snr,@drift,@distance,@azimuth,@reporter,@reporter_loc,@dt)";

                    connection.Open();

                    //TimeSpan time = Convert.ToDateTime(cells[1]);

                    command.Parameters.AddWithValue("@datetime", DX.datetime);
                    command.Parameters.AddWithValue("@band", DX.band);
                    command.Parameters.AddWithValue("@tx_sign", DX.tx_sign);
                    command.Parameters.AddWithValue("@tx_loc", DX.tx_loc);
                    command.Parameters.AddWithValue("@frequency", DX.frequency);
                    command.Parameters.AddWithValue("@power", DX.power);
                    command.Parameters.AddWithValue("@snr", DX.snr);
                    command.Parameters.AddWithValue("@drift", DX.drift);
                    command.Parameters.AddWithValue("@distance", DX.distance);
                    command.Parameters.AddWithValue("@azimuth", DX.azimuth);
                    command.Parameters.AddWithValue("@reporter", DX.reporter);
                    command.Parameters.AddWithValue("@reporter_loc", my_loc);
                    command.Parameters.AddWithValue("@dt", DX.dt);

                    command.ExecuteNonQuery();


                    connection.Close();

                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                }
            }

        }
        private bool find_reported(int tablecount) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rpt";

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                

                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                //SELECT* FROM your_table ORDER BY your_date_column DESC LIMIT 500;
                command.CommandText = "SELECT * FROM received ORDER BY datetime DESC, frequency ASC LIMIT " + maxrows;
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    found = true;

                    if (i < maxrows && i < tablecount)    //only show first maxrows rows, or to length of reported table
                    {

                        DX.datetime = (DateTime)Reader["datetime"];
                        DX.band = (Int16)Reader["band"];

                        DX.tx_sign = (string)Reader["tx_sign"];
                        DX.tx_loc = (string)Reader["tx_loc"];

                        DX.frequency = (double)Reader["frequency"];
                        DX.power = (Int16)Reader["power"];
                        DX.snr = (int)Reader["snr"];
                        DX.drift = (Int16)Reader["drift"];
                        DX.distance = (int)Reader["distance"];
                        DX.azimuth = (Int16)Reader["azimuth"];
                        DX.reporter = (string)Reader["reporter"];
                        DX.reporter_loc = (string)Reader["reporter_loc"];
                        DX.dt = (float)Reader["dt"];

                        cells[0] = DX.datetime.ToString("yyyy-MM-dd HH:mm");
                        cells[1] = DX.tx_sign;
                        double f = Convert.ToDouble(DX.frequency);
                        //f = f / 1000000;
                        string formattedF = f.ToString("F6");
                        cells[2] = formattedF;
                        if (DX.tx_sign == "nil rcvd")
                        {
                            cells[3] = "";
                            cells[4] = "";
                            cells[5] = "";
                            cells[6] = "";
                            cells[7] = "";
                            cells[8] = "";
                            cells[9] = "";
                        }
                        else
                        {
                            string snr = Convert.ToString(DX.snr);
                            if (DX.snr > 0)
                            {
                                snr = "+" + snr;
                            }
                            cells[3] = snr;  //snr
                            //cells[3] = DX.snr.ToString();
                            cells[4] = DX.drift.ToString();
                            cells[5] = DX.power.ToString();
                            cells[6] = DX.tx_loc;
                            if (DX.distance > -1)
                            {
                                cells[7] = DX.distance.ToString();
                                string mls = convert_to_miles(DX.distance);
                                cells[8] = mls;
                            }
                            else
                            {
                                cells[7] = "";
                                cells[8] = "";
                            }

                            if (DX.azimuth > -1)
                            {
                                cells[9] = DX.azimuth.ToString();
                            }
                            else
                            {
                                cells[9] = "";
                            }

                        }
                        update_grid(); //add this row to the datagridview
                        i++;
                    }
                    else
                    {
                        break;
                    }

                }
                Reader.Close();
                connection.Close();

            }
            catch
            {
                //databaseError = true; //stop wasting time trying to connect if database error - ignore for present
                found = false;
                connection.Close();
            }
            return found;
        }

        public void Save_Config(string serverName, string db_user, string db_pass)
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_rpt";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            DateTime date = new DateTime();
            MySqlCommand command = connection.CreateCommand();
            lock (_lock)
            {
                try
                {

                    

                    command.CommandText = "INSERT INTO rxconfig(id,deep,quick,osd,upload)";
                    command.CommandText += "VALUES(0," + DeepcheckBox.Checked + "," + QuickcheckBox.Checked + ", " + OSDlistBox.SelectedIndex + ", " + uploadcheckBox.Checked + ")";
                    command.CommandText += "ON DUPLICATE KEY UPDATE deep = " + DeepcheckBox.Checked + ", quick = " + QuickcheckBox.Checked + ", osd = " + OSDlistBox.SelectedIndex;
                    command.CommandText += ", upload = " + uploadcheckBox.Checked;
                    //ON DUPLICATE KEY UPDATE username = VALUES(username), email = VALUES(email);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
                catch
                {
                    connection.Close();
                }
            }

        }

        private void Read_Config()
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rpt";

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                

                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM rxconfig";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    DeepcheckBox.Checked = (bool)Reader["deep"];
                    QuickcheckBox.Checked = (bool)Reader["quick"];
                    int osdvalue = (int)Reader["osd"];
                    OSDlistBox.SelectedIndex = osdvalue;
                    uploadcheckBox.Checked = (bool)Reader["upload"];

                }
                Reader.Close();
                connection.Close();

            }
            catch
            {
                connection.Close();
            }
        }


        private string convert_to_miles(int km)
        {
            int m = 0;
            double miles = 0;
            miles = km * 0.621371;
            m = Convert.ToInt32(miles);
            return m.ToString();
        }

        private static (int km, int az) Calculate_km_az(string tx_loc, string my_loc)
        {
            double km = MaidenheadLocator.Distance(tx_loc, my_loc);
            double az = MaidenheadLocator.Azimuth(tx_loc, my_loc);
            return ((int)km, (int)az);
        }



        private void DeepcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DeepcheckBox.Focused)
            {
                if (QuickcheckBox.Checked)
                {
                    QuickcheckBox.Checked = false;
                }
            }

        }

        private void QuickcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (QuickcheckBox.Focused)
            {
                if (DeepcheckBox.Checked)
                {
                    DeepcheckBox.Checked = false;
                }
            }
        }

        private void OSDlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string warn = "";
            if (OSDlistBox.SelectedIndex > 1)
            {
                warn = " (slow)";
            }
            OSDlabel.Text = "OSD " + OSDlistBox.SelectedItem.ToString() + warn;
        }

        private void filterbutton_Click(object sender, EventArgs e)
        {
            MessageForm nForm = new MessageForm();
            Msg.TCMessageBox("Please wait....", "", 20000,nForm);
            if (filterbutton.Text == "Apply")
            {
                filter_results(server, user, pass);
                filterbutton.Text = "Clear";
            }
            else
            {
                show_results(server, user, pass);
                filterbutton.Text = "Apply";
            }
            nForm.Dispose();
        }

        public async Task show_results(string server, string user, string pass) // read back from the reported table to populate the datagridview
        {
            lock (_lock)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
                                                                                             //DateTime dt = DateTime.Now.ToUniversalTime();
                                                                                             //dt = dt.AddHours(-2);
                                                                                             // string date = dt.ToString("yyyy-MM-dd HH:mm:00");
                int rows = table_count(server, user, pass);
                if (rows > 0)
                {
                    find_reported(rows);

                }
                dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
            }
        }

        private void filter_results(string server, string user, string pass)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
            DateTime dt1 = dateTimePicker1.Value;
            DateTime dt2 = dateTimePicker2.Value;
            //dt = dt.AddHours(-2);
            string from = dt1.ToString("yyyy-MM-dd HH:mm:00");
            string to = dt2.ToString("yyyy-MM-dd HH:mm:00");
            int rows = table_count(server, user, pass);
            string mhz = find_band();

            if (rows > 0)
            {
                find_selected(from, to, mhz, rows);

            }

            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
        }

        private bool databaseError = false;
        private bool find_selected(string datetime1, string datetime2, string mhz, int tablecount) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rpt";

            string and = "";
            string bandstr = "";
            string q = "";
            if (mhz != "")
            {
                bandstr = " frequency LIKE '" + mhz + "%' ";
                and = "AND";
            }

            string callstr = "";
            string fromstr = "";
            string dstr = "";
            string tostr = "";


            //command.Parameters.AddWithValue("@datetime", DX.datetime);

            if (!databaseError)
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);
                try
                {
                   

                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    if (callFiltertextBox.Text.Trim() != "")
                    {
                        callstr = and + " tx_sign LIKE '" + callFiltertextBox.Text.Trim() + "%' ";
                        and = "AND";
                    }
                    fromstr = DFromtextBox.Text.Trim();
                    tostr = DTotextBox.Text.Trim();
                    if (fromstr != "")
                    {
                        if (!kmcheckBox.Checked)
                        {
                            Double k = Convert.ToInt32(fromstr);
                            k = k * 1.609;
                            int K = (int)k; ;
                            fromstr = K.ToString();
                        }
                        fromstr = and + " distance >= " + fromstr + " ";
                        and = "AND";
                    }
                    if (tostr != "")
                    {
                        if (!kmcheckBox.Checked)
                        {
                            Double k = Convert.ToInt32(tostr);
                            k = k * 1.609;
                            int K = (int)k;
                            tostr = K.ToString();
                        }
                        tostr = and + " distance <= " + tostr + " ";
                        and = "AND";
                    }
                    //command.CommandText = "SELECT * FROM reported ORDER BY time WHERE time >= '" + time1 + "' AND time <= '" + time2 + "' AND band = '" + bandstr + "' DESC LIMIT " + maxrows;
                    if (datecheckBox.Checked)
                    {
                        command.CommandText = "SELECT * FROM received WHERE datetime >= '" + datetime1 + "' AND datetime <= '" + datetime2 + "' AND " + bandstr + callstr + fromstr + tostr + " ORDER BY datetime DESC LIMIT " + maxrows;
                    }
                    else
                    {
                        command.CommandText = "SELECT * FROM received WHERE " + bandstr + callstr + fromstr + tostr + " ORDER BY datetime DESC LIMIT " + maxrows;
                    }



                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        found = true;

                        if (i < maxrows && i < tablecount)   //only show first maxrows rows, or to length of reported table
                        {

                            DX.datetime = (DateTime)Reader["datetime"];
                            DX.band = (Int16)Reader["band"];

                            DX.tx_sign = (string)Reader["tx_sign"];
                            DX.tx_loc = (string)Reader["tx_loc"];

                            DX.frequency = (double)Reader["frequency"];
                            DX.power = (Int16)Reader["power"];
                            DX.snr = (int)Reader["snr"];
                            DX.drift = (Int16)Reader["drift"];
                            DX.distance = (int)Reader["distance"];
                            DX.azimuth = (Int16)Reader["azimuth"];
                            DX.reporter = (string)Reader["reporter"];
                            DX.reporter_loc = (string)Reader["reporter_loc"];
                            DX.dt = (float)Reader["dt"];

                            cells[0] = DX.datetime.ToString("yyyy-MM-dd HH:mm");
                            cells[1] = DX.tx_sign;
                            double f = Convert.ToDouble(DX.frequency);
                            //f = f / 1000000;
                            string formattedF = f.ToString("F6");
                            cells[2] = formattedF;
                            if (DX.tx_sign == "nil rcvd")
                            {
                                cells[3] = "";
                                cells[4] = "";
                                cells[5] = "";
                                cells[6] = "";
                                cells[7] = "";
                                cells[8] = "";
                                cells[9] = "";
                            }
                            else
                            {
                                string snr = Convert.ToString(DX.snr);
                                if (DX.snr > 0)
                                {
                                    snr = "+" + snr;
                                }
                                cells[3] = snr;  //snr
                                                 //cells[3] = DX.snr.ToString();
                                cells[4] = DX.drift.ToString();
                                cells[5] = DX.power.ToString();
                                cells[6] = DX.tx_loc;
                                if (DX.distance > -1)
                                {
                                    cells[7] = DX.distance.ToString();
                                    string mls = convert_to_miles(DX.distance);
                                    cells[8] = mls;
                                }
                                else
                                {
                                    cells[7] = "";
                                    cells[8] = "";
                                }

                                if (DX.azimuth > -1)
                                {
                                    cells[9] = DX.azimuth.ToString();
                                }
                                else
                                {
                                    cells[9] = "";
                                }

                            }
                            update_grid(); //add this row to the datagridview
                            i++;
                        }
                        else
                        {
                            break;
                        }

                    }
                    Reader.Close();
                    connection.Close();
                    databaseError = false;

                }
                catch
                {

                    //databaseError = true; //stop wasting time trying to connect if database error - ignore for present
                    found = false;
                    connection.Close();

                }
            }
            return found;
        }
        private string find_band()
        {
            int s = bandlistBox.SelectedIndex;
            string b = "";
            switch (s)
            {
                case -1:
                    b = ""; //all
                    break;
                case 0:
                    b = ""; //all
                    break;
                case 1:
                    b = "0.136";
                    break;
                case 2:
                    b = "0.47";
                    break;
                case 3:
                    b = "1.8";
                    break;
                case 4:
                    b = "3.5";
                    break;
                case 5:
                    b = "5.";
                    break;
                case 6:
                    b = "7.";
                    break;
                case 7:
                    b = "10";
                    break;
                case 8:
                    b = "14";
                    break;
                case 9:
                    b = "18";
                    break;
                case 10:
                    b = "21";
                    break;
                case 11:
                    b = "24";
                    break;
                case 12:
                    b = "28";
                    break;
                case 13:
                    b = "50";
                    break;
                case 14:
                    b = "70";
                    break;
                case 15:
                    b = "144";
                    break;
                case 16:
                    b = "432";
                    break;
                case 17:
                    b = "1296";
                    break;
                default:
                    b = "";
                    break;
            }
            return b;
        }
        private double get_band(int bandno)
        {
            double b = -2;
            switch (bandno)
            {
                case 0:
                    b = -2; //all
                    break;
                case 1:
                    b = -1; //lf
                    break;
                case 2:
                    b = 0;  //mf
                    break;
                case 3:
                    b = 1;  //1.8
                    break;
                case 4:
                    b = 3;
                    break;
                case 5:
                    b = 5;
                    break;
                case 6:
                    b = 7;
                    break;
                case 7:
                    b = 10;
                    break;
                case 8:
                    b = 14;
                    break;
                case 9:
                    b = 18;
                    break;
                case 10:
                    b = 21;
                    break;
                case 11:
                    b = 24;
                    break;
                case 12:
                    b = 28;
                    break;
                case 13:
                    b = 50;
                    break;
                case 14:
                    b = 70;
                    break;
                case 15:
                    b = 144;
                    break;
                case 16:
                    b = 432;
                    break;
                case 17:
                    b = 1296;
                    break;
                default:
                    b = -2; //all
                    break;
            }
            return b;
        }

        private void dateTimePicker1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void DFromtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            int maxDistance = 20000; //35km short of max
            string maxstr = "20000";
            if (!kmcheckBox.Checked)
            {
                maxstr = convert_to_miles(maxDistance);
            }
            int maxD = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will allow offset from -10 to +210 for adjustment
            {
                e.Handled = true;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsSymbol("-", e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(DFromtextBox.Text + e.KeyChar, out t);
                if (t < 0 || t > maxD)
                {
                    MessageBox.Show("Error: range 0-" + maxD, "");
                    e.Handled = true;
                }
            }
        }

        private void DTotextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            int maxDistance = 20035; //half circumference of earth
            string maxstr = "20035";
            if (!kmcheckBox.Checked)
            {
                maxstr = convert_to_miles(maxDistance);
            }
            int maxD = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(DTotextBox.Text + e.KeyChar, out t);
                if (t < 0 || t > maxD)
                {
                    MessageBox.Show("Error: range 10-" + maxD, "");
                    e.Handled = true;
                }
            }
        }

        private void kmcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!kmcheckBox.Checked)
            {
                Ulabel.Text = "mls";
                Dlabel.Text = "0 - 12,453";
            }
            else
            {
                Ulabel.Text = "km";
                Dlabel.Text = "0 - 20,035";
            }
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && allowDeletecheckBox.Checked)
            {
                if (dataGridView1.Rows.Count < 1)
                {
                    return;
                }
                else
                {
                    deletethisRow(true);
                }
            }
        }

        private void deletethisRow(bool rightbutton)
        {

            string datetime;
            string call;
            string freq;
            int i = 0;
            int row = 0;
            string msgT = "1";
            for (i = 0; i < dataGridView1.Rows.Count; i++)
            {
                try
                {
                    if (dataGridView1.Rows[i].Selected)
                    {
                        datetime = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                        call = Convert.ToString(dataGridView1.Rows[i].Cells[1].Value);
                        freq = Convert.ToString(dataGridView1.Rows[i].Cells[2].Value);

                        row = i; break;
                    }
                }
                catch { }
            }
            DialogResult res;
            if (rightbutton)
            {
                res = Msg.ynMessageBox("Delete this row (Y/N)?", "Delete Slot");
                try
                {
                    if (res == DialogResult.Yes)
                    {

                        DeleteRow(row);
                        DeleteGridRow(row);
                    }

                }
                catch
                {
                    Msg.TMessageBox("Error deleting row", "", 1000);
                }
            }
        }

        private void DeleteGridRow(int row)
        {
            try
            {
                dataGridView1.Rows.RemoveAt(row);
                //for (int col = 0; col < dataGridView1.Rows[row].Cells.Count; col++)
                //{
                //    dataGridView1.Rows[row].Cells[col].Value = "";
                //}
            }
            catch
            {

            }
        }
        private void DeleteRow(int row)
        {
            string c = "";

            string datetime = dataGridView1.Rows[row].Cells[0].Value.ToString();
            string call = dataGridView1.Rows[row].Cells[1].Value.ToString();
            string freq = dataGridView1.Rows[row].Cells[2].Value.ToString();

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rpt";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();

                c = "DELETE FROM received WHERE received.datetime = '" + datetime + "' AND received.tx_sign = '" + call + "' AND received.frequency = " + freq;


                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();


            }
            catch
            {
                Msg.OKMessageBox("Error deleting row", "");
                connection.Close();
            }
        }

        private void datecheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RXofflabel_Click(object sender, EventArgs e)
        {

        }

        private void stopRXcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!stopRXcheckBox.Checked)
            {
                RXofflabel.Text = "RX running";
            }
            else
            {
                RXofflabel.Text = "RX STOPPED";
            }
        }

        private void callFiltertextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void callFiltertextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_*".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
        public void updateLabels(string loc)
        {
            my_loc = loc;
            myloclabel.Text = my_loc;
        }

      
    }
}

