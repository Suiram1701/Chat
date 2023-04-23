using System.Windows;
using static Chat.Properties.Settings;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.Net;
using Localization;

namespace Chat
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
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

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
