using DiscordRPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DiscordWatchingAnime
{
    public partial class Form1 : Form
    {
        private DiscordRpcClient client;
        private List<Browser> browsers = new List<Browser>();
        private List<StreamingService> streams = new List<StreamingService>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            browsers.Add(new Browser() { Name = "Opera", Regex = "(.+) – Opera" });

            streams.Add(new StreamingService() { Name = "Crunchyroll", Regex = "Crunchyroll - Watch (.+) Folge.+", Icon="cr" });
            streams.Add(new StreamingService() { Name = "Anime on Demand", Regex = "(.+) bei Anime on Demand online schauen", Icon="aod" });
            streams.Add(new StreamingService() { Name = "Wakanim", Regex = ".+ - (.+) auf Wakanim.TV !", Icon="wa" });


            // MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            //MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            NotifyIcon notifyIcon = new NotifyIcon();
            MenuItem item = new MenuItem("Close");
            item.Click += Item_Click;
            notifyIcon.Icon = Properties.Resources.AppIcon;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
              { item });
            notifyIcon.Visible = true;


            RichPresence presence = new RichPresence()
            {
                Details = "TestDetails",
                State = "TestState",
                Assets = new Assets()
            };

            using (client = new DiscordRpcClient("427501855191334924", true, 0))
            {
                //Set the loggers
                client.Logger = new DiscordRPC.Logging.ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Info };

                //Initialize the connection
                client.Initialize();

                //Send the presence
                client.SetPresence(presence);

                //CommandInterface(client);
                PollInterface();
            }

            /*
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 15000;
            timer.Enabled = true;
            */
        }

        private void Item_Click(object sender, EventArgs e)
        {
            client.Dispose();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

        }

        private void Client_OnSpectate(object sender, DiscordRPC.Message.SpectateMessage args)
        {
            Debug.WriteLine(args.Secret);
            Process.Start(args.Secret);
        }

        private void PollInterface()
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
                        foreach (var browser in browsers)
                        {
                            Regex browserRegex = new Regex(browser.Regex);
                            if (browserRegex.IsMatch(p.MainWindowTitle))
                            {
                                var s = browserRegex.Match(p.MainWindowTitle).Groups[1].Value;
                                foreach (var st in streams)
                                {
                                    Regex streamRegex = new Regex(st.Regex);
                                    if (streamRegex.IsMatch(s))
                                    {
                                        var c = client.CurrentPresence;
                                        //var test = streamRegex.Match(s);
                                        c.Details = streamRegex.Match(s).Groups[1].Value;
                                        c.State = st.Name;
                                        c.Assets.LargeImageKey = st.Icon;
                                        //c.Secrets.SpectateSecret = "http://" + s.Replace("Crunchyroll - Watch ", "");
                                        client.SetPresence(c);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
