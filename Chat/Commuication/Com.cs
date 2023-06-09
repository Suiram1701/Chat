﻿using Chat.Events;
using Chat.Extensions;
using Chat.Model;
using Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using static Chat.Properties.Settings;
using Menu = Chat.View.Menu;

namespace Chat.Commuication
{
    internal static class Com
    {
        private static readonly int _BufferSize = 4096;

        /// <summary>
        /// Own connection for client and listen connection for host
        /// </summary>
        public static Socket Connection { get; private set; }

        /// <summary>
        /// All clients that are connected with host
        /// </summary>
        public static Dictionary<string, (string username, Socket connection, byte[] buffer)> Clients { get; private set; }

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
        public static void InitClient(IPAddress address)
        {
            IPEndPoint endPoint = new IPEndPoint(address, Default.DefaultPort);
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);

            // Connect
            try { Connection.ConnectAsync(endPoint); }
            catch { }

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
                byte[] greetBuffer = new Message
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Join,
                    Content = App.Password
                }.SerializeToByteArray();
                Connection.Send(greetBuffer);

                byte[] buffer = new Message()
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Sync,
                    Content = null
                }.SerializeToByteArray();
                int bytes = Connection.Send(buffer, SocketFlags.None);
            }
            else
            {
                EndAll();
                MessageBox.Show(LangHelper.GetString("Com.CantCon"), LangHelper.GetString("Com.NoCon"), MessageBoxButton.OK, MessageBoxImage.Error);
                new Menu().Show();
                Application.Current.MainWindow.Close();
            }
        }

        /// <summary>
        /// Initalize the connections for host
        /// </summary>
        public static void InitHost()
        {
            App.HostLocalIP = App.LocalOwnIP;

            Clients = new Dictionary<string, (string username, Socket connection, byte[] buffer)>();

            // Setup local ip bind
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Default.DefaultPort);
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connection.Bind(localEndPoint);
            Connection.Listen(10);
            Connection.BeginAccept(new AsyncCallback(AcceptClient), Connection);

            // Setup sending
            MessageSendEventHandler += (sender, e) =>
            {
                foreach ((_, Socket socket, _) in Clients.Values)
                {
                    if (socket.Connected && sender?.ToString() != ((IPEndPoint)socket.RemoteEndPoint).Address.ToString())
                    {
                        byte[] sendBuffer = new Message()
                        {
                            Sender = e.Sender,
                            SendTime = e.SendTime,
                            Subject = e.Subject,
                            Content = e.Message,
                        }.SerializeToByteArray();
                        socket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
                    }
                }
            };
        }

        /// <summary>
        /// Accept a client as host
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AcceptClient(IAsyncResult ar)
        {
            // Setup connection
            Socket socket;
            try { socket = Connection?.EndAccept(ar) ?? null; }
            catch { socket = null; }
            if (socket == null)
                return;

            System.Diagnostics.Debug.WriteLine($"Host received connection request!");

            string ipSender = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
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
                if (msg.Content.ToString() != App.Password)
                {
                    byte[] sendBuffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Kick,
                        Content = LangHelper.GetString("Com.FPasswd")
                    }.SerializeToByteArray();
                    socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(sendAr =>
                    {
                        socket.EndSend(sendAr);
                        socket.Disconnect(false);
                        socket.Close();
                    }), socket);

                    Connection?.BeginAccept(new AsyncCallback(AcceptClient), Connection);
                    return;
                }
                else if (Clients.Values.Any(v => v.username == msg.Sender))
                {
                    byte[] sendBuffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Kick,
                        Content = LangHelper.GetString("Com.FNickn")
                    }.SerializeToByteArray();
                    socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(sendAr =>
                    {
                        socket.EndSend(sendAr);
                        socket.Disconnect(false);
                        socket.Close();
                    }), socket);
                    Connection?.BeginAccept(new AsyncCallback(AcceptClient), Connection);
                    return;
                }

                // Add client list
                if (Clients.ContainsKey(ipSender))
                    goto End;

                Clients.Add(ipSender, (msg.Sender, socket, new byte[_BufferSize]));
                System.Diagnostics.Debug.WriteLine($"Host connected with client: {msg.Sender}, ip: {ipSender}");

                MessageReceivedEventHandler?.Invoke(ipSender, new MessageEventArgs(msg.Sender, msg.SendTime, Subject.Join, null));
                MessageSendEventHandler?.Invoke(ipSender, new MessageEventArgs(msg.Sender, msg.SendTime, Subject.Join, null));

                // Setup receiving
                IAsyncResult proccess;
                try { proccess = socket.BeginReceive(Clients[ipSender].buffer, 0, Clients[ipSender].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), socket); }
                catch { goto End; }
                Clients[ipSender] = (msg.Sender, socket, Clients[ipSender].buffer);
            }
        End:
            Connection?.BeginAccept(new AsyncCallback(AcceptClient), Connection);
        }

        /// <summary>
        /// End connection and as host kick all
        /// </summary>
        public static void EndAll()
        {
            try
            {
                if (App.IsHost)
                {
                    Connection?.Close();
                    Connection = null;
                }
                else
                {
                    byte[] buffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Leave,
                        Content = null
                    }.SerializeToByteArray();
                    Connection?.Send(buffer, SocketFlags.None);
                    Connection?.Shutdown(SocketShutdown.Both);
                    Connection?.Disconnect(false);
                }
            }
            catch { }

            if (App.IsHost)
            {
                foreach ((_, Socket socket, _) in Clients.Values)
                {
                    byte[] buffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Kick,
                        Content = LangHelper.GetString("Com.HShutdown")
                    }.SerializeToByteArray();
                    socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ar =>
                    {
                        socket?.EndSend(ar);
                        socket?.Shutdown(SocketShutdown.Both);
                        socket?.Disconnect(false);
                        socket?.Close();
                    }), socket);
                }
                Clients?.Clear();
            }
        }

        /// <summary>
        /// Invoke <see cref="MessageSendEventHandler"/>
        /// </summary>
        /// <param name="msg"></param>
        public static void SendMsg(string msg) =>
            MessageSendEventHandler?.Invoke(null, new MessageEventArgs(App.Nickname, DateTime.Now, Subject.Msg, msg));

        /// <summary>
        /// Kick a user async as admin
        /// </summary>
        /// <param name="user">The user ip to kick</param>
        /// <param name="reason">Why will you kick him</param>
        public static void KickUsr(string user, string reason)
        {
            if (App.IsHost && Clients.ContainsKey(user))
            {
                (string username, Socket socket, _) = Clients[user];

                // Send kick msg and remove
                byte[] buffer = new Message
                {
                    Sender = App.Nickname,
                    SendTime = DateTime.Now,
                    Subject = Subject.Kick,
                    Content = reason
                }.SerializeToByteArray();
                socket.Send(buffer, SocketFlags.None);
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Disconnect(false);
                socket?.Close();
                Clients.Remove(user);

                MessageReceivedEventHandler?.Invoke(user, new MessageEventArgs(username, DateTime.Now, Subject.Leave, null));
            }
        }

        #region Receiving
        private static byte[] _Receivebuffer = new byte[_BufferSize];

        /// <summary>
        /// Handle 'BeginReceive' for client
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivingAsyncClient(IAsyncResult ar)
        {
            string sender = ((IPEndPoint)((Socket)ar.AsyncState).RemoteEndPoint).Address.ToString();
            int bytes;

            try { bytes = Connection?.EndReceive(ar) ?? 0; }
            catch { bytes = 0; }

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
                catch (Exception)
                {
                    stream.Dispose();
                    goto End;
                }
                finally { _Receivebuffer = new byte[_BufferSize]; }

                // Interpret message
                switch (message.Subject)
                {
                    case Subject.Msg:
                    case Subject.Join:
                    case Subject.Leave:
                        MessageReceivedEventHandler?.Invoke(sender, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                        System.Diagnostics.Debug.WriteLine($"Client receive: {message.Content} from {message.Sender}");
                        break;
                    case Subject.Sync:
                        // Get host ip addresses and set them
                        string[] ips = message.Content.ToString().Split(' ');
                        App.HostLocalIP = IPAddress.Parse(ips[0]);
                        break;
                    case Subject.Kick:
                        if (!App.IsHost)
                        {
                            EndAll();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                new Menu().Show();
                                View.ChatWindow.Instance.Close();
                                MessageBox.Show(message.Content?.ToString(), LangHelper.GetString("Com.Kicked"), MessageBoxButton.OK, MessageBoxImage.None);
                            });
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
            string sender;
            int bytes;
            try
            {
                sender = ((IPEndPoint)((Socket)ar.AsyncState).RemoteEndPoint).Address.ToString();
                bytes = Clients[sender].connection.EndReceive(ar);
            }
            catch { return; }

            if (bytes <= 0)
                goto End;

            Message message;
            using (MemoryStream stream = new MemoryStream(Clients[sender].buffer))
            {
                // Decode message
                XmlSerializer serializer = new XmlSerializer(typeof(Message));

                try
                {
                    message = (Message)serializer.Deserialize(stream);
                }
                catch (Exception)
                {
                    stream.Dispose();
                    goto End;
                }
                finally
                {
                    Clients[sender] = (Clients[sender].username, Clients[sender].connection, new byte[_BufferSize]);
                }
            }

            // Interpret message
            switch (message.Subject)
            {
                case Subject.Leave:
                    Socket con = Clients[sender].connection;
                    con.Shutdown(SocketShutdown.Both);
                    con.Disconnect(false);
                    con.Close();
                    Clients.Remove(sender);
                    MessageReceivedEventHandler?.Invoke(sender, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, string.Empty));
                    MessageSendEventHandler?.Invoke(sender, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, string.Empty));
                    System.Diagnostics.Debug.WriteLine($"Client {message.Sender} left the chat. ip: {sender}");
                    return;
                case Subject.Msg:
                case Subject.Join:
                    MessageReceivedEventHandler?.Invoke(sender, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                    MessageSendEventHandler?.Invoke(sender, new MessageEventArgs(message.Sender, message.SendTime, message.Subject, message.Content.ToString()));
                    System.Diagnostics.Debug.WriteLine($"Host receive: {message.Content}");
                    break;
                case Subject.Sync:
                    // Send ip address
                    byte[] ipSyncBuffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.Sync,
                        Content = App.LocalOwnIP.ToString()
                    }.SerializeToByteArray();
                    Clients[sender].connection.Send(ipSyncBuffer, SocketFlags.None);

                    // Send all chat messages
                    List<Message> messages = ViewModel.ChatWindow.s_Chats.ToList();
                    foreach (Message msg in messages)
                    {
                        // If system message edit it
                        if (msg.Subject == Subject.Join || msg.Subject == Subject.Leave)
                            msg.Sender = msg.Content?.ToString().Split(' ')[0];

                        byte[] msgBuffer = msg.SerializeToByteArray();
                        Clients[sender].connection.Send(msgBuffer, SocketFlags.None);
                    }

                    // Sync users
                    List<User> users = ViewModel.ChatWindow.s_Users.ToList();
                    StringBuilder formatUsr = new StringBuilder();
                    foreach (User user in users)     // Format to string
                        formatUsr.Append(user.Name + "," + user.IP + " ");
                    byte[] usrBuffer = new Message()
                    {
                        Sender = App.Nickname,
                        SendTime = DateTime.Now,
                        Subject = Subject.SyncUsr,
                        Content = formatUsr.ToString(),
                    }.SerializeToByteArray();
                    Clients[sender].connection.Send(usrBuffer, SocketFlags.None);
                    break;
                case Subject.Kick:
                    break;
            }

        End:
            IAsyncResult proccess = Clients[sender].connection?.BeginReceive(Clients[sender].buffer, 0, Clients[sender].buffer.Length, SocketFlags.None, new AsyncCallback(ReceivingAsyncHost), Clients[sender].connection);
            Clients[sender] = (Clients[sender].username, Clients[sender].connection, Clients[sender].buffer);
        }
        #endregion
    }
}
