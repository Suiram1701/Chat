using System.Windows;
using static Chat.Properties.Settings;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.Net;
using Localization;
using System.Linq;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;
using Chat.Commuication;
using System.Net.Sockets;

namespace Chat
{
    public partial class App : Application
    {
        /// <summary>
        /// Is the current pc host or not
        /// </summary>
        public static bool IsHost;

        /// <summary>
        /// Your nickname
        /// </summary>
        public static string Nickname;

        /// <summary>
        /// Password of the current season
        /// </summary>
        public static string Password;

        /// <summary>
        /// IP of the running pc
        /// </summary>
        public static IPAddress OwnIP;

        /// <summary>
        /// Local ip of the running pc
        /// </summary>
        public static IPAddress LocalOwnIP;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Check if app is already running
            Process ownProcess = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(ownProcess.ProcessName).Cast<Process>().Count() > 1)
            {
                MessageBox.Show(LangHelper.GetString("App.RunErr"), LangHelper.GetString("App.RunErrTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }

            // If not nickname set current username
            if (string.IsNullOrEmpty(Default.Username))
            {
                string name = WindowsIdentity.GetCurrent().Name;
                Default.Username = name.Substring(name.LastIndexOf('\\') + 1);
                Default.Save();
            }

            // Check network connection
            int MaxPing = Default.MaxPing;
            using (Ping ping = new Ping())
            {
                PingReply pingResult = default;
                try
                {
                    pingResult = await ping.SendPingAsync("www.google.com", 5000);
                }
                catch
                {
                }

                if (pingResult is null)     // No Network connection
                    MessageBox.Show(LangHelper.GetString("App.NoNetConErr"), LangHelper.GetString("App.NetConErr"), MessageBoxButton.OK, MessageBoxImage.Error);

                else if (pingResult.Status == IPStatus.TimedOut || pingResult.RoundtripTime > MaxPing)     // Too high ping
                    MessageBox.Show(LangHelper.GetString("App.NetConTimeoutErr", pingResult.RoundtripTime.ToString(), MaxPing.ToString()), LangHelper.GetString("App.NetConErr"), MessageBoxButton.OK, MessageBoxImage.Error);

                else if (pingResult.Status != IPStatus.Success)     // Other error
                    MessageBox.Show(LangHelper.GetString("App.OtherNetErr", pingResult.Status.ToString()), LangHelper.GetString("App.NetConErr"), MessageBoxButton.OK, MessageBoxImage.Error);

                if (pingResult?.Status != IPStatus.Success)     // End app with network error
                {
                    ping.Dispose();
                    Shutdown();
                }
            }

            // Get own public ip
            using (WebClient client = new WebClient())
            {
                try
                {
                    string data = await client.DownloadStringTaskAsync("http://checkip.dyndns.org");
                    string ipString = Regex.Match(data, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Value;
                    if (!IPAddress.TryParse(ipString, out IPAddress ip))
                        throw new InvalidCastException(ipString);

                    OwnIP = ip;
                }
                catch (WebException ex)     // Error while connecting to website
                {
                    client.Dispose();
                    MessageBox.Show(LangHelper.GetString("App.PIPErr") + $" Details: {ex.Status}, {ex.Response}", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
                catch (InvalidCastException ex)
                {
                    client.Dispose();
                    MessageBox.Show(LangHelper.GetString("App.InvIPErr") + $": {ex.Message}", LangHelper.GetString("App.InvIPErr") + "!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }

            // Get own local ip
            LocalOwnIP = (await Dns.GetHostAddressesAsync(Dns.GetHostName())).FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.None;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // End all connections
            Com.EndAll();

            base.OnExit(e);
        }
    }
}
