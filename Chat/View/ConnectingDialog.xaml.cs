using Localization;
using System.Windows;

namespace Chat.View
{
    /// <summary>
    /// Loading dialog for connecting
    /// </summary>
    public partial class ConnectingDialog : Window
    {
        #region Localization
        public string L_Title =>
            LangHelper.GetString("ConnectingDialog.Title", IP);
        #endregion

        private string IP = string.Empty;

        public ConnectingDialog(string ip)
        {
            IP = ip;
            InitializeComponent();
        }
    }
}
