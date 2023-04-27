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
using System.IO;
using System.Xml.Serialization;

namespace Chat.Commuication
{
    internal static class Com
    {
        private static readonly int _BufferSize = 1024;

        /// <summary>
        /// Own connection for client and listen connection for host
        /// </summary>
        public static Socket Connection { get; private set; }

        /// <summary>
        /// All clients that are connected with host
        /// </summary>
        public static Dictionary<string, (Socket connection, byte[] buffer)> Clients { get; private set; }

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
            IPEndPoint endPoint = new IPEndPoint(address, Default.DefaultPort);
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Add send event handler
            MessageSendEventHandler += (_, e) =>
            {
                byte[] buffer = new Message()
                {
                    Sender = e.Sender,
                    SendTime = e.SendTime,
                    Subject = e.Subject,
                    Content = e.Message
                }.SerializeToByteArray();
                Connection?.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) => Connection?.EndSend(ar)), (nickname, Connection));
            };

            // Connect
            Connection.BeginConnect(endPoint, new AsyncCallback(ar =>
            {
                Connection.EndConnect(ar);
                MessageSendEventHandler?.Invoke(null, new MessageEventArgs(nickname, DateTime.Now, Subject.Join, null));
            }), (nickname, password, Connection));

            // Add reciving
            Connection?.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncClient), Connection);
        }

        /// <summary>
        /// Initalize the connections for host
        /// </summary>
        public static void InitHost(string nickname, string password)
        {
            IPEndPoint endPoint = new IPEndPoint(App.OwnIP, Default.DefaultPort);
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Connection.Bind(endPoint);
            Connection.Listen(10);

            // Setup connection accepting
            Connection.BeginAccept(new AsyncCallback(ar =>
            {
                // Setup connection
                Tuple<string, string, Socket> request = ar.AsyncState as Tuple<string, string, Socket> ?? throw new ArgumentNullException(nameof(ar.AsyncState));
                Socket socket = Connection.EndAccept(ar);

                if (request.Item2 == password)
                {
                    Clients.Add(request.Item1, (socket, new byte[_BufferSize]));

                    // Setup sending
                    MessageSendEventHandler += (_, e) =>
                    {
                        socket.Send(new Message()
                        {
                            Sender = e.Sender,
                            SendTime = e.SendTime,
                            Subject = e.Subject,
                            Content = e.Message,
                        }.SerializeToByteArray());
                    };

                    // Setup receiving
                    socket.BeginReceive(Clients[request.Item1].buffer, 0, Clients[request.Item1].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), socket);
                }
                else
                {
                    socket.Send(new Message()
                    {
                        Sender = nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Kick,
                        Content = null
                    }.SerializeToByteArray());
                }
            }), Connection);
        }

        /// <summary>
        /// End connection and as host kick all
        /// </summary>
        public static void EndAll()
        {
            Connection?.Shutdown(SocketShutdown.Both);
            Connection?.Disconnect(false);
            Connection?.Close();
            Connection = null;

            foreach ((Socket socket, _) in Clients.Values)
            {
                socket.Send(new Message()
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Kick,
                    Content = "The has been host shutdown the server!",
                }.SerializeToByteArray());
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Disconnect(false);
                socket?.Close();
            }
            Clients.Clear();
        }

        #region Receiving
        private static byte[] _Receivebuffer = new byte[_BufferSize];

        /// <summary>
        /// Handle 'BeginReceive' for client
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivingAsyncClient(IAsyncResult ar)
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
            Connection?.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncClient), Connection);
        }

        /// <summary>
        /// Handle receiving as host
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivingAsyncHost(IAsyncResult ar)
        {
            int bytes = Connection.EndReceive(ar);
            string sender = (ar.AsyncState as (string, Socket)?)?.Item1;

            if (bytes <= 0)
                goto End;

            using (MemoryStream stream = new MemoryStream(Clients[sender].buffer))
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
                    case Subject.Leave:
                        Socket con = Clients[message.Sender].connection;
                        con.Shutdown(SocketShutdown.Both);
                        con.Disconnect(false);
                        con.Close();
                        Clients.Remove(message.Sender);
                        MessageReceivedEventHandler?.Invoke(null, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        MessageSendEventHandler?.Invoke(null, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        break;
                    case Subject.Msg:
                    case Subject.Join:
                        MessageReceivedEventHandler?.Invoke(null, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        MessageSendEventHandler?.Invoke(null, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        break;
                    case Subject.Sync:
                        if (App.IsHost)
                            break;     //TODO: Syncronize
                        break;
                    case Subject.Kick:
                        break;
                }
            }

        End:
            Clients[sender].connection?.BeginReceive(Clients[sender].buffer, 0, Clients[sender].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncClient), Connection);
        }
        #endregion
    }
}
