using Chat.Commuication;
using Chat.Model;
using Chat.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Chat.View
{
    /// <summary>
    /// Chat window
    /// </summary>
    public partial class ChatWindow : Window
    { 
        public static ChatWindow Instance { get; private set; }

        /// <summary>
        /// New chat window
        /// </summary>
        /// <param name="messages">Saved chat</param>
        public ChatWindow(IEnumerable<Message> messages = null)
        {
            InitializeComponent();
            Instance = this;

            // Load saved history if it given
            if (messages != null)
            {
                ((ViewModel.ChatWindow)DataContext).Messages = new ObservableCollection<Message>(messages);
                ViewModel.ChatWindow.s_Chats = messages;
            }
        }

        #region Window chrome
        /// <summary>
        /// On click on window chrome move window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowChrome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // If maximized back to normal
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;

                    // Set win pos
                    Point MousePos = e.GetPosition(this);
                    Left = MousePos.X;
                    Top = MousePos.Y;
                }

                DragMove();
            }
        }

        /// <summary>
        /// Minimize app with custom button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        /// <summary>
        /// Maximmize app with custom button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Exit app with custom button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Com.EndAll();
            Application.Current.Shutdown();
        }

        #endregion

        /// <summary>
        /// Send message on enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MsgInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SendMsg.Command.CanExecute(null))
                SendMsg.Command.Execute(null);
        }
    }
}
