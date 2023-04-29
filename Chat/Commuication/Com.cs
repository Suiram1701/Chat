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
using Chat.View;

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
            App.IsHost = false;
            IPEndPoint endPoint = new IPEndPoint(address, Default.DefaultPort);
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Connect
            Connection.Connect(endPoint);

            if (Connection.Connected)
            {
                // Add reciving
                Connection?.BeginReceive(_Receivebuffer, 0, _Receivebuffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncClient), Connection);

                // Add send event handler
                MessageSendEventHandler += (_, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Client send: {e.Message}");

                    byte[] sendBuffer = new Message()
                    {
                        Sender = e.Sender,
                        SendTime = e.SendTime,
                        Subject = e.Subject,
                        Content = e.Message
                    }.SerializeToByteArray();
                    if (Connection.Connected)
                        Connection?.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
                };

                // Greet msg
                byte[] buffer = new Message
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Join,
                    Content = App.Password
                }.SerializeToByteArray();
                Connection.Send(buffer);
            }
            else
            {
                EndAll();
                MessageBox.Show("Cannot connect to the specified IP address!", "No connection!", MessageBoxButton.OK, MessageBoxImage.Error);
                new Menu().Show();
                Application.Current.MainWindow.Close();
            }
        }

        /// <summary>
        /// Initalize the connections for host
        /// </summary>
        public static void InitHost()
        {
            App.IsHost = true;
            Clients = new Dictionary<string, (Socket connection, byte[] buffer)>();

            // Setup local ip bind
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Default.DefaultPort);
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connection.Bind(localEndPoint);
            Connection.Listen(10);
            Connection.BeginAccept(new AsyncCallback(AcceptClient), Connection);
        }

        /// <summary>
        /// Accept a client as host
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AcceptClient(IAsyncResult ar)
        {
            // Setup connection
            Socket socket = Connection.EndAccept(ar);
            System.Diagnostics.Debug.WriteLine($"Host received connection request!");

            byte[] buffer = new byte[_BufferSize];
            int size = socket.Receive(buffer);
            Message msg;
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Message));
                msg = (Message)serializer.Deserialize(stream);
            }

            if (msg.Subject == Subject.Join)
            {
                Clients.Add(msg.Sender, (socket, new byte[_BufferSize]));
                System.Diagnostics.Debug.WriteLine($"Host connected with client: {msg.Sender}");

                // Setup receiving
                socket.BeginReceive(Clients[msg.Sender].buffer, 0, Clients[msg.Sender].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), socket);

                // Setup sending
                MessageSendEventHandler += (_, e) =>
                {
                    byte[] sendBuffer = new Message()
                    {
                        Sender = e.Sender,
                        SendTime = e.SendTime,
                        Subject = e.Subject,
                        Content = e.Message,
                    }.SerializeToByteArray();
                    socket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
                };
            }

            /* 
            if (request.Item2 == App.Password && !Clients.ContainsKey(request.Item1))
            
                // Setup sending
                MessageSendEventHandler += (_, e) =>
                {
                    byte[] buffer = new Message()
                    {
                        Sender = e.Sender,
                        SendTime = e.SendTime,
                        Subject = e.Subject,
                        Content = e.Message,
                    }.SerializeToByteArray();
                    socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                };

                // Setup receiving
                socket.BeginReceive(Clients[request.Item1].buffer, 0, Clients[request.Item1].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), socket);
            }
            else if (request.Item2 != App.Password)
            {
                byte[] buffer = new Message()
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Kick,
                    Content = "Your password was incorect!"
                }.SerializeToByteArray();
                socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            else if (!Clients.ContainsKey(request.Item1))
            {
                byte[] buffer = new Message()
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Kick,
                    Content = "Your nickname is already used! Please choose another nickname!"
                }.SerializeToByteArray();
                socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            */
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

            foreach ((Socket socket, _) in Clients?.Values)
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

        /// <summary>
        /// Invoke <see cref="MessageSendEventHandler"/>
        /// </summary>
        /// <param name="msg"></param>
        public static void SendMsg(string msg) =>
            MessageSendEventHandler?.Invoke(null, new MessageEventArgs(App.Nickname, DateTime.Now, Subject.Msg, msg));

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
                        System.Diagnostics.Debug.WriteLine($"Client receive: {message.Content}");
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
                        System.Diagnostics.Debug.WriteLine($"Host receive: {message.Content}");
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
            Clients[sender].connection?.BeginReceive(Clients[sender].buffer, 0, Clients[sender].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), Clients[sender].connection);
        }
        #endregion
    }
}
