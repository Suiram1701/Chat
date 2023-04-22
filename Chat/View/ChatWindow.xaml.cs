using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chat.View
{
    /// <summary>
    /// Chat window
    /// </summary>
    public partial class ChatWindow : Window
    { 
        public ChatWindow()
        {
            InitializeComponent();
        }

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
        private void Exit_Click(object sender, RoutedEventArgs e) =>
            Application.Current.Shutdown();
    }
}
