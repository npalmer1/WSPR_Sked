using Google.Protobuf.WellKnownTypes;
using MathNet.Numerics;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WSPRlive;
using WsprSharp;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static WSPR_Sked.Form1;

namespace WSPR_Sked
{
    public partial class LiveForm : Form
    {



        public string call = "";
        private string[] cells = new string[20];
        private string Callsign = "";
        private bool databaseError = false;

        int timespan = 10; //timespan x minutes
        int liveLimit = 1000;
        int maxrows = 2500;
        bool update = false;

        MessageClass Msg = new MessageClass();

        public bool stopUrl = false;
        private static readonly object _lock = new object();
        public LiveForm()
        {

            InitializeComponent();

            //dataGridView1.Font = new System.Drawing.Font("Consolas", 9); // Set font to Arial with size 12


        }
        public void set_header(string call, string serverName, string db_user, string db_pass)
        {
            this.Text = "Received transmissions for: " + call;
            Callsign = call;
            server = serverName;
            user = db_user;
            pass = db_pass;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm";
            //dateTimePicker1.ShowUpDown = true;
            bandlistBox.SelectedIndex = 0;

        }

        public struct RX_data
        {
            public Int64 id;
            public DateTime time;
            public Int16 band;
            public string rx_sign;
            public float rx_lat;
            public float rx_lon;
            public string rx_loc;
            public string tx_sign;
            public float tx_lat;
            public float tx_lon;
            public string tx_loc;
            public int distance;
            public int azimuth;
            public int rx_azimuth;
            public int frequency;
            public int power;
            public int snr;
            public int drift;
            public string version;
            public int code;
        }
        RX_data RX = new RX_data();

        private string server;
        private string user;
        private string pass;

        DataTable RXtable = new DataTable();

        private void testDBbutton_Click(object sender, EventArgs e)
        {
            var live = new Wspr_live();
            live.SQL_Get();
            //live.Get_Received(Callsign, 0, 60, 10); //find 10 entries for last 1 hour
            if (live.textBox1.Text != "" || live.Reply != "error")
            {

            }
            else if (live.Reply == "error")
            {
                MessageBox.Show("Connection error");
            }
            else
            {
                MessageBox.Show("No entries found for: " + call);
            }

        }

        public async Task process_data(string data)
        {
            try
            {
                string[] R = data.Split('\t');
                RX.id = Convert.ToInt64(R[0].Trim());
                RX.time = Convert.ToDateTime(R[1]);
                RX.band = Convert.ToInt16(R[2]);
                RX.rx_sign = R[3];
                RX.rx_lat = (float)Convert.ToDouble(R[4]);
                RX.rx_lon = (float)Convert.ToDouble(R[5]);
                RX.rx_loc = R[6];
                RX.tx_sign = R[7];
                RX.tx_lat = (float)(Convert.ToDouble(R[8]));
                RX.tx_lon = (float)(Convert.ToDouble(R[9]));
                RX.tx_loc = R[10];
                RX.distance = Convert.ToInt32(R[11]);
                RX.azimuth = Convert.ToInt32(R[12]);
                RX.rx_azimuth = Convert.ToInt32(R[13]);
                RX.frequency = Convert.ToInt32(R[14]);
                RX.power = Convert.ToInt16(R[15]);
                RX.snr = Convert.ToInt16(R[16]);
                RX.drift = Convert.ToInt16(R[17]);
                RX.version = R[18];
                RX.code = Convert.ToInt16(R[19]);
            }
            catch
            {

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

        private void updatebutton_Click(object sender, EventArgs e)
        {
            updateResults();
        }
        private async void updateResults()
        {
            show_results(server, user, pass);
        }

        public async Task get_results(string call, string freq, string server, string db_user, string db_pass, int timespan)
        {
            //timespan eg. last 5 minutes, limit eg. 500 - no. of entries to retrieve
            var live = new Wspr_live();
            int band = 0;

            if (stopUrl)
            {
                return;
            }
            if (!await live.checkSQL())
            {
                return;
            }

            await live.Get_Received(call, band, timespan, liveLimit);   //callsign, band (from freq - not used), timespan (las ten minutes), max entries to get
            //string received = live.Reply;
            textBox1.Text = live.textBox1.Text;
            using var reader = new StringReader(textBox1.Text);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                await process_data(line);
                await Save_Received(server, db_user, db_pass);

            }

            await Task.Delay(1000);

            await show_results(server, user, pass);

        }

        private async Task show_results(string server, string user, string pass) // read back from the reported table to populate the datagridview
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
                    find_received(rows);

                }
                dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
            }
        }

        private int table_count(string server, string user, string pass)
        {
            int count;
            string connectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rx";

            try
            {
                //string connectionString = "Server=server;Port=3306;Database=wspr;User ID=user;Password=pass;";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM reported", connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                    connection.Close();
                }
                return count;

            }
            catch
            {
                return 0;
            }
        }

        private bool find_received(int tablecount) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rx";

            if (!databaseError)
            {
                try
                {
                    MySqlConnection connection = new MySqlConnection(myConnectionString);

                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    //SELECT* FROM your_table ORDER BY your_date_column DESC LIMIT 500;

                    command.CommandText = "SELECT * FROM reported ORDER BY time DESC LIMIT " + maxrows;
                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        found = true;

                        if (i < maxrows && i < tablecount)    //only show first maxrows rows, or to length of reported table
                        {

                            RX.time = (DateTime)Reader["time"];
                            RX.band = (Int16)Reader["band"];
                            RX.rx_sign = (string)Reader["rx_sign"];
                            RX.rx_loc = (string)Reader["rx_loc"];
                            RX.tx_sign = (string)Reader["tx_sign"];
                            RX.tx_loc = (string)Reader["tx_loc"];
                            RX.distance = (int)Reader["distance"];
                            RX.azimuth = (int)Reader["azimuth"];
                            RX.frequency = (int)Reader["frequency"];
                            RX.power = (Int16)Reader["power"];
                            RX.snr = (Int16)Reader["snr"];
                            RX.drift = (Int16)Reader["drift"];
                            RX.version = (string)Reader["version"];


                            cells[0] = RX.time.ToString("yyyy-MM-dd HH:mm"); //time
                            cells[1] = RX.tx_sign; //tx sign
                            double f = Convert.ToDouble(RX.frequency);
                            f = f / 1000000;
                            string formattedF = f.ToString("F6");
                            cells[2] = formattedF; //freq
                            string snr = Convert.ToString(RX.snr);
                            if (RX.snr > 0)
                            {
                                snr = "+" + snr;
                            }
                            cells[3] = snr;  //snr
                            cells[4] = RX.drift.ToString();  //drift
                            cells[5] = RX.tx_loc;  //tx loc
                            cells[6] = RX.power.ToString();   //power dBm
                            cells[7] = RX.rx_sign;  //reporter
                            cells[8] = RX.rx_loc;    //rx loc

                            cells[9] = RX.distance.ToString();   //km
                            int km = Convert.ToInt32(RX.distance);    //miles
                            cells[10] = convert_to_miles(km);
                            cells[11] = RX.azimuth.ToString();
                            cells[12] = RX.version;   //version
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

                }
            }
            return found;
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
            int band = get_band(bandlistBox.SelectedIndex);
            if (rows > 0)
            {
                find_selected(from, to, band, rows);

            }

            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);  //order by date
        }

        private int get_band(int bandno)
        {
            int b = -2; //all
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


        private bool find_selected(string time1, string time2, int band, int tablecount) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_rx";

            string bandstr = "";
            string q = "";
            if (band == -2) //all bands
            {
                bandstr = "-1";
                q = ">";
            }
            else
            {
                bandstr = band.ToString();
                q = "=";
            }
            string callstr = "";
            string fromstr = "";
            string tostr = "";
            if (!databaseError)
            {
                try
                {
                    MySqlConnection connection = new MySqlConnection(myConnectionString);

                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    if (callFiltertextBox.Text.Trim() != "")
                    {
                        callstr = " AND rx_sign LIKE '" + callFiltertextBox.Text.Trim() + "%' ";
                    }
                    fromstr = DFromtextBox.Text.Trim();
                    tostr = DTotextBox.Text.Trim();
                    if (fromstr != "")
                    {
                        if (!kmcheckBox.Checked)
                        {
                            Double k = Convert.ToInt32(fromstr);
                            k = k * 1.609;
                            int K = (int)k;
                            fromstr = K.ToString();
                        }
                        fromstr = " AND distance >= " + fromstr + " ";
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
                        tostr = " AND distance <= " + tostr + " ";
                    }
                    //command.CommandText = "SELECT * FROM reported ORDER BY time WHERE time >= '" + time1 + "' AND time <= '" + time2 + "' AND band = '" + bandstr + "' DESC LIMIT " + maxrows;
                    if (datecheckBox.Checked)
                    {
                        command.CommandText = "SELECT * FROM reported WHERE time >= '" + time1 + "' AND time <= '" + time2 + "' AND band " + q + " '" + bandstr + "' " + callstr + fromstr + tostr + " ORDER BY time DESC LIMIT " + maxrows;
                    }
                    else
                    {
                        command.CommandText = "SELECT * FROM reported WHERE band " + q + " " + bandstr + " " + callstr + fromstr + tostr + " ORDER BY time DESC LIMIT " + maxrows;
                    }



                    MySqlDataReader Reader;
                    Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        found = true;

                        if (i < maxrows && i < tablecount)   //only show first maxrows rows, or to length of reported table
                        {

                            RX.time = (DateTime)Reader["time"];
                            RX.band = (Int16)Reader["band"];
                            RX.rx_sign = (string)Reader["rx_sign"];
                            RX.rx_loc = (string)Reader["rx_loc"];
                            RX.tx_sign = (string)Reader["tx_sign"];
                            RX.tx_loc = (string)Reader["tx_loc"];
                            RX.distance = (int)Reader["distance"];
                            RX.azimuth = (int)Reader["azimuth"];
                            RX.frequency = (int)Reader["frequency"];
                            RX.power = (Int16)Reader["power"];
                            RX.snr = (Int16)Reader["snr"];
                            RX.drift = (Int16)Reader["drift"];
                            RX.version = (string)Reader["version"];


                            cells[0] = RX.time.ToString("yyyy-MM-dd HH:mm"); //time
                            cells[1] = RX.tx_sign; //tx sign
                            double f = Convert.ToDouble(RX.frequency);
                            f = f / 1000000;
                            string formattedF = f.ToString("F6");
                            cells[2] = formattedF; //freq
                            string snr = Convert.ToString(RX.snr);
                            if (RX.snr > 0)
                            {
                                snr = "+" + snr;
                            }
                            cells[3] = snr;  //snr
                            cells[4] = RX.drift.ToString();  //drift
                            cells[5] = RX.tx_loc;  //tx loc
                            cells[6] = RX.power.ToString();   //power dBm
                            cells[7] = RX.rx_sign;  //reporter
                            cells[8] = RX.rx_loc;    //rx loc

                            cells[9] = RX.distance.ToString();   //km
                            int km = Convert.ToInt32(RX.distance);    //miles
                            cells[10] = convert_to_miles(km);
                            cells[11] = RX.azimuth.ToString();
                            cells[12] = RX.version;   //version
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

                }
            }
            return found;
        }
        private void update_grid() //add rows to the datagridview
        {

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            for (int i = 0; i < 13; i++)
            {

                row.Cells[i].Value = cells[i];
            }

            dataGridView1.Rows.Add(row);
        }
        public async Task Save_Received(string serverName, string db_user, string db_pass)
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_rx";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            DateTime date = new DateTime();

            lock (_lock)
            {
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT IGNORE INTO reported(id,time,band,rx_sign,rx_lat,rx_lon,rx_loc,tx_sign,tx_lat,tx_lon,tx_loc,distance,azimuth,rx_azimuth,frequency,power,snr,drift,version,code) ";
                    command.CommandText += "VALUES(@id,@time,@band,@rx_sign,@rx_lat,@rx_lon,@rx_loc,@tx_sign,@tx_lat,@tx_lon,@tx_loc,@distance,@azimuth,@rx_azimuth,@frequency,@power,@snr,@drift,@version,@code)";

                    connection.Open();

                    //TimeSpan time = Convert.ToDateTime(cells[1]);
                    command.Parameters.AddWithValue("@id", RX.id);
                    command.Parameters.AddWithValue("@time", RX.time);
                    command.Parameters.AddWithValue("@band", RX.band);
                    command.Parameters.AddWithValue("@rx_sign", RX.rx_sign);
                    command.Parameters.AddWithValue("@rx_lat", RX.rx_lat);
                    command.Parameters.AddWithValue("@rx_lon", RX.rx_lon);
                    command.Parameters.AddWithValue("@rx_loc", RX.rx_loc);
                    command.Parameters.AddWithValue("@tx_sign", RX.tx_sign);
                    command.Parameters.AddWithValue("@tx_lat", RX.tx_lat);
                    command.Parameters.AddWithValue("@tx_lon", RX.tx_lon);
                    command.Parameters.AddWithValue("@tx_loc", RX.tx_loc);
                    command.Parameters.AddWithValue("@distance", RX.distance);
                    command.Parameters.AddWithValue("@azimuth", RX.azimuth);
                    command.Parameters.AddWithValue("@rx_azimuth", RX.rx_azimuth);
                    command.Parameters.AddWithValue("@frequency", RX.frequency);
                    command.Parameters.AddWithValue("@power", RX.power);
                    command.Parameters.AddWithValue("@snr", RX.snr);
                    command.Parameters.AddWithValue("@drift", RX.drift);
                    command.Parameters.AddWithValue("@version", RX.version);
                    command.Parameters.AddWithValue("@code", RX.code);
                    command.ExecuteNonQuery();


                    connection.Close();

                }
                catch
                {         //if row already exists then try updating it in database

                }
            }

        }
        private bool update_received(string serverName, string db_user, string db_pass)  //not used
        {
            string c = "";
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_rx";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            try
            {
                MySqlCommand command = connection.CreateCommand();
                c = "UPDATE reported SET id = " + RX.id + ", time = " + RX.time + ", band = " + RX.band;
                c = c + ", rx_sign = '" + RX.rx_sign + "', rx_lat = " + RX.rx_lat + ", rx_lon = " + RX.rx_lon + ", rx_loc = '" + RX.rx_loc + "'";
                c = c + ", tx_sign = '" + RX.tx_sign + "', tx_lat = " + RX.tx_lat + ", tx_lon = " + RX.tx_lon + ", tx_loc = '" + RX.tx_loc + "', distance = " + RX.distance;
                c = c + ", azimuth = " + RX.azimuth + ", rx_azimuth = " + RX.rx_azimuth;
                c = c + ", frequency = " + RX.frequency + ", power = " + RX.power + ", snr = " + RX.snr + ", drift = " + RX.drift + ", version = '" + RX.version + "', code = " + RX.code;
                //c = c + " WHERE slots.Date = '" + d + "' AND slots.Time = '" + t + "'"; // + ";";              
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
                //MessageBox.Show("Unable to save to database", "");
                return false;
            }
        }

        private async void WXbutton_Click(object sender, EventArgs e)
        {
            var live = new Wspr_live();

            //await live.Get_Weather();   //
            string spots = live.spots;
            string info = live.Info;
            MessageBox.Show("Not implemented");
        }



        private void filterbutton_Click(object sender, EventArgs e)
        {
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

            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
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

        private void DFromtextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void callFiltertextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, digits, and basic punctuation
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !".,-_ ".Contains(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void Nowbutton_Click(object sender, EventArgs e)
        {
            updateNow();

        }
        private async Task updateNow()
        {
            int min = 30;
            if (PlistBox.SelectedIndex > -1)
            {
                min = findP();
            }
            string url = "http://db1.wspr.live";
            if (stopUrl)
            {
                Msg.TMessageBox("Internet is disabled", "", 2000);
                return;
            }
            var live = new Wspr_live();
            if (!await live.checkSQL())
            {
                Msg.TMessageBox("Cannot connect", "", 2000);
            }
            string freq = "";
            if (!timer1.Enabled)
            {

                get_results(Callsign, freq, server, user, pass, min);
                PlistBox.SelectedIndex = 0;
            }
            else
            {

                return;
            }
            timer1.Interval = 140000;
            timer1.Enabled = true;
            timer1.Start(); //prevent multiple presses within 2 minutes
            Nowbutton.Text = "Wait ...";
        }

        private int findP()
        {

            int p = PlistBox.SelectedIndex;
            int i = 0;
            switch (p)
            {
                case 0:
                    p = 10;
                    break;
                case 1:
                    i = 30;
                    break;
                case 2:
                    i = 60;
                    break;
                case 3:
                    i = 180;
                    break;
                case 4:
                    i = 360;
                    break;
                case 5:
                    i = 720;
                    break;
                case 6:
                    i = 1440;
                    break;
                default:
                    i = 0;
                    break;
            }
            return i;


        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            Nowbutton.Text = "Update now";
            timer1.Stop();
            timer1.Enabled = false;

        }

        private void LiveForm_Load(object sender, EventArgs e)
        {

        }

        private void PlistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Plabel.Text = PlistBox.SelectedItem.ToString();
        }
    }
}
