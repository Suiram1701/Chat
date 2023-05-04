using Chat.Commuication;
using Chat.Model;
using Chat.View;
using Localization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.RightsManagement;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Serialization;
using static Chat.Properties.Settings;

namespace Chat.ViewModel
{
    internal class MenuWindow : ViewModelBase
    {
        #region Localization
        public string L_Title =>
            LangHelper.GetString("MenuWindow.Title");
        public string L_NiNameInput =>
            LangHelper.GetString("MenuWindow.NiNameInput");
        public string L_PasswdInput =>
            LangHelper.GetString("MenuWindow.PasswdInput");
        public string L_SChat =>
            LangHelper.GetString("MenuWindow.SChat");
        public string L_NChat =>
            LangHelper.GetString("MenuWindow.NChat");
        public string L_NChath =>
            LangHelper.GetString("MenuWindow.NChath");
        public string L_LoadsChath =>
            LangHelper.GetString("MenuWindow.LoadsChath");
        public string L_JChat =>
            LangHelper.GetString("MenuWindow.JChat");
        public string L_JChatBtn =>
            LangHelper.GetString("MenuWindow.JChatBtn");
        public string L_IPadJoin =>
            LangHelper.GetString("MenuWindow.IPadJoin");
        #endregion

        public MenuWindow()
        {
            // Init commands
            StartChatCommand = new DelegateCommand(parameter => !HasError(nameof(Nickname)) && !HasError(nameof(Password)), parameter =>
            {
                App.Nickname = Nickname + " (Host)";
                App.Password = Password;
                App.IsHost = true;
                Com.InitHost();

                // Init saved chat history
                if (!string.IsNullOrEmpty(SelectedFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Message>));
                    List<Message> messages;
                    using (StreamReader str = new StreamReader(SelectedFile))
                        messages = (List<Message>)serializer.Deserialize(str);

                    // Add system hint
                    messages.Add(new Message()
                    {
                        Sender = "System",
                        SendTime = DateTime.Now,
                        Subject = Subject.Msg,
                        Content = LangHelper.GetString("Menu.LoadH")
                    });

                    new View.ChatWindow(messages).Show();
                }
                else
                    new View.ChatWindow().Show();
                Application.Current.MainWindow.Close();
            });

            LoadChatHistoryCommand = new DelegateCommand(parameter =>
            {
                // Select file
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Title = LangHelper.GetString("Menu.SHFile"),
                    Filter = LangHelper.GetString("Menu.HistoryFile"),
                    CheckFileExists = true,
                    Multiselect = false
                };

                if (ofd.ShowDialog() == true)
                {
                    string fileName = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\') + 1);

                    // Check if the file is serialiable
                    bool fileSerializable = true;
                    using (Stream stream = ofd.OpenFile())
                    {
                        try
                        {
                            _ = new XmlSerializer(typeof(List<Message>)).Deserialize(stream);
                        }
                        catch
                        {
                            stream.Dispose();
                            fileSerializable = false;
                        }
                       
                    }

                    if (!fileSerializable)
                    {
                        MessageBox.Show(LangHelper.GetString("Menu.Err.NVFile", fileName), LangHelper.GetString("Menu.Err.NVFileTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        SelectedFile = ofd.FileName;
                }
            });

            NewHistoryCommand = new DelegateCommand(parameter => !string.IsNullOrEmpty(SelectedFile), parameter => SelectedFile = string.Empty);

            JoinChatCommand = new DelegateCommand(parameter => !HasError(nameof(Nickname)) && !HasError(nameof(Password)) && !HasError(nameof(JoinIP)),parameter =>
            {
                App.Nickname = Nickname;
                App.Password = Password;
                App.IsHost = false;
                Com.InitClient(IPAddress.Parse(JoinIP));
                new View.ChatWindow().Show();
                Application.Current.MainWindow.Close();
            });
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

        /// <summary>
        /// clear chat history field
        /// </summary>
        public DelegateCommand NewHistoryCommand { get; }

        /// <summary>
        /// Join the given chat
        /// </summary>
        public DelegateCommand JoinChatCommand { get; }

        // Data
        /// <summary>
        /// The viewing of the selected file
        /// </summary>
        public string SelectedFileView =>
            !string.IsNullOrEmpty(SelectedFile) ? SelectedFile.Substring(SelectedFile.LastIndexOf('\\') + 1) : L_NChat;

        /// <summary>
        /// Letters left in nickname input box
        /// </summary>
        public string LettersLeft =>
            LangHelper.GetString("MenuWindow.ValiErr.LF", (20 - Nickname.Length > 0 ? 20 - Nickname.Length : 0).ToString());

        /// <summary>
        /// IP to join in chat
        /// </summary>
        public string JoinIP
        {
            get => _JoinIP;
            set
            {
                if (value != _JoinIP)
                {
                    #region Validate
                    ClearErrors();
                    if (!IPAddress.TryParse(value, out _))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.JIPInv"));

                    if (value == (App.LocalOwnIP?.ToString() ?? value))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.JIPOwn"));

                    RaiseErrorsChanged();
                    #endregion

                    _JoinIP = value;
                    RaisePropertyChanged();
                    JoinChatCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _JoinIP = string.Empty;

        /// <summary>
        /// The password to join or create a chat
        /// </summary>
        public string Password
        {
            get => _Password;
            set
            {
                if (value != _Password)
                {
                    #region Validation
                    ClearErrors();

                    if (value.Length < 8)
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.PasswdMin"));

                    if (value.Length > 100)
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.MaxLet"));

                    if (value.Contains(" "))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.NoSpaces"));

                    if (string.IsNullOrEmpty(value))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.NContent"));

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
                            AddError(LangHelper.GetString("MenuWindow.ValiErr.FLetter", sb.ToString()));
                        else
                            AddError(LangHelper.GetString("MenuWindow.ValiErr.FLetters", sb.ToString()));
                    }

                    RaiseErrorsChanged();
                    #endregion

                    _Password = value;
                    RaisePropertyChanged();
                    StartChatCommand.RaiseCanExecuteChanged();
                    JoinChatCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _Password = "Password";

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
                    NewHistoryCommand.RaiseCanExecuteChanged();
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
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.MaxLetR"));

                    if (value.Contains(" "))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.NoSpaces"));

                    if (string.IsNullOrEmpty(value))
                        AddError(LangHelper.GetString("MenuWindow.ValiErr.NContent"));

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
                            AddError(LangHelper.GetString("MenuWindow.ValiErr.FLetter", sb.ToString()));
                        else
                            AddError(LangHelper.GetString("MenuWindow.ValiErr.FLetters", sb.ToString()));
                    }

                    RaiseErrorsChanged();
                    #endregion

                    _Nickname = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(LettersLeft));
                    StartChatCommand.RaiseCanExecuteChanged();
                    JoinChatCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _Nickname = Default.Username;
    }
}
