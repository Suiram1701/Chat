using Chat.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Chat.Properties.Settings;
using static Chat.Model.Message;
using Chat.Model;
using Chat.Extensions;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Navigation;

namespace Chat.Commuication
{
    internal static class Com
    {
        /// <summary>
        /// Token source to trigger end all async message treads
        /// </summary>
        public static CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// Own connection for client and listen connection for host
        /// </summary>
        public static Socket Connection { get; private set; }

        /// <summary>
        /// All clients that are connected with host
        /// </summary>
        public static Dictionary<string, Socket> Clients { get; private set; }

        /// <summary>
        /// Invoked when a message sended
        /// </summary>
        public static event EventHandler<MessageEventArgs> MessageSendEventHandler;

        /// <summary>
        /// Invoked when a message received
        /// </summary>
        public static event EventHandler<MessageEventArgs> MessageReceivedEventHandler;

        /// <summary>
        /// Initalize the connection for the client
        /// </summary>
        /// <param name="address">Address to join</param>
        public static void InitClient(IPAddress address, string nickname, string password)
        {
            // Setup
            Cts = new CancellationTokenSource();
            IPEndPoint endPoint = new IPEndPoint(address, Default.DefaultPort);
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Add send event handler
            MessageSendEventHandler += (_, e) => Connection.Send(new Message()
            {
                Sender = e.Sender,
                SendTime = e.SendTime,
                Subject = e.Subject,
                Content = e.Message
            }.SerializeToByteArray());

            // Connect
            Connection.BeginConnect(endPoint, new AsyncCallback(ar =>
            {
                // Item1 = access granted or denied; Item2 = deny reason
                Tuple<bool, string, Socket> resonse = ar.AsyncState as Tuple<bool, string, Socket> ?? throw new ArgumentNullException(nameof(ar.AsyncState));
                Connection.EndConnect(ar);

                if (!resonse.Item1)
                {
                    EndAll();
                    MessageBox.Show($"Access denied! {resonse.Item2}", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    MessageSendEventHandler?.Invoke(null, new MessageEventArgs(nickname, DateTime.Now, Subject.Join, null));
            }), (nickname, password, Connection));

            // Add reciving
            Connection?.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsync), Connection);
        }

        /// <summary>
        /// Initalize the connections for host
        /// </summary>
        public static void InitHost()
        {

        }

        /// <summary>
        /// End connection and as host kick all
        /// </summary>
        public static void EndAll()
        {
            Cts.Cancel();

            Connection?.Shutdown(SocketShutdown.Both);
            Connection?.Disconnect(false);
            Connection?.Close();
            Connection = null;
        }

        #region Receiving
        private static byte[] _Receivebuffer = new byte[1024];

        /// <summary>
        /// Handle 'BeginReceive'
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivingAsync(IAsyncResult ar)
        {
            int bytes = Connection.EndReceive(ar);

            if (bytes <= 0)
                goto End;

            using (MemoryStream stream = new MemoryStream(_Receivebuffer))
            {
                // Decode message
                XmlSerializer serializer = new XmlSerializer(typeof(Message));
                Message message;

                try
                {
                    message = (Message)serializer.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    Task.Run(() => MessageBox.Show(ex.Message, "Something went wrong during receiving!", MessageBoxButton.OK, MessageBoxImage.Error));
                    stream.Dispose();
                    goto End;
                }

                // Interpret message
                switch (message.Subject)
                {
                    case Subject.Msg:
                    case Subject.Join:
                    case Subject.Leave:
                        MessageReceivedEventHandler?.Invoke(null, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        break;
                    case Subject.Sync:
                        if (App.IsHost)
                            break;     //TODO: Syncronize
                        break;
                    case Subject.Kick:
                        if (!App.IsHost)
                        {
                            MessageReceivedEventHandler?.Invoke(null, new MessageEventArgs(App.Nickname, DateTime.Now, Subject.Leave, null));
                            EndAll();
                            MessageBox.Show("You was kicked from the chat!", "You was kicked!", MessageBoxButton.OK, MessageBoxImage.None);
                        }
                        break;
                }
            }

            End:
            Connection?.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsync), Connection);
        }
        #endregion
    }
}
