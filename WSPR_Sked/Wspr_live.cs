using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using MathNet.Numerics;
using Microsoft.VisualBasic.ApplicationServices;
using Org.BouncyCastle.Tls;
using System.Reflection.Metadata;
using System.Net.Http;
using System.Drawing;
using System.Threading;



namespace WSPRlive
{
    
    internal class Wspr_live
    {
       
        public string Reply = "";
        public string Info = "";
        public string spots = "";

        public TextBox textBox1 = new TextBox();
        public Wspr_live()
        {
        }


        public async void SQL_Get()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync("http://db1.wspr.live/?query=SELECT%20version()");
                if (response != "" || response != null)
                {

                    MessageBox.Show("SQL version: " + response + " - successful GET");
                }
            }
            catch
            {
                MessageBox.Show("Error connecting");
            }
        }

        public async Task<bool> checkSQL()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync("http://db1.wspr.live/?query=SELECT%20version()");
                if (response != "" || response != null)
                {
                    return true;
                   
                }
            }
            catch
            {
                return false;
            }
            return false;
        }


        public async Task Get_Received(string tx_call, int band, int timespan,int limit)
        {
            //note: band not currently used
            textBox1.Multiline = true;
            bool isUnlocked = false;
            int tries = 0;
            Reply = "";
            
            while (!isUnlocked)
            {

                try
                {
                    using var client = new HttpClient();
                    string query = "http://db1.wspr.live/?query=SELECT * FROM wspr.rx WHERE tx_sign LIKE '%" + tx_call + "%' AND time >= subtractMinutes(now()," + timespan + ") AND time <= subtractMinutes(now(),2) LIMIT " + limit;
                    textBox1.Text = await client.GetStringAsync(query);
                    if (textBox1.Text != null)
                    {
                        Reply = "ok";
                       
                        isUnlocked = true;
                    }
                }
                catch
                {
                    if (tries > 3)
                    {
                        Reply = "error";
                        isUnlocked = true;
                        return;
                    }
                    Thread.Sleep(800);
                    tries++;

                    
                }
            }

        }

        public async Task Get_Weather()
        {
            //note: not currently used
            Info = "";
            try
            {
                using var client = new HttpClient();
                string query = "http://db1.wspr.live/?query=SELECT * FROM weather_eisn WHERE time = now()";
                var response = await client.GetStringAsync(query);
                if (response != "" || response != null)
                {
                    spots = response;
                }
            }
            catch
            {
                spots = "error";
            }
            try
            {
                using var client = new HttpClient();
                string query = "http://db1.wspr.live/?query=SELECT * FROM weather_kp_ap WHERE time = now()";
                var response = await client.GetStringAsync(query);
                if (response != "" || response != null)
                {
                    Info = response;
                }
            }
            catch
            {
                Info = "error";
            }



        }
    }


}


   
