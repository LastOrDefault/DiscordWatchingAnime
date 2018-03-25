﻿using DiscordRPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordWatchingAnime
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            //MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = DiscordWatchingAnime.Properties.Resources.AppIcon;
            //notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            //  { configMenuItem, exitMenuItem });
            notifyIcon.Visible = true;


            RichPresence presence = new RichPresence()
            {
                Details = "TestDetails",
                State = "TestState",
            };

            using (DiscordRpcClient client = new DiscordRpcClient("427501855191334924", true, 0))
            {
                //Set the loggers
                client.Logger = new DiscordRPC.Logging.ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Info };

                //Initialize the connection
                client.Initialize();

                //Send the presence
                client.SetPresence(presence);

                //CommandInterface(client);
                PollInterface(client);
            }
            /*
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 15000;
            timer.Enabled = true;
            */
        }

        private void PollInterface(DiscordRpcClient client)
        {
            while (true)
            {
                //Invoke the clients events
                if (client != null)
                {
                    client.Invoke();
                }

                Process[] processes = Process.GetProcesses();

                foreach (Process p in processes)
                {
                    if (!String.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        if (p.MainWindowTitle.EndsWith(" – Opera"))
                        {
                            var s = p.MainWindowTitle.Replace(" – Opera", "");
                            if (s.Contains("Crunchyroll - Watch "))
                            {
                                var c = client.CurrentPresence;
                                c.Details = s.Replace("Crunchyroll - Watch ", "");
                                client.SetPresence(c);
                            }
                        }
                    }
                }

                // var p = client.CurrentPresence;
                //p.Details = "Tiesjgkldjg kldr";
                //client.SetPresence(p);

                //This can be what ever value you want, as long as it is faster than 30 seconds.
                //Console.Write("+");
                Thread.Sleep(10000);
            }
        }
    }
}
