using Arduino;
using FSK;
//using Google.Protobuf.WellKnownTypes;
using Logging;

using Maidenhead;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using MySql.Data.MySqlClient;
using NAudio.Wave;
using Newtonsoft.Json.Linq;

using Security;
using System;
using System.Collections.Generic;

using System.Data;
using System.Diagnostics;

using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;

using System.Runtime.InteropServices;

using System.Text;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using W410A;

using WsprSharp;


//solar data source:
//https://services.swpc.noaa.gov/text/daily-geomagnetic-indices.txt
//https://www.swpc.noaa.gov/products/station-k-and-indices

namespace WSPR_Sked
{
    public partial class Form1 : Form
    {


        string serverName = "127.0.0.1";
        DateTime selectedDate = new DateTime();
        DataTable dtable = new DataTable();
        //string selectedTime;

        int configID = 0; //primary key of config settings database table
        string callsign = "G4GCI"; //full caLLSIGN


        string base_call = "G4GCI"; //base callsign
        string location = "IO90";
        string full_location = "IO90";

        int msgType = 1;
        int currentMsgType = 1;
        int slotNo = 1; //type 1 or 2 slot no.

        int greyoffset = 0;
        int rpt_type = 0;


        string TXFrequency = "140956";
        string TXAntenna = "Windom";
        string startF = "";
        string oldAnt;

        double defaultF = 14.0956;
        int defaultoffset = 0;
        int defaultdB = 0;
        double defaultW = 0;
        string defaultAnt = "Windom";

        string ant = "dipole";

        string exeDir = "";

        int azi = 0;
        string FList = "";
        double defaultAlpha = 0.1;
        int defaultAudio = 1;
        float Volume = 1.0f;


        DateTime dt = new DateTime();
        string RowText = "";
        string tick = "   \u221A";
        string cross = "   x";
        int EditRow = 0;
        string dateformat = "yyyy-MM-dd";
        string[] cells = new string[16];

        bool prepDone = false;

        string Results;
        bool stopUrl = false;

        bool stopSolar = false;

        bool solarStarted = false;
        bool stopRX = false;

        int sunriseoffset = 0;
        int sunsetoffset = 0;
        string timezone = "UTC";
        int utcOffset = 0;

        bool DBdown = false;

        bool repeatStatus = false;


        DateTime currentSelectedDate;

        bool noRigctld;
        public struct SlotData
        {
            public string Date;
            public string Time;
            public double Freq;
            public int Offset;
            public int PowerdB;
            public double PowerW;
            public string Ant;
            public int Tuner;
            public int Switch;
            public string Endslot;
            public string Active;
            public string Rpt;
            public string EndTime;
            public string RptTime;
            public int SlotNo;
            public string Parent;
            public int SwPort;
            public int MessageType;


        }


        SlotData Slot = new SlotData();
        SlotData SlotRow = new SlotData();

        public struct SunTimes
        {
            public DateTime R;
            public DateTime S;
        }
        SunTimes sunTimes = new SunTimes();

        string CurrTime = "";

        string date = "";
        string time = "";
        int maxcol = 16;
        List<string> parents = new List<string>();

        byte[] wsprLevels;
        bool showmsg = true; //show an error message for database access
        static bool databaseError = false;
        bool justLoaded = true; //first time loaded app
        bool slotActive = false;

        string RigctlCOM = "";
        string Rigctlbaud = "";
        string RigctlPort = "4532";
        string RigctlIPv4 = "127.0.0.1";


        string Radio = "";
        bool flatcode = false; //if testing with single tone
        bool stopPlay = false;
        bool blockTXonErr = false;
        string HamlibPath = "C:\\WSPR_Sked"; //path to rigctl directory
        bool Flag = false;
        bool recordFlag = false;

        int countdown = 0;
        int prevDayofYear;
        int startCount = 0;
        int startCountMax = 360; //<5 mins

        int OpSystem = 0; //Windows = 0, Linux = 1, MacOS = 2
        string slash = "\\"; //default to Windows
        string root = "C:\\"; //default to Windows root
        public struct Hardware
        {
            public int Id;
            public string Name;
            public string Protocol;
            public string Port;
            public string IP;
            public string Baud;
            public string Serial;
            public string Type;
            public int Channels;

        }
        List<Hardware> HW = new List<Hardware>();
        List<Hardware> SW = new List<Hardware>();
        List<Hardware> TU = new List<Hardware>();

        public struct SwitchType
        {
            public string TypeName;
            public int Channels;
        }

        List<SwitchType> CH = new List<SwitchType>(); //channels on each switch

        string db_pass = "wspr";
        string db_user = "admin";

        public struct Antenna
        {
            public int AntNo;
            public string AntName;
            public string Description;
            public int Switch;
            public int Tuner;
            public int SwitchPort;

        }

        List<Antenna> Ant = new List<Antenna>();
        //LiveForm liveForm = new LiveForm();
        RXForm rxForm = new RXForm();

        //Solar solarForm = new Solar();


        int keypresses = 0;


        private static readonly object _lock = new object();

        int random = 0;

        bool slotFound = false;

        string audioOutName;
        string audioInName;
        int audioOutDevice;
        int audioInDevice;
        int inLevel = 1;
        int outLevel = 1;



        string wsprdfilepath = "C:\\WSPR_Sked";

        string userdir = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

        Random randno = new Random();

        MessageClass Msg = new MessageClass();
        public Form1()
        {
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            this.Shown += Form1_Shown;
            InitializeComponent();

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // if a key has been presed reset count
            keypresses = 0;

        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

            databaseError = false;
            DateTime prevDate = selectedDate;
            selectedDate = monthCalendar1.SelectionStart;
            if (prevDate.DayOfYear != selectedDate.DayOfYear)
            {
                changeDateTimes(timelistBox.Text, selectedDate.ToString(dateformat), true);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string ver = "0.1.16";
            this.Text = "WSPR Scheduler                       V." + ver + "    GNU GPLv3 License";
            dateformat = "yyyy-MM-dd";
            OpSystem = 0; //default to Windows
            slash = "\\"; //default to Windows
            root = "C:\\";
            RigCtlPathtextBox.Text = HamlibPath;

            getUserandPassword();
            if (checkSlotDB())
            {
                DateTime localTime = DateTime.Now; // your current local time
                DateTime utcTime = localTime.ToUniversalTime();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    OpSystem = 0; //Windows
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    OpSystem = 1; //Linux
                    slash = "/"; //Linux uses forward slash
                    root = "/"; //Linux root
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    OpSystem = 2; //MacOS
                    slash = "/"; //MacOS uses forward slash
                    root = "/"; //MacOS root
                }
                else if (OperatingSystem.IsAndroid())
                {
                    OpSystem = 3; //Android
                    slash = "/"; //Android uses forward slash
                    root = "/"; //Android root
                }

                wsprdfilepath = root + "WSPR_Sked"; //default path to WSPR_Sked folder

                baseCalltextBox.Text = "";
                defaultF = 14.0956;
                defaultOfftextBox.Text = Convert.ToString(defaultoffset);
                defaultpwrcomboBox.Text = Convert.ToString(defaultdB);
                CalltextBox.Text = callsign;
                LocatortextBox.Text = location;




                GridHeading();
                dataGridView1.DataSource = dtable;
                setColumnWidth();

                //selectedTime = dt.Date; //.ToString("hh:mm");


                daytimer.Enabled = true;
                daytimer.Start();



                ReadAntennas();
                ReadFrequencies();


                ReadHardware("switches");
                ReadHardware("tuners");
                ReadConfig();
                TXRXAntlabel.Text = defaultAnt;
                TXRXAntlabel2.Text = defaultAnt;


                DateTime dt;

                if (LTcheckBox.Checked)
                {
                    dt = DateTime.Now;

                }
                else
                {
                    dt = DateTime.Now.ToUniversalTime();

                }
                selectedDate = dt.Date;
                //monthCalendar1.TodayDate = dt;
                monthCalendar1.SetDate(dt);
                string selTime = dt.Hour.ToString().PadLeft(2, '0');
                showmsg = true;
                Datelabel.Text = dt.ToString("dddd-dd-MMM-yyyy");

                //changeDateTimes(selTime, dt.ToString(dateformat), true);
                getComports();
                readRigctl();
                if (!noRigctld)
                {
                    getRigList();


                    runRigCtlD(); //strt rig ctld
                }
                else
                {
                    rigrunlabel.Text = "rigctld not running";
                    Riglabel.Text = "rigctld not running";
                    Riglabel1.Text = "rigctld not running";
                }

                TXrunbutton.BackColor = Color.Olive;
                TXrunbutton2.BackColor = Color.Olive;

                string process = "rigctld";

                if (Process.GetProcessesByName(process).Length > 0 && !noRigctld)
                {
                    rigrunlabel.Text = "rigctld running";
                    Riglabel.Text = "rigctld not running";
                    Riglabel1.Text = rigrunlabel.Text;
                }
                set410Mode();
                currHour(false, false);
                trackSlotscheckBox.Checked = true;
                MonitorMic();
                gain = trackBarGain.Value / 100f;
                rxForm.gain = gain;
                gainlabel.Text = gain.ToString();

                findSound(); //populate sound listboxes with audio in/out devices
                Read_Audio();

                startCount = 0;
                rxForm.set_header(baseCalltextBox.Text.Trim(), serverName, db_user, db_pass, full_location, audioInDevice, wsprdfilepath, ver, OpSystem);
                if (!noRigctld) { getRigF(); }

                //rxForm.set_frequency(defaultF.ToString("F6"));
                if (!stopRX) { rxForm.Show(); }
                else
                {
                    Msg.TMessageBox("RX Disabled - enable in TX Config settings", "RX Disabled", 4000);
                }

                startCount = startCountMax - 60;

                random = randno.Next(0, 7); //random number to spread uploads to wsprnet


                utcOffset = getUTCoffset(DateTime.Now);

                LatLng ll;
                DateTime r;
                DateTime s;
                string loc = LocatortextBox.Text.Trim().Substring(0, 4);
                ll = MaidenheadLocator.LocatorToLatLng(loc);

                Sunrise_Sunset(ll.Lat, ll.Long, dt);

                if (sunTimes.R == DateTime.MinValue)
                {
                    var sunT = find_sunrise_sunset(loc, dt.ToString("MM-dd"));
                    if (sunT == null || sunT.Result.R == DateTime.MinValue)
                    {
                        utcriselabel.Text = "N/A";
                        utcsetlabel.Text = "N/A";
                    }
                    else
                    {   //change LT to UTC
                        try
                        {
                            r = sunT.Result.R;
                            s = sunT.Result.S;
                            localriselabel.Text = r.ToString("HH:mm");
                            localsetlabel.Text = s.ToString("HH:mm");
                            s = s.AddHours(utcOffset * -1);
                            r = r.AddHours(utcOffset * -1);
                            utcriselabel.Text = r.ToString("HH:mm");
                            utcsetlabel.Text = s.ToString("HH:mm");
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    r = sunTimes.R;
                    s = sunTimes.S;

                    localriselabel.Text = r.ToString("HH:mm");
                    localsetlabel.Text = s.ToString("HH:mm");
                    r = r.AddHours(utcOffset * -1);
                    s = s.AddHours(utcOffset * -1);
                    utcriselabel.Text = r.ToString("HH:mm");
                    utcsetlabel.Text = s.ToString("HH:mm");
                }

                if (grey_table_count() < 365)
                {
                    Msg.TMessageBox("No sunrise/sunset times - building table - please wait", "Sunrise/sunset Times", 4000);
                    updateSunriseSunset();
                }
            } //if checkslotDB
            else
            {
                Msg.TMessageBox("Unable to contact database", "", 2000);
                daytimer.Stop();
                daytimer.Enabled = false;

            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            if (!checkSlotDB())
            {
                this.Hide();
                //Msg.TMessageBox("Unable to connect to mySQL", "Check mySQL", 4000);
                LoadError loadError = new LoadError();
                loadError.Show();
            }

        }



        private void startSolar()
        {
            Process.Start(@"C:\WSPR_Sked\WSPR_SolarEXE\WSPR_Solar.exe");

        }

        string FindAppPath(string appName)
        {
            string query = "SELECT * FROM Win32_Product WHERE Name LIKE '%" + appName + "%'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["InstallLocation"]?.ToString();
                }
            }
            return null;
        }

        private bool checkSlotDB()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";

            MySqlConnection connection = new MySqlConnection(myConnectionString);


            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch
            {

                connection.Close();
                return false;
            }
        }


        //private  async Task<bool> findSlot(int slot, string date, string time) //find a slot in the database corresponding to the date/time from the slot to transmit/receive
        private bool findSlot(int slot, string date, string time)
        {
            DataTable Slots = new DataTable();

            bool slotFound = false;
            int readcount = 0;
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";

            while (readcount < 2)
            {

                MySqlConnection connection = new MySqlConnection(myConnectionString);


                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT * FROM slots WHERE Date = '" + date + "' AND Time = '" + time.Trim() + "'";
                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        slotFound = true;
                        double freq = (double)Reader["Frequency"];
                        freq = Math.Round(freq, 4);
                        Slot.Freq = freq;

                        TXFrequency = (freq * 1000000).ToString();

                        int offset = (int)Reader["Offset"];
                        Slot.Offset = offset;
                        int powerdB = (int)Reader["Power"];
                        Slot.PowerdB = powerdB;
                        double powerW = (double)Reader["PowerW"];
                        Slot.PowerW = powerW;
                        ant = (string)Reader["Antenna"];
                        Slot.Ant = ant;
                        int tuner = (int)Reader["Tuner"];
                        Slot.Tuner = tuner;
                        int swi = (int)Reader["Switch"];
                        Slot.Switch = swi;
                        int swPort = (int)Reader["SwitchPort"];
                        Slot.SwPort = swPort;
                        TXAntenna = ant;
                        if (ant != null)
                        {
                            AntselcomboBox.Text = ant;

                        }
                        //TXRXAntlabel.Text = ant;
                        if (tuner > -1) { selTunertextBox.Text = tuner.ToString(); }
                        if (swi > -1) { selSwitchtextBox.Text = swi.ToString(); }
                        if (swPort > -1) { selSwPorttextBox.Text = swPort.ToString(); }
                        string endslot;
                        try
                        {
                            dt = (DateTime)Reader["End"];
                            endslot = dt.ToString("yyyy-MM-dd");
                        }
                        catch
                        {
                            endslot = "2024-01-01";
                        }
                        Slot.Endslot = endslot;
                        bool a = false;
                        a = (bool)Reader["Active"];
                        string active;
                        if (a)
                        {
                            active = tick;
                            slotActive = true;
                        }
                        else
                        {
                            active = cross;
                            slotActive = false;
                        }
                        Slot.Active = active;

                        a = false;
                        a = (bool)Reader["Repeating"];
                        string rpt;
                        if (a) { rpt = tick; } else { rpt = cross; }
                        Slot.Rpt = rpt;

                        TimeSpan t = (TimeSpan)Reader["TimeEnd"];
                        string endTime;
                        endTime = t.ToString(@"hh\:mm");
                        Slot.EndTime = endTime;

                        a = (bool)Reader["RptTime"];
                        if (a)
                        {
                            Slot.RptTime = "1";
                            repeatTimecheckBox.Checked = true;
                        }
                        else
                        {
                            Slot.RptTime = "0";
                            repeatTimecheckBox.Checked = false;
                        }

                        string prt = "";
                        try
                        { prt = (string)Reader["Parent"]; }
                        catch { prt = ""; }
                        if (slot > -1)
                        {
                            parents[slot] = prt;
                        }
                        Slot.Parent = prt;

                        slotNo = (int)Reader["SlotNo"];
                        Slot.SlotNo = slotNo;
                        currentMsgType = (int)Reader["MsgType"];
                        Slot.MessageType = currentMsgType;
                        msgType = currentMsgType;
                        if (slotActive && enableTXcheckBox.Checked)
                        {
                            if (Type2checkBox.Checked && overcheckBox.Checked)
                            {
                                msgType = 3;
                            }
                            else if (overcheckBox.Checked)
                            {
                                msgType = 1;
                            }
                        }

                        rpt_type = (sbyte)Reader["RptType"];
                        findRptType(rpt_type);
                        greyoffset = (sbyte)Reader["GreyOffset"];
                        if (greyoffset != null)
                        {
                            greylistBox.Text = greyoffset.ToString();
                        }
                        else
                        {
                            greylistBox.Text = "1";
                        }

                    }
                    readcount = 2;
                    Reader.Close();
                    connection.Close();

                    if (DBdown)
                    {
                        currHour(false, true);
                    }
                    dblabel.Text = "Slot Database OK";
                    DBdown = false;
                    dblabel.BackColor = Color.LightBlue;

                }
                catch
                {
                    dblabel.Text = "Slot Database Error";
                    DBdown = true;
                    dblabel.BackColor = Color.Pink;
                    if (showmsg)
                    {
                        Msg.TMessageBox("Unable to read from database", "", 1000);
                        showmsg = false;


                    }
                    //databaseError = true; //stop wasting time trying to connect if database error
                    slotFound = false;
                    connection.Close();

                    //}
                }
                readcount++;
            }
            return slotFound;
        }

        private void findRptType(int type)
        {
            switch (type)
            {
                case 0:
                    {
                        repeatcheckBox.Checked = false;
                        break;
                    }
                case 1:
                    {
                        repeatTimecheckBox.Checked = true;
                        break;
                    }
                case 2:
                    {
                        DaycheckBox.Checked = true;
                        break;
                    }
                case 3:
                    {
                        NightcheckBox.Checked = true;
                        break;
                    }
                case 4:
                    {
                        AllcheckBox.Checked = true;
                        break;
                    }
                default:
                    repeatcheckBox.Checked = false;
                    break;
            }

        }

        private bool findSlotRow(int slot, string date, string time) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();

            bool found = false;
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";

            if (!databaseError)
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);


                try
                {

                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT * FROM slots WHERE Date = '" + date + "' AND Time = '" + time.Trim() + "'";
                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        found = true;
                        double freq = (double)Reader["Frequency"];
                        freq = Math.Round(freq, 4);
                        SlotRow.Freq = freq;


                        int offset = (int)Reader["Offset"];
                        SlotRow.Offset = offset;
                        int powerdB = (int)Reader["Power"];
                        SlotRow.PowerdB = powerdB;
                        double powerW = (double)Reader["PowerW"];
                        SlotRow.PowerW = powerW;
                        ant = (string)Reader["Antenna"];
                        SlotRow.Ant = ant;
                        int tuner = (int)Reader["Tuner"];
                        SlotRow.Tuner = tuner;
                        int swi = (int)Reader["Switch"];
                        SlotRow.Switch = swi;
                        int swPort = (int)Reader["SwitchPort"];
                        SlotRow.SwPort = swPort;
                        /*TXAntenna = ant;
                        if (ant != null)
                        {
                            AntselcomboBox.Text = ant;

                        }*/
                        //TXRXAntlabel.Text = ant;
                        //if (tuner > -1) { selTunertextBox.Text = tuner.ToString(); }
                        //if (swi > -1) { selSwitchtextBox.Text = swi.ToString(); }
                        //if (swPort > -1) { selSwPorttextBox.Text = swPort.ToString(); }
                        string endslot;
                        try
                        {
                            dt = (DateTime)Reader["End"];
                            endslot = dt.ToString("yyyy-MM-dd");
                        }
                        catch
                        {
                            endslot = "2024-01-01";
                        }
                        SlotRow.Endslot = endslot;
                        bool a = false;
                        a = (bool)Reader["Active"];
                        string active;
                        if (a)
                        {
                            active = tick;

                        }
                        else
                        {
                            active = cross;

                        }
                        SlotRow.Active = active;

                        a = false;
                        a = (bool)Reader["Repeating"];
                        string rpt;
                        if (a) { rpt = tick; } else { rpt = cross; }
                        SlotRow.Rpt = rpt;

                        TimeSpan t = (TimeSpan)Reader["TimeEnd"];
                        string endTime;
                        endTime = t.ToString(@"hh\:mm");
                        SlotRow.EndTime = endTime;

                        a = (bool)Reader["RptTime"];
                        if (a)
                        {
                            SlotRow.RptTime = "1";
                            //repeatTimecheckBox.Checked = true;
                        }
                        else
                        {
                            SlotRow.RptTime = "0";
                            //repeatTimecheckBox.Checked = false;
                        }

                        string prt = "";
                        try
                        { prt = (string)Reader["Parent"]; }
                        catch { prt = ""; }
                        if (slot > -1)
                        {
                            parents[slot] = prt;
                        }
                        SlotRow.Parent = prt;

                        slotNo = (int)Reader["SlotNo"];
                        SlotRow.SlotNo = slotNo;
                        int msgT = (int)Reader["MsgType"];
                        SlotRow.MessageType = msgT;

                        rpt_type = (sbyte)Reader["RptType"];
                        findRptType(rpt_type);
                        greyoffset = (sbyte)Reader["GreyOffset"];
                        if (greyoffset != null)
                        {
                            greylistBox.Text = greyoffset.ToString();
                        }
                        else
                        {
                            greylistBox.Text = "1";
                        }


                    }
                    Reader.Close();
                    connection.Close();
                    dblabel.Text = "Slot Database OK";
                    dblabel.BackColor = Color.LightBlue;

                }
                catch
                {
                    dblabel.Text = "Slot Database Error";
                    dblabel.BackColor = Color.Pink;

                    if (showmsg)
                    {
                        Msg.TMessageBox("Unable to read from database", "", 1000);
                        showmsg = false;

                    }
                    databaseError = true; //stop wasting time trying to connect if database error
                    found = false;
                    connection.Close();
                }
            }
            return found;
        }



        private void GridHeading()
        {

            dtable.Columns.Add("Date   "); //0
            dtable.Columns.Add("Time"); //1
            dtable.Columns.Add("Frequency"); //2
            dtable.Columns.Add("Offset"); //3
            dtable.Columns.Add("Pwr dBm"); //4
            dtable.Columns.Add("Pwr W"); //5
            dtable.Columns.Add("Antenna");  //6
            dtable.Columns.Add("Tuner"); //7
            dtable.Columns.Add("Switch");  //8
            // dtable.Columns.Add("Rotator");
            //dtable.Columns.Add("Azimuth");          
            dtable.Columns.Add("End     ");  //9
            dtable.Columns.Add("Rpt");  //10
            dtable.Columns.Add("Active");  //11
            dtable.Columns.Add("Endtime"); //12
            dtable.Columns.Add("rptT"); //13
            dtable.Columns.Add("Slot"); //14
            dtable.Columns.Add("Type"); //15 msg type
            dataGridView1.Columns.Clear();  //delete existing cells in datagrid view - to replace them with dtable
        }

        private void setColumnWidth()
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None; //set all columns to fixed width

            }
            dataGridView1.Columns[0].Width = 80; //date
            dataGridView1.Columns[1].Width = 50; //time
            dataGridView1.Columns[2].Width = 70; //frequency
            dataGridView1.Columns[3].Width = 40; //offset
            dataGridView1.Columns[4].Width = 56; //power dBm
            dataGridView1.Columns[5].Width = 56; //power W
            dataGridView1.Columns[6].Width = 120; //antenna
            dataGridView1.Columns[7].Width = 50; //tuner
            dataGridView1.Columns[8].Width = 50; //switch
            //dataGridView1.Columns[9].Width = 60; //rotator
            //dataGridView1.Columns[10].Width = 60; //azimuth
            dataGridView1.Columns[9].Width = 80; //end date
            dataGridView1.Columns[10].Width = 44; //repeat
            dataGridView1.Columns[11].Width = 44; //active
            dataGridView1.Columns[12].Width = 50; //end time
            dataGridView1.Columns[13].Width = 45; //repeat time
            dataGridView1.Columns[14].Width = 44; //slot no
            dataGridView1.Columns[15].Width = 44; //message type
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[10].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[11].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[14].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[15].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


        }

        private void deleteNextslot(int slot)
        {
            DialogResult res = Msg.ynMessageBox("Delete all repeating slots too (Y/N)?", "Delete Repeat Slots");
            if (res == DialogResult.Yes)
            {

                DeleteSlot(true, slot, Slot.Parent);  //check where can get PARENT from in current row (its not in the current gridview)
            }
            else
            {
                DeleteSlot(false, slot, Slot.Parent);
            }
            DeleteRow(slot);
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            greygroupBox.Visible = false;
            for (int s = 0; s < dataGridView1.Rows.Count; s++)
            {
                if (dataGridView1.Rows[s].Selected)
                {
                    date = Convert.ToString(dataGridView1.Rows[s].Cells[0].Value);

                }

            }

            DateTime d;
            bool isValid = DateTime.TryParse(date, out d);

            DateTime td;
            if (!LTcheckBox.Checked)
            {
                td = DateTime.Now.ToUniversalTime();
            }
            else
            { td = DateTime.Now; }

            if (d.DayOfYear < td.DayOfYear)
            {
                Msg.OKMessageBox("Note: Editing date before today?", "");
            }

            EditSlot();

        }
        private void EditSlot() //open groupbox to allow slot to be edited or created
        {

            slotgroupBox.Visible = true;


            int rows = dataGridView1.Rows.Count;

            for (int i = 0; i < maxcol; i++)
            {
                cells[i] = "";
            }
            try
            {
                for (int i = 0; i < rows; i++)
                {
                    if (dataGridView1.Rows[i].Selected)
                    {
                        EditRow = i;

                        for (int c = 0; c < dataGridView1.Rows[i].Cells.Count; c++)
                        {
                            if (c == 0)
                            {
                                date = Convert.ToString(dataGridView1.Rows[i].Cells[c].Value);
                                cells[0] = date;

                            }
                            else if (c == 1)
                            {
                                time = Convert.ToString(dataGridView1.Rows[i].Cells[c].Value);
                                cells[1] = time;
                            }
                            {
                                cells[c] = Convert.ToString(dataGridView1.Rows[i].Cells[c].Value);
                            }
                        }
                    }
                }
                datetimelabel.Text = date + "  " + time;
                try
                {
                    if (cells[2].Trim() == "")
                    {
                        editslotcheckBox.Checked = false; //creating new
                        FreqcomboBox.SelectedItem = defaultF.ToString();
                        OffsettextBox.Text = defaultoffset.ToString();
                        string DB = defaultdB.ToString();
                        dBmcomboBox.SelectedItem = DB;
                        PowertextBox.Text = dBtoWatts(DB);
                        AntselcomboBox.SelectedItem = defaultAnt;
                        int a = AntselcomboBox.FindString(defaultAnt);

                        if (a > -1)
                        {
                            int s = Ant[a].Switch;
                            if (s > -1)
                            {

                                selSwitchtextBox.Text = s.ToString();
                            }
                            int p = Ant[a].SwitchPort;
                            if (p > -1)
                            {
                                p++;
                                selSwPorttextBox.Text = p.ToString(); //show index 0 as 1 as ports start from 1
                            }
                        }

                    }
                    else
                    {

                        editslotcheckBox.Checked = true;  //editing existing
                        FreqcomboBox.SelectedItem = cells[2];
                        OffsettextBox.Text = cells[3];
                        dBmcomboBox.SelectedItem = cells[4];
                        PowertextBox.Text = cells[5];
                        AntselcomboBox.SelectedItem = cells[6];
                        selTunertextBox.Text = cells[7];
                        selSwitchtextBox.Text = cells[8];
                        int a = AntselcomboBox.SelectedIndex;
                        if (a > -1)
                        {
                            int s = Ant[a].Switch;
                            if (s > -1)
                            {

                                selSwitchtextBox.Text = s.ToString();
                            }
                            int p = Ant[a].SwitchPort;
                            if (p > -1)
                            {
                                p++;
                                selSwPorttextBox.Text = p.ToString(); //show index 0 as 1 as ports start from 1
                            }
                        }
                        if (cells[15] == "2")
                        {
                            msgTlabel.Text = "Message type 2";
                        }
                        else
                        {
                            msgTlabel.Text = "Message type 1";
                        }

                    }
                }
                catch
                {

                }                //cells[9] = ""; //rotator
                //cells[10] = ""; //azimuth
                dateEnd.Format = DateTimePickerFormat.Custom;
                dateEnd.CustomFormat = "yyyy-MM-dd";
                try
                {
                    dateEnd.Value = Convert.ToDateTime(cells[9]);
                }
                catch { }
                timeEnd.Format = DateTimePickerFormat.Custom;
                timeEnd.CustomFormat = "HH:mm";
                try
                {
                    timeEnd.Value = Convert.ToDateTime(cells[12]);
                }
                catch
                {

                }
                if (cells[11].Contains("x") || cells[11] == "")
                {
                    ActivecheckBox.Checked = false;
                }
                else
                {
                    ActivecheckBox.Checked = true;
                }
                if (cells[10].Contains("x") || cells[10] == "")
                {
                    repeatcheckBox.Checked = false;
                }
                else
                {
                    repeatcheckBox.Checked = true;
                }
                if (cells[13].Contains("1")) //if slot end time then 
                {
                    repeatTimecheckBox.Checked = true;
                    DateTime t;
                    DateTime.TryParse(cells[12], out t); //slot end time T
                    timeEnd.Value = t;

                }
                if (cells[2].Trim() == "") //new slot
                {
                    ActivecheckBox.Checked = true;
                    repeatTimecheckBox.Checked = false;
                    repeatTimecheckBox.Checked = false;
                }
                repeatStatus = repeatcheckBox.Checked;
            }
            catch
            {
                Msg.OKMessageBox("Error editing slot", "");
            }
        }



        private void CancelSlotbutton_Click(object sender, EventArgs e)
        {
            slotgroupBox.Visible = false;
            repeatStatus = false;
            this.Focus();
            this.BringToFront();
            dataGridView1.Focus();
        }

        private bool checkNextSlot(int i)
        {
            DialogResult res;
            string F = Convert.ToString(dataGridView1.Rows[i + 1].Cells[2].Value);
            if (F.Trim() != "")
            {
                Msg.OKMessageBox("Type 2 message - adjacent slot occupied", "Next slot occupied");
                res = Msg.ynMessageBox("Overwrite adjacent slot(s) (y/n)?", "");
                if (res == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            return true;
        }

        private async void SaveSlotbutton_Click(object sender, EventArgs e)
        {

            slotNo = 1;
            int msgT = 1;
            bool this_slot = false;
            bool oneTX = asOnecheckBox.Checked;

            this.Focus();
            this.BringToFront();
            try
            {
                if (validateSlot())
                {
                    //MessageForm mForm = new MessageForm();

                    if (Type2checkBox.Checked)
                    {
                        msgT = checkCall(callsign.Trim(), LocatortextBox.Text.Trim());
                    }
                    if (!ActivecheckBox.Checked)
                    {
                        msgT = 1;
                    }
                    if (msgT == 2 || msgT == 3)
                    {
                        var res = Msg.ynMessageBox("Save as message type 2/3 (Y/N)?", "Message type 2/3");
                        if (res == DialogResult.No)
                        {
                            msgT = 1;
                        }
                        else
                        {
                            msgTlabel.Text = "Message type 2";
                        }
                    }

                    if (repeatcheckBox.Checked && editslotcheckBox.Checked)
                    {
                        var res = Msg.ynMessageBox("Update all repeating slots (Y/N)?", "Repeating slots");
                        if (res == DialogResult.No)
                        {
                            this_slot = true;
                        }
                    }
                    else if (!repeatcheckBox.Checked)
                    {
                        this_slot = true;
                    }
                    else
                    {
                        this_slot = false;
                    }

                    Msg.TMessageBox("Saving (message type " + msgT + ") .. please wait", "Save slot", 20000);
                    Savelabel.Text = "Saving - please wait....";
                    repeatStatus = false;

                    if ((msgT == 2 || msgT == 3) && !asOnecheckBox.Checked && checkNextSlot(EditRow))
                    {
                        if ((DaycheckBox.Checked || NightcheckBox.Checked) && !this_slot) //don't update all if only this one
                        {
                            await SaveSlot_Sun(false, msgT, this_slot);
                            EditRow++;

                            slotNo = 2;
                            await SaveSlot_Sun(true, msgT, this_slot);
                        }
                        else
                        {

                            await SaveSlot(false, msgT, this_slot);
                            EditRow++;

                            slotNo = 2;
                            await SaveSlot(true, msgT, this_slot);
                        }

                        //EditRow--;
                    }
                    else //if (msgT != 2 || (msgT == 3 && asOnecheckBox.Checked))
                    {
                        if ((DaycheckBox.Checked || NightcheckBox.Checked) && !this_slot) //don't update all if only this one
                        {
                            await SaveSlot_Sun(false, msgT, this_slot);
                        }
                        else
                        {
                            await SaveSlot(false, msgT, this_slot);
                        }
                    }
                    Savelabel.Text = "--";
                    slotgroupBox.Visible = false;
                    greygroupBox.Visible = false;
                    //mForm.Dispose();
                }
            }
            catch { }
            this.Focus();
            this.BringToFront();
            dataGridView1.Focus();

        }
        private bool validateSlot()
        {
            if (FreqcomboBox.Text == "" || OffsettextBox.Text == "" || dBmcomboBox.Text == "")
            {
                Msg.OKMessageBox("Missing data", "");
                return false;
            }

            DateTime dt;
            DateTime date;
            bool isValid = DateTime.TryParse(datetimelabel.Text, out dt);
            string[] D = datetimelabel.Text.Split(' ');
            isValid = DateTime.TryParse(D[0], out date);

            //string date = dt.ToString(dateformat);

            string time = dt.ToString("HH:mm"); //current time

            DateTime T;
            DateTime.TryParse(time, out T);

            DateTime endD;
            DateTime endT;

            //string date1 = dt.ToString(dateformat);
            DateTime.TryParse(dateEnd.Value.ToString("yyyy-MM-dd"), out endD);
            DateTime.TryParse(timeEnd.Value.ToString("HH:mm"), out endT);

            if (!DaycheckBox.Checked && !NightcheckBox.Checked && !AllcheckBox.Checked)
            {
                if (endD < date && repeatcheckBox.Checked)
                {
                    Msg.OKMessageBox("End date must be >= slot start date", "");
                    return false;
                }
                if (endT < T && repeatTimecheckBox.Checked)
                {
                    Msg.OKMessageBox("End date must be >= slot start date", "");
                    return false;
                }
            }

            return true;
        }
        private async Task SaveSlot(bool slot2, int msgT, bool this_slot) //update the gridview
        {
            int i = EditRow;
            DataGridViewRow DataRow = dataGridView1.Rows[i];


            try
            {

                DateTime dt;
                bool isValid = DateTime.TryParse(datetimelabel.Text, out dt);

                if (slot2)
                {
                    dt = dt.AddMinutes(2);
                }
                string date1 = dt.ToString(dateformat);
                cells[0] = date1;
                Slot.Date = date1;
                string time1 = dt.ToString("HH:mm"); //current time
                cells[1] = time1;
                Slot.Time = time1;

                double f = Convert.ToDouble(FreqcomboBox.Text);
                f = Math.Round(f, 4);
                cells[2] = f.ToString();
                Slot.Freq = f;
                cells[3] = OffsettextBox.Text;
                Slot.Offset = Convert.ToInt32(OffsettextBox.Text);
                cells[4] = dBmcomboBox.Text;
                Slot.PowerdB = Convert.ToInt32(dBmcomboBox.Text);
                cells[5] = PowertextBox.Text;
                Slot.PowerW = Convert.ToInt32(PowertextBox.Text);
                if (AntselcomboBox.SelectedIndex > -1)
                {
                    cells[6] = AntselcomboBox.SelectedItem.ToString();
                }
                Slot.Ant = AntselcomboBox.SelectedItem.ToString();

                cells[7] = selTunertextBox.Text;
                Slot.Tuner = Convert.ToInt32(selTunertextBox.Text);

                cells[8] = selSwitchtextBox.Text;
                Slot.Switch = Convert.ToInt32(selSwitchtextBox.Text);


                //cells[9] = null; //rotator
                //cells[10] = null; //azimuth
                string enddate;
                string endtime;
                if (repeatcheckBox.Checked)
                {
                    enddate = dateEnd.Value.ToString(dateformat);
                }
                else
                {
                    enddate = date1;
                }
                if (repeatTimecheckBox.Checked)
                {
                    endtime = timeEnd.Value.ToString("HH:mm");
                }
                else
                {
                    endtime = time1;
                }
                Slot.EndTime = endtime;
                cells[12] = endtime;

                cells[9] = enddate;
                Slot.Endslot = enddate;
                Slot.EndTime = endtime;
                if (ActivecheckBox.Checked) { cells[11] = tick; Slot.Active = tick; }
                else { cells[11] = cross; Slot.Active = cross; }
                if (repeatcheckBox.Checked) { cells[10] = tick; Slot.Rpt = tick; }
                else { cells[10] = cross; Slot.Rpt = cross; }
                cells[13] = "0"; //repeat time?
                Slot.RptTime = "0";
                if (repeatTimecheckBox.Checked)
                {
                    cells[13] = "1"; Slot.RptTime = "1";
                }
                cells[14] = slotNo.ToString();
                Slot.SlotNo = slotNo;

                Slot.SwPort = Convert.ToInt32(selSwPorttextBox.Text);
                Slot.SwPort++; //zeroise it
                string MT = msgT.ToString();

                cells[15] = MT;

                if (locateSlotMembersDT(date1, time1, enddate, endtime, this_slot)) //if able to save data
                {
                    for (i = 0; i < maxcol; i++)
                    {
                        if (cells[i] != null)
                        {
                            DataRow.Cells[i].Value = cells[i];

                        }
                    }
                }
                string act = DataRow.Cells[11].Value.ToString();
                if (act.Contains(tick))
                {
                    dataGridView1.Rows[EditRow].Cells[11].Style.ForeColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[EditRow].Cells[11].Style.ForeColor = Color.Blue;
                }
                DataGridViewCell cell = dataGridView1.Rows[EditRow].Cells[11];
                cell.Style.Font = new System.Drawing.Font(dataGridView1.Font, FontStyle.Bold);

                dataGridView1.Columns[12].Visible = false; // Hide the repeat time column
                dataGridView1.Columns[13].Visible = false; // Hide the end time column
                //dataGridView1.Columns[15].Visible = false; // Hide the end time column


                //will need to add function to find all slots in curren set to end
            }
            catch
            {
                Msg.OKMessageBox("Error updating cells", "");
            }
        }

        private async Task SaveSlot_Sun(bool slot2, int msgT, bool this_slot) //update the gridview
        {
            int i = EditRow;
            DataGridViewRow DataRow = dataGridView1.Rows[i];


            try
            {
                DateTime rise = DateTime.MinValue;
                DateTime set = DateTime.MinValue;
                LatLng latlon;

                bool show = true;
                string loc = LocatortextBox.Text.Trim().Substring(0, 4);

                DateTime dt;
                bool isValid = DateTime.TryParse(datetimelabel.Text, out dt);

                int utcOff = getUTCoffset(dt);
                string sundate = dt.ToString("MM-dd");
                try
                {
                    var sunTimes = find_sunrise_sunset(loc, sundate);

                    rise = sunTimes.Result.R;
                    set = sunTimes.Result.S;
                    rise = rise.AddHours(utcOff * -1);   //convert to UTC                   
                    set = set.AddHours(utcOff * -1);
                }
                catch
                {
                    Msg.TMessageBox("Error: no sunrise/set times - see TX Config", "Error: no sunrise/set times", 3000);
                    return;
                }

                DateTime riseM;
                DateTime setM;
                if (DaycheckBox.Checked)
                {

                    rise = rise.AddMinutes(sunriseoffset * -1);
                    set = set.AddMinutes(sunsetoffset);
                    //riseM = rise.AddMinutes(-2);
                    //setM = set.AddMinutes(2);
                    if (dt.Hour < rise.Hour || dt.Hour > set.Hour)  //was dt.Hour and rise.Hour
                    {
                        show = false;
                    }
                }
                else if (NightcheckBox.Checked)
                {
                    rise = rise.AddMinutes(sunriseoffset);
                    //riseM = rise.AddMinutes(2);
                    set = set.AddMinutes(sunsetoffset * -1);
                    //setM = set.AddMinutes(-2);
                    if (dt.Hour >= rise.Hour && dt.Hour <= set.Hour)  //was dt.Hour and rise.Hour
                    {
                        show = false;
                    }
                }



                if (slot2)
                {
                    dt = dt.AddMinutes(2);
                }
                string date1 = dt.ToString(dateformat);
                cells[0] = date1;
                Slot.Date = date1;
                string time1 = dt.ToString("HH:mm"); //current time
                cells[1] = time1;
                Slot.Time = time1;

                double f = Convert.ToDouble(FreqcomboBox.Text);
                f = Math.Round(f, 4);
                cells[2] = f.ToString();
                Slot.Freq = f;
                cells[3] = OffsettextBox.Text;
                Slot.Offset = Convert.ToInt32(OffsettextBox.Text);
                cells[4] = dBmcomboBox.Text;
                Slot.PowerdB = Convert.ToInt32(dBmcomboBox.Text);
                cells[5] = PowertextBox.Text;
                Slot.PowerW = Convert.ToInt32(PowertextBox.Text);
                if (AntselcomboBox.SelectedIndex > -1)
                {
                    cells[6] = AntselcomboBox.SelectedItem.ToString();
                }
                Slot.Ant = AntselcomboBox.SelectedItem.ToString();

                cells[7] = selTunertextBox.Text;
                Slot.Tuner = Convert.ToInt32(selTunertextBox.Text);

                cells[8] = selSwitchtextBox.Text;
                Slot.Switch = Convert.ToInt32(selSwitchtextBox.Text);


                //cells[9] = null; //rotator
                //cells[10] = null; //azimuth
                string enddate;
                string endtime;
                if (repeatcheckBox.Checked)
                {
                    enddate = dateEnd.Value.ToString(dateformat);
                }
                else
                {
                    enddate = date1;
                }
                if (repeatTimecheckBox.Checked)
                {
                    endtime = timeEnd.Value.ToString("HH:mm");
                }
                else
                {
                    endtime = time1;
                }
                Slot.EndTime = endtime;
                cells[12] = endtime;

                cells[9] = enddate;
                Slot.Endslot = enddate;
                Slot.EndTime = endtime;
                if (ActivecheckBox.Checked) { cells[11] = tick; Slot.Active = tick; }
                else { cells[11] = cross; Slot.Active = cross; }
                if (repeatcheckBox.Checked) { cells[10] = tick; Slot.Rpt = tick; }
                else { cells[10] = cross; Slot.Rpt = cross; }
                cells[13] = "0"; //repeat time?
                Slot.RptTime = "0";
                if (repeatTimecheckBox.Checked)
                {
                    cells[13] = "1"; Slot.RptTime = "1";
                }
                cells[14] = slotNo.ToString();
                Slot.SlotNo = slotNo;

                Slot.SwPort = Convert.ToInt32(selSwPorttextBox.Text);
                Slot.SwPort++; //zeroise it
                string MT = msgT.ToString();

                cells[15] = MT;

                bool ok = await locateSlotMembersDT_Sun(date1, time1, enddate, endtime, this_slot);
                if (ok && show) //if able to save data
                {
                    for (i = 0; i < maxcol; i++)
                    {
                        if (cells[i] != null)
                        {
                            DataRow.Cells[i].Value = cells[i];

                        }
                    }
                }
                else if (!ok)
                {
                    return;
                }
                string act = DataRow.Cells[11].Value.ToString();
                if (act.Contains(tick))
                {
                    dataGridView1.Rows[EditRow].Cells[11].Style.ForeColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[EditRow].Cells[11].Style.ForeColor = Color.Blue;
                }
                DataGridViewCell cell = dataGridView1.Rows[EditRow].Cells[11];
                cell.Style.Font = new System.Drawing.Font(dataGridView1.Font, FontStyle.Bold);

                dataGridView1.Columns[12].Visible = false; // Hide the repeat time column
                dataGridView1.Columns[13].Visible = false; // Hide the end time column
                //dataGridView1.Columns[15].Visible = false; // Hide the end time column


                //will need to add function to find all slots in curren set to end
            }
            catch
            {
                Msg.OKMessageBox("Error updating cells", "");
            }
        }





        private bool locateSlotMembersDT(string date1, string time1, string enddate, string endtime, bool this_slot)
        {
            DateTime dt;
            DateTime Dend;

            DateTime endT;
            DateTime T;


            // time1 is current time
            try
            {
                bool isValid = DateTime.TryParse(date1, out dt); //dt is output of checking datatime label (current) date
                isValid = DateTime.TryParse(enddate, out Dend); //ditto but end date


                //calculate sunrise-sunset times

                bool isValidT = DateTime.TryParse(timeEnd.Value.ToString("HH:mm"), out endT); //endtime
                isValidT = DateTime.TryParse(time1, out T); //current time

                DateTime StartCount = dt.AddHours(T.Hour).AddMinutes(T.Minute);
                DateTime End = Dend.AddHours(endT.Hour).AddMinutes(endT.Minute);

                if (repeatcheckBox.Checked && repeatTimecheckBox.Checked)
                {
                    if (endT < T)
                    {
                        Msg.OKMessageBox("Error: slot end time is before slot time", "Repeating slot error");
                        return false;
                    }
                }

                if (!repeatTimecheckBox.Checked) //same mins each hour?
                {

                    endT = T;
                }
                if (this_slot) //only update this slot
                {
                    Dend = dt;
                    endT = T;
                }
                DateTime startT = T;
                if (AllcheckBox.Checked && !this_slot) //all day
                {
                    startT = DateTime.MinValue.AddHours(0).AddMinutes(T.Minute); //midnight + XX mins

                    endT = DateTime.MinValue.AddHours(23).AddMinutes(59);

                }

                TimeSpan TS = End - StartCount;


                if (dt <= Dend)
                {

                    while (dt <= Dend)
                    {
                        T = startT;
                        while (T.TimeOfDay <= endT.TimeOfDay)
                        {
                            string newdate = dt.ToString(dateformat);
                            time1 = T.ToString("HH:mm"); //update time by one hour

                            if (!SaveSlotData(newdate, time1))
                            {
                                return false;
                            }

                            T = T.AddHours(1);
                            if (T.TimeOfDay >= endT.TimeOfDay)
                            {
                                dt = dt.AddDays(1);
                                break;
                            }
                            if (T.Hour == 0)
                            {
                                dt = dt.AddDays(1);
                                break;
                            }

                        }
                    }

                }
                else
                {
                    string newdate = dt.ToString(dateformat);
                    if (!SaveSlotData(newdate, time1))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        private async Task<bool> locateSlotMembersDT_Sun(string date1, string time1, string enddate, string endtime, bool this_slot)
        {
            DateTime dt;
            DateTime Dend;

            DateTime endT;
            DateTime T;


            LatLng latlon;
            DateTime rise = DateTime.MinValue;
            DateTime set = DateTime.MinValue;

            bool night = false;
            int count = 0;
            int daycount = 0;

            try
            {
                string o = greylistBox.Text;
                sunriseoffset = Convert.ToInt32(o);
                sunsetoffset = Convert.ToInt32(o);
            }
            catch
            {
                sunriseoffset = 0;
                sunsetoffset = 0;
            }

            latlon = MaidenheadLocator.LocatorToLatLng(LocatortextBox.Text.Trim());

            // time1 is current time
            try
            {
                bool isValid = DateTime.TryParse(date1, out dt); //dt is output of checking datatime label (current) date
                isValid = DateTime.TryParse(enddate, out Dend); //ditto but end date



                bool isValidT = DateTime.TryParse(timeEnd.Value.ToString("HH:mm"), out endT); //endtime
                isValidT = DateTime.TryParse(time1, out T); //current time
                DateTime mins = T; //keep current minute

                DateTime StartCount = dt.AddHours(T.Hour).AddMinutes(T.Minute);
                DateTime End = Dend.AddHours(endT.Hour).AddMinutes(endT.Minute);


                if (DaycheckBox.Checked)
                {
                    night = false;
                }
                else if (NightcheckBox.Checked)
                {
                    night = true;
                }
                DateTime startT = DateTime.UtcNow;



                //TimeSpan TS = End - StartCount;

                if (this_slot) //only update this slot
                {
                    Dend = dt;
                }


                if (dt.Date <= Dend.Date)
                {

                    string loc = LocatortextBox.Text.Trim().Substring(0, 4);

                    isValid = DateTime.TryParse(datetimelabel.Text, out dt);

                    string sundate = dt.ToString("MM-dd");

                    int utcOff = getUTCoffset(dt);
                    try
                    {
                        var sunTimes = find_sunrise_sunset(loc, sundate);

                        rise = sunTimes.Result.R;
                        rise = rise.AddHours(utcOff * -1);   //convert to UTC
                        set = sunTimes.Result.S;
                        set = set.AddHours(utcOff * -1);
                    }
                    catch
                    {
                        Msg.TMessageBox("Error: no sunrise/set times - see TX Config", "Error: no sunrise/set times", 3000);
                        return false;
                    }



                    if (!night)
                    {
                        startT = rise.AddMinutes(sunriseoffset * -1);
                        endT = set.AddMinutes(sunsetoffset);

                    }
                    else //night
                    {
                        startT = DateTime.MinValue.AddHours(0).AddMinutes(0); //midnight
                        endT = rise.AddMinutes(sunriseoffset);

                    }
                    if (startT.Minute % 2 != 0)
                    {
                        startT = startT.AddMinutes(-1);
                    }

                    if (endT.Minute % 2 != 0)
                    {
                        endT = endT.AddMinutes(1);
                    }
                    T = startT;

                    int priorUTCoff = utcOff;

                    while (dt.Date <= Dend.Date)
                    {
                        if (night)
                        {
                            count = 0;  //if night need to run from midinight to sunrise and sunset to midnight
                        }
                        else
                        {
                            count = 1; //else just run once during day
                        }
                        sundate = dt.ToString("MM-dd");
                        bool getsun = false;
                        if (night && count == 0 || !night && count == 1)
                        {
                            getsun = true;
                        }
                        if (daycount % 7 == 0 && getsun) //every 7 days recalculate sunrise/sunset(reduce times interrogating sunrise/set database)
                        {

                            try
                            {
                                utcOff = getUTCoffset(dt);
                                var sunTimes = find_sunrise_sunset(loc, sundate);
                                rise = sunTimes.Result.R;
                                rise = rise.AddHours(utcOff * -1);   //convert to UTC
                                set = sunTimes.Result.S;
                                set = set.AddHours(utcOff * -1);
                                priorUTCoff = utcOff;
                            }
                            catch
                            {
                                Msg.TMessageBox("Error: no sunrise/set times - see TX Config", "Error: no sunrise/set times", 3000);
                                return false;
                            }
                        }

                        //while (T.TimeOfDay < endT.TimeOfDay && count < 2)
                        while (T.Hour < endT.Hour + 2 && count < 2)   //add an hour to the end so that T.Hour will always be eventually < endT.Hour
                        {

                            string newdate = dt.ToString(dateformat);

                            T = new DateTime(T.Year, T.Month, T.Day, T.Hour, mins.Minute, 0); // set minutes to same as curr time
                            time1 = T.ToString("HH:mm"); //update time by one hour

                            if (!SaveSlotData(newdate, time1))
                            {
                                return false;
                            }

                            T = T.AddHours(1);
                            if ((T.TimeOfDay > endT.TimeOfDay) && !night)
                            //if ((T.Hour > endT.Hour) && !night)
                            {
                                dt = dt.AddDays(1);
                                utcOff = getUTCoffset(dt);
                                /*sundate = dt.ToString("MM-dd");
                                var sunTimes = find_sunrise_sunset(loc, sundate);
                                rise = sunTimes.Result.R;
                                set = sunTimes.Result.S;*/
                                if (priorUTCoff != utcOff)
                                {
                                    rise = rise.AddHours(utcOff * -1);   //convert to UTC                                
                                    set = set.AddHours(utcOff * -1);
                                    priorUTCoff = utcOff;
                                }

                                T = rise.AddMinutes(sunriseoffset * -1);
                                endT = set.AddMinutes(sunsetoffset);
                                daycount++;
                                break;
                            }
                            if (T.Hour == 0 && !night)
                            {
                                dt = dt.AddDays(1);
                                utcOff = getUTCoffset(dt);
                                /*
                                sundate = dt.ToString("MM-dd");
                                var sunTimes = find_sunrise_sunset(loc, sundate);
                                rise = sunTimes.Result.R;
                                set = sunTimes.Result.S;
                                */
                                if (priorUTCoff != utcOff)
                                {
                                    rise = rise.AddHours(utcOff * -1);   //convert to UTC                                
                                    set = set.AddHours(utcOff * -1);
                                    priorUTCoff = utcOff;
                                }
                                rise = rise.AddHours(utcOff * -1);   //convert to UTC                               
                                set = set.AddHours(utcOff * -1);
                                daycount++;
                                break;
                            }
                            if ((T.TimeOfDay > endT.TimeOfDay || T.Hour == 0) && night)
                            //if ((T.Hour > endT.Hour || T.Hour == 0) && night)
                            {
                                if (count == 0)
                                {
                                    T = set.AddMinutes(sunsetoffset * -1);
                                    endT = new DateTime(endT.Year, endT.Month, endT.Day, 23, 59, 0);
                                }
                                else
                                {
                                    dt = dt.AddDays(1);
                                    /*
                                    utcOff = getUTCoffset(dt);
                                    sundate = dt.ToString("MM-dd");
                                    var sunTimes = find_sunrise_sunset(loc, sundate);
                                    rise = sunTimes.Result.R;
                                    rise = rise.AddHours(utcOff * -1);   //convert to UTC
                                    set = sunTimes.Result.S;
                                    set = set.AddHours(utcOff * -1);*/

                                    T = DateTime.MinValue.AddHours(0).AddMinutes(0); //midnight
                                    endT = rise.AddMinutes(sunriseoffset);
                                    daycount++;
                                    break;
                                }
                                count++;
                            }

                        }

                        //count++;
                    }

                }
                else
                {
                    string newdate = dt.ToString(dateformat);
                    if (!SaveSlotData(newdate, time1))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> locateSlotMembersDT_Sun_OLD(string date1, string time1, string enddate, string endtime, bool this_slot)
        {
            DateTime dt;
            DateTime Dend;

            DateTime endT;
            DateTime T;


            LatLng latlon;
            DateTime rise = DateTime.MinValue;
            DateTime set = DateTime.MinValue;

            bool night = false;
            int count = 0;

            try
            {
                string o = greylistBox.Text;
                sunriseoffset = Convert.ToInt32(o);
                sunsetoffset = Convert.ToInt32(o);
            }
            catch
            {
                sunriseoffset = 0;
                sunsetoffset = 0;
            }

            latlon = MaidenheadLocator.LocatorToLatLng(LocatortextBox.Text.Trim());

            // time1 is current time
            try
            {
                bool isValid = DateTime.TryParse(date1, out dt); //dt is output of checking datatime label (current) date
                isValid = DateTime.TryParse(enddate, out Dend); //ditto but end date



                bool isValidT = DateTime.TryParse(timeEnd.Value.ToString("HH:mm"), out endT); //endtime
                isValidT = DateTime.TryParse(time1, out T); //current time
                DateTime mins = T; //keep current minute

                DateTime StartCount = dt.AddHours(T.Hour).AddMinutes(T.Minute);
                DateTime End = Dend.AddHours(endT.Hour).AddMinutes(endT.Minute);


                if (DaycheckBox.Checked)
                {
                    night = false;
                }
                else if (NightcheckBox.Checked)
                {
                    night = true;
                }
                DateTime startT = DateTime.UtcNow;



                //TimeSpan TS = End - StartCount;

                if (this_slot) //only update this slot
                {
                    Dend = dt;
                }


                if (dt.Date <= Dend.Date)
                {



                    string loc = LocatortextBox.Text.Trim().Substring(0, 4);

                    isValid = DateTime.TryParse(datetimelabel.Text, out dt);

                    string sundate = dt.ToString("MM-dd");



                    try
                    {
                        var sunTimes = find_sunrise_sunset(loc, sundate);

                        rise = sunTimes.Result.R;
                        set = sunTimes.Result.S;
                    }
                    catch
                    {
                        Msg.TMessageBox("Error: no sunrise/set times - see TX Config", "Error: no sunrise/set times", 3000);
                        return false;
                    }


                    if (!night)
                    {
                        startT = rise.AddMinutes(sunriseoffset * -1);
                        endT = set.AddMinutes(sunsetoffset);

                    }
                    else //night
                    {
                        startT = DateTime.MinValue.AddHours(0).AddMinutes(0); //midnight
                        endT = rise.AddMinutes(sunriseoffset);

                    }
                    if (startT.Minute % 2 != 0)
                    {
                        startT = startT.AddMinutes(-1);
                    }

                    if (endT.Minute % 2 != 0)
                    {
                        endT = endT.AddMinutes(1);
                    }
                    T = startT;
                    while (dt.Date <= Dend.Date)
                    {

                        if (night)
                        {
                            count = 0;  //if night need to run from midinight to sunrise and sunset to midnight
                        }
                        else
                        {
                            count = 1; //else just run once during day
                        }
                        //T = startT;                       

                        while (T <= endT && count < 2)
                        {

                            string newdate = dt.ToString(dateformat);

                            T = new DateTime(T.Year, T.Month, T.Day, T.Hour, mins.Minute, 0); // set minutes to same as curr time
                            time1 = T.ToString("HH:mm"); //update time by one hour

                            if (!SaveSlotData(newdate, time1))
                            {
                                return false;
                            }

                            T = T.AddHours(1);
                            if (T >= endT)
                            {
                                if (!night || (night && count == 1))
                                {
                                    dt = dt.AddDays(1);
                                    T = startT;
                                }
                                break;
                            }
                            if (T.Hour == 0)
                            {
                                dt = dt.AddDays(1);

                                break;
                            }
                            if (T.Hour > endT.Hour && night)
                            {
                                T = set.AddMinutes(sunsetoffset * -1);
                                endT = new DateTime(endT.Year, endT.Month, endT.Day, 23, 59, 0);
                            }
                            count++;
                        }
                    }

                }
                else
                {
                    string newdate = dt.ToString(dateformat);
                    if (!SaveSlotData(newdate, time1))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        private bool SaveSlotData(string d, string t)  //to database
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            DateTime date = new DateTime();
            //string d = "";

            string e = "";
            string p = "";
            int msgT = 1;
            rpt_type = 0;
            greyoffset = 0;
            if (repeatcheckBox.Checked)
            {
                if (repeatTimecheckBox.Checked)
                {
                    rpt_type = 1;
                }
                else if (DaycheckBox.Checked)
                {
                    rpt_type = 2;
                }
                else if (NightcheckBox.Checked)
                {
                    rpt_type = 3;
                }
                else if (AllcheckBox.Checked)
                {
                    rpt_type = 4;
                }
            }
            else
            {
                rpt_type = 0;
            }
            try
            {
                greyoffset = Convert.ToInt32(greylistBox.Text.Trim());
            }
            catch
            {

            }
            lock (_lock)
            {
                try
                {


                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO slots(Date,Time,Frequency,Offset,Power,PowerW,Antenna,Tuner,Switch,SwitchPort,End,Active,Repeating,TimeEnd,RptTime,Parent,SlotNo,MsgType,RptType,GreyOffset) ";
                    command.CommandText += "VALUES(@Date,@Time,@Frequency,@Offset,@Power,@PowerW,@Antenna,@Tuner,@Switch,@SwitchPort,@End,@Active,@Repeating,@TimeEnd,@RptTime,@Parent,@SlotNo,@MsgType,@RptType,@GreyOffset)";

                    connection.Open();


                    //TimeSpan time = Convert.ToDateTime(cells[1]);
                    command.Parameters.AddWithValue("@Date", d);
                    command.Parameters.AddWithValue("@Time", t);
                    command.Parameters.AddWithValue("@Frequency", Slot.Freq);
                    command.Parameters.AddWithValue("@Offset", Slot.Offset);
                    command.Parameters.AddWithValue("@Power", Slot.PowerdB);
                    command.Parameters.AddWithValue("@PowerW", Slot.PowerW);
                    command.Parameters.AddWithValue("@Antenna", Slot.Ant);
                    command.Parameters.AddWithValue("@Tuner", Slot.Tuner);
                    command.Parameters.AddWithValue("@Switch", Slot.Switch);
                    command.Parameters.AddWithValue("@SwitchPort", Slot.SwPort);


                    e = Convert.ToString(Slot.Endslot);

                    command.Parameters.AddWithValue("@End", e);
                    bool A = false; ;
                    if (cells[11].Contains(tick))
                    {
                        A = true;
                    }
                    bool R = false;
                    if (cells[10].Contains(tick))
                    {
                        R = true;
                    }
                    command.Parameters.AddWithValue("@Active", A);
                    command.Parameters.AddWithValue("@Repeating", R);
                    //string endtime = timeEnd.Value.ToString("HH:mm");
                    command.Parameters.AddWithValue("@TimeEnd", Slot.EndTime);
                    command.Parameters.AddWithValue("@RptTime", Slot.RptTime);

                    p = Convert.ToString(cells[0]);
                    p = p + " " + cells[1]; //date and time of parent of this slot
                    Slot.Parent = p;
                    command.Parameters.AddWithValue("@Parent", p);
                    command.Parameters.AddWithValue("@SlotNo", slotNo);

                    if (Type2checkBox.Checked)
                    {
                        msgT = checkCall(callsign, location);
                    }

                    command.Parameters.AddWithValue("@MsgType", msgT);
                    command.Parameters.AddWithValue("@RptType", rpt_type);
                    command.Parameters.AddWithValue("@GreyOffset", greyoffset);

                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                    return UpdateSlotData(d, t, e, p, msgT);

                }
            }
        }
        private bool UpdateSlotData(string d, string t, string e, string p, int msgT)
        {
            string c = "";
            string act = "0";
            string r = "0";
            if (cells[11].Contains(tick))
            {
                act = "1";
            }
            if (cells[10].Contains(tick))
            {
                r = "1";
            }
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            lock (_lock)
            {
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    c = "UPDATE slots SET Frequency = " + Slot.Freq + ", Offset = " + Slot.Offset + ", Power = " + Slot.PowerdB;
                    c = c + ", PowerW = " + Slot.PowerW + ", Antenna = '" + Slot.Ant + "', Tuner = " + Slot.Tuner + ", Switch = " + Slot.Switch;
                    c = c + ", SwitchPort = " + Slot.SwPort + ", End = '" + e + "', Active = " + act + ", Repeating = " + r + ", TimeEnd = '" + Slot.EndTime;
                    c = c + "', RptTime = " + Slot.RptTime + ", Parent = '" + p + "'";
                    c = c + ", SlotNo = " + slotNo + ", MsgType = " + msgT + ", RptType = " + rpt_type + ", GreyOffset = " + greyoffset + " WHERE slots.Date = '" + d + "' AND slots.Time = '" + t + "'"; // + ";";              
                                                                                                                                                                                                          //UPDATE `slots` SET `Antenna` = 'GP' WHERE `slots`.`Date` = '2025-02-28' AND `slots`.`Time` = '16:02:00'; 
                    command.CommandText = c;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;

                }
                catch
                {   //exhausted insert and update
                    connection.Close();
                    Msg.OKMessageBox("Unable to save to database", "");
                    return false;
                }
            }
        }
        private string dBtoWatts(string db)
        {
            string W = "";

            int dBm = int.Parse(db);
            switch (dBm)
            {
                case 0: { W = "0.001"; break; }
                case 3: { W = "0.002"; break; }
                case 7: { W = "005"; break; }
                case 10: { W = "0.010"; break; }
                case 13: { W = "0.020"; break; }
                case 17: { W = "0.050"; break; }
                case 20: { W = "0.100"; break; }
                case 23: { W = "0.200"; break; }
                case 27: { W = "0.500"; break; }
                case 30: { W = "1"; break; }
                case 33: { W = "2"; break; }
                case 37: { W = "5"; break; }
                case 40: { W = "10"; break; }
                case 43: { W = "20"; break; }
                case 47: { W = "50"; break; }
                case 50: { W = "100"; break; }
                case 53: { W = "200"; break; }
                case 57: { W = "500"; break; }
                case 60: { W = "1000"; break; }
            }
            return W;
        }

        private void dBmcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PowertextBox.Text = dBtoWatts(dBmcomboBox.SelectedItem.ToString());
        }



        private void OffsettextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            int t;

            if (e.KeyChar == 45)
            {
                if (OffsettextBox.Text.Contains("-"))
                {
                    e.Handled = true;
                }
                return;
            }
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !"-".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
            else
            {
                int.TryParse(OffsettextBox.Text + e.KeyChar, out t);
                if (t > 220 || t < -20)
                {
                    Msg.OKMessageBox("Error: range 0-200 Hz", "");
                    e.Handled = true;
                }
            }

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                deletethisRow(true);
            }
        }
        private void deletethisRow(bool rightbutton)
        {
            int i = 0;
            int slot = 0;
            string msgT = "1";
            this.Focus();
            this.BringToFront();
            for (i = 0; i < dataGridView1.Rows.Count; i++)
            {
                try
                {
                    if (dataGridView1.Rows[i].Selected)
                    {
                        date = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);

                        time = Convert.ToString(dataGridView1.Rows[i].Cells[1].Value);
                        string MT = dataGridView1.Rows[i].Cells[15].Value.ToString();

                        msgT = MT;
                        slot = i; break;
                    }
                }
                catch { }
            }

            DialogResult res;
            if (rightbutton)
            {
                res = Msg.ynMessageBox("Delete this slot (Y/N)?", "Delete Slot");
                try
                {
                    if (res == DialogResult.Yes)
                    {
                        //time = dataGridView1.Rows[dataGridView1.]
                        res = Msg.ynMessageBox("Delete all repeating slots too (Y/N)?", "Delete Repeat Slots");
                        if (res == DialogResult.Yes)
                        {

                            DeleteSlot(true, slot, parents[slot]);  //check where can get PARENT from in current row (its not in the current gridview)

                            if ((msgT == "2" || msgT == "3") && !asOnecheckBox.Checked)
                            {
                                DeleteSlot(true, slot + 1, parents[slot + 1]);  //if type 2
                                parents[slot + 1] = "";
                            }
                        }
                        else
                        {
                            DeleteSlot(false, slot, parents[slot]);
                            if ((msgT == "2" || msgT == "3") && !asOnecheckBox.Checked)
                            {
                                DeleteSlot(false, slot + 1, parents[slot + 1]);  //type 2
                                parents[slot + 1] = "";
                            }
                        }
                        DeleteRow(slot);
                        if ((msgT == "2" || msgT == "3") && !asOnecheckBox.Checked)
                        {
                            DeleteRow(slot + 1);
                        }
                        parents[slot] = "";
                    }
                }
                catch
                {
                    Msg.TMessageBox("Error deleting slot", "", 1000);
                }
            }
            this.Focus();
            this.BringToFront();
            dataGridView1.Focus();
        }

        private void DeleteRow(int slot)
        {
            try
            {
                for (int col = 2; col < dataGridView1.Rows[slot].Cells.Count; col++)
                {
                    dataGridView1.Rows[slot].Cells[col].Value = "";
                }
            }
            catch
            {

            }
        }
        private void DeleteSlot(bool all, int slot, string parent)
        {
            string c = "";
            try
            {
                DateTime s, e = new DateTime();
                if (all)
                {
                    Slot.Endslot = Convert.ToString(dataGridView1.Rows[slot].Cells[10].Value);

                }
                else
                {
                }
            }
            catch
            {

            }
            string time = dataGridView1.Rows[slot].Cells[1].Value.ToString();
            string date = dataGridView1.Rows[slot].Cells[0].Value.ToString();

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                if (!all)
                {
                    c = "DELETE FROM slots WHERE slots.Date = '" + date + "' AND slots.Time = '" + time + "'";
                }
                else if (parent != "" || parent != null)
                {
                    c = "DELETE FROM slots WHERE Parent = '" + parent + "'";
                }
                command.CommandText = c;

                command.ExecuteNonQuery();
                connection.Close();


            }
            catch
            {
                Msg.OKMessageBox("Error deleting slot", "");
                connection.Close();
            }
        }

        private void deleteAllSlotsbutton_Click(object sender, EventArgs e)
        {
            selectRangtoDelete();
        }
        private void selectRangtoDelete()
        {
            string date1, date2;
            DateTime dt;

            if (deleteAllSlotsbutton.Text == "Delete all slots from")
            {
                currentSelectedDate = monthCalendar1.SelectionStart;
                deleteAllSlotsbutton.Text = "Select date range from";
                monthCalendar1.MaxSelectionCount = 365;
                return;
            }
            else
            {

                if (selectedStartDatelabel.Text == "date1" || selectedEndDatelabel.Text == "date2")
                {
                    DialogResult R = Msg.ynMessageBox("Invalid date range - cancel (Y/N)?", "Invalid date range");
                    if (R == DialogResult.Yes)
                    {
                        deleteAllSlotsbutton.Text = "Delete all slots from";
                        return;
                    }
                    else { return; }
                }

            }

            DialogResult res = Msg.ynMessageBox("Delete all slots between selected dates (y/n)?", "Delete selected days");
            if (res == DialogResult.Yes)
            {

                date1 = selectedStartDatelabel.Text;
                date2 = selectedEndDatelabel.Text;
                if (deleteAllSlotsBetween(date1, date2))
                {

                    monthCalendar1.MaxSelectionCount = 1;
                    selectedStartDatelabel.Text = "date1";
                    selectedEndDatelabel.Text = "date2";
                    //flushRows();
                    Msg.OKMessageBox("Deleted slots from: " + date1 + " to: " + date2, "Success");
                    deleteAllSlotsbutton.Text = "Delete all slots from";
                }

            }
            else
            {
                monthCalendar1.MaxSelectionCount = 1;
                selectedStartDatelabel.Text = "date1";
                selectedEndDatelabel.Text = "date2";
                deleteAllSlotsbutton.Text = "Delete all slots from";
            }
        }
        private void flushRows()
        {
            string selDate = currentSelectedDate.ToString(dateformat);
            string[] t = timelistBox.Text.Split(':');
            try
            {
                int h = 0;
                if (t[0] == "23")
                {
                    h = 0;
                }
                else
                {
                    h = Convert.ToInt32(t[0]);
                    h++;
                }
                string time = h.ToString().PadLeft(2, '0');
                time = time + ":00"; // + t[1];
                selectDT(time, selDate, false); //select another time - don;t find slots from db

                selectDT(timelistBox.Text, selDate, true); //then go back to selected to flush the rows - check db

                monthCalendar1.SelectionStart = Convert.ToDateTime(currentSelectedDate);
            }
            catch
            {

            }
        }
        private bool deleteAllSlotsBetween(string date1, string date2)
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";
            string arrow = ">";
            string c = "";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                c = "DELETE FROM slots WHERE slots.Date >= '" + date1 + "' AND slots.Date <= '" + date2 + "'";

                command.CommandText = c;

                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {
                Msg.OKMessageBox("Error deleting slots", "");
                return false;
                connection.Close();
            }
        }

        private void dateStart_ValueChanged(object sender, EventArgs e)
        {

        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (monthCalendar1.MaxSelectionCount > 1)
            {

                DateTime dt = monthCalendar1.SelectionStart;

                selectedStartDatelabel.Text = dt.ToString(dateformat);
                dt = monthCalendar1.SelectionEnd;
                selectedEndDatelabel.Text = dt.ToString(dateformat);
            }

        }

        private void repeatcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!repeatcheckBox.Checked && repeatStatus)
            {
                var res = Msg.ynMessageBox("Only modify this slot (Y/N)?", "Modify slots");
                if (res == DialogResult.No)
                {
                    repeatcheckBox.Checked = true;
                }
            }
            dateEnd.Enabled = repeatcheckBox.Checked;
            timeEnd.Visible = repeatcheckBox.Checked;
            repeatTimecheckBox.Visible = repeatcheckBox.Checked;
            DaycheckBox.Visible = repeatcheckBox.Checked;
            NightcheckBox.Visible = repeatcheckBox.Checked;
            AllcheckBox.Visible = repeatcheckBox.Checked;
            if ((NightcheckBox.Checked || DaycheckBox.Checked) && repeatcheckBox.Checked)
            {
                greygroupBox.Visible = true;
            }
            else
            {
                greygroupBox.Visible = false;
            }
        }

        //--------------------------------WSPR Sharp encoding------------------------------------

        public async Task StartTX(bool test)
        {
            //int[] levels = null;
            //levels = WSPR_Encode();
            //startF = TXFrequency;


            daytimer.Stop();
            daytimer.Enabled = false;
            if (!rigctldcheckBox.Checked)
            { getRigF(); }
            daytimer2.Enabled = true;
            daytimer2.Start();

            WSPR_Transmit(test);
        }
        public async void WSPR_Transmit(bool test)
        {
            int offS = 15; //set by sender as the offset from the base frequency in the wspr window
            double basefreq = 1400; //wspr base frequency (window from 1400-1600 Hz)
                                    //double mod = 1.46; //FSK modulation frequency offset
                                    //int bitlen = 683; //length of each bit is 0.683 seconds (x 161 bits = 110.6 second message duration)

            double freq = basefreq + Slot.Offset; //basefreq 1400 + offset from 0-200 Hz
                                                  //int bitcount = 161; //161 bits in wspr message
                                                  //countdownlabel.Text = "TX started";
                                                  //countdownlabel2.Text = "TX started";
            int[] levels = null;
            stopPlay = false;
            if (flatcode)  //tx test
            {
                offS = defaultoffset;
            }
            else
            {
                offS = Slot.Offset;
            }
            //MessageBox.Show("got here");
            if (!Type2checkBox.Checked) { msgType = 1; }  //only allow type 2 messages if the box is unchecked
            try
            {
                levels = WSPR_Encode();


                if (levels == null)
                {
                    daytimer2.Stop();
                    daytimer2.Enabled = false;
                    daytimer.Enabled = true;
                    daytimer.Start();
                    //Msg.TMessageBox("Error encoding message", "",3000);
                    return;
                }
            }
            catch
            {
                daytimer2.Stop();
                daytimer2.Enabled = false;
                daytimer.Enabled = true;
                daytimer.Start();
                Msg.TMessageBox("Error encoding message", "", 3000);
                return;
            }

            await Task.Delay(800); // add this to start at about +1 second
            if (!testTonescheckBox.Checked && !test)
            {
                if (slotActive && enableTXcheckBox.Checked)
                {
                    PTT(true);

                }
                wsprTXtimer.Enabled = true;
                wsprTXtimer.Start();
            }

            if (slotActive && (levels != null) && enableTXcheckBox.Checked)
            {
                if (TXrunbutton.Text.Contains("RX"))
                {
                    getRigF();
                }
                try
                {
                    //rxForm.blockDecodes = true;
                    double newalpha = 0.1;
                    if (defaultAlpha < 1 || defaultAlpha > 0 || defaultAlpha != null)
                    {
                        newalpha = defaultAlpha;
                    }

                    float volume = 0.5f;
                    double F = Convert.ToDouble(TXFrequency);
                    F = F / 1000000;
                    volume = findFreqVol(F);
                    Volume = volume; //Volume is dynamic signal; volume value when adjusting freq from freq&ant

                    var fsk = new FSK.FSKMod(levels, offS, testTonescheckBox.Checked, newalpha, volume);

                    DateTime dt = DateTime.Now;

                    while (dt.Second < 1)  //start at even min. + 1 second
                    {
                        dt = DateTime.Now;
                    }
                    await Task.Run(() =>
                    {
                        MonitorFSK(fsk);

                    });
                    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string filepath = homeDirectory;
                    string filename = "WSPR_Log";
                    string content = "";

                    dt = DateTime.Now.ToUniversalTime();

                    dt = dt.AddMinutes(-1);
                    string cs = base_call;
                    if (msgType == 2)
                    {
                        cs = callsign;
                    }
                    content = dt.ToString("yyyy-MM-dd HH:mm") + " UTC, " + cs + ", " + F.ToString() + " MHz, " + location + ", " + Slot.PowerdB + " dBm, Antenna: " + TXRXAntlabel.Text;
                    var TXlog = new Log(filepath, filename, content, OpSystem);

                }
                catch (Exception e)
                {
                    Msg.OKMessageBox("Error in transmitting message", "");

                    PTT(false);
                    testTonescheckBox.Checked = false;
                }
            }
            else
            {
                if (levels == null) { Msg.OKMessageBox("Error encoding message", ""); }
            }
            testTonescheckBox.Checked = false;
            //if (!testPTTtimer.Enabled) { PTT(false); }
            //return false;
        }

        private float findFreqVol(double F)
        {

            int i = FreqlistBox.FindString(F.ToString());
            string[] s = FreqlistBox.Items[i].ToString().Split('\t');
            if (s[2] != null)
            {
                float result;
                if (float.TryParse(s[2], out result))
                {
                    return result;
                }
            }
            else
            {
                Msg.TMessageBox("Cannot find level for this frequency", "F level", 1000);
                return 0.5f;
            }
            return 0.5f;
        }

        private async Task MonitorFSK(FSK.FSKMod fsk)
        {

            while (!fsk.stopPlay)
            {
                fsk.volumeFactor = Volume; //dynmiucally change amplitude of wspr signal
                if (stopPlay)
                {
                    fsk.stopPlay = true;
                }
            }
        }




        private void showMsgType(int type)
        {
            string S = "Message type ";
            S = S + type.ToString();
            msgTypelabel.Text = S;
            msgTypelabel2.Text = S;
        }
        private int[] WSPR_Encode() //encode the message into a byte array of wspr Levels (0,1,2 or 3) of 161 bytes length
        {
            //slotNo = 1; 
            if (flatcode)  //test tx button pressed
            {
                slotNo = 1;
                msgType = 1;
            }
            if (CalltextBox.Text == "" || LocatortextBox.Text == "")
            {
                Msg.OKMessageBox("Invalid callsign or locator", "");
                return null;
            }
            string loc = location;
            if (asOnecheckBox.Checked && slotNo == 1 && msgType == 3)
            {
                //leave lcoation alone
            }
            else if (location.Length > 4 && slotNo != 2)
            {
                loc = location.Substring(0, 4);
            }
            if (!longcheckBox.Checked)
            {
                loc = location.Substring(0, 4);
            }
            if (longcheckBox.Checked && !Type2checkBox.Checked)
            {
                loc = location.Substring(0, 4);
            }
            if (location.Length < 4)
            {
                loc = location.PadRight(4);
            }
            bool ok = false;
            string error = "";
            //MessageBox.Show(msgType.ToString() + " " + slotNo.ToString());

            mtypelabel.Text = msgType.ToString();
            slotnolabel.Text = slotNo.ToString();
            if (overcheckBox.Checked && slotActive && enableTXcheckBox.Checked)
            {
                overlabel.Text = "msg type override on";
            }
            else
            {
                overlabel.Text = "";
            }

            if ((msgType == 2 && !asOnecheckBox.Checked) || (msgType == 3 && slotNo == 2))
            {
                showMsgType(msgType);

                var wspr = new WsprTransmission();
                wspr.wsprmsgPath = wsprmsgtextBox.Text;
                wsprLevels = wspr.WsprTxn(callsign, loc, Slot.PowerdB, slotNo, msgType, asOnecheckBox.Checked, OpSystem);

                if (wspr.IsValid)
                {
                    ok = true;
                }
                else
                {
                    wsprLevels = null;
                    ok = false;
                }
                ;
                error = wspr.ErrorMessage;
            }
            else
            if ((slotNo == 1 || slotNo == 0) && (msgType == 1 || msgType == 3)) //slot 1
            {
                showMsgType(1);
                int callok = 0;
                string call = callsign;

                callok = checkCall(baseCalltextBox.Text, LocatortextBox.Text);

                if (msgType == 1)
                {
                    //if (!Type2checkBox.Checked && !asOnecheckBox.Checked)
                    //{
                    call = baseCalltextBox.Text;
                    msgType = 1;
                    //}
                }
                if (flatcode && callok > 0)
                {
                    call = baseCalltextBox.Text;
                }

                if (callok == 0)
                {
                    Msg.OKMessageBox("Invalid base call", "Check base call");
                    ok = false;
                }


                if (callok > 0)
                {
                    var wspr = new WsprTransmission();
                    wspr.wsprmsgPath = wsprmsgtextBox.Text;
                    wsprLevels = wspr.WsprTxn(call, loc, Slot.PowerdB, slotNo, msgType, asOnecheckBox.Checked, OpSystem);

                    if (wspr.IsValid)
                    {
                        ok = true;
                    }
                    else
                    {
                        ok = false;
                        wsprLevels = null;
                    }
                    error = wspr.ErrorMessage;
                }
                else
                {
                    ok = false;
                }
            }
            else if (slotNo == 2 && !Type2checkBox.Checked)
            {
                Msg.TMessageBox("Type 2 messages not enabled", "", 3000);
                //ok = false;
                //return null;
            }
            else
            {
                ok = false;
            }


            try
            {
                if (ok)
                {

                    int[] intlevels = new int[wsprLevels.Length];
                    for (int i = 0; i < wsprLevels.Length; i++)
                    {
                        if (flatcode) //use single unmod tone
                        {
                            intlevels[i] = 0;
                        }
                        else
                        {
                            if (wsprLevels != null)
                            {
                                intlevels[i] = (Convert.ToInt32(wsprLevels[i]));
                            }
                        }
                    }
                    flatcode = false;

                    return intlevels;
                }
                else
                {
                    Msg.TMessageBox("Error encoding WSPR data", "", 2000);
                    return null;
                }
            }
            catch
            {
                Msg.TMessageBox("Error encoding WSPR data", "", 2000);
                return null;
            }
        }

        private double getModOffset(int level, double mod) //get modulation offset F for each bit (0,1,2 or 3)
        {
            if (level > -1 && level < 4)
            {
                switch (level)
                {
                    case 0:
                        {
                            //return 100;
                            return mod;
                        }
                    case 1:
                        {
                            //return 200;
                            return mod * 2;
                        }
                    case 2:
                        {
                            //return 300;
                            return mod * 3;
                        }
                    case 3:
                        {
                            //return 400;
                            return mod * 4;
                        }
                }
            }
            return 0;
        }


        private void TXTestbutton_Click(object sender, EventArgs e)
        {
            slotNo = 1;
            if (Type2checkBox.Checked)
            {
                msgType = 3;    //send as one message type 3

            }
            else
            {
                msgType = 1;
            }
            showMsgType(msgType);
            Slot.Offset = defaultoffset;
            TXAntenna = defaultAnt;
            if (rigctldcheckBox.Checked)
            {
                testFtextBox.Text = defaultF.ToString();
            }
            //TXFrequency = Convert.ToString(defaultF * 1000000);
            if (testFtextBox.Text == "" && !rigctldcheckBox.Checked)
            {
                Msg.OKMessageBox("Select frequency from list first", "");
                return;
            }
            if (CalltextBox.Text == "" || defaultOfftextBox.Text == "")
            {
                Msg.OKMessageBox("Blank callsign or offset", "");
                return;
            }
            if (!enableTXcheckBox.Checked)
            {
                Msg.TMessageBox("TX not enabled", "", 3000);
                return;
            }
            if (!checkRigctld())
            {
                Msg.TMessageBox("Error: RigCtld not running", "", 3000);
            }
            else
            {
                WSPRtimer.Enabled = true;
                WSPRtimer.Start();
                prepDone = false;
                slotActive = true;
            }
        }
        private void WSPRtimer_Tick(object sender, EventArgs e)
        {
            WSPRtimer_Action();

        }

        private async void WSPRtimer_Action()
        {
            DateTime now;

            if (LTcheckBox.Checked)
            {
                now = DateTime.Now;
            }
            else
            {
                now = DateTime.Now.ToUniversalTime();
            }
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;

            int down = 0;
            int even = 0;
            string TXRX = "TX in ";

            /*if (!slotActive)
            {
                return;
            }*/
            if (m % 2 == 0)
            {
                even = 60;
            }
            else { even = 0; }
            down = 60 + even - s;
            if (down < 120)
            {

                if (slotActive)
                {
                    TXRX = "TX in ";
                }
                else
                {
                    TXRX = "RX in: ";
                }
                countdownlabel.Text = TXRX + down.ToString() + " seconds";
                countdownlabel2.Text = TXRX + down.ToString() + " seconds";
            }

            if (down > 4)
            {
                prepDone = false;
            }
            if (down <= 5 && !prepDone)
            {
                prepDone = true;
                await activateAntSwitch(TXAntenna);
                await Task.Delay(100);
                await activateTX(TXFrequency);
                double freq = Convert.ToDouble(TXFrequency);
                freq = freq / 1000000;
                if (!noRigctld)
                {
                    await rxForm.set_frequency(freq.ToString("F6"));
                }

            }

            if ((down == 1 || down >= 120))
            {
                if (slotActive && enableTXcheckBox.Checked)
                {
                    countdownlabel.Text = "TX start";
                    countdownlabel2.Text = "TX start";
                    await StartTX(false);
                }
                else if (!slotActive)
                {
                    countdownlabel.Text = "RX start";
                    countdownlabel2.Text = "RX start";
                    if (!rigctldcheckBox.Checked)
                    { getRigF(); }
                }
                //await StartTX(false);
                WSPRtimer.Stop();
                WSPRtimer.Enabled = false;

            }
        }


        private async Task activateTX(string TXFrequency)
        {
            string TXF = TXFrequency;
            var c = await changeFreq(TXF);
            if (c)
            {
                double freq = Convert.ToDouble(TXFrequency);
                freq = freq / 1000000;
                if (!noRigctld)
                {
                    await rxForm.set_frequency(freq.ToString("F6"));
                }
            }
        }


        private async Task<bool> changeFreq(string f)
        {
            string rigcmd = "F " + f; //send F <frequency> to RigCtl daemon via TCP port 4532
            double freq = 0;
            bool ok = false;

            if (noRigctld)
            {
                if (FlistBox2.SelectedIndex > -1)
                {
                    string F = FlistBox2.SelectedItem.ToString();
                    Msg.TMessageBox("Frequency selected: " + F + " MHz", "", 1000);
                    rxForm.set_frequency(F);
                    return true;
                }
                else
                {
                    Msg.TMessageBox("Don't forget to select the TX/RX frequency", "", 2000);
                    return false;
                }
            }
            else
            {
                await Task.Run(() =>
                {
                    var reply = sendTXRigCommand(rigcmd);
                    string R = reply.ToString();
                    if (R.StartsWith("RPRT -"))
                    {
                        Msg.TMessageBox("Error in rig comms", "", 2000);
                    }
                    else if (R == "error")
                    {
                        blockTXonErr = true;
                        Msg.TMessageBox("Error in rig comms", "", 2000);

                    }
                    else
                    {
                        try
                        {
                            freq = Convert.ToDouble(f);
                            freq = freq / 1000000;
                            ok = true;
                        }
                        catch { }
                        Msg.TMessageBox("Changed to: " + freq.ToString() + " MHz", "", 1000);

                    }

                });
                if (ok) { rxForm.set_frequency(freq.ToString("F6")); return true; }
                else
                {
                    return false;
                }
            }


        }

        private async Task setDBPassword(string password, string database)
        {
            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=" + database;
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                c = "ALTER USER 'admin'@'%' IDENTIFIED BY '" + password + "'";

                command.CommandText = c;

                command.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
                connection.Close();
            }
        }
        private async void setAllPasswords(string password)
        {
            await setDBPassword(password, "wspr");
            await setDBPassword(password, "wspr_configs");
            await setDBPassword(password, "wspr_rpt");
            await setDBPassword(password, "wspr_rx");
            await setDBPassword(password, "wspr_slots");
            await setDBPassword(password, "wspr_sol");
        }
        private bool saveDBPassword(string password)
        {
            // ALTER USER 'username'@'localhost' IDENTIFIED BY 'new_password';

            try
            {

                setAllPasswords(password);
                db_pass = password;
                saveUserandPassword(db_user, password);
                return true;

            }
            catch
            {
                //Msg.OKMessageBox("Unable to update new password", "DB password");
                return false;
            }
        }
        private void savePasswordbutton_Click(object sender, EventArgs e)
        {
            updatePassword();
        }
        private void updatePassword()
        {
            if (PasstextBox.Text.Length < 4)
            {
                Msg.OKMessageBox("Password is either blank or too short", "Password > 3?");
                return;
            }
            if (PasstextBox.Text != PasstextBox2.Text)
            {
                Msg.OKMessageBox("Passwords do not match", "Database password");
                return;
            }
            if (saveDBPassword(PasstextBox.Text.Trim()))
            {
                PasstextBox2.Visible = false;
                Passlabel2.Visible = false;
                showPasscheckBox.Checked = false;
            }
            else
            {
                Msg.OKMessageBox("Unable to update password", "DB password");
                DialogResult res = Msg.ynMessageBox("Unable to save password - use this to recover (y/n)?", "Password update error");
                if (res == DialogResult.Yes)
                {
                    db_pass = PasstextBox.Text;
                    Msg.OKMessageBox("OK - now try saving again with this password", "Password recovery?");
                }
            }
        }

        private bool saveUserandPassword(string user, string password)
        {
            string key = "wsproundtheworld";
            Encryption enc = new Encryption();
            string encryptedpassword = enc.Encrypt(password, key);

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filepath = homeDirectory;
            string content = "db_user: " + user + " db_pass: " + encryptedpassword;
            string dbcall = "db_call: " + baseCalltextBox.Text.Trim();
            if (Path.Exists(filepath))
            {
                if (filepath.EndsWith(slash))
                {
                    slash = "";
                }
                filepath = filepath + slash + "DBcredential";
                try
                {
                    using (StreamWriter writer = new StreamWriter(filepath, false))
                    {
                        writer.WriteLine(content);
                        writer.WriteLine(dbcall);
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        private bool getUserandPassword()
        {
            string key = "wsproundtheworld";
            Encryption enc = new Encryption();
            string encryptedpassword;
            string content = "";
            string dbcall = "";
            bool ok = false;

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filepath = homeDirectory;
            //string content = "db_user: " + user + " db_pass: " + passwordhash;

            if (Path.Exists(filepath))
            {

                if (filepath.EndsWith(slash))
                {
                    slash = "";
                }
                filepath = filepath + slash + "DBcredential";
                if (File.Exists(filepath))
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(filepath))
                        {
                            content = reader.ReadLine();
                            dbcall = reader.ReadLine();
                            reader.Close();
                        }
                        if (content != null || content != "")
                        {
                            if (content.Contains("db_pass:"))
                            {
                                encryptedpassword = content.Substring(content.IndexOf("db_pass: ") + "db_pass: ".Length);
                                string password = enc.Decrypt(encryptedpassword, key);
                                if (password.Length > 0 && password != null)
                                {
                                    db_pass = password;
                                    PasstextBox.Text = password;

                                    ok = true;
                                }
                            }

                        }
                        if (!ok)
                        {
                            Msg.TMessageBox("Unable to read database credentials", "", 1000);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Msg.TMessageBox("Unable to read database credentials", "", 1000);
                        return false;
                    }
                }
            }
            PasstextBox2.Visible = false;
            Passlabel2.Visible = false;
            return ok;
        }

        async Task GetLocationFromIP()
        {
            string url = "https://ipinfo.io/json"; // No token needed for basic usage
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(url);
                    JObject locationData = JObject.Parse(response);
                    string location = (string)locationData["loc"];
                    string[] S = location.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (S.Length > 1)
                    {
                        try
                        {
                            double lat = Convert.ToDouble(S[0]);
                            double lon = Convert.ToDouble(S[1]);

                            string L = MaidenheadLocator.LatLngToLocator(lat, lon);


                            DialogResult res = Msg.ynMessageBox("Locator: " + L + " - save this location (Y/N)?", "Update location");
                            if (res == DialogResult.No)
                            {
                                location = L;
                                LocatortextBox.Text = L;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void locationbutton_Click(object sender, EventArgs e)
        {
            GetLocationFromIP();
        }

        private void saveConfigbutton_Click(object sender, EventArgs e)
        {
            SaveAll();
        }
        private async void SaveAll()
        {
            int msgT = 1;
            FList = "";
            bool saved = false;

            string C = CalltextBox.Text;
            string L = LocatortextBox.Text;
            HamlibPath = RigCtlPathtextBox.Text.Trim();
            rxForm.updateLabels(L);

            if (C == "")
            {
                Msg.OKMessageBox("Callsign cannot be empty", "");
                return;
            }


            msgT = checkCall(C, L);
            if (!Type2checkBox.Checked)
            {
                msgT = 1;
            }

            if (msgT == 0)
            {
                string baseC = findBaseCall(C);
                baseCalltextBox.Text = baseC;
                base_call = baseC;
                return;
            }
            if (DefaultAntcomboBox.SelectedIndex > -1)
            {
                defaultAnt = DefaultAntcomboBox.SelectedItem.ToString();
            }
            if (defaultFcomboBox.SelectedIndex > -1)
            {
                defaultF = Convert.ToDouble(defaultFcomboBox.SelectedItem);
            }

            if (msgT == 1 || msgT == 0)
            {

                string baseC = findBaseCall(C);
                baseCalltextBox.Text = baseC;
                base_call = baseC;

                //Msg.TMessageBox("Type 1 message format", "WSPR message format", 2000);

            }
            else if (msgT == 2 || msgT == 3)  //type 2 or 3 message
            {

                string baseC = findBaseCall(C);

                if (baseC.Trim() == "")
                {
                    Msg.OKMessageBox("Enter your base call", "Base call?");
                }
                else
                {
                    Msg.OKMessageBox("Check if your base call is correct", "Base call correct?");
                    baseCalltextBox.Text = baseC;
                    base_call = baseC;
                }

            }

            msgTypelabel.Text = "Message type 1";
            if (msgT == 2)
            {
                msgTypelabel.Text = "Message type 2";
            }
            if (msgT == 3)
            {
                msgTypelabel.Text = "Message type 3";
            }
            msgTypelabel2.Text = msgTypelabel.Text;
            callsign = C;
            defaultoffset = Convert.ToInt32(defaultOfftextBox.Text);
            defaultdB = Convert.ToInt32(defaultpwrcomboBox.SelectedItem);
            defaultW = Convert.ToDouble(defaultWtextBox.Text);

            string loc = LocatortextBox.Text.Trim().Substring(0, 4);
            if (!location.Contains(loc))
            {
                Msg.TMessageBox("Locator changed - update sunrise/sunset?", "Locator changed", 2500);
            }
            if (checkLocator(LocatortextBox.Text))
            {

                full_location = LocatortextBox.Text;
                if (msgT == 1 || msgT == 0)
                {
                    location = LocatortextBox.Text.Substring(0, 4);
                }
                else if (msgT == 2 && !asOnecheckBox.Checked)
                {
                    location = LocatortextBox.Text.Substring(0, 4); ///can modify to take 6 character locator later
                }
                else
                {
                    if (LocatortextBox.Text.Length > 5)
                    {
                        location = LocatortextBox.Text.Trim().Substring(0, 6);
                    }
                    if (!longcheckBox.Checked)
                    {
                        location = LocatortextBox.Text.Trim().Substring(0, 4);
                    }
                }
                Slot.PowerdB = defaultdB;
                if (SaveConfig(msgT))
                {
                    saved = true;
                }
            }

            Msg.OKMessageBox("Settings saved", "");


        }

        private void defaultpwrcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            defaultWtextBox.Text = dBtoWatts(defaultpwrcomboBox.SelectedItem.ToString());
            Slot.PowerdB = Convert.ToInt32(defaultpwrcomboBox.SelectedItem.ToString());
        }

        private bool SaveConfig(int msgT) //save configuration settings
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_configs";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            lock (_lock)
            {
                try
                {

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO settings(ConfigID,Callsign,BaseCall,Offset,DefaultF,Power,PowerW,Locator,LocatorLong,DefaultAnt,Alpha,DefaultAudio,HamlibPath,MsgType,AllowType2,oneMsg,WsprmsgPath,stopsolar,stopRX) ";
                    command.CommandText += "VALUES(@ConfigID,@Callsign,@BaseCall,@Offset,@DefaultF,@Power,@PowerW,@Locator,@LocatorLong,@DefaultAnt,@Alpha,@DefaultAudio,@HamlibPath,@MsgType,@AllowType2,@oneMsg,@WsprmsgPath,@stopsolar,@stopRX)";

                    connection.Open();

                    command.Parameters.AddWithValue("@ConfigID", configID); //normally zero
                    command.Parameters.AddWithValue("@Callsign", callsign);
                    command.Parameters.AddWithValue("@BaseCall", baseCalltextBox.Text);
                    command.Parameters.AddWithValue("@Offset", defaultoffset);
                    command.Parameters.AddWithValue("@DefaultF", defaultF);
                    command.Parameters.AddWithValue("@Power", defaultdB);
                    command.Parameters.AddWithValue("@PowerW", defaultW);

                    command.Parameters.AddWithValue("@Locator", full_location);
                    bool L = longcheckBox.Checked;
                    command.Parameters.AddWithValue("@LocatorLong", L);
                    command.Parameters.AddWithValue("@DefaultAnt", defaultAnt);
                    command.Parameters.AddWithValue("@Alpha", defaultAlpha);
                    command.Parameters.AddWithValue("@DefaultAudio", 1);

                    string HL = HamlibPath.Replace('\\', '/'); //make mysql friendly

                    command.Parameters.AddWithValue("@HamlibPath", HL);
                    command.Parameters.AddWithValue("@MsgType", msgT);

                    command.Parameters.AddWithValue("@AllowType2", Type2checkBox.Checked);
                    command.Parameters.AddWithValue("@oneMsg", asOnecheckBox.Checked);
                    string wsprmsgP = wsprmsgtextBox.Text;
                    wsprmsgP = wsprmsgP.Replace("\\", "/");
                    command.Parameters.AddWithValue("@WsprmsgPath", wsprmsgP);

                    command.Parameters.AddWithValue("@stopsolar", stopSolar);
                    command.Parameters.AddWithValue("@stopRX", stopRX);
                    string zone = "UTC";
                    if (LTcheckBox.Checked)
                    {
                        zone = "LT";
                    }
                    command.Parameters.AddWithValue("@TimeZone", zone);

                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                    if (UpdateConfig(msgT))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }
        private bool UpdateConfig(int msgT)
        {
            string c = "";
            int defA = 1;
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_configs";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                bool L = longcheckBox.Checked;
                string baseC = baseCalltextBox.Text;
                string zone = "UTC";
                if (LTcheckBox.Checked)
                {
                    zone = "LT";
                }
                string HL = HamlibPath.Replace('\\', '/'); //make mysql friendly

                string wsprmsgP = wsprmsgtextBox.Text;
                wsprmsgP = wsprmsgP.Replace("\\", "/");
                MySqlCommand command = connection.CreateCommand();
                c = "UPDATE settings SET ConfigID = " + configID + ", Callsign = '" + callsign + "', BaseCall = '" + baseC + "', Offset = " + defaultoffset + ", DefaultF = " + defaultF + ", ";
                c = c + "Power = " + defaultdB + ", PowerW = " + defaultW + ", Locator = '" + full_location + "', LocatorLong = " + L + ", DefaultAnt = '" + defaultAnt + "'";
                c = c + ", Alpha = " + defaultAlpha + ", DefaultAudio = " + defA + ", HamlibPath = '" + HL + "', MsgType = " + msgT;
                c = c + ", AllowType2 = " + Type2checkBox.Checked + ", oneMsg = " + asOnecheckBox.Checked + ", WsprmsgPath = '" + wsprmsgP + "', TimeZone = '" + zone + "', stopsolar = " + stopSolar + ", stopRX = " + stopRX + " WHERE settings.ConfigID = " + configID;

                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {   //exhausted insert and update
                connection.Close();
                Msg.OKMessageBox("Unable to save settings", "");
                return false;
            }

        }

        private void defaultOfftextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            int t;

            if (e.KeyChar == 45)
            {
                if (defaultOfftextBox.Text.Contains("-"))
                {
                    e.Handled = true;
                }
                return;
            }
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !"-".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
            else
            {
                int.TryParse(defaultOfftextBox.Text + e.KeyChar, out t);
                if (t > 220 || t < -20)
                {
                    Msg.OKMessageBox("Error: range 0-200 Hz", "");
                    e.Handled = true;
                }
            }
        }
        private void ReadConfig()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_configs";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {

                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM settings";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    //found = true;
                    configID = (int)Reader["ConfigID"];
                    callsign = (string)Reader["Callsign"];
                    base_call = (string)Reader["BaseCall"];
                    baseCalltextBox.Text = base_call;
                    defaultF = (double)Reader["DefaultF"];
                    defaultoffset = (int)Reader["Offset"];
                    defaultdB = (int)Reader["Power"];
                    //FList = (string)Reader["FList"];
                    full_location = (string)Reader["Locator"];
                    location = full_location;
                    bool L = (bool)Reader["LocatorLong"];
                    longcheckBox.Checked = L;
                    defaultAnt = (string)Reader["DefaultAnt"];
                    defaultAlpha = (double)Reader["Alpha"];
                    defaultAudio = (int)Reader["DefaultAudio"];
                    string HL = (string)Reader["HamlibPath"];
                    int msgT = (int)Reader["MsgType"];
                    string zone = (string)Reader["TimeZone"];
                    Type2checkBox.Checked = (bool)Reader["AllowType2"];
                    asOnecheckBox.Checked = (bool)Reader["oneMsg"];
                    string wsprmsgP = (string)Reader["WsprmsgPath"];
                    stopSolar = (bool)Reader["stopsolar"];
                    solarcheckBox.Checked = !stopSolar;
                    stopRX = (bool)Reader["stopRX"];
                    stopRXcheckBox.Checked = stopRX;

                    if (OpSystem == 0)
                    {
                        wsprmsgP = wsprmsgP.Replace('/', '\\');
                    }

                    wsprmsgtextBox.Text = wsprmsgP;
                    if (zone == "LT")
                    {
                        LTcheckBox.Checked = true;
                    }
                    else
                    {
                        LTcheckBox.Checked = false;
                    }
                    msgTypelabel.Text = "Message type 1";
                    if ((msgT == 2 || msgT == 3) && Type2checkBox.Checked)
                    {
                        msgTypelabel.Text = "Message type 2/3";
                    }
                    msgTypelabel2.Text = msgTypelabel.Text;


                    if (OpSystem == 0) //windows
                    {
                        HamlibPath = HL.Replace('/', '\\');
                    }
                    else //linux
                    {
                        HamlibPath = HL;
                    }

                    RigCtlPathtextBox.Text = HamlibPath;

                    CalltextBox.Text = callsign;
                    LocatortextBox.Text = location;
                    defaultFcomboBox.Text = Convert.ToString(defaultF);
                    defaultOfftextBox.Text = Convert.ToString(defaultoffset);
                    defaultpwrcomboBox.Text = Convert.ToString(defaultdB);
                    //defaultAnttextBox.Text = defaultAnt;
                    DefaultAntcomboBox.Text = defaultAnt;
                    AlphacomboBox.Text = Convert.ToString(defaultAlpha);

                }
                Reader.Close();
                connection.Close();
            }
            catch
            {
                Msg.TMessageBox("Unable to load settings", "", 1000);
                connection.Close();
            }
        }

        private bool SaveAntenna(int antNo) //save configuration settings
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            lock (_lock)
            {
                try
                {

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO antennas(AntNo,Antenna,Description,Switch,Tuner,SwitchPort) ";
                    command.CommandText += "VALUES(@AntNo,@Antenna,@Description,@Switch,@Tuner,@SwitchPort)";


                    connection.Open();

                    int sw = 0;
                    int tu = 0;

                    //int antNo = AntlistBox.Items.Count; //add new antenna to listbox
                    string ant = AntnametextBox.Text.Trim();
                    string desc = AntdesctextBox.Text;
                    int swP = 0;
                    if (ShowSwlistBox.SelectedIndex < 0)
                    {
                        sw = 0;
                    }
                    else
                    {
                        sw = ShowSwlistBox.SelectedIndex;
                    }
                    if (ShowTulistBox.SelectedIndex < 0)
                    {
                        tu = 0;
                    }
                    else
                    {
                        tu = ShowTulistBox.SelectedIndex;
                    }
                    if (AntPortlistBox.SelectedIndex < 0)
                    {
                        swP = 0;
                    }
                    else
                    {
                        swP = AntPortlistBox.SelectedIndex;
                    }
                    command.Parameters.AddWithValue("@AntNo", antNo);
                    command.Parameters.AddWithValue("@Antenna", ant);
                    command.Parameters.AddWithValue("@Description", desc);
                    command.Parameters.AddWithValue("@Switch", sw);
                    command.Parameters.AddWithValue("@Tuner", tu);
                    command.Parameters.AddWithValue("@SwitchPort", swP);

                    command.ExecuteNonQuery();

                    connection.Close();
                    return true;
                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                    if (UpdateAntenna(antNo))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }
        private bool UpdateAntenna(int antNo)
        {
            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {

                int sw = 0;
                int tu = 0;
                int swP = 0;
                string ant = AntnametextBox.Text;
                string desc = AntdesctextBox.Text;
                if (ShowSwlistBox.SelectedIndex < 0)
                {
                    sw = 0;
                }
                else
                {
                    sw = ShowSwlistBox.SelectedIndex;
                }
                if (ShowTulistBox.SelectedIndex < 0)
                {
                    tu = 0;
                }
                else
                {
                    tu = ShowTulistBox.SelectedIndex;
                }
                if (AntPortlistBox.SelectedIndex < 0)
                {
                    swP = 0;
                }
                else
                {
                    swP = AntPortlistBox.SelectedIndex;
                }

                MySqlCommand command = connection.CreateCommand();
                c = "UPDATE antennas SET Antenna = '" + ant + "', Description = '" + desc + "', Switch = " + sw + ", ";
                c = c + "Tuner = " + tu + ", SwitchPort = " + swP;
                c = c + " WHERE antennas.AntNo = " + antNo;

                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {   //exhausted insert and update
                connection.Close();
                Msg.OKMessageBox("Unable to save antenna settings", "");
                return false;
            }

        }
        private void ReadAntennas()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {

                int antNo;
                string antenna;
                string antDescription;
                int antSwitch;
                int antTuner;
                int antSwPort;
                Antenna A;
                Ant.Clear();

                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM antennas";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {

                    //found = true;
                    antNo = (int)Reader["AntNo"];
                    antenna = (string)Reader["Antenna"];
                    antDescription = (string)Reader["Description"];
                    antSwitch = (int)Reader["Switch"];
                    antTuner = (int)Reader["Tuner"];
                    antSwPort = (int)Reader["SwitchPort"];
                    A.AntNo = antNo;
                    A.AntName = antenna;
                    A.Description = antDescription;
                    A.Switch = antSwitch;
                    A.Tuner = antTuner;
                    A.SwitchPort = antSwPort;

                    string ant = antenna.PadRight(40, ' ');
                    ant = ant.Substring(0, 40);
                    ant = ant + "\t" + antSwitch.ToString() + "\t" + antTuner.ToString(); // + "\t" + antSwPort.ToString() + "\t" + antDescription;

                    AntlistBox.Items.Add(ant);
                    DefaultAntcomboBox.Items.Add(antenna);
                    AntselcomboBox.Items.Add(antenna);
                    Ant.Add(A);

                }
                Reader.Close();
                connection.Close();
            }
            catch
            {
                Msg.TMessageBox("Unable to load settings", "", 1000);
                connection.Close();
            }
        }

        private bool DeleteAntenna(int sel)
        {
            string c = "";

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                string ant = AntlistBox.SelectedItem.ToString();
                string[] A = ant.Split('\t');

                //c = "DELETE FROM antennas WHERE Antenna = '" + A[0].Trim() + "'";
                c = "DELETE FROM antennas WHERE AntNo = " + Ant[sel].AntNo;

                command.CommandText = c;

                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch
            {
                Msg.OKMessageBox("Error deleting antenna", "");
                connection.Close();
                return false;
            }
        }

        private void ReadFrequencies()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                double freq;
                double level;
                string antenna;

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM frequencies";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();
                FreqlistBox.Items.Clear();

                while (Reader.Read())
                {
                    //found = true;
                    freq = (double)Reader["Frequency"];
                    level = (double)Reader["AudioLevel"];
                    string lvl = level.ToString();
                    string F = freq.ToString() + "\t\t" + lvl;

                    FreqlistBox.Items.Add(F);

                }
                Reader.Close();
                connection.Close();
            }
            catch
            {
                Msg.TMessageBox("Unable to load frequenciess", "", 1000);
                connection.Close();
            }
        }
        private bool SaveFrequency(string freq) //save configuration settings
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            lock (_lock)
            {
                try
                {

                    MySqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "INSERT INTO frequencies(Frequency,AudioLevel) ";
                    command.CommandText += "VALUES(@Frequency,@AudioLevel)";

                    double f;
                    double value;
                    if (freq == "")
                    {
                        f = Convert.ToDouble(FtextBox.Text.Trim());
                        value = Convert.ToDouble(Levellabel.Text);
                    }
                    else
                    {
                        f = Convert.ToDouble(freq);
                        value = 1.0;
                    }


                    command.Parameters.AddWithValue("@Frequency", f);
                    command.Parameters.AddWithValue("@AudioLevel", value);


                    command.ExecuteNonQuery();

                    connection.Close();
                    return true;
                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                    if (UpdateFrequency())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }
        private bool UpdateFrequency()
        {

            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                double freq = Convert.ToDouble(FtextBox.Text.Trim());

                double value = Convert.ToDouble(Levellabel.Text);


                MySqlCommand command = connection.CreateCommand();

                c = "UPDATE frequencies SET Frequency = " + freq + ", AudioLevel = " + value + " WHERE frequencies.Frequency = " + freq;
                //UPDATE `slots` SET `Antenna` = 'GP' WHERE `slots`.`Date` = '2025-02-28' AND `slots`.`Time` = '16:02:00'; 
                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {   //exhausted insert and update
                connection.Close();
                Msg.OKMessageBox("Unable to save frequency settings", "");
                return false;
            }
        }

        private bool DeleteFrequency(string freq)
        {
            string c = "";

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                if (freq == "")
                {
                    freq = FreqlistBox.SelectedItem.ToString();
                }
                string[] F = freq.Split('\t');


                c = "DELETE FROM frequencies WHERE Frequency = " + F[0].Trim();

                command.CommandText = c;

                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch
            {
                Msg.OKMessageBox("Error deleting frequency", "");
                connection.Close();
                return false;
            }
        }






        private void daytimer_Tick(object sender, EventArgs e)
        {
            daytimer_Action();
        }
        private async void daytimer_Action()
        {
            DateTime now;
            DateTime LT = DateTime.Now;
            if (LTcheckBox.Checked)
            {
                now = DateTime.Now;

            }
            else
            {
                now = DateTime.Now.ToUniversalTime();

            }
            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;

            keypresses++;
            if (keypresses > 180 && (s == 4) && !slotgroupBox.Visible) //if 3 minutes passed without keypress and not editing a slot 
            {
                currHour(true, trackSlotscheckBox.Checked); // update the date/time if no-one has been working in the app in 5 minutes
                if (!trackSlotscheckBox.Checked) { keypresses = 0; } // update the date/time if no-one has been working in the app in 5 minutes (track exact slot time if no activity)
                //noSkedcheckBox.Checked = false;
            }



            DateTime nextT;
            LTlabel.Text = LT.ToString("HH:mm").Trim();


            DateTime hourmin;
            hourmin = now;

            string time = hourmin.ToString("HH:mm");

            string fulltime = now.ToString("HH:mm:ss");

            CurrTime = fulltime;
            Timelabel.Text = fulltime;
            rxForm.set_time(fulltime);


            /*if (s == 19)
            {
                if (!checkSlotDB())
                {
                    Msg.TMessageBox("Error: Cannot access slot database", "Database error", 3000);
                    daytimer.Stop();
                    daytimer.Enabled = false;
                    idletimer.Enabled = true;
                    idletimer.Start();
                }
            }*/
            if (now.DayOfYear % 30 == 0 && h == 2 && m == 11 && s == 20)
            {
                updateSunriseSunset();
            }
            if (s == 30)
            {
                string process = "rigctld";
                if (Process.GetProcessesByName(process).Length > 0)
                {
                    rigrunlabel.Text = "rigctld running";

                }
                else
                {
                    rigrunlabel.Text = "rigctld not running";
                }
                Riglabel.Text = rigrunlabel.Text;
                Riglabel1.Text = rigrunlabel.Text;
            }
            if (s == 40)
            {
                if (now.DayOfYear > prevDayofYear)
                {
                    monthCalendar1.TodayDate = now;
                    //monthCalendar1.SetDate(now);

                    Datelabel.Text = now.ToString("ddd-dd-MMM-yyyy");
                    prevDayofYear = now.DayOfYear;
                }
            }


            if (m % 2 == 1 && s == 27)
            {
                random = randno.Next(0, 7); //random number to spread uploads to wsprnet
            }
            if (s == 2 || s == 38)
            {
                getRigF();
            }

            if (((m % 2 == 1 && s == 59) || (m % 2 == 0 && s == 0)) || (m % 2 == 0 && s == 1))
            {
                if (!recordFlag)
                {
                    recordFlag = true;
                    if (enableTXcheckBox.Checked && slotActive)
                    {

                    }
                    else
                    {
                        if (!stopRX) { await rxForm.Start_Receive(OpSystem); } //start recording from RX
                    }
                }

            }
            if ((m % 2 == 0 && (s >= 7 && s <= 9) && recordFlag))
            {
                recordFlag = false;   //use flag to account for possible missed timer ticks
            }
            if (m % 2 == 1 && s == 48)
            {

                rxForm.set_prev_frequency();    //prevent RX decode being recorded for wrong freq
            }



            if (m % 2 == 0 && (s > 2 && s < 5))
            {
                Flag = false;
            }

            if ((m % 2 == 1 && (s == 52 || s == 53 || s == 54) && !Flag) || justLoaded) //if odd minute and 53/4 second past minute
            {

                nextT = now.AddMinutes(1);
                string nexttime = nextT.ToString("HH:mm:00");

                Flag = true;
                DateTime d = now.Date;

                string date = d.ToString("yyyy-MM-dd");


                showmsg = true;
                databaseError = false;
                slotFound = false;
                //if (await (findSlot(-1, date, nexttime)))
                if (findSlot(-1, date, nexttime))
                {
                    if (noSkedcheckBox.Checked)
                    {
                        Msg.TMessageBox("Schedule not enabled!", "Slot Scheduler", 2500);
                        return;
                    }
                    if (noRigctld)
                    {
                        Msg.TMessageBox("Ignoring slot frequency - RigCtlD disabled", "Frequency", 1000);
                    }
                    if (noSkedcheckBox.Checked)
                    {
                        Msg.TMessageBox("Slot schedule not enabled", "", 800);
                        return;
                    }
                    if (!enableTXcheckBox.Checked && slotActive)
                    {
                        //do nothing
                        Msg.TMessageBox("Warning: TX not enabled", "TX Status", 4000);
                        slotActive = false;
                    }
                    if (!checkRigctld() && !justLoaded)
                    {
                        Msg.TMessageBox("Error: RigCtld not running", "", 3000);
                    }
                    else
                    {
                        blockTXonErr = false; //unblock old errors
                        WSPRtimer.Enabled = true;
                        WSPRtimer.Start(); //start the time to starty the TX 
                        prepDone = false;

                    }
                }
                justLoaded = false;

            }


        }


        private int decodeDelay()
        {
            int dly = 0;
            if (rxForm.useDeep)
            {
                dly = 12;
            }
            if (rxForm.OSD > 0)
            {
                dly = dly + (rxForm.OSD * 2);
            }
            dly = dly + 4 + random;
            return dly;
        }

        private async void getRigF()
        {
            string MHz = "";
            double mhz = 0;
            string f = "";
            string btnText = "RX/TX Idle";

            try
            {
                if (!noRigctld)
                {
                    await Task.Run(() =>
                    {
                        var F = sendRigCommand("f", false);
                        if (F.Status == TaskStatus.Faulted)
                        {
                            f = "";
                        }
                        else
                        {
                            f = F.Result;
                        }

                    });
                    f = f.Replace('\n', ' ').Trim();
                }
                else
                {
                    if (FlistBox2.SelectedIndex > -1)
                    {
                        f = FlistBox2.SelectedItem.ToString();
                    }
                    else
                    {
                        f = Convert.ToString(defaultF);

                    }

                }

                if (!noRigctld)
                {
                    if (f.All(char.IsDigit))
                    {

                        double.TryParse(f, out mhz);
                        if (mhz > 0 && mhz != null)
                        {
                            mhz = mhz / 1000000;
                            MHz = mhz.ToString();
                        }
                    }
                }
                else
                {
                    MHz = f;
                }


                //if ((wsprTXtimer.Enabled || testPTTtimer.Enabled) && enableTXcheckBox.Checked && slotActive) //is it transmitting?
                if (enableTXcheckBox.Checked && slotActive) //is it transmitting?
                {
                    btnText = "TX: " + MHz + " MHz";
                }
                if (wsprTXtimer.Enabled || testPTTtimer.Enabled)
                {
                    btnText = "TX: " + MHz + " MHz";
                }
                else
                {
                    btnText = "RX: " + MHz + " MHz";
                    //rxForm.set_frequency(MHz);
                }

                if (!noRigctld)
                {
                    await rxForm.set_frequency(MHz);
                }
                //rxForm.Frequency = MHz;
                TXrunbutton.Text = btnText;
                TXrunbutton2.Text = btnText;

            }
            catch
            {
                await rxForm.set_frequency(MHz);
                TXrunbutton.Text = btnText;
                TXrunbutton2.Text = btnText;
            }
        }



        private void testButton_Click(object sender, EventArgs e)
        {
            //var fsk = new FSK.FSKMod();
            StartTX(true);
        }


        private async void PTT(bool TX)
        {
            // PTT is a value: ‘0’ (RX), ‘1’ (TX), ‘2’ (TX mic), or ‘3’ (TX data). 
            if (!noRigctld)
            {
                string PTT = "T 0";
                if (TX)
                {
                    PTT = "T 1";
                }
                await Task.Run(() =>
                {
                    var reply = sendTXRigCommand(PTT);
                    string R = reply.ToString();
                    if (R.StartsWith("RPRT -"))
                    {
                        Msg.TMessageBox("Unable to key TX", "", 1000);
                    }
                    else if (R == "error")
                    {
                        blockTXonErr = true;
                    }

                });
            }
            else
            {
                Msg.TMessageBox("PTT operated by VOX", "", 1000);

            }
        }
        private async Task<string> sendTXRigCommand(string msg) //send a TX message to RigCtlD and wait for reply
        {
            string ip = RigctlIPv4;
            string port = RigctlPort;

            if (!blockTXonErr)
            {
                if (!noRigctld)
                {
                    var rig = new RigControlDaemon.RigCtlD(ip, port, msg);

                    if ((rig.reply == "error") && !blockTXonErr)
                    {
                        blockTXonErr = true;
                        Msg.TMessageBox("Error contacting RigCtlD - is it running?", "Warning", 4000);
                        return rig.reply;
                    }
                    else
                    {
                        return "ok";
                    }
                }
                else
                {
                    return "ok";
                }


            }
            else
            {
                return "error";
            }

        }

        private async Task<string> sendRigCommand(string msg, bool showerror) //send a message to RigCtlD and wait for reply
        {
            string ip = RigctlIPv4;
            string port = RigctlPort;

            var rig = new RigControlDaemon.RigCtlD(ip, port, msg);

            if (rig.reply == "error")
            {

                if (showerror)
                {
                    Msg.TMessageBox("Error contacting RigCtlD - is it running?", "Warning", 4000);
                }
                return rig.reply;
            }
            else
            {
                return rig.reply;
            }
        }

        private void testFtextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double f = Convert.ToDouble(testFtextBox.Text);
                f = f * 1000000;
                TXFrequency = Convert.ToString(f);
            }
            catch
            {
                TXFrequency = Convert.ToString(defaultF * 1000000);
            }

        }

        private async void startRigCtlbutton_Click(object sender, EventArgs e)
        {
            try
            {
                if (rigctldcheckBox.Checked)
                {
                    Msg.TMessageBox("RigCtlD is not enabled", "", 2000);
                    return;
                }
                runRigCtlD();
                //Task.Delay(300);           
                await Task.Delay(1000);
                bool found = false;
                if (checkRigctld())
                {
                    found = true;
                }

                if (found)
                {
                    Msg.TMessageBox("RigCtlD started successfully", "", 3000);

                }
                else
                {
                    //Msg.OKMessageBox("Unable to start RigCtlD", "");
                }
            }
            catch
            {

            }


        }

        private void runRigCtlD()
        {
            if (OpSystem == 0) //windows
            {
                startRigCtlD();
            }
            else //linux
            {
                startRigCtlDLinux();
            }

        }
        private async void startRigCtlD()
        {
            //rigctld -m <rig> -r <ip address> -t <port> - 1046=FT450, 127.0.0.1, 4532

            string rigctldfile = userdir + slash + "rigctld_launch.bat";
            string process1 = "rigctld";
            string process2 = "rigctld.exe";
            bool running = false;
            try
            {
                string rpath = RigCtlPathtextBox.Text.Trim();
                string content = rpath + slash + "rigctld";
                string args = "-m " + Radio + " -r " + RigctlCOM + " -s " + Rigctlbaud + " -T " + RigctlIPv4 + " -t " + RigctlPort;


                await Task.Run(() =>
                 {

                     runAsyncProcess(content, args);


                 });
                await Task.Delay(2000);
                if (Process.GetProcessesByName(process1).Length > 0)
                {
                    running = true;
                }
                if (Process.GetProcessesByName(process2).Length > 0)
                {
                    running = true;
                }
                if (running)
                {
                    rigrunlabel.Text = "rigctld running";

                }
                else
                {
                    rigrunlabel.Text = "rigctld not running";
                }
                Riglabel.Text = rigrunlabel.Text;
                Riglabel1.Text = rigrunlabel.Text;

            }
            catch { }
        }

        private async void startRigCtlDLinux()
        {
            //rigctld -m <rig> -r <ip address> -t <port> - 1046=FT450, 127.0.0.1, 4532

            string rigctldfile = userdir + slash + "rigctld_launch.bat";
            string process1 = "rigctld";
            string process2 = "rigctld.exe";
            bool running = false;
            try
            {
                string radio = Radio.Trim(' ');
                string[] str = radio.Split(' ');
                radio = str[0].Trim(' ');
                string rigctld = "rigctld";
                string args = "-m " + radio + " -r " + RigctlCOM + " -s " + Rigctlbaud + " -T " + RigctlIPv4 + " -t " + RigctlPort + " &";
                await Task.Run(() =>
                {

                    runAsyncProcess(rigctld, args);


                });
                if (Process.GetProcessesByName(process1).Length > 0)
                {
                    running = true;
                }
                if (Process.GetProcessesByName(process2).Length > 0)
                {
                    running = true;
                }
                if (running)
                {
                    rigrunlabel.Text = "rigctld running";

                }
                else
                {
                    rigrunlabel.Text = "rigctld not running";
                    Msg.TMessageBox("Unable to start RigCtlD - check Hamlib path", "", 3000);
                }
                Riglabel.Text = rigrunlabel.Text;
                Riglabel1.Text = rigrunlabel.Text;

            }
            catch { }
        }





        private async Task runAsyncProcess(string cmd, string args)
        {
            try
            {

                ProcessStartInfo processInfo = new ProcessStartInfo()
                {
                    FileName = cmd, // Command to run
                    Arguments = args, // Arguments for the command
                    RedirectStandardOutput = false, // Redirect output if needed
                    RedirectStandardError = false,  // Redirect error stream if needed
                    UseShellExecute = false,       // Necessary for redirection
                    CreateNoWindow = true,          // Run without a visible window
                                                    //WorkingDirectory = @"C:\Program Files\hamlib-w64-4.6.2\bin"
                };


                Process process = new Process
                {
                    StartInfo = processInfo
                };

                // Start the process
                process.Start();


                process.WaitForExit();

                //return true;
            }
            catch (Exception ex)
            {
                Msg.OKMessageBox(ex.Message, "");
                //return false;
            }

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
        private bool checkRigctld()
        {
            if (noRigctld)
            {
                return true;
            }
            bool found = false;
            string process = "rigctld.exe";
            foreach (Process proc in Process.GetProcessesByName(process))
            {
                found = true;
            }
            process = "rigctld";
            foreach (Process proc in Process.GetProcessesByName(process))
            {
                found = true;
            }
            if (found)
            {
                rigrunlabel.Text = "rigctld running";
            }
            else
            {
                rigrunlabel.Text = "rigctld not running";
            }
            return found;

        }


        private void stopRigCtlDbutton_Click(object sender, EventArgs e)
        {
            stopRigCtlD(false);  //false = form not closing
        }
        private void stopRigCtlD(bool closing)
        {
            string process = "rigctld";
            bool found = false;

            foreach (Process proc in Process.GetProcessesByName(process))
            {
                found = true;
                proc.Kill();
            }
            if (!closing)
            {

                if (found)
                {
                    Msg.TMessageBox("Stopped RigCtlD", "", 3000);
                }
                else
                {
                    Msg.TMessageBox("RigCtlD not running", "", 2000);
                }
                rigrunlabel.Text = "rigctld not running";
                Riglabel.Text = rigrunlabel.Text;
                Riglabel1.Text = rigrunlabel.Text;
            }
        }

        private async void TestTXbutton_Click(object sender, EventArgs e)
        {

            if (rigctldcheckBox.Checked)
            {
                testFtextBox.Text = defaultF.ToString();
            }
            if (testFtextBox.Text == "" && !rigctldcheckBox.Checked)
            {
                Msg.OKMessageBox("No frequency selected", "");
                return;
            }
            if (!checkRigctld())
            {
                Msg.OKMessageBox("Error: RigCtld not running", "");
                return;
            }
            if (!enableTXcheckBox.Checked)
            {
                Msg.OKMessageBox("TX not enabled", "");
                return;
            }

            blockTXonErr = false; //unblock any errors contacting rigctld

            double f = Convert.ToDouble(testFtextBox.Text);
            f = f * 1000000; //convert to MHz
            await selectAntenna();
            //await activateAntSwitch(DefaultAntcomboBox.Text);
            await activateTX(f.ToString());
            Task.Delay(2000);
            //PTT(true); //key TX
            flatcode = true;

            testPTTtimer.Enabled = true;
            testPTTtimer.Start();
            slotActive = true;
            showMsgType(1);
            StartTX(false);

        }

        private void testPTTtimer_Tick(object sender, EventArgs e)
        {
            testPTTtimer.Stop();

            PTT(false); //stop TX test after 5 seconds
                        //  daytimer2.Stop();
            daytimer.Enabled = true;
            daytimer.Start();
            daytimer2.Stop();
            daytimer2.Enabled = false;
            testPTTtimer.Enabled = false;
            slotActive = false;
            flashTX(false);


        }

        private void flashTX(bool slot)
        {
            Color flashOn = Color.OrangeRed;
            Color flashOff = Color.Orange;
            string TXRX = "TX: ";
            string TXRXtime = "TX time: ";
            if (slot)
            {

                if (slotActive && enableTXcheckBox.Checked)
                {
                    if (TXrunbutton.BackColor == flashOn)
                    {
                        TXrunbutton.BackColor = flashOff;
                        TXrunbutton2.BackColor = flashOff;
                    }
                    else
                    {
                        TXrunbutton.BackColor = flashOn;
                        TXrunbutton2.BackColor = flashOn;
                    }
                }
                else
                {
                    if (noRigctld)
                    {
                        TXrunbutton.BackColor = Color.DarkKhaki;
                        TXrunbutton2.BackColor = Color.DarkKhaki;
                    }
                    else
                    {
                        TXrunbutton.BackColor = Color.RoyalBlue;
                        TXrunbutton2.BackColor = Color.RoyalBlue;
                    }
                    TXRX = "RX: ";
                }

                if (slotActive)
                {
                    TXRXtime = "TX time: ";
                }
                else
                {
                    TXRXtime = "RX time: ";
                }
                if (countdown != 111)
                {
                    countdownlabel.Text = TXRXtime + countdown.ToString();
                    countdownlabel2.Text = TXRXtime + countdown.ToString();
                }
                countdown++;
            }
            else
            {
                TXrunbutton.BackColor = Color.Olive;
                TXrunbutton.Text = "TX/RX idle";
                TXrunbutton2.BackColor = Color.Olive;
                TXrunbutton2.Text = "TX/RX idle";
                countdownlabel.Text = "";
                countdownlabel2.Text = "";
                countdown = 0;
            }
        }
        private void getComports()
        {

            string[] ports = SerialPort.GetPortNames();
            COMcomboBox.Items.Clear();
            COMcomboBox.Items.AddRange(ports);
            HwPortcomboBox.Items.Clear();
            HwPortcomboBox.Items.AddRange(ports);


        }

        private void comRbutton_Click(object sender, EventArgs e)
        {
            try
            {
                int c = COMcomboBox.SelectedIndex;
                int r = RigcomboBox.SelectedIndex;

                getComports();
                getRigList();
                COMcomboBox.SelectedIndex = c;
                RigcomboBox.SelectedIndex = r;
            }
            catch { }
        }
        private void getRigList()
        {
            //riglistBox.Items.Clear();
            RigcomboBox.Items.Clear();

            string command = RigCtlPathtextBox.Text.Trim() + slash + "rigctl";
            string args = "-l";
            if (OpSystem == 0) //windows
            {

            }
            else
            {
                //Linux etc.
                command = "rigctl";
            }

            try
            {
                // Start the process
                Process process = new Process();
                process.StartInfo.FileName = command; //"rigctl";
                process.StartInfo.Arguments = args;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                // Collect output
                List<string> output = new List<string>();
                process.OutputDataReceived += (sender, args) => output.Add(args.Data);

                process.Start();
                process.BeginOutputReadLine();


                process.WaitForExit();
                foreach (string line in output)
                {
                    if (line != null)
                    {
                        //riglistBox.Items.Add(line);
                        RigcomboBox.Items.Add(line);
                    }
                }

                int index = RigcomboBox.Items.IndexOf(RigcomboBox.Text);
                RigcomboBox.SelectedIndex = index;

            }
            catch (Exception e)
            {
                //MessageBox.Show("Unable to read rig list");
                Msg.OKMessageBox(e.Message, "");
            }
        }

        private void COMcomboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }


        private void baudcomboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void RigcomboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private async void SaveRigctlbutton_Click(object sender, EventArgs e)
        {
            SaveRigctlButton_Action();
        }
        private async void SaveRigctlButton_Action()
        {
            if (SaveRigctl())
            {
                if (!rigctldcheckBox.Checked)
                {
                    RigctlCOM = COMcomboBox.SelectedItem.ToString();
                    Rigctlbaud = baudcomboBox.SelectedItem.ToString();
                    RigctlPort = PorttextBox.Text;
                    RigctlIPv4 = IPtextBox.Text;
                    Radio = RigcomboBox.SelectedItem.ToString();

                    string rigctldfile = "";
                    string content = "";
                    string args = "";
                    string rpath = RigCtlPathtextBox.Text.Trim();
                    if (OpSystem == 0)
                    {
                        content = rpath + slash + "rigctld";
                        args = "-m " + Radio + " -r " + RigctlCOM + " -s " + Rigctlbaud + " -T " + RigctlIPv4 + " -t " + RigctlPort;
                    }
                    else
                    {
                        //Linux etc.
                        content = "rigctld";
                        args = "-m " + Radio + " -r " + RigctlCOM + " -s " + Rigctlbaud + " -T " + RigctlIPv4 + " -t " + RigctlPort + " &";
                    }

                    await Task.Run(() =>    //update rigctld file and run it
                    {
                        runAsyncProcess(content, args);

                    });
                }
                Msg.TMessageBox("Settings saved", "rigctld", 2000);
            }
        }

        private bool SaveRigctl()
        {

            if (!rigctldcheckBox.Checked)
            {
                if (RigcomboBox.SelectedIndex < 0 || COMcomboBox.SelectedIndex < 0 || baudcomboBox.SelectedIndex < 0 || IPtextBox.Text == "" || PorttextBox.Text == "")
                {
                    Msg.OKMessageBox("Error: some items not selected", "");
                    return false;

                }
            }

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            lock (_lock)
            {
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO rigctl(RigctlID, Radio, COMport, Baud, IPv4, Port,norigctld) ";
                    command.CommandText += "VALUES(@RigctlID, @Radio, @COMport, @Baud, @IPv4, @Port,@norigctld)";

                    connection.Open();

                    command.Parameters.AddWithValue("@RigctlID", 0);
                    command.Parameters.AddWithValue("@Radio", RigcomboBox.SelectedItem);
                    command.Parameters.AddWithValue("@COMport", COMcomboBox.SelectedItem);
                    command.Parameters.AddWithValue("@Baud", baudcomboBox.SelectedItem);
                    command.Parameters.AddWithValue("@IPv4", IPtextBox.Text);
                    command.Parameters.AddWithValue("@Port", PorttextBox.Text);
                    command.Parameters.AddWithValue("@norigctld", noRigctld);


                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch
                {         //if already exists then try updating it in database
                    connection.Close();
                    return UpdateRigctl();

                }
            }
        }
        private bool UpdateRigctl()
        {
            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                c = "UPDATE rigctl SET Radio = '" + RigcomboBox.SelectedItem + "', COMport = '" + COMcomboBox.SelectedItem + "', ";
                c = c + "Baud = '" + baudcomboBox.SelectedItem + "', IPv4 = '" + IPtextBox.Text + "', Port = '" + PorttextBox.Text + "', norigctld = " + noRigctld;
                c = c + " WHERE RigctlID = 0";

                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {   //exhausted insert and update
                connection.Close();
                Msg.OKMessageBox("Unable to save settings", "");
                return false;
            }
        }
        private bool readRigctl()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";

            MySqlConnection connection = new MySqlConnection(myConnectionString);


            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM rigctl";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    //found = true;
                    //configID = (int)Reader["RigctlID"];
                    Radio = (string)Reader["Radio"];
                    RigctlCOM = (string)Reader["COMport"];
                    Rigctlbaud = (string)Reader["Baud"];
                    RigctlIPv4 = (string)Reader["IPv4"];
                    RigctlPort = (string)Reader["Port"];
                    rigctldcheckBox.Checked = (bool)Reader["norigctld"];
                    RigcomboBox.SelectedItem = Radio;
                    RigcomboBox.Text = Radio;

                    COMcomboBox.SelectedItem = RigctlCOM;
                    baudcomboBox.SelectedItem = Rigctlbaud;
                    IPtextBox.Text = RigctlIPv4;
                    PorttextBox.Text = RigctlPort;

                }
                Reader.Close();
                connection.Close();
                return true;
            }
            catch
            {
                Msg.TMessageBox("Unable to load settings", "", 1000);
                connection.Close();
                return false;
            }
        }



        private void wsprTXtimer_Tick(object sender, EventArgs e)
        {
            wsprTXtimer_Action();
        }

        private void wsprTXtimer_Action()
        {
            //Flag = false;
            daytimer2.Stop();
            daytimer.Enabled = true;
            daytimer.Start();
            daytimer2.Enabled = false;
            stopPlay = true;
            Task.Delay(100);
            PTT(false);
            wsprTXtimer.Stop();
            wsprTXtimer.Enabled = false;
            slotActive = false;
            flashTX(false);
            mtypelabel.Text = "-";
            slotnolabel.Text = "-";
        }
        private void stopTestbutton_Click(object sender, EventArgs e)
        {
            stopTX();
        }
        private void stopTX()
        {
            PTT(false);
            daytimer2.Stop();
            daytimer.Enabled = true;
            daytimer.Start();
            daytimer2.Enabled = false;
            stopPlay = true;
            Task.Delay(400);
            PTT(false);

            wsprTXtimer.Stop();
            wsprTXtimer.Enabled = false;

            WSPRtimer.Stop();
            WSPRtimer.Enabled = false;
            testPTTtimer.Stop();
            testPTTtimer.Enabled = false;
            flashTX(false);
            mtypelabel.Text = "-";
            slotnolabel.Text = "-";

        }

        private void enableTXcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (enableTXcheckBox.Checked)
            {
                enableTXcheckBox.BackColor = Color.Red;
                TXenablebutton.BackColor = Color.Red;
                TXenablebutton.Text = "TX Enabled";
            }
            else
            {
                enableTXcheckBox.BackColor = Color.Green;
                TXenablebutton.BackColor = Color.Green;
                TXenablebutton.Text = "Enable TX";
            }
        }

        private void stopWAVbutton_Click(object sender, EventArgs e)
        {
            stopPlay = true;
        }

        private void defaultAnttextBox_TextChanged(object sender, EventArgs e)
        {
            defaultAnt = DefaultAntcomboBox.Text;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void AlphacomboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                defaultAlpha = Convert.ToDouble(AlphacomboBox.SelectedItem);
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            testArduino();

        }
        private async void testArduino()
        {
            string reply = "";
            string ip = "192.168.0.207";
            int port = 5000;
            ArduinoPorttextBox.Text = "5000";
            int pin = 0;
            string S = "error";
            HiLolabel.Text = "pending";
            bool ok = false;
            ardReplylabel.Text = "Test in progress - please wait";

            if (checkIP(ArduinoIPtextBox.Text) || ArduinoPorttextBox.Text != "" || pintextBox.Text != "")
            {
                ip = ArduinoIPtextBox.Text;


                port = Convert.ToInt32(ArduinoPorttextBox.Text);
                if (port < 1 || port > 65535)
                {
                    Msg.OKMessageBox("Invalid TCP port", "");
                    return;
                }
            }
            else
            {
                Msg.OKMessageBox("Invalid IP,port or pin", "");
                return;
            }
            try
            {

                pin = Convert.ToInt32(pintextBox.Text);
                if (!LEDcheckBox.Checked)
                {
                    S = "Pin " + pintextBox.Text + " high";
                    LEDcheckBox.Checked = true;
                }
                else
                {
                    S = S = "Pin " + pintextBox.Text + " low";
                    LEDcheckBox.Checked = false;
                }
                bool high = LEDcheckBox.Checked;
                await Task.Run(() =>
                {
                    ArduinoComms ard = new ArduinoComms(ip, port, pin, high);
                    reply = ard.response;
                    ok = ard.ok;
                });

            }
            catch
            {

            }

            Msg.TMessageBox("Received: " + reply, "Switch reply", 2000);
            if (reply != "Switching error".Trim() && reply != "" && ok)
            {
                ardReplylabel.Text = "Test ok!";
                HiLolabel.Text = S;
            }
            else
            {
                ardReplylabel.Text = "Test fail?";
                HiLolabel.Text = "error";
            }
        }



        private void daytimer2_Tick(object sender, EventArgs e)
        {
            daytimer2_Action(); //keep displaying time, but don't process anythign else
        }
        private async void daytimer2_Action()
        {
            DateTime now;
            DateTime LT = DateTime.Now;
            if (LTcheckBox.Checked)
            {
                now = DateTime.Now;
            }
            else
            {
                now = DateTime.Now.ToUniversalTime();
            }

            int h = now.Hour;
            int m = now.Minute;
            int s = now.Second;
            keypresses++;
            startCount++;  //count to X mins to update received database
            if (keypresses > 180 && s == 5 && !slotgroupBox.Visible) //if 3 minutes passed without keypress or slot editing box not opened - change current time
            {
                currHour(true, trackSlotscheckBox.Checked); // update the date/time if no-one has been working in the app in 5 minutes (track exect slot time if no activity)
                if (!trackSlotscheckBox.Checked) { keypresses = 0; }
                //noSkedcheckBox.Checked = false;
            }

            string time = Convert.ToString(h).PadLeft(2, '0');
            time = time + ":" + Convert.ToString(m).PadLeft(2, '0');
            string fulltime = time + ":" + Convert.ToString(s).PadLeft(2, '0');
            CurrTime = fulltime;
            Timelabel.Text = CurrTime;

            await rxForm.set_time(fulltime);

            LTlabel.Text = LT.ToString("HH:mm").Trim();

            if (wsprTXtimer.Enabled)
            {
                flashTX(true);

            }
            if (m % 2 == 1 && s == 45 && slotActive && enableTXcheckBox.Checked) //stop decoding in case WSPR txn ends before last decode starts
            {
                rxForm.blockDecodes = true;
                RXblocktimer.Enabled = true;
                RXblocktimer.Start();
            }
            if (m % 2 == 1 && s == 59 || (m % 2 == 0 && s == 0) || (m % 2 == 0 && s == 1))
            {
                if (!recordFlag)
                {
                    recordFlag = true;
                    if (enableTXcheckBox.Checked && slotActive)
                    {

                    }
                    else
                    {
                        if (!stopRX) { await rxForm.Start_Receive(OpSystem); }  //start recording from RX
                    }
                }
            }
            if ((m % 2 == 0 && (s >= 7 && s <= 9) && recordFlag))
            {
                recordFlag = false;   //use flag to account for possible missed timer ticks
            }
            if (m % 2 == 1 && s == 48)
            {

                rxForm.set_prev_frequency();
            }

            if ((m % 2 == 1 && (s >= 56 && s < 58) && Flag))
            {
                Flag = false;   //use flag to account for possible missed timer ticks
            }
            if (m % 2 == 0 && (s > 2 && s < 5))
            {
                Flag = false;
            }

        }

        private void FreqlistBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            editF();
        }

        private void editF()
        {
            if (FreqlistBox.SelectedIndex < 0)
            {
                Msg.OKMessageBox("No frequency selected", "");
                return;
            }
            FreqlistBox.Enabled = false;
            string F = FreqlistBox.SelectedItem.ToString();
            string[] Freq = F.Split('\t');
            FtextBox.Text = Freq[0];
            double v = Convert.ToDouble(Freq[2]);
            trackBar1.Value = (int)(v * 100);
            Levellabel.Text = Freq[2];
            newFcheckBox.Checked = false;

            if (FstandardtlistBox.Items.Contains(Freq[0]))
            {
                if (standardFcheckBox.Checked)
                {
                    FtextBox.Enabled = true;
                }
                else
                {
                    FtextBox.Enabled = false;
                }
            }
            else
            {
                FtextBox.Enabled = true;
            }

            FreqgroupBox.Visible = true;
            FBgroupBox.Visible = false;
        }

        private void addF()
        {
            //FreqlistBox.Enabled = false;
            trackBar1.Value = 100;
            Levellabel.Text = "1.0";
            FtextBox.Enabled = true;
            FtextBox.Text = "";
            newFcheckBox.Checked = true;


            FreqgroupBox.Visible = true;
            FBgroupBox.Visible = false;
        }

        private void deleteF()
        {
            if (FreqlistBox.SelectedIndex < 0)
            {
                Msg.OKMessageBox("No frequency selected", "");
                return;
            }
            FreqlistBox.Enabled = false;
            string del = FreqlistBox.SelectedItem.ToString();
            string[] F = del.Split('\t');
            DialogResult res;
            if (FstandardtlistBox.Items.Contains(F[0]))
            {
                res = Msg.ynMessageBox("Standard WSPR frequency - go ahead?: " + F[0], "Standard F");
                if (res == DialogResult.No)
                {
                    return;
                }
            }
            res = Msg.ynMessageBox("Delete frequency: " + F[0], "Delete F");
            if (res == DialogResult.Yes)
            {
                if (DeleteFrequency(""))
                {
                    FreqlistBox.Items.Remove(del);
                }
            }
            FreqlistBox.Enabled = true;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            double t = Convert.ToDouble(trackBar1.Value);
            double v = (t / 100);
            string lvl = v.ToString();
            Levellabel.Text = lvl;
            Volume = (float)(v); //dymamic volume that allows signal to be adjusted whilst in TX
        }

        private void addFbutton_Click(object sender, EventArgs e)
        {
            addF();
        }


        private void editFbutton_Click(object sender, EventArgs e)
        {
            editF();
        }

        private void Fcancelbutton_Click(object sender, EventArgs e)
        {
            FreqlistBox.Enabled = true;
            FreqgroupBox.Visible = false;
            FBgroupBox.Visible = true;
        }

        private void Fsavebutton_Click(object sender, EventArgs e)
        {
            saveFreq();
        }
        private void saveFreq()
        {
            if (!double.TryParse(FtextBox.Text, out double result) || FtextBox.Text == "")
            {
                Msg.OKMessageBox("Invalid frequency", "");
                return;
            }
            if (newFcheckBox.Checked)
            {
                string[] fl;
                for (int i = 0; i < FreqlistBox.Items.Count; i++)
                {
                    fl = FreqlistBox.Items[i].ToString().Split('\t');
                    if (fl[0].Trim() == FtextBox.Text.Trim())
                    {
                        Msg.OKMessageBox("Frequency already exists", "");

                        return;
                    }
                }
            }
            try
            {
                if (SaveFrequency(""))
                {
                    string F = FtextBox.Text + "\t\t" + Levellabel.Text;
                    Volume = (float)(Convert.ToDouble(Levellabel.Text));
                    if (newFcheckBox.Checked) //add
                    {
                        FreqlistBox.Items.Add(F);
                    }
                    else //edit
                    {
                        FreqlistBox.Items[FreqlistBox.SelectedIndex] = F;
                    }
                }
            }
            catch
            {

            }
            FreqgroupBox.Visible = false;
            FreqlistBox.Enabled = true;
            FBgroupBox.Visible = true;

            updateFrequencyboxes();
        }
        private void updateFrequencyboxes()
        {
            try
            {
                defaultFcomboBox.Items.Clear();
                FlistBox.Items.Clear();
                for (int i = 0; i < FreqlistBox.Items.Count; i++)
                {
                    string A = FreqlistBox.Items[i].ToString();
                    string[] Ant = A.Split('\t');
                    Ant[0] = Ant[0].Trim();
                    FlistBox.Items.Add(Ant[0]);
                    defaultFcomboBox.Items.Add(Ant[0]);
                }
            }
            catch { }
        }

        private void Levellabel_Click(object sender, EventArgs e)
        {

        }

        private void deleteFbutton_Click(object sender, EventArgs e)
        {
            deleteF();
        }

        private void FtextBox_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(FtextBox.Text, out double result) && FtextBox.Text != "")
            {
                Msg.OKMessageBox("Invalid frequency", "");
            }
        }

        private void FtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)46)
            {
                e.Handled = false;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void addAnt()
        {
            try
            {
                antNolabel.Text = AntlistBox.Items.Count.ToString();
                AntnametextBox.Text = "";
                AntdesctextBox.Text = "";
                ShowSwlistBox.SelectedIndex = -1;
                ShowTulistBox.SelectedIndex = -1;
                AntPortlistBox.SelectedIndex = -1;
                AntPortlistBox.Visible = false;
                newAcheckBox.Checked = true; //flag add new
                AntgroupBox.Visible = true;
                AgroupBox.Visible = false;
                AntlistBox.Enabled = false;
            }
            catch
            {

            }
        }
        private void editAnt()
        {
            int sel = AntlistBox.SelectedIndex;
            if (sel < 0)
            {
                Msg.OKMessageBox("No antenna selected", "");
                return;
            }
            try
            {
                antNolabel.Text = Convert.ToString(Ant[sel].AntNo);
                string A = AntlistBox.SelectedItem.ToString();
                string[] ant = A.Split('\t');
                A = ant[0].TrimEnd(' ');
                AntnametextBox.Text = A;
                oldAnt = A; //remember old ant name 
                //ShowSwlistBox.Text = ant[1];
                ShowSwlistBox.SelectedIndex = Ant[sel].Switch;
                ShowTulistBox.SelectedIndex = Ant[sel].Tuner;
                int P = Ant[sel].SwitchPort;

                if (P > -1 && P < AntPortlistBox.Items.Count)
                {
                    AntPortlistBox.SelectedIndex = P; //ports start from 1
                }
                if (Ant[sel].Switch > 0)
                {
                    AntPortlistBox.Visible = true;
                }
                else
                {
                    AntPortlistBox.Visible = false;
                }

                AntdesctextBox.Text = Ant[sel].Description;

                newAcheckBox.Checked = false; //flag editing existing and not new
                AntgroupBox.Visible = true;
                AntgroupBox.Visible = true;
                AgroupBox.Visible = false;
                AntlistBox.Enabled = false;
            }
            catch
            {
                Msg.OKMessageBox("Error", "");
                AntlistBox.Enabled = true;
                AntgroupBox.Visible = false;
                AgroupBox.Visible = true;

            }
        }
        private void delAnt()
        {
            int sel = 0;
            if (AntlistBox.SelectedIndex < 0)
            {
                Msg.OKMessageBox("No antenna selected", "");
                return;
            }
            else
            {
                sel = AntlistBox.SelectedIndex;
            }
            AntlistBox.Enabled = false;
            string ant = AntlistBox.SelectedItem.ToString();
            string A = ant.Substring(0, 20).Trim();

            DialogResult res = Msg.ynMessageBox("Delete antenna: " + A + "...", "Delete Ant");
            if (res == DialogResult.Yes)
            {
                if (DeleteAntenna(sel))
                {
                    AntlistBox.Items.Remove(ant);
                    DefaultAntcomboBox.Items.Remove(ant);
                    AntselcomboBox.Items.Remove(ant);
                }
            }
            AntlistBox.Enabled = true;
        }
        private bool saveAnt()
        {
            Antenna A;
            try
            {
                int sel = AntlistBox.SelectedIndex;
                int S = ShowSwlistBox.SelectedIndex;
                int T = ShowTulistBox.SelectedIndex;
                int P = AntPortlistBox.SelectedIndex;
                if (S < 0)
                {
                    S = 0;
                }
                if (T < 0)
                {
                    T = 0;
                }
                if (P < 0)
                {
                    P = 0;
                }


                A.AntNo = Convert.ToInt32(antNolabel.Text);
                A.AntName = AntnametextBox.Text.Trim();
                A.Switch = S;
                A.Tuner = T;
                A.Description = AntdesctextBox.Text;
                A.SwitchPort = P; //ports start at 1
                if (!newAcheckBox.Checked)
                {
                    Ant[sel] = A;
                }
                else
                {
                    Ant.Add(A);
                }
                string AN = AntnametextBox.Text.PadRight(40, ' ');
                AN = AN.Substring(0, 40);
                AN = AN + "\t" + S + "\t" + T; // + "\t" + P + "\t"+AntdesctextBox.Text;
                if (newAcheckBox.Checked)
                {
                    string[] al;
                    for (int i = 0; i < AntlistBox.Items.Count; i++)
                    {
                        al = AntlistBox.Items[i].ToString().Split('\t');
                        if (al[0].Trim() == AntnametextBox.Text.Trim())
                        {
                            Msg.OKMessageBox("Antenna already exists", "");

                            return false;
                        }
                    }
                }

                if (newAcheckBox.Checked) //add
                {
                    AntlistBox.Items.Add(AN);
                    DefaultAntcomboBox.Items.Add(A.AntName);
                    AntselcomboBox.Items.Add(A.AntName);
                }
                else //edit
                {
                    int i = AntlistBox.SelectedIndex;
                    AntlistBox.Items[i] = AN;
                    DefaultAntcomboBox.Items[i] = A.AntName;
                    AntselcomboBox.Items[i] = A.AntName;
                }
                AntgroupBox.Visible = false;
                AntlistBox.Enabled = true;
                AgroupBox.Visible = true;
            }
            catch
            {
                Msg.OKMessageBox("Error saving antenna", "");
                AntgroupBox.Visible = false;
                AntlistBox.Enabled = true;
                AgroupBox.Visible = true;
                return false;
            }
            return SaveAntenna(A.AntNo);

        }

        private void AddAbutton_Click(object sender, EventArgs e)
        {
            addAnt();
        }

        private void SaveAbutton_Click(object sender, EventArgs t)
        {
            if (saveAnt() && (!newAcheckBox.Checked))
            {
                DialogResult res = Msg.ynMessageBox("Update all slots from now with new name (Y/N)?", "Update slots?");
                if (res == DialogResult.Yes)
                {
                    UpdateAntennasinSlots();
                }
            }
        }

        private bool UpdateAntennasinSlots()
        {
            try
            {
                string Ant = AntnametextBox.Text;
                DateTime now;
                if (!LTcheckBox.Checked)
                {
                    now = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    now = DateTime.Now;
                }
                string date = now.ToString(dateformat);
                if (!UpdateSlotAntennas(date, Ant, oldAnt))
                {
                    Msg.OKMessageBox("Error in some slots", "Updating slots");
                    return false;
                }
            }
            catch
            {
                Msg.OKMessageBox("Error in some slots", "Updating slots");
                return false;
            }
            return true;
        }

        private bool UpdateSlotAntennas(string today, string Ant, string oldAnt)
        {
            string c = "";
            string act = "0";
            string r = "0";

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_slots";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                c = "UPDATE slots SET Antenna = '" + Ant + "' WHERE slots.Antenna = '" + oldAnt + "' AND slots.Date >= '" + today + "'"; // + ";";              

                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch
            {
                connection.Close();

                return false;
            }
        }

        private void EditAbutton_Click(object sender, EventArgs e)
        {
            editAnt();
        }

        private void DelAbutton_Click(object sender, EventArgs e)
        {
            delAnt();
        }

        private void CancelAbutton_Click(object sender, EventArgs e)
        {
            AntgroupBox.Visible = false;
            AgroupBox.Visible = true;
            AntlistBox.Enabled = true;

        }

        private void FlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FlistBox2.SelectedIndex = FlistBox.SelectedIndex;
            testFreq(false);
        }

        private void defaultFcomboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void defaultpwrcomboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void PopulateFbutton_Click(object sender, EventArgs e)
        {
            DialogResult res = Msg.ynMessageBox("Reset frequency list to WSPR standard list (Y/N)?", "Reset freq list");
            if (res == DialogResult.Yes)
            {
                try
                {
                    for (int a = 0; a < FreqlistBox.Items.Count; a++)
                    {
                        string[] F = FreqlistBox.Items[a].ToString().Split('\t');
                        F[0] = F[0].Trim();
                        if (F[0] != "")
                        {

                            DeleteFrequency(F[0]);
                        }
                    }
                    FreqlistBox.Items.Clear();
                    for (int i = 0; i < FstandardtlistBox.Items.Count; i++)
                    {
                        string f = FstandardtlistBox.Items[i].ToString();
                        FreqlistBox.Items.Add(f + "\t\t 1.0");

                        SaveFrequency(f);

                    }
                    Msg.OKMessageBox("Datebase and list updated", "Reset complete");
                }
                catch
                {
                    Msg.OKMessageBox("Error", "");
                }

            }
        }


        private void editSw()
        {
            if (SwlistBox.SelectedIndex < 0)
            {
                Msg.OKMessageBox("No switch selected", "");
                return;
            }
            hideEditButtons(true);
            try
            {
                SwlistBox.Enabled = false;

                string[] S = SwlistBox.SelectedItem.ToString().Split('\t');
                HwtextBox.Text = S[1];
                Hwnolabel.Text = S[0];
                HwgroupBox.Visible = true;
            }
            catch
            {
                hideEditButtons(false);
            }
        }




        private void SaveSw()
        {
            if (HwtextBox.Text == "")
            {
                Msg.OKMessageBox("Switch name is empty", "");
                return;
            }
            if (SaveHardware("switches"))
            {

                string S = Hwnolabel.Text + "\t" + HwtextBox.Text.Trim();
                int i = SwlistBox.SelectedIndex;
                SwlistBox.Items[i] = S;
                ShowSwlistBox.Items[i] = S;


            }
            HwgroupBox.Visible = false;
            SwlistBox.Enabled = true;
        }





        private void SaveTu()
        {
            if (HwtextBox.Text == "")
            {
                Msg.OKMessageBox("Tuner name is empty", "");
                return;
            }
            if (SaveHardware("tuners"))
            {
                string S = Hwnolabel.Text + "\t" + HwtextBox.Text.Trim();
                int i = TulistBox.SelectedIndex;
                TulistBox.Items[i] = S;
                ShowTulistBox.Items[i] = S;

            }
            HwgroupBox.Visible = false;
            TulistBox.Enabled = true;
        }


        private void editSwbutton_Click(object sender, EventArgs e)
        {
            editButtonAction("switch");
        }
        private void editButtonAction(string hwtype)
        {
            int sel = 0;
            if (hwtype == "switch")
            {
                sel = SwlistBox.SelectedIndex;
            }
            else
            {
                sel = TulistBox.SelectedIndex;
            }
            if (sel == 0)
            {
                Msg.OKMessageBox("Cannot edit \"No " + hwtype + "\"", "");
                return;
            }
            DialogResult res = Msg.ynMessageBox("Edit " + hwtype + " settings (Y/N)?", "Edit " + hwtype);
            if (res == DialogResult.Yes)
            {
                if (hwtype == "switch")
                {
                    hwtype = "switches";
                    HwgroupBox.Text = "Switch";
                }
                else { hwtype = "tuners"; HwgroupBox.Text = "Tuner"; }


                editHW(hwtype);
            }
        }

        private void hideEditButtons(bool show)
        {
            editTubutton.Visible = !show;
            editSwbutton.Visible = !show;
        }

        private void editTubutton_Click(object sender, EventArgs e)
        {
            editButtonAction("tuner");
        }


        private void SaveHwList(int no, string hw, string protocol, string port, string IP, string baud, string serial, string type, int channels, string table)
        {
            try
            {

                Hardware H = new Hardware();
                H.Name = hw;
                H.Protocol = protocol;
                H.Port = port;
                H.IP = IP;
                H.Baud = baud;
                H.Serial = serial;
                H.Type = type;
                H.Channels = channels;

                if (table == "switches")
                {
                    H.Type = type;
                    SW[no] = H;
                }
                else
                {
                    H.Type = type;
                    TU[no] = H;
                }

            }
            catch { }
        }


        private void editHW(string table)
        {
            int selected = -1;
            string[] S = { "\t" };
            string type = "";
            if (table == "switches")
            {
                selected = SwlistBox.SelectedIndex;
                if (selected > -1)
                {
                    S = SwlistBox.SelectedItem.ToString().Split('\t');
                    type = SW[selected].Type;
                    int index = HwSwTypelistBox.Items.IndexOf(type);
                    if (index > -1)
                    {
                        HwSwTypelistBox.SelectedIndex = index;
                        HwAntportstextBox.Text = SW[selected].Channels.ToString();
                    }

                }

            }
            else if (table == "tuners")
            {
                selected = TulistBox.SelectedIndex;
                if (selected > -1)
                {
                    S = TulistBox.SelectedItem.ToString().Split('\t');
                    type = TU[selected].Type;
                    int index = HwTuTypelistBox.Items.IndexOf(type);
                    if (index > -1)
                    {
                        HwTuTypelistBox.SelectedIndex = index;
                    }
                }

            }

            if (selected < 0)
            {
                Msg.OKMessageBox("No " + table + " selected", "");
                return;
            }
            try
            {
                showChannelBox(table);
                hideEditButtons(true);

                HwtextBox.Text = S[1];
                Hwnolabel.Text = S[0];
                loadHWtoGroupBox(selected, table);
                HwgroupBox.Visible = true;
            }
            catch
            {
                hideHwBoxes();
                hideEditButtons(false);
            }
        }


        private void loadHWtoGroupBox(int i, string table)
        {
            Hardware hwItem = new Hardware();
            if (table == "switches")
            {
                hwItem = SW[i];
            }
            else
            {
                hwItem = TU[i];
            }
            try
            {
                if (hwItem.Protocol == "")
                {
                    HwProtocolcomboBox.SelectedIndex = -1;
                    HwBaudcomboBox.Text = "";
                    HwPortcomboBox.Text = "";
                    HwDatacomboBox.Text = "";
                    HwParitycomboBox.Text = "";
                    HwStopcomboBox.Text = "";
                    HwFlowcomboBox.Text = "";
                    HwIPtextBox.Text = "";
                    HwPorttextBox.Text = "";
                    return;
                }
            }
            catch { }

            try
            {
                HwProtocolcomboBox.SelectedItem = hwItem.Protocol;
                if (hwItem.Protocol == "Serial")
                {
                    setSerial();

                    HwBaudcomboBox.Text = hwItem.Baud;
                    HwPortcomboBox.Text = hwItem.Port;
                    string[] S = hwItem.Serial.Split(',');
                    HwDatacomboBox.Text = S[0];
                    HwParitycomboBox.Text = S[1];
                    HwStopcomboBox.Text = S[2];
                    HwFlowcomboBox.Text = S[3];

                }
                else //tcp/udp
                {
                    setTCPUDP();
                    HwIPtextBox.Text = hwItem.IP;
                    HwPorttextBox.Text = hwItem.Port;
                }
            }
            catch { }

        }



        private void showChannelBox(string table)
        {

            if (table == "switches")
            {

                HwSwTypegroupBox.Visible = true;
                HwTuTypegroupBox.Visible = false;
            }
            else
            {

                HwSwTypegroupBox.Visible = false;
                HwTuTypegroupBox.Visible = true;
            }
        }






        private void ReadHardware(string table)
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {
                connection.Open();
                int hwId = 0;
                string hw = "";
                string protocol = "";
                string port = "";
                string IP = "";
                string baud = "";
                string serial = "";
                string type = "";
                int channels = 0;

                int count = 0;
                if (table == "switches")
                {
                    count = SwlistBox.Items.Count;
                    SW.Clear();
                }
                else
                {
                    count = TulistBox.Items.Count;
                    TU.Clear();
                }



                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM " + table;
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                Hardware N = new Hardware();

                N.Id = 0;
                N.Name = "";
                N.Protocol = "";
                N.Port = "";
                N.IP = "";
                N.Baud = "";
                N.Serial = "";
                N.Type = "";
                N.Channels = 1;

                for (int h = 0; h < count; h++)
                {
                    if (table == "switches")
                    {
                        SW.Add(N);
                    }
                    else
                    {
                        TU.Add(N);
                    }
                }
                while (Reader.Read())
                {

                    //found = true;
                    hwId = (int)Reader["Id"]; //starts at zero, but 0 displayed as 1
                    hw = (string)Reader["Name"];

                    protocol = (string)Reader["Protocol"];
                    port = (string)Reader["Port"];
                    IP = (string)Reader["IP"];
                    baud = (string)Reader["Baud"];
                    serial = (string)Reader["Serial"];
                    type = (string)Reader["Type"];
                    channels = (int)Reader["Channels"];

                    try
                    {
                        if (hw != "")
                        {
                            if (table == "switches")
                            {
                                SwlistBox.Items[hwId] = hwId.ToString() + "\t" + hw;
                                ShowSwlistBox.Items[hwId] = hwId.ToString() + "\t" + hw;
                            }
                            else
                            {
                                TulistBox.Items[hwId] = hwId.ToString() + "\t" + hw;
                                ShowTulistBox.Items[hwId] = hwId.ToString() + "\t" + hw;
                            }
                            AddtoHwList(hwId, hw, protocol, port, IP, baud, serial, type, channels, table);
                        }
                    }
                    catch
                    {

                    }

                }

                Reader.Close();
                connection.Close();
            }
            catch
            {
                Msg.TMessageBox("Unable to load " + table, "", 1000);
                connection.Close();
            }
        }
        private void AddtoHwList(int hwId, string hw, string protocol, string port, string IP, string baud, string serial, string type, int channels, string table)
        { //maintain a list as well as a listbox of hardware
            Hardware N = new Hardware();

            N.Id = hwId; //id starts from 1
            N.Name = hw;
            N.Protocol = protocol;
            N.Port = port;
            N.IP = IP;
            N.Baud = baud;
            N.Serial = serial;
            N.Type = type;
            N.Channels = channels;
            if (table == "switches")
            {
                SW[hwId] = N;
            }
            else
            {
                TU[hwId] = N;
            }
        }

        private bool SaveHardware(string table) //save hw settings
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            string hw = HwtextBox.Text.Trim();
            int no = 0;


            string protocol = "";
            string baud = "";
            string IP = "";
            string serial = "";
            string port = "";
            string s1 = "";
            string s2 = "";
            string s3 = "";
            string s4 = "";
            string type = "";
            int channels = 1;
            lock (_lock)
            {
                try
                {

                    MySqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "INSERT INTO " + table + "(Id,Name,Protocol,Port,IP,Baud,Serial,Type,Channels) ";
                    command.CommandText += "VALUES(@Id,@Name,@Protocol,@Port,@IP,@Baud,@Serial,@Type,@Channels)";

                    hw = HwtextBox.Text.Trim();
                    no = Convert.ToInt32(Hwnolabel.Text);


                    if (HwProtocolcomboBox.SelectedIndex > -1)
                    {
                        protocol = HwProtocolcomboBox.SelectedItem.ToString();
                    }

                    if (protocol.Contains("Serial"))
                    {
                        if (HwPortcomboBox.SelectedIndex > -1)
                        {
                            port = HwPortcomboBox.SelectedItem.ToString();
                        }

                        if (HwBaudcomboBox.SelectedIndex > -1)
                        {
                            baud = HwBaudcomboBox.SelectedItem.ToString();
                        }
                        if (HwDatacomboBox.SelectedIndex > -1)
                        {
                            s1 = HwDatacomboBox.SelectedItem.ToString();
                        }
                        try
                        {
                            if (HwParitycomboBox.SelectedIndex > -1)
                            {
                                s2 = HwParitycomboBox.SelectedItem.ToString();
                            }
                            if (HwStopcomboBox.SelectedIndex > -1)
                            {

                                s3 = HwStopcomboBox.SelectedItem.ToString();
                            }
                            if (HwFlowcomboBox.SelectedIndex > -1)
                            {
                                s4 = HwFlowcomboBox.SelectedItem.ToString();
                            }

                        }
                        catch
                        {

                        }
                        serial = s1 + "," + s2 + "," + s3 + "," + s4;
                    }
                    else
                    {
                        port = HwPorttextBox.Text;
                        IP = HwIPtextBox.Text;
                    }
                    try
                    {
                        if (table == "switches")
                        {
                            if (HwSwTypelistBox.SelectedIndex > -1)
                            {

                                string sw = HwSwTypelistBox.SelectedItem.ToString();
                                string[] s = sw.Split('\t');
                                type = s[0];
                            }
                            channels = Convert.ToInt32(HwAntportstextBox.Text);
                        }
                        else
                        {
                            if (HwTuTypelistBox.SelectedIndex > -1)
                            {

                                string tu = HwTuTypelistBox.SelectedItem.ToString();
                                string[] t = tu.Split('\t');
                                type = t[0];
                            }
                            channels = 1; //tuner
                        }
                    }
                    catch
                    {

                    }

                    command.Parameters.AddWithValue("@Id", no);
                    command.Parameters.AddWithValue("@Name", hw);
                    command.Parameters.AddWithValue("@Protocol", protocol);
                    command.Parameters.AddWithValue("@Port", port);
                    command.Parameters.AddWithValue("@IP", IP);
                    command.Parameters.AddWithValue("@Baud", baud);
                    command.Parameters.AddWithValue("@Serial", serial);
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Channels", channels);


                    command.ExecuteNonQuery();

                    connection.Close();
                    SaveHwList(no, hw, protocol, port, IP, baud, serial, type, channels, table);

                    return true;
                }
                catch
                {         //if row already exists then try updating it in database
                    connection.Close();
                    if (UpdateHardware(no, hw, protocol, port, IP, baud, serial, type, channels, table))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }


        private bool UpdateHardware(int no, string hw, string protocol, string port, string IP, string baud, string serial, string type, int channels, string table)
        {

            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {


                MySqlCommand command = connection.CreateCommand();

                c = "UPDATE " + table + " SET Name = '" + hw + "', ";
                c = c + "Protocol = '" + protocol + "', Port = '" + port + "', IP = '" + IP + "', ";
                c = c + "Baud = '" + baud + "', Serial = '" + serial + "', Type = '" + type + "', Channels = " + channels + " WHERE " + table + ".Id = " + no; //oldhw[1].Trim();

                command.CommandText = c;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                SaveHwList(no, hw, protocol, port, IP, baud, serial, type, channels, table);
                return true;

            }
            catch
            {   //exhausted insert and update
                connection.Close();
                Msg.OKMessageBox("Unable to save " + table + " settings", "");
                return false;
            }
        }

        private void HwSavebutton_Click(object sender, EventArgs e)
        {
            if (HwProtocolcomboBox.SelectedIndex < 0)
            {
                Msg.OKMessageBox("No protocol selected", "");
                return;
            }
            if (HwProtocolcomboBox.SelectedItem.ToString() == "Serial")
            {
                if (HwPortcomboBox.SelectedIndex < 0 || HwBaudcomboBox.SelectedIndex < 0)
                {
                    Msg.OKMessageBox("Invalid port or baud rate", "");
                    return;
                }
            }
            else
            {
                if (!checkIP(HwIPtextBox.Text) || !checkIsNumber(HwPorttextBox.Text))
                {
                    Msg.OKMessageBox("Invalid IP or port", "");
                    return;
                }
            }   //otherwise (mostly) ok


            if (HwgroupBox.Text == "Switch")
            {
                if (HwAntportstextBox.Text == "" || HwAntportstextBox.Text == "0")
                {
                    Msg.OKMessageBox("Error: No channels on this switch", "Switch channels 1-16");
                    return;
                }
                if (HwSwTypelistBox.SelectedIndex < 0)
                {
                    Msg.OKMessageBox("You must select and highlight a switch type", "No switch type");
                    return;
                }
                SaveSw();
            }
            else if (HwgroupBox.Text == "Tuner")
            {
                if (HwTuTypelistBox.SelectedIndex < 0)
                {
                    Msg.OKMessageBox("You must select and highlight a tuner type", "No tuner type");
                    return;
                }
                SaveTu();
            }

            hideHwBoxes();
            hideEditButtons(false);
            HwgroupBox.Visible = false;
            Msg.TMessageBox("Saved", "", 500);

        }



        private void HwCancelbutton_Click(object sender, EventArgs e)
        {
            hideHwBoxes();
            HwgroupBox.Visible = false;
            SwlistBox.Enabled = true;
            TulistBox.Enabled = true;
            hideEditButtons(false);
        }

        private void HwProtocolcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int p = HwProtocolcomboBox.SelectedIndex;
            if (p < 0)
            {
                return;
            }
            if (p == 0)
            {
                setSerial();
            }
            else
            {
                setTCPUDP();
            }
        }
        private void setSerial()
        {
            setHwBoxes(true);
        }
        private void setTCPUDP()
        {
            setHwBoxes(false);
        }
        private void setHwBoxes(bool serial)
        {
            HwPortcomboBox.Visible = serial;
            BPlabel.Visible = serial;
            HwBaudcomboBox.Visible = serial;
            HwPorttextBox.Visible = !serial;
            HwIPtextBox.Visible = !serial;

            if (serial)
            {
                BPlabel.Text = "Baud:";
            }
            else
            {
                BPlabel.Text = "IP:";
            }
            BPlabel.Visible = true;
            HwPortlabel.Visible = true;
            HwSerialgroupBox.Visible = serial;
        }
        private void hideHwBoxes()
        {
            bool hide = false;
            HwPortcomboBox.Visible = hide;
            HwPorttextBox.Visible = hide;
            BPlabel.Visible = hide;
            HwBaudcomboBox.Visible = hide;
            HwIPtextBox.Visible = hide;
            HwPortlabel.Visible = hide;
            HwSerialgroupBox.Visible = hide;
        }


        private void HwPorttextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)46)
            {
                e.Handled = true;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void HwIPtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)46)
            {
                e.Handled = false;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private bool checkIP(string ip)
        {
            try
            {
                string[] IP = ip.Split('.'); //eg. 127.0.0.1
                if (IP.Length != 4)
                {
                    return false;
                }
                int D = 0;
                for (int i = 0; i < 4; i++)
                {
                    D = Convert.ToInt32(IP[i]);
                    if (i == 0)
                    {
                        if (D < 1 || D > 254)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (D < 0 || D > 255)
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool checkIsNumber(string text)
        {
            if (int.TryParse(text, out int result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void HwSwtypecomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void HwSwTypelistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string S = HwSwTypelistBox.Text;

            int sw = HwSwTypelistBox.SelectedIndex;
            if (sw > -1)
            {

                HwAntportstextBox.Text = SW[sw].Channels.ToString();
            }

        }

        private void HwSwTypelistBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void ShowSwlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool V = false;
            if (ShowSwlistBox.SelectedIndex == 0)
            {
                V = false;

            }
            else
            {
                V = true;
            }
            AntPortlistBox.Visible = V;
            AntPortlabel.Visible = V;
        }

        private void ArduinoPorttextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ArduinoPorttextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ArduinoIPtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            if (e.KeyChar == (char)46)
            {
                e.Handled = false;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void AntselcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < Ant.Count; i++)
            {

                if (Ant[i].AntName.Trim() == AntselcomboBox.Text.Trim())
                {
                    selSwitchtextBox.Text = Ant[i].Switch.ToString();
                    selTunertextBox.Text = Ant[i].Tuner.ToString();
                    int a = Ant[i].SwitchPort;
                    a++;
                    selSwPorttextBox.Text = a.ToString();
                }
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void timeEnd_ValueChanged(object sender, EventArgs e)
        {
            DateTime dt;
            bool isValid = DateTime.TryParse(datetimelabel.Text, out dt);
            // string date1 = dt.ToString(dateformat);

            string time1 = dt.ToString("HHmm");
            string selTime = timeEnd.Value.ToString("HHmm");
            int t1 = Convert.ToInt32(time1);
            int selt = Convert.ToInt32(selTime);
            if (selt < t1)
            {
                Msg.OKMessageBox("Error: End time before slot time", "");
            }

        }

        private void repeatTimecheckBox_CheckedChanged(object sender, EventArgs e)
        {

            if (repeatTimecheckBox.Checked)
            {
                timeEnd.Enabled = repeatTimecheckBox.Checked;

                DaycheckBox.Checked = false;
                NightcheckBox.Checked = false;
                greygroupBox.Visible = false;
                AllcheckBox.Checked = false;

            }


        }

        private void dateEnd_ValueChanged(object sender, EventArgs e)
        {
            DateTime dt;
            bool isValid = DateTime.TryParse(datetimelabel.Text, out dt);

            if (dateEnd.Value.DayOfYear < dt.DayOfYear)
            {
                Msg.OKMessageBox("Note: Date before today", "");
            }
        }


        private void TXenablebutton_Click(object sender, EventArgs e)
        {
            if (TXenablebutton.BackColor == Color.Green)
            {
                TXenablebutton.BackColor = Color.Red;
                TXenablebutton.Text = "TX Enabled";
                enableTXcheckBox.Checked = true;
            }
            else
            {
                TXenablebutton.BackColor = Color.Green;
                TXenablebutton.Text = "Enable TX";
                enableTXcheckBox.Checked = false;
                //stopTX();
            }

        }

        private void RigCtlPbutton_Click(object sender, EventArgs e)
        {

            //RigCtlfolderBrowserDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            RigCtlfolderBrowserDialog.InitialDirectory = "C:\\Wspr_Sked";
            RigCtlfolderBrowserDialog.ShowDialog();
            var Rpath = RigCtlfolderBrowserDialog.SelectedPath;
            RigCtlPathtextBox.Text = Rpath;
            if (!File.Exists(Rpath + slash + "rigctld.exe"))
            {
                Msg.OKMessageBox("rigctld.exe daemon not in this folder", "");
            }

        }

        private void PathAddbutton_Click(object sender, EventArgs e)
        {
            string P = RigCtlPathtextBox.Text.Trim();
            try
            {
                if (P != "" && Directory.Exists(P))
                {
                    if (File.Exists(P + slash + "rigctld.exe"))
                    {

                        // Get the current PATH environment variable
                        string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

                        // Append the new path if it's not already present
                        if (!currentPath.Contains(P))
                        {
                            string updatedPath = currentPath + ";" + P;
                            Environment.SetEnvironmentVariable("PATH", updatedPath, EnvironmentVariableTarget.Machine);
                            Msg.OKMessageBox("Folder added to PATH", "");
                        }
                        else
                        {
                            Msg.OKMessageBox("Already in PATH", "");
                        }
                    }
                    else
                    {
                        Msg.OKMessageBox("Invalid path to rigctld", "");
                    }
                }
                else
                {
                    Msg.OKMessageBox("Directory does not exist", "");
                }
            }
            catch { }
        }

        private void SaveRPathbutton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(@"" + RigCtlPathtextBox.Text + slash + "rigctld.exe"))
            {
                Msg.OKMessageBox("rigctld.exe daemon not in this folder", "");
                return;
            }
            SaveAll();
        }

        private void TXrunbutton_Click(object sender, EventArgs e)
        {
            if (slotActive)
            {
                if (TXrunbutton.BackColor == Color.Olive)
                {
                    return;
                }
                DialogResult res = Msg.ynMessageBox("Stop TX (y/n)?", "Stop TX");
                if (res == DialogResult.Yes)
                {
                    stopTX();

                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopRigCtlD(true); //true form closing
            rxForm.Save_Config(serverName, db_user, db_pass);
            Task.Delay(1000);
        }




        private string findBaseCall(string call)
        {
            string[] C;

            C = call.Split('/');
            if (C.Length > 1) //compound call
            {
                if ((C[0].Length >= 3) && (C[1].Length < 4))
                {
                    return C[0];
                }
                else if ((C[0].Length < 3) && (C[1].Length >= 3))
                {
                    return C[1];
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return call;
            }
            return "";
        }
        private int checkCall(string Call, string locator)
        {
            bool containsNo = false;
            string tmpCall = Call.PadRight(6);
            string tmpCall2 = Call.PadRight(10);
            if (!longcheckBox.Checked)
            {
                locator = locator.Substring(0, 4);
            }


            if (locator.Length > 4)
            {
                return 3;
            }
            if (Call.Trim() == "")
            {
                return 0;
            }
            foreach (char c in Call)
            {
                if (char.IsDigit(c)) // Check if the character is a digit
                {
                    containsNo = true;
                }
            }
            if (!containsNo)
            {
                Msg.OKMessageBox("Call should contain at least one number", "");
                return 0;
            }
            if (tmpCall2.Length > 10)
            {
                Msg.OKMessageBox("Callsign over 10 characters", "");
                return 0;
            }
            if (tmpCall.Length > 6)  //type 2 message
            {
                if (asOnecheckBox.Checked)
                {
                    return 3;
                }
                else
                {
                    return 2;
                }
            }
            else { return 1; } //type 1 message
        }
        private bool checkLocator(string locator)
        {
            string L = locator;
            if (L.Length < 4 || L.Length == 5 || L.Length > 6)
            {
                Msg.OKMessageBox("Locator should be 4 or 6 characters in length", "");
                return false;
            }
            bool ok = false;
            char c = L[2];
            if (char.IsDigit(c))
            {
                c = L[3];
                if (char.IsDigit(c))
                {
                    return true;
                }
                else { ok = false; }
            }
            else { ok = false; }
            if (!ok)
            {
                Msg.OKMessageBox("3rd and 4th character should be digits", "");
                return false;
            }
            return false;

        }

        private void LocatortextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
            else
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }


        }


        private void CalltextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ /".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
            else
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
                if (e.KeyChar == '\r')
                {
                    baseCalltextBox.Text = findBaseCall(CalltextBox.Text);
                }
            }
        }

        private void testbutton2_Click(object sender, EventArgs e)
        {


        }

        private void label78_Click(object sender, EventArgs e)
        {

        }

        private void LTcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LTcheckBox.Checked)
            {
                UTClabel.Text = "LT";
                UTClabel2.Text = "LT";
                LTlabel.Visible = false;
                LTlabel2.Visible = false;
                //changeDateTimes()
            }
            else
            {
                UTClabel.Text = "UTC";
                UTClabel2.Text = "UTC";
                LTlabel.Visible = true;
                LTlabel2.Visible = true;
            }

        }

        private async Task activateAntSwitch(string TXAntenna)
        {
            int sw = 0;
            int tu = 0;
            int ch = 0;
            bool found = false;
            for (int i = 0; i < Ant.Count; i++)
            {
                if (Ant[i].AntName == TXAntenna)
                {
                    sw = Ant[i].Switch;
                    tu = Ant[i].Tuner;
                    ch = Ant[i].SwitchPort;
                    found = true;
                }
            }
            if (!found) //use defaujlt ant instead
            {
                for (int i = 0; i < Ant.Count; i++)
                {
                    if (Ant[i].AntName == defaultAnt)
                    {
                        sw = Ant[i].Switch;
                        tu = Ant[i].Tuner;
                        ch = Ant[i].SwitchPort;
                    }
                }
            }

            changeAntenna(TXAntenna, sw, tu, ch);

        }
        private async void set410Mode()
        {
            var ret = "";
            int sw = 0;
            int mode = 0;

            for (int i = 0; i < SW.Count; i++)
            {
                if (SW[i].Type.StartsWith("W-410A"))
                {
                    sw = i;
                    mode = 2; //1 TX shares 4 ants
                }
            }
            await Task.Run(() =>
            {
                ret = W410A_setMode(sw, mode).Result;
            });
            string msg = "mode 2 (1TX-4Ant)";
            Msg.TMessageBox("W410A: " + msg, "", 3000);
        }
        private async Task<bool> changeAntenna(string antName, int sw, int tu, int channel)
        {
            try
            {
                channel++; //the channels start from 1 not zero
                string swType = SW[sw].Type;
                var ret = "";

                if (sw != 0 && swType != "" && swType != null)
                {
                    if (swType.StartsWith("Arduino"))
                    {
                        await Task.Run(() =>
                        {
                            ret = Arduino_Comms_1(sw, channel).Result;
                        });
                        if (ret == "ok")
                        {
                            string ant = antName.PadRight(20);
                            ant = ant.Substring(0, 20);
                            Msg.TMessageBox("Switch: " + sw.ToString() + ", Ant: " + ant.Trim(), "Switch reply", 3000);
                            TXRXAntlabel.Text = antName;
                            TXRXAntlabel2.Text = antName;
                            return true;
                        }
                        else
                        {
                            Msg.TMessageBox("Antenna switch error", "Switch reply", 3000);
                            return false;
                        }


                    }
                    else if (swType.StartsWith("W-410A"))
                    {
                        await Task.Run(() =>
                        {
                            ret = W410A_setAnt(sw, channel).Result;
                        });

                        string R = "";
                        if (ret != null)
                        {
                            R = ret.Trim();
                            if (R != "null" && R != "")
                            {
                                Msg.TMessageBox("W410A: " + antName, "Switch reply", 2500);
                            }
                        }
                        else
                        {
                            Msg.TMessageBox("W410A: " + antName, "Switch reply", 2500);
                        }
                        TXRXAntlabel.Text = antName;
                        TXRXAntlabel2.Text = antName;
                        return true;

                    }
                }
            }
            catch
            {
                Msg.TMessageBox("Error selecting antenna switch", "", 4000);
                return false;
            }
            return false;

        }

        private async Task<string> Arduino_Comms_1(int sw, int channel)
        {
            string ip = "192.168.0.207";
            int port = 5000;
            bool high = true;
            ip = SW[sw].IP;
            if (SW[sw].Port.StartsWith("COM"))
            {
                Msg.TMessageBox("Port error - should be a number", "Port:" + SW[sw].Port, 3000);
                return "error";
            }
            int.TryParse(SW[sw].Port, out port);
            bool ok = false;
            string reply = "";


            await Task.Run(() =>
            {
                ArduinoComms ard = new ArduinoComms(ip, port, channel - 1, high); //channels start at 1, board data pins at 0
                reply = ard.response;
                if (!ard.ok)
                {
                    ok = false;

                    Msg.TMessageBox(ard.response, "Error", 3000);
                }
                else { ok = true; }
            });
            if (!ok)
            {
                Msg.TMessageBox(reply, "Error", 3000);
                return "error";
            }
            else { return "ok"; }
        }
        private async Task<string> W410A_setAnt(int swno, int antno)
        {
            try
            {
                string reply = "";
                string com = SW[swno].Port;
                int baud = 0;
                int.TryParse(SW[swno].Baud, out baud);
                string serial = SW[swno].Serial;
                string[] S = serial.Split(',');
                int.TryParse(S[0], out int data);
                string parity = S[1];
                int.TryParse(S[2], out int stop);
                string flow = S[3];


                await Task.Run(() =>
                {
                    var w410a = new W410A.W410A_Switch(com, baud, data, parity, stop, flow, "W410A0+P" + antno);
                    reply = w410a.reply;
                });

                if (reply != null)
                {
                    return reply;
                }
                else
                {
                    return "";
                }

            }
            catch
            {

                return "error";
            }
        }

        private async Task<string> W410A_setMode(int swno, int mode)
        {
            try
            {
                string reply = "";
                string com = SW[swno].Port;
                int baud = 0;
                int.TryParse(SW[swno].Baud, out baud);
                string serial = SW[swno].Serial;
                string[] S = serial.Split(',');
                int.TryParse(S[0], out int data);
                string parity = S[1];
                int.TryParse(S[2], out int stop);
                string flow = S[3];


                await Task.Run(() =>
                {
                    var w410a = new W410A.W410A_Switch(com, baud, data, parity, stop, flow, "W410A0+M" + mode);
                    reply = w410a.reply;
                });
                //Msg.TMessageBox(reply, "Switch reply", 5000);
                return reply;

            }
            catch
            {
                Msg.TMessageBox("Antenna switching error", "", 4000);
                return "error";
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            W410A_setAnt(2, 2);


        }

        private void HwAntportstextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            string txt = HwAntportstextBox.Text;
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (txt != "")
                {
                    int num = Convert.ToInt32(txt);
                    if (num < 1 || num > 16)
                    {
                        Msg.OKMessageBox("Error: invalid switch channels", "Switch channels 1-16?");
                        e.Handled = true;
                    }
                }
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

        }

        private async Task selectAntenna()
        {
            if (wsprTXtimer.Enabled == false)
            {

                await activateAntSwitch(DefaultAntcomboBox.Text);
                TXRXAntlabel.Text = DefaultAntcomboBox.SelectedItem.ToString();
                TXRXAntlabel2.Text = DefaultAntcomboBox.SelectedItem.ToString();
            }
        }


        private async void DefaultAntcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await activateAntSwitch(DefaultAntcomboBox.Text);

        }

        private void label82_Click(object sender, EventArgs e)
        {

        }

        private void AntlistBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            editAnt();
        }

        private void currHourbutton_Click(object sender, EventArgs e)
        {
            currHour(false, true);
        }
        private void currHour(bool nochangeIfmatch, bool exactMin)
        {
            string oldtime = "";

            if (timelistBox.SelectedIndex > -1)
            {
                oldtime = timelistBox.SelectedItem.ToString();

            }
            string selDate = selectedDate.ToString("yyyy-MM-dd");
            DateTime dt;
            if (LTcheckBox.Checked)
            {
                dt = DateTime.Now;

            }
            else
            {
                dt = DateTime.Now.ToUniversalTime();
            }
            string timeH = dt.Hour.ToString().PadLeft(2, '0');
            string timeM = dt.Minute.ToString().PadLeft(2, '0');
            string date = dt.ToString(dateformat);
            string time = timeH + ":00";
            if (exactMin)
            {
                if (dt.Minute % 2 == 1)
                {
                    if (dt.Second > 52)
                    {
                        dt = dt.AddMinutes(1);
                    }
                    else
                    {
                        dt = dt.AddMinutes(-1);
                    }
                }
                timeM = dt.Minute.ToString().PadLeft(2, '0');
                time = timeH + ":" + timeM;

            }

            if (oldtime == time && selDate == date && nochangeIfmatch)
            {
                return;
            }
            timelistBox.Text = time;

            selectDT(time, date, true);
            try
            {
                if (exactMin)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[1].Value.ToString() == (time))
                        {
                            timelistBox.Text = timeH + ":00";
                            dataGridView1.Rows[i].Selected = true;
                            dataGridView1.FirstDisplayedScrollingRowIndex = i;

                            break;

                        }
                    }
                }
                DateTime d = new DateTime();
                d = Convert.ToDateTime(date);
                monthCalendar1.SetDate(d);
            }
            catch
            {

            }

        }
        private void timelistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selDate = selectedDate.ToString();
            selectDT(timelistBox.Text, selDate, true);

        }

        private void selectDT(string time, string date, bool find)
        {

            showmsg = true;
            databaseError = false;
            changeDateTimes(time, date, find);

        }
        private async void changeDateTimes(string selTime, string selDate, bool find)
        {
            //selectedTime = listBox1.Text;
            int slot = 0;

            dt = DateTime.Now;
            bool nextTX = ((dt.Minute % 2 == 1) && (dt.Second >= 52)); //don't update grid whilst countdown to next slot

            if (nextTX)
            {
                //return;
            }
            try
            {
                dataGridView1.AllowUserToAddRows = true;
                dtable.Rows.Clear();
                parents.Clear();
                string[] hr = selTime.Split(':');
                string tm = hr[0];

                DateTime d = new DateTime();
                d = Convert.ToDateTime(selDate);
                //monthCalendar1.SetDate(d);                
                DateTime selD = d.AddHours(Convert.ToInt32(tm));

                string date = d.ToString("yyyy-MM-dd");
                string sundate = selD.ToString("MM-dd");

                int offset = getUTCoffset(selD);

                string loc = LocatortextBox.Text.Trim().Substring(0, 4);
                try
                {
                    var sunTimes = find_sunrise_sunset(loc, sundate);
                    if (sunTimes.Result.R == DateTime.MinValue)
                    {
                        localriselabel.Text = "N/A";
                        localsetlabel.Text = "N/A";
                        utcriselabel.Text = "N/A";
                        utcsetlabel.Text = "N/A";
                    }
                    else
                    {
                        DateTime r = sunTimes.Result.R;
                        DateTime s = sunTimes.Result.S;
                        string R = r.ToString("HH:mm");
                        string S = s.ToString("HH:mm");

                        localriselabel.Text = R;
                        localsetlabel.Text = S;
                        s = s.AddHours(offset * -1);
                        r = r.AddHours(offset * -1);
                        utcriselabel.Text = r.ToString("HH:mm");
                        utcsetlabel.Text = s.ToString("HH:mm");

                    }
                }
                catch
                {
                    string error = "error";
                }
                for (int i = 0; i <= 58; i++)
                {
                    if (i % 2 == 0)
                    {
                        tm = hr[0];
                        if (hr[0] == "")
                        { tm = "00"; }
                        tm = tm + ":" + i.ToString().PadLeft(2, '0');
                        dtable.Rows.Add(date, tm); //, freq, offset, powerdB, powerW, ant, tuner, swi, end, active,rpt);                        
                        parents.Add("");
                    }

                }
                //dataGridView1.DataSource = dtable;


                dataGridView1.Columns[12].Visible = false;
                dataGridView1.Columns[13].Visible = false;
                //dataGridView1.Columns[15].Visible = false;


                for (slot = 0; slot < dataGridView1.Rows.Count - 1; slot++)
                {
                    try
                    {
                        date = Convert.ToString(dataGridView1.Rows[slot].Cells[0].Value);
                        //d = Convert.ToDateTime(date);
                        // date = d.ToString("yyyy-MM-dd");
                        time = Convert.ToString(dataGridView1.Rows[slot].Cells[1].Value);
                    }
                    catch { }

                    if (find)
                    {

                        if (findSlotRow(slot, date, time))
                        {
                            populateGridSlot(slot);

                        }

                    }

                }
                dataGridView1.AllowUserToAddRows = false;
            }
            catch
            {
                Msg.OKMessageBox("Error selecting date", "");
            }
        }

        private void populateGridSlot(int s)  //having found slot in database - populate the gridview with slot
        {

            DataGridViewRow DataRow = dataGridView1.Rows[s];
            try
            {


                cells[0] = "";
                cells[1] = "";

                cells[2] = SlotRow.Freq.ToString();
                cells[3] = SlotRow.Offset.ToString();
                cells[4] = SlotRow.PowerdB.ToString(); ;
                cells[5] = SlotRow.PowerW.ToString();
                cells[6] = SlotRow.Ant;
                cells[7] = SlotRow.Tuner.ToString();
                cells[8] = SlotRow.Switch.ToString();
                //cells[9] = null; //rotator
                //cells[10] = null; //azimuth                

                cells[9] = SlotRow.Endslot;
                cells[11] = SlotRow.Active;
                cells[10] = SlotRow.Rpt;
                cells[12] = SlotRow.EndTime;


                cells[13] = SlotRow.RptTime;

                cells[14] = SlotRow.SlotNo.ToString();
                cells[15] = SlotRow.MessageType.ToString();

                for (int i = 2; i < maxcol; i++)
                {
                    if (cells[i] != null)
                    {
                        DataRow.Cells[i].Value = cells[i];

                    }
                }
                dataGridView1.Columns[12].Visible = false; //hide end time
                dataGridView1.Columns[13].Visible = false; //amd rpr time flag
                                                           //dataGridView1.Columns[15].Visible = false; //and msg type
                string act = DataRow.Cells[11].Value.ToString();
                if (act.Contains(tick))
                {
                    dataGridView1.Rows[s].Cells[11].Style.ForeColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[s].Cells[11].Style.ForeColor = Color.Blue;
                }
                DataGridViewCell cell = dataGridView1.Rows[s].Cells[11];
                cell.Style.Font = new System.Drawing.Font(dataGridView1.Font, FontStyle.Bold);
            }
            catch
            {
                Msg.TMessageBox("Unable to show row", "", 1000);
            }
        }

        private async Task delayFind(int slot, string date, string time)  //not used
        {
            DateTime dt = DateTime.Now;
            do
            {
                dt = DateTime.Now;
            } while (dt.Minute % 2 == 1);
        }

        private void SwlistBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            editButtonAction("switch");
        }

        private void TulistBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            editButtonAction("tuner");
        }

        private void showPasscheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (showPasscheckBox.Checked)
            {
                PasstextBox.PasswordChar = '\0';
                PasstextBox2.PasswordChar = '\0';
            }
            else
            {
                PasstextBox.PasswordChar = '*';
                PasstextBox2.PasswordChar = '*';
            }

        }

        private void PasstextBox_TextChanged(object sender, EventArgs e)
        {
            PasstextBox2.Visible = true;
            Passlabel2.Visible = true;
        }

        private void Type2checkBox_CheckedChanged(object sender, EventArgs e)
        {
            bool C = Type2checkBox.Checked;
            if (!C)
            {
                //msgType = 1;
                msgTypelabel.Text = "Message type 1";
                msgTypelabel2.Text = "Message type 1";
                longcheckBox.Checked = false;

            }
            else
            {
                //msgType = 2;
                msgTypelabel.Text = "Message type 1 or 2 or 3";
                msgTypelabel2.Text = "Message type 1 or 2 or 3";
            }
            wsprmsgbutton.Visible = C;
            wsprmsglabel.Visible = C;
            wsprmsgtextBox.Visible = C;
            asOnecheckBox.Visible = C;
        }

        private void wsprmsgbutton_Click(object sender, EventArgs e)
        {

            wsprmsgfolderBrowserDialog.InitialDirectory = root; //Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            wsprmsgfolderBrowserDialog.ShowDialog();
            var wsprmsgPath = wsprmsgfolderBrowserDialog.SelectedPath;
            wsprmsgtextBox.Text = wsprmsgPath;
            string filename = wsprmsgPath + slash + "wspr_enc.exe";
            if (!File.Exists(filename))
            {
                Msg.TMessageBox("Path does not contain wspr_enc.exe", "", 3000);
            }

        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            keypresses = 0;
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            keypresses = 0;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            keypresses = 0;
        }

        async void testFreq(bool change_IdleF)
        {
            try
            {
                if (rigctldcheckBox.Checked)
                {
                    Msg.TMessageBox("Rigctld disabled - change frequency manually", "", 3000);

                    changeFmanually(FlistBox);
                    FlistBox2.SelectedItem = FlistBox.SelectedItem;
                    return;
                }
                testFtextBox.Text = FlistBox.SelectedItem.ToString();

                double freq = Convert.ToDouble(testFtextBox.Text);

                TXFrequency = (freq * 1000000).ToString();
                var c = await changeFreq(TXFrequency);
                if (c)
                {
                    if (change_IdleF)
                    {
                        TXrunbutton.Text = "TX: " + testFtextBox.Text + "MHz";
                        TXrunbutton2.Text = TXrunbutton.Text;
                        //rxForm.Frequency = testFtextBox.Text.Trim();
                        //rxForm.set_frequency(testFtextBox.Text.Trim());
                    }
                }
            }
            catch { }
        }
        private void FlistBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            testFreq(true);
        }


        private void pintextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
                return;
            }
            int t;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(pintextBox.Text + e.KeyChar, out t);
                if (t > 13 || t < 0)
                {
                    Msg.OKMessageBox("Error: range0-13", "");
                    e.Handled = true;
                }
            }
        }
        private void findSound()
        {
            try
            {
                audioInlistBox.Items.Clear();
                audioOutlistBox.Items.Clear();
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                {
                    var caps = WaveOut.GetCapabilities(i);
                    audioOutlistBox.Items.Add($"{i}: {caps.ProductName}");
                    if (samecheckBox.Checked)
                    {
                        audioInlistBox.Items.Add($"{i}: {caps.ProductName}");
                    }
                }
                if (!samecheckBox.Checked)
                {
                    for (int b = 0; b < WaveIn.DeviceCount; b++)
                    {
                        var caps = WaveIn.GetCapabilities(b);
                        audioInlistBox.Items.Add($"{b}: {caps.ProductName}");
                    }
                }
            }
            catch
            {

            }
        }

        private void audioUpdatebutton_Click(object sender, EventArgs e)
        {
            findSound();
        }

        private void audioOutlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (audioOutlistBox.SelectedIndex > -1)
            {
                AudioOutlabel.Text = audioOutlistBox.SelectedItem.ToString();
                audioOutName = AudioOutlabel.Text;
                audioOutDevice = audioOutlistBox.SelectedIndex;
            }
        }

        private void audioInlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (audioInlistBox.SelectedIndex > -1)
            {
                AudioInlabel.Text = audioInlistBox.SelectedItem.ToString();
                audioInName = AudioInlabel.Text;
                audioInDevice = audioInlistBox.SelectedIndex;
            }
        }


        private WaveInEvent waveIn;
        private float gain = 1.0f; // Default gain

        public void MonitorMic()
        {
            try
            {            // Setup mic input
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(44100, 1)
                };

                waveIn.DataAvailable += OnDataAvailable;
                waveIn.StartRecording();

                // Setup gain slider
                trackBarGain.Minimum = 1;  // 0.01x
                trackBarGain.Maximum = 500; // 3.0x
                                            //trackBarGain.Value = 100;   // 1.0x

                if (inLevel < 1)
                {
                    inLevel = 1;
                }
                trackBarGain.Value = inLevel;
                gain = trackBarGain.Value / 100f;
                trackBarGain.Scroll += (s, e) =>
                {
                    gain = trackBarGain.Value / 100f;
                };
            }
            catch
            {

            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            float max = 0;

            try
            {
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                    float sample32 = (sample / 32768f) * gain;
                    sample32 = Math.Max(-1.0f, Math.Min(1.0f, sample32)); // Clamp
                    if (Math.Abs(sample32) > max) max = Math.Abs(sample32);
                }
            }
            catch
            {

            }

            // Update volume meter on UI thread
            try
            {
                Invoke((System.Windows.Forms.MethodInvoker)(() =>
                {
                    volumeMeter1.Amplitude = max;
                }));
            }
            catch { }
        }



        private void Monitorbutton_Click(object sender, EventArgs e)
        {

            MonitorMic();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            waveIn?.StopRecording();
            //waveIn?.Dispose();
        }

        private void SaveAudiobutton_Click(object sender, EventArgs e)
        {
            inLevel = trackBarGain.Value;
            if (inLevel < 1)
            {
                inLevel = 1;
            }
            rxForm.gain = gain;
            Save_Audio();
            Msg.OKMessageBox("Audio settings saved", "RX audio");
        }

        public void Save_Audio()
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_configs";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            DateTime date = new DateTime();

            lock (_lock)
            {
                try
                {
                    connection.Open();

                    string wpath = wsprdfilepath.Replace("\\", "/");  //make sql friendly
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "INSERT INTO rxsettings(id,outputname,outputdevice,inputname,inputdevice,outlevel,inlevel,wsprdpath,samedev)";
                    command.CommandText += "VALUES(0,'" + audioOutName + "', " + audioOutDevice + ", '" + audioInName + "', " + audioInDevice + ", " + outLevel + ", " + inLevel + ", '" + wpath + "', " + samecheckBox.Checked + ") ";
                    command.CommandText += "ON DUPLICATE KEY UPDATE outputname = '" + audioOutName + "', outputdevice = " + audioOutDevice;
                    command.CommandText += ", inputname = '" + audioInName + "', inputdevice = " + audioInDevice;
                    command.CommandText += ", outlevel = " + outLevel + ", inlevel = " + inLevel + ", wsprdpath = '" + wpath + "', samedev = " + samecheckBox.Checked;


                    command.ExecuteNonQuery();
                    connection.Close();

                }
                catch
                {
                    connection.Close();
                }
            }

        }

        private void Read_Audio()
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_configs";


            try
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);

                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM rxsettings";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    audioOutName = (string)Reader["outputname"];
                    audioOutDevice = (int)Reader["outputdevice"];
                    audioInName = (string)Reader["inputname"];
                    audioInDevice = (int)Reader["inputdevice"];
                    outLevel = (int)Reader["outlevel"];
                    inLevel = (int)Reader["inlevel"];
                    samecheckBox.Checked = (bool)Reader["samedev"];

                    string wpath = (string)Reader["wsprdpath"];

                    if (OpSystem == 0)
                    {
                        wsprdfilepath = wpath.Replace('/', '\\');
                    }
                    else
                    {
                        wsprdfilepath = wpath;
                    }

                    wsprdtextBox.Text = wsprdfilepath;

                }
                Reader.Close();
                connection.Close();
                match_audio_list();
            }
            catch
            {

            }
        }
        private void match_audio_list()
        {
            int index = audioOutlistBox.FindString(audioOutName);
            if (index > -1)
            {
                audioOutlistBox.SelectedIndex = index;
                audioOutDevice = index;
                AudioOutlabel.Text = audioOutName;
            }
            else
            {
                AudioOutlabel.Text = "No output device";
            }

            index = audioInlistBox.FindString(audioInName);
            if (index > -1)
            {
                audioInlistBox.SelectedIndex = index;
                audioInDevice = index;
                AudioInlabel.Text = audioOutName;
            }
            else
            {
                AudioInlabel.Text = "No inout device";
            }
            trackBarGain.Value = inLevel;
        }

        private void trackBarGain_ValueChanged(object sender, EventArgs e)
        {
            inLevel = trackBarGain.Value;
            gain = inLevel / 100f;
            rxForm.gain = gain;
            gainlabel.Text = gain.ToString();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "RX & Sound config")
            {
                //MonitorMic();
            }
        }

        private void wsprdbutton_Click(object sender, EventArgs e)
        {
            wsprdBrowserDialog.SelectedPath = wsprdfilepath;
            wsprdBrowserDialog.ShowDialog();
            wsprdfilepath = wsprdBrowserDialog.SelectedPath;
            wsprdtextBox.Text = wsprdfilepath;
            string filename = wsprdfilepath + slash + "wsprd.exe";
            if (!File.Exists(filename))
            {
                Msg.TMessageBox("Path does not contain wsprd.exe", "", 3000);
            }
            rxForm.wsprdfilepath = wsprdfilepath;
        }

        private void testEncodebutton_Click(object sender, EventArgs e)
        {
            byte[] levels = null;
            int slot = 1;
            int msgT = 1;
            testdbmlabel.Text = defaultdB.ToString();
            string cs = callsign;
            string loc = location;
            if (slotTestlistBox.SelectedIndex > -1)
            {
                msgT = slotTestlistBox.SelectedIndex + 1;
            }
            if (slotlistBox.SelectedIndex > -1)
            {
                slot = slotlistBox.SelectedIndex + 1;
            }
            if (msgT == 1)
            {

                cs = baseCalltextBox.Text.Trim().ToUpper();
            }

            var wspr = new WsprTransmission();
            wspr.wsprmsgPath = wsprmsgtextBox.Text;
            bool oneTX = false;
            if (msgT == 3)
            {
                oneTX = true;
                Msg.TMessageBox("One transmission only for message type 3", "", 1500);
            }
            else if (msgT == 2)
            {
                Msg.TMessageBox("Two transmissions for message type 2", "", 1500);
                oneTX = false;
            }
            else if (msgT == 1)
            {
                Msg.TMessageBox("One transmissions for message type 1", "", 1500);
                loc = location.Substring(0, 4);
                oneTX = true;
            }
            if (!longcheckBox.Checked)
            {
                loc = location.Substring(0, 4);
            }
            levels = wspr.WsprTxn(cs, loc, defaultdB, slot, msgT, oneTX, OpSystem);
            leveltextBox.Text = wspr.LevelsString;
        }

        private void selSwitchtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void selTunertextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void selSwPorttextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void baseCalltextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void PasstextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void PasstextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void AntnametextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void AntdesctextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void HwtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private async void DefaultAntcomboBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void RXblocktimer_Tick(object sender, EventArgs e)
        {
            rxForm.blockDecodes = false;    //block RX decoders for 12 seconds until RX recommences
            RXblocktimer.Stop();
            RXblocktimer.Enabled = false;
        }

        private void stopInternetcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (stopInternetcheckBox.Checked)
            {
                stopUrl = true;
                //solarForm.stopUrl = true;
                rxForm.stopUrl = true;
                //liveForm.stopUrl = true;
            }
            else
            {
                stopUrl = false;
                //solarForm.stopUrl = false;
                rxForm.stopUrl = false;
                //liveForm.stopUrl = false;
            }
        }

        private void rigctldcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (rigctldcheckBox.Checked)
            {
                noRigctld = true;
                FlistBox2.Visible = true;
                Flabel.Visible = true;
                Fhelplabel.Visible = true;
            }
            else
            {
                FlistBox2.Visible = false;
                Flabel.Visible = false;
                Fhelplabel.Visible = false;
                noRigctld = false;
                getRigList();

                runRigCtlD(); //strt rig ctld
            }
        }

        private void FlistBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            FlistBox.SelectedItem = FlistBox2.SelectedItem;
            changeFmanually(FlistBox2);
        }
        private void changeFmanually(ListBox listB)
        {
            TXrunbutton.Text = FlistBox2.SelectedItem.ToString() + " MHz";
            TXrunbutton2.Text = FlistBox2.SelectedItem.ToString() + " MHz";
            rxForm.set_frequency(FlistBox2.SelectedItem.ToString());
            TXrunbutton.Text = listB.SelectedItem.ToString() + " MHz";
            TXrunbutton2.Text = listB.SelectedItem.ToString() + " MHz";
            rxForm.set_frequency(listB.SelectedItem.ToString());
        }

        private async void solarcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            /*
            if (solarcheckBox.Checked)
            {
                solarStarted = false;
                stopSolar = false;
                solarForm.Show();
                if (stopSolar)
                {

                    if (!stopUrl)
                    {
                        solarForm.stopUrl = false;
                        solarForm.setConfig(serverName, db_user, db_pass);
                    }
                    else
                    {
                        Msg.TMessageBox("Note: internet connection is stopped", "Solar data", 1500);
                    }


                }

            }
            else
            {
                stopSolar = true;
                solarForm.stopUrl = true;
                solarForm.Hide();

            }
            */
        }

        private void rigsearchbutton_Click(object sender, EventArgs e)
        {
            string search = rigtextBox.Text.ToUpper();
            for (int i = 0; i < RigcomboBox.Items.Count; i++)
            {
                if (RigcomboBox.Items[i].ToString().ToUpper().Contains(search))
                {
                    RigcomboBox.SelectedIndex = i;
                    return;
                }
            }

        }
        private async Task SaveSunRiseSunset(DateTime date, int days)
        {   //saves local times to database
            int count = 0;
            LatLng ll;
            string sunrise = "";
            string sunset = "";
            string locator = LocatortextBox.Text.Trim().Substring(0, 4).ToUpper();

            try
            {
                ll = MaidenheadLocator.LocatorToLatLng(locator);

                for (int i = 0; i < days; i++)
                {
                    //if (i % 7 == 0) //get sunrise/set every week
                    //{
                    Sunrise_Sunset(ll.Lat, ll.Long, date);
                    sunrise = sunTimes.R.ToString("HH:mm");
                    sunset = sunTimes.S.ToString("HH:mm");
                    //}
                    string Date = date.ToString("MM-dd");

                    var ok = Save_SunSet_SunRise(locator, Date, sunrise, sunset);
                    if (!ok.Result)
                    {
                        Msg.TMessageBox("Unable to update sunrise/sunset times", "Sunrise/set", 2000);
                        break;
                    }
                    date = date.AddDays(1);
                }
            }
            catch
            {

                Msg.TMessageBox("Error while saving sunrise/sunset times", "Sunrise/set", 2000);
            }

        }

        //static async Task<(DateTime R, DateTime S)> Sunrise_Sunset(double latitude, double longitude, DateTime Date)
        private async void Sunrise_Sunset(double latitude, double longitude, DateTime Date)
        {       //retrieves times as LocalConstant not UTC

            DateTime sunrise = DateTime.MinValue;
            DateTime sunset = DateTime.MinValue;
            try
            {
                string date = Date.ToString("yyyy-MM-dd");

                string url = $"https://api.sunrise-sunset.org/json?lat={latitude}&lng={longitude}&date={date}&formatted=0";

                using HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(url);

                JObject json = JObject.Parse(response);
                var results = json["results"];

                sunrise = DateTime.Parse(results["sunrise"].ToString());
                sunset = DateTime.Parse(results["sunset"].ToString());
            }
            catch
            {
                sunrise = DateTime.MinValue;
                sunset = DateTime.MinValue;
                sunTimes.R = sunrise;
                sunTimes.S = sunset;
            }
            sunTimes.R = sunrise;
            sunTimes.S = sunset;

        }

        private void DaycheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DaycheckBox.Checked)
            {
                NightcheckBox.Checked = false;
                repeatTimecheckBox.Checked = false;
                AllcheckBox.Checked = false;
                timeEnd.Enabled = false;
                greygroupBox.Visible = true;
                greylistBox.Text = "1";

            }

        }

        private void NightcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (NightcheckBox.Checked)
            {
                DaycheckBox.Checked = false;

                repeatTimecheckBox.Checked = false;
                timeEnd.Enabled = false;
                AllcheckBox.Checked = false;
                greygroupBox.Visible = true;
                greylistBox.Text = "1";

            }
        }

        private void AllcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AllcheckBox.Checked)
            {
                DaycheckBox.Checked = false;
                NightcheckBox.Checked = false;
                repeatTimecheckBox.Checked = false;
                timeEnd.Enabled = false;
                greygroupBox.Visible = false;

            }
        }

        private async Task<bool> Save_SunSet_SunRise(string locator, string date, string sunrise, string sunset)
        {   //note these are saved as local times not UTC

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_grey";
            MySqlConnection connection = new MySqlConnection(myConnectionString);


            //lock (_lock)
            //{
            try
            {

                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT IGNORE INTO sunrise_sunset(locator,date,sunrise,sunset) ";
                command.CommandText += "VALUES(@locator,@date,@sunrise,@sunset)";
                command.CommandText += " ON DUPLICATE KEY UPDATE sunrise = '" + sunrise + "', sunset = '" + sunset + "'";



                command.Parameters.AddWithValue("@locator", locator);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@sunrise", sunrise);
                command.Parameters.AddWithValue("@sunset", sunset);

                command.ExecuteNonQuery();


                connection.Close();
                return true;

            }
            catch
            {         //if row already exists then try updating it in database
                connection.Close();
                return false;
            }
            //}

        }

        private async Task<(DateTime R, DateTime S)> find_sunrise_sunset(string locator, string date) //find sunsrise/set times form database
        {   //read as local times - so need to be converted to UTC

            string sunrise = "";
            string sunset = "";



            DateTime sunriseT = DateTime.MinValue;
            DateTime sunsetT = DateTime.MinValue;
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_grey";

            MySqlConnection connection = new MySqlConnection(myConnectionString);

            try
            {

                connection.Open();
                MySqlCommand command = connection.CreateCommand();


                command.CommandText = "SELECT * FROM sunrise_sunset WHERE locator = '" + locator + "' AND date = '" + date + "'";


                MySqlDataReader Reader;
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    sunrise = (string)Reader["sunrise"];
                    sunset = (string)Reader["sunset"];
                }



                Reader.Close();
                connection.Close();
                databaseError = false;

            }
            catch
            {
                sunriseT = DateTime.MinValue;
                sunsetT = DateTime.MinValue;
                connection.Close();
                return (sunriseT, sunsetT);
            }

            sunriseT = DateTime.Parse(sunrise.ToString());
            sunsetT = DateTime.Parse(sunset.ToString());
            return (sunriseT, sunsetT);

        }

        private async void sunrisebutton_Click(object sender, EventArgs e)
        {
            updateSunriseSunset();
        }
        private async void updateSunriseSunset()
        {
            Msg.TMessageBox("Sunrise and sunset times updating", "Sunrise/set", 3000);
            LatLng ll;
            ll = MaidenheadLocator.LocatorToLatLng(LocatortextBox.Text.Trim());
            Sunrise_Sunset(ll.Lat, ll.Long, dt);
            utcriselabel.Text = sunTimes.R.ToString("HH:mm");
            utcsetlabel.Text = sunTimes.S.ToString("HH:mm");
            DateTime today = DateTime.Now;
            SaveSunRiseSunset(today, 365);

        }


        private int getUTCoffset(DateTime date)
        {
            string timeZoneId = TimeZoneInfo.Local.Id;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            TimeSpan t = tz.GetUtcOffset(date);
            return t.Hours;

        }

        private int grey_table_count()
        {
            int count;
            string connectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_grey";
            var connection = new MySqlConnection(connectionString);

            try
            {
                //string connectionString = "Server=server;Port=3306;Database=wspr;User ID=user;Password=pass;";

                using (connection)
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM sunrise_sunset", connection))
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

        private void idletimer_Tick(object sender, EventArgs e)
        {
            if (checkSlotDB())
            {
                daytimer.Enabled = true;
                daytimer.Start();

                idletimer.Enabled = false;
                idletimer.Stop();
            }
        }

        private void stopRXcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (stopRXcheckBox.Checked)
            {
                stopRX = true;
                rxForm.Hide();
            }
            else
            {
                stopRX = false;
                rxForm.Show();
            }
        }

        private void samecheckBox_CheckedChanged(object sender, EventArgs e)
        {
            findSound();

        }

        private void RigCtlfolderBrowserDialog_HelpRequest(object sender, EventArgs e)
        {

        }

        private void noSkedcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (noSkedcheckBox.Checked)
            {
                skedstoplabel.Text = "Sked disabled";

            }
            else
            {
                skedstoplabel.Text = "Sked enabled";
            }
        }

        private void skedstoplabel_DoubleClick(object sender, EventArgs e)
        {
            if (skedstoplabel.Text == "Sked disabled")
            {
                noSkedcheckBox.Checked = false;
                //skedstoplabel.Text = "Sked OK";
            }
            else
            {
                noSkedcheckBox.Checked = true;
                //skedstoplabel.Text = "Sked disabled";
            }
        }
    }


}



