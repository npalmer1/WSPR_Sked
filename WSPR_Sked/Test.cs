using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Forms;

namespace WSPR_Sked
{
    internal class Test
    {   //code that can be used for testing
        /*
        private bool GetDataFill() //fills datagridview straightr from databse - this methiod is not in use!
        {
            string myConnectionString = "server=" + serverName + ";user id=admin;password=wspr;database=wspr";
            try
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);
                connection.Open();
                string query = "SELECT * FROM slots";
                MySqlCommand command = new MySqlCommand(query);
                command.Connection = connection;
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = command;
                DataTable dt = new DataTable();

                adapter.Fill(dt);
                BindingSource binding = new BindingSource();
                binding.DataSource = dt;
                dataGridView1.DataSource = binding;
                connection.Close();
                return true;

            }
            catch
            {
                MessageBox.Show("Unable to open database");
                return false;
            }
        }


        private DataTable GetData() //test function to display contemnts of database
        {
            DataTable Slots = new DataTable();
            string myConnectionString = "server=" + serverName + ";user id=admin;password=wspr;database=wspr";

            dtable.Columns.Add("Date   ");
            dtable.Columns.Add("Time");
            dtable.Columns.Add("Frequency");
            dtable.Columns.Add("Offset");
            dtable.Columns.Add("Pwr dB");
            dtable.Columns.Add("Pwr W");
            dtable.Columns.Add("Antenna");
            dtable.Columns.Add("Tuner");
            dtable.Columns.Add("Switch");
            // dtable.Columns.Add("Rotator");
            //dtable.Columns.Add("Azimuth");           
            dtable.Columns.Add("End slot");
            dtable.Columns.Add("Rpt");
            dtable.Columns.Add("Active");


            try
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);
                connection.Open();


                MySqlCommand command = connection.CreateCommand();

                //command.CommandText = "SELECT Date,Time,Frequency,Offset,Power,PowerW,Antenna,Tuner,Switch,Azimuth,Start,End,Active FROM slots ORDER BY Date,Time;";
                //command.CommandText = "SELECT * FROM slots WHERE Time LIKE '09%' ORDER BY Date,Time;";
                command.CommandText = "SELECT * FROM slots ORDER BY Date,Time;";
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    try
                    {
                        DateTime dt = new DateTime();
                        dt = (DateTime)Reader["Date"];
                        TimeSpan ts = new TimeSpan();
                        string date = dt.ToString();
                        ts = (TimeSpan)Reader["Time"];
                        string time = ts.ToString(@"hh\:mm");
                        freq = (double)Reader["Frequency"];
                        freq = Math.Round(freq, 4);
                        offset = (int)Reader["Offset"];
                        powerdB = (int)Reader["Power"];
                        powerW = (double)Reader["PowerW"];
                        ant = (string)Reader["Antenna"];
                        tuner = (int)Reader["Tuner"];
                        swi = (int)Reader["Switch"];
                        try
                        {
                            dt = (DateTime)Reader["End"];
                            endslot = dt.ToString(dateformat);
                        }
                        catch
                        {
                            endslot = "2025-01-01";
                        }
                        bool a = (bool)Reader["Active"];
                        if (a)
                        { active = tick; }
                        else { active = cross; }
                        a = (bool)Reader["Repeating"];
                        if (a)
                        { rpt = tick; }
                        else { rpt = cross; }


                        dtable.Rows.Add(date, time, freq, offset, powerdB, powerW, ant, tuner, swi, endslot, rpt, active);
                    }
                    catch
                    {
                        MessageBox.Show("Error reading data");
                    }
                }
                Reader.Close();
                connection.Close();
                dataGridView1.DataSource = dtable;

            }
            catch
            {
                MessageBox.Show("Unable to open database");
                return null;
            }
            return Slots;

        }
        */
    }

}
