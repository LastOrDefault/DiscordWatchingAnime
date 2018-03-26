using DiscordRPC;
using DiscordWatchingAnime.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordWatchingAnime
{
    public class NotifyApplicationContext: ApplicationContext
    {
        private NotifyIcon trayIcon;
        private DiscordRpcClient client;
        private List<Browser> browsers = new List<Browser>();
        private List<StreamingService> streams = new List<StreamingService>();

        private bool stopRpc = false;

        public NotifyApplicationContext()
        {
            /*
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                Debug.WriteLine(p.MainWindowTitle);
            }
            */

            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Close", Close)}),
                Visible = true
            };

            //Read browsers
            using (StreamReader sr = new StreamReader("browser.config"))
            {
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] data = line.Split(';');
                    if (data.Length != 2)
                        Debug.WriteLine($"Could not read Browser:'{line}'");
                    else
                        browsers.Add(new Browser() { Name = data[0], Regex = data[1] });
                }
            }

            //Read streaming services
            using (StreamReader sr = new StreamReader("stream.config"))
            {
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] data = line.Split(';');
                    if (data.Length != 3)
                        Debug.WriteLine($"Could not read streaming service:'{line}'");
                    else
                        streams.Add(new StreamingService() { Name = data[0], Regex = data[1], Icon = data[2] });
                }
            }

            /*
            browsers.Add(new Browser() { Name = "Opera", Regex = "(.+) – Opera" });
            browsers.Add(new Browser() { Name = "Chrome", Regex = "(.+) - Google Chrome" });
            browsers.Add(new Browser() { Name = "Firefox", Regex = "(.+) - Mozilla Firefox" });
            browsers.Add(new Browser() { Name = "Edge", Regex = "(.+) - Microsoft Edge" });

            streams.Add(new StreamingService() { Name = "Crunchyroll", Regex = "Crunchyroll - Watch (.+) Folge.+", Icon = "cr" });
            streams.Add(new StreamingService() { Name = "Anime on Demand", Regex = "(.+) bei Anime on Demand online schauen", Icon = "aod" });
            streams.Add(new StreamingService() { Name = "Wakanim", Regex = ".+ - (.+) auf Wakanim.TV !", Icon = "wa" });
            */

            Thread thread = new Thread(new ThreadStart(StartDiscordRpc));
            thread.Start();
        }

        private void StartDiscordRpc()
        {
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

                while (!stopRpc)
                {
                    //Invoke the clients events
                    if (client != null)
                    {
                        client.Invoke();
                    }


                    bool streamFound = false;
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
                                            streamFound = true;

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

                    if (!streamFound)
                    {
                        var c = client.CurrentPresence;
                        //var test = streamRegex.Match(s);
                        c.Details = "";
                        c.State = "";
                        c.Assets.LargeImageKey = "";
                        //c.Secrets.SpectateSecret = "http://" + s.Replace("Crunchyroll - Watch ", "");
                        client.SetPresence(c);
                    }
                }

                //Close everything
                client.Dispose();
                ExitThread();

            }
        }

        private void PollInterface()
        {
            
        }

        private void Close(object sender, EventArgs e)
        {
            Debug.WriteLine("Close Application now.");
            stopRpc = true;
        }
    }
}
