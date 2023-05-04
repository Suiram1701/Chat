using Localization;
using System.Windows;

namespace Chat.View
{
    /// <summary>
    /// A dialog for a reason why the user will kicked
    /// </summary>
    public partial class KickDialog : Window
    {
        #region Localization
        public string L_Title
        {
            get => LangHelper.GetString("KickDialog.Title", User);
        }
        public string L_Reason =>
            LangHelper.GetString("KickDialog.Reason");
        public string L_Abort =>
            LangHelper.GetString("KickDialog.Abort");
        public string L_OK =>
            LangHelper.GetString("KickDialog.OK");
        #endregion

        /// <summary>
        /// The reason why you would kick the player
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The user to kick
        /// </summary>
        public string User { get; set; }

        public KickDialog(string user)
        {
            User = user;
            InitializeComponent();
        }

        private void Kick_Click(object sender, RoutedEventArgs e) =>
            DialogResult = true;

        private void Cancel_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}
