﻿using Chat.Model;
using System.IO;
using System.Xml.Serialization;

namespace Chat.Extensions
{
    internal static class MessageExtensions
    {
        /// <summary>
        /// Convert a <see cref="Message"/> instance to byte
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] SerializeToByteArray(this Message message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, message);
            return stream.GetBuffer();
        }
    }
}
