using System;
using System.Xml.Serialization;

namespace Chat.Model
{
    /// <summary>
    /// All message subjects
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="Join"/> = a user join the chat
    ///         </item>
    ///         <item>
    ///             <see cref="Leave"/> = a user left the chat
    ///         </item>
    ///         <item>
    ///             <see cref="Msg"/> = a user send a message
    ///         </item>
    ///         <item>
    ///             <see cref="Sync"/> = syncronize the chat
    ///         </item>
    ///         <item>
    ///             <see cref="Kick"/> = Kick a user from the chat
    ///         </item>
    ///     </list>
    /// </remarks>
    public enum Subject
    {
        Join = 0,
        Leave = 1,
        Msg = 2,
        Sync = 3,
        Kick = 4,
        SyncUsr = 5,
    }

    public class Message
    {
        [XmlAttribute("sender")]
        /// <summary>
        /// User that send the message
        /// </summary>
        public string Sender { get; set; }

        [XmlAttribute("sendTime")]
        /// <summary>
        /// Time of send message
        /// </summary>
        public DateTime SendTime { get; set; }

        public Subject Subject { get; set; }

        [XmlElement("content")]
        /// <summary>
        /// Content of the message
        /// </summary>
        public object Content { get; set; }
    }
}