using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chat.ViewModel
{
    internal class Menu : ViewModelBase
    {
        // Data
        /// <summary>
        /// Nickname in chat
        /// </summary>
        public string Nickname
        {
            get => _Nickname;
            set
            {
                if (value != _Nickname)
                {
                    _Nickname = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _Nickname = string.Empty;
    }
}
