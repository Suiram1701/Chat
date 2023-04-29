using Chat.Commuication;
using Chat.Model;
using Chat.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using static Localization.LangHelper;

namespace Chat.ViewModel
{
    /// <summary>
    /// Viewmodel for Chat window
    /// </summary>
    internal class Chat : ViewModelBase
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
            SendCommand = new DelegateCommand(parameter => !string.IsNullOrEmpty(MessageInput),
                parameter =>
                {
                    Com.SendMsg(MessageInput);
                    if (App.IsHost)
                    {
                        Messages.Add(new Message()
                        {
                            Sender = App.Nickname,
                            SendTime = DateTime.Now,
                            Subject = Subject.Msg,
                            Content = MessageInput
                        });
                        RaisePropertyChanged(nameof(Messages));
                    }

                    MessageInput = string.Empty;
                });

            // Setup chat
            Com.MessageReceivedEventHandler += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (e.Subject)
                    {
                        case Subject.Msg:
                            Messages.Add(new Message()
                            {
                                Sender = e.Sender,
                                SendTime = e.SendTime,
                                Subject = e.Subject,
                                Content = e.Message
                            });
                            break;
                        case Subject.Join:
                            Users.Add(new User()
                            {
                                Name = e.Sender,
                                IP = sender.ToString(),
                            });
                            Messages.Add(new Message()
                            {
                                Sender = "System",
                                SendTime = e.SendTime,
                                Subject = e.Subject,
                                Content = $"{e.Sender} joined the chat"
                            });
                            RaisePropertyChanged(nameof(Users));
                            break;
                        case Subject.Leave:
                            Users.Remove(Users.FirstOrDefault(u => u.IP == sender.ToString()));
                            Messages.Add(new Message()
                            {
                                Sender = "System",
                                SendTime = e.SendTime,
                                Subject = e.Subject,
                                Content = $"{e.Sender} left the chat"
                            });
                            RaisePropertyChanged(nameof(Users));
                            break;
                    }
                    RaisePropertyChanged(nameof(Messages));
                });
            };

            // Add self
            if (App.IsHost)
            {
                Users.Add(new User()
                {
                    Name = App.Nickname,
                    IP = App.LocalOwnIP.ToString()
                });
                RaisePropertyChanged(nameof(Users));
            }
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
        public ObservableCollection<Message> Messages
        {
            get => _Messages;
            set
            {
                _Messages = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<Message> _Messages = new ObservableCollection<Message>();

        /// <summary>
        /// List of users in chat
        /// </summary>
        public ObservableCollection<User> Users
        {
            get => _Users;
            set
            {
                _Users = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<User> _Users = new ObservableCollection<User>();
    }
}
