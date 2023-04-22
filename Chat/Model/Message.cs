using System;

namespace Chat.Model
{
    internal class Message
    {
        /// <summary>
        /// User that send the message
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Time of send message
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        public string Content { get; set; }
    }
}