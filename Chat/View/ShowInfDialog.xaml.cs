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
            LangHelper.GetString("InfWindow.Address");

        public string L_Passwd =>
            LangHelper.GetString("InfWindow.Passwd");
        #endregion

        public string LocalIP { get; }

        public string Passwd { get; }

        public ShowInfDialog(string localIP, string passwd)
        {
            LocalIP = localIP;
            Passwd = passwd;

            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e) =>
            Close();
    }
}
