using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Model
{
    internal class User
    {
        /// <summary>
        /// Unique id for the user
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Save locarion for the picture
        /// </summary>
        public string UserPicture { get; set; }

        /// <summary>
        /// Time of the last message
        /// </summary>
        public DateTime LastMessage { get; set; }
    }
}
