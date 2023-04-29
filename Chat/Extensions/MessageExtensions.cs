using Chat.Model;
using Microsoft.Win32;
using System.IO;
using System.Xml;
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
            int trys = 0;
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            Deserialize:
            MemoryStream stream = new MemoryStream();
            stream.SetLength(0);
            serializer.Serialize(stream, message);

            try
            {
                _ = serializer.Deserialize(stream);
            }
            catch
            {
                if (trys <= 10)
                {
                    trys++;
                    goto Deserialize;
                }
                else
                    return stream.GetBuffer();

            }

            return stream.GetBuffer();
        }
    }
}
