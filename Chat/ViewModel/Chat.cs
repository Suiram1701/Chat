using Chat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ViewModel
{
    /// <summary>
    /// Viewmodel for Chat window
    /// </summary>
    internal class Chat : ViewModel
    {
        /// <summary>
        /// List of users in chat
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

        private List<Message> _Messages = new List<Message>()
        {
            new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },
            new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Heute ist ein toller tag!\nAußdem muss ich noch hausaufgabe machen :( hudehuishiuhuisfhuisfhuisfhrihuihuihhiusdhcuisdhuisfhuisdrghugirhguidfghuidfghdfuihduighuihduidhuigi"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },new Message()
            {
                Sender = "TESTGVG",
                SendTime = DateTime.Now,
                Content = "Hallo, Welt!"
            },
        };

        private List<User> _Users = new List<User>()
        {
            new User()
            {
                Name = "TESTGVG"
            }, new User()
            {
                Name = "gzugzufutd"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }, new User()
            {
                Name = "TEST"
            }
        };
    }
}
