using Chat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chat.Model.Message;

namespace Chat.Events
{
    internal class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// User that send the message
        /// </summary>
        public string Sender { get; }

        /// <summary>
        /// Time on that send the message
        /// </summary>
        public DateTime SendTime { get; }

        /// <summary>
        /// The subject of the message
        /// </summary>
        public Subject Subject { get; }

        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; }

        public MessageEventArgs(string sender, DateTime sendTime, Subject subject, string message)
        {
            Sender = sender;
            SendTime = sendTime;
            Subject = subject;
            Message = message;
        }
    }
}
