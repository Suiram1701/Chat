using Chat.Model;
using Chat.View;
using System.Collections.Generic;
using System.Diagnostics;
using static Localization.LangHelper;

namespace Chat.ViewModel
{
    /// <summary>
    /// Viewmodel for Chat window
    /// </summary>
    internal class Chat : ViewModel
    {
        #region Localization
        /// <summary>
        /// Window title for this window
        /// </summary>
        public string L_Title => GetString("ChatWindow.Title");

        /// <summary>
        /// 'User in Chat' label
        /// </summary>
        public string L_Users => GetString("ChatWindow.Users");

        /// <summary>
        /// 'Chat History' label
        /// </summary>
        public string L_ChatHistory => GetString("ChatWindow.ChatHistory");

        /// <summary>
        /// 'Send' tooltip text
        /// </summary>
        public string L_Send => GetString("ChatWindow.Send");

        /// <summary>
        /// Message input placeholder
        /// </summary>
        public string L_Msg => GetString("ChatWindow.MsgInputPh");
        #endregion

        public Chat()
        {
            // Setup commands
            SendCommand = new DelegateCommand(
                parameter => !string.IsNullOrEmpty(MessageInput),
                parameter =>
                {
                    MessageInput = string.Empty;

                    // Send Msg
                });
        }

        // Commands
        /// <summary>
        /// Send the message
        /// </summary>
        public DelegateCommand SendCommand { get; }

        // Data
        /// <summary>
        /// Place holder for message input
        /// </summary>
        public string MessageInputPlaceHolder =>
            string.IsNullOrEmpty(MessageInput) ? L_Msg : string.Empty;

        /// <summary>
        /// Message to send
        /// </summary>
        public string MessageInput
        {
            get => _MessageInput;
            set
            {
                if (value != MessageInput)
                {
                    _MessageInput = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MessageInputPlaceHolder));
                    SendCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string _MessageInput = string.Empty;

        /// <summary>
        /// List of users in messages in chat
        /// </summary>
        public List<Message> Messages
        {
            get => _Messages;
            set
            {
                _Messages = value;
                RaisePropertyChanged();
            }
        }
        private List<Message> _Messages = new List<Message>();

        /// <summary>
        /// List of users in chat
        /// </summary>
        public List<User> Users
        {
            get => _Users;
            set
            {
                _Users = value;
                RaisePropertyChanged();
            }
        }
        private List<User> _Users = new List<User>();
    }
}
