using Localization;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static Chat.Properties.Settings;

namespace Chat.ViewModel
{
    internal class Menu : ViewModelBase
    {
        // Localization


        public Menu()
        {
            // Init commands
            StartChatCommand = new DelegateCommand(parameter => !HasError(nameof(Nickname)), parameter =>
            {
                //TODO: Start new chat
            });

            LoadChatHistoryCommand = new DelegateCommand(parameter =>
            {
                // Select file
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = "Select saved chat history",
                    Filter = "Chat history file (*.chat)|*.chat",
                    CheckFileExists = true,
                    Multiselect = false
                };

                if (ofd.ShowDialog() == true)
                {
                    string fileName = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\') + 1);

                    //TODO: Check if file is a valid file
                    if (false)
                    {
                        MessageBox.Show($"The file {fileName} isn't a valid chat history file!", "Not valid file selected!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SelectedFile = ofd.FileName;
                }
            });

            NewHistoryCommad = new DelegateCommand(parameter => !string.IsNullOrEmpty(SelectedFile), parameter => SelectedFile = string.Empty);
        }

        // Commands
        /// <summary>
        /// Start a new chat as host
        /// </summary>
        public DelegateCommand StartChatCommand { get; }

        /// <summary>
        /// Load a saved chat history
        /// </summary>
        public DelegateCommand LoadChatHistoryCommand { get; }

        public DelegateCommand NewHistoryCommad { get; }

        // Data
        public string SelectedFileView =>
            !string.IsNullOrEmpty(SelectedFile) ? SelectedFile.Substring(SelectedFile.LastIndexOf('\\') + 1) : "New Chat";

        /// <summary>
        /// Letters left in nickname input box
        /// </summary>
        public string LettersLeft =>
            "Letters left: " + (20 - Nickname.Length > 0 ? 20 - Nickname.Length : 0);

        public string SelectedFile
        {
            get => _SelectedFile;
            set
            {
                if (_SelectedFile != value)
                {
                    _SelectedFile = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SelectedFileView));
                    NewHistoryCommad.RaiseCanExecuteChanged();
                }
            }
        }
        private string _SelectedFile = string.Empty;

        /// <summary>
        /// Nickname in chat
        /// </summary>
        public string Nickname
        {
            get => _Nickname;
            set
            {
                if (value != _Nickname)
                {
                    #region Validation
                    ClearErrors();

                    if (value.Length > 20)
                        AddError("Maximum letters reached!");

                    if (value.Contains(" "))
                        AddError("Mustn't contain spaces!");

                    if (string.IsNullOrEmpty(value))
                        AddError("Field cannot be empty!");

                    // Check for invalid letters
                    StringBuilder sb = new StringBuilder();
                    foreach (char c in value)
                        if (!char.IsLetterOrDigit(c) && c != ' ')
                            if (!sb.ToString().Contains(c.ToString()))
                                sb.Append($"{c}, ");
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                        if (sb.Length == 1)
                            AddError(sb.ToString() + " isn't a valid letter!");
                        else
                            AddError(sb.ToString() + " aren't a valid letters!");
                    }

                    RaiseErrorsChanged();
                    #endregion

                    _Nickname = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(LettersLeft));
                    StartChatCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _Nickname = Default.Username;
    }
}
