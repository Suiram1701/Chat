using Localization;
using System.Windows;

namespace Chat.View
{
    /// <summary>
    /// Interaktionslogik für ShowInfDialog.xaml
    /// </summary>
    public partial class ShowInfDialog : Window
    {
        #region Localisation
        public string L_Title =>
            LangHelper.GetString("ChatWindow.ViewInf");

        public string L_SameNet =>
            LangHelper.GetString("InfWindow.SameNet");

        public string L_OtherNet =>
            LangHelper.GetString("InfWindow.OtherNet");

        public string L_Passwd =>
            LangHelper.GetString("InfWindow.Passwd");
        #endregion

        public string LocalIP { get; }

        public string PublicIP { get; }

        public string Passwd { get; }

        public ShowInfDialog(string localIP, string publicIP, string passwd)
        {
            LocalIP = localIP;
            PublicIP = publicIP;
            Passwd = passwd;

            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e) =>
            Close();
    }
}
