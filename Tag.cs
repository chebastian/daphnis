using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BorderlessAlphaWin
{
    internal class Tag : INotifyPropertyChanged
    {
        private string _name;

        public String tName
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                SetPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void SetPropertyChanged([CallerMemberName] String name = null)
        {
            if(PropertyChanged != null)
            {
                if (name != null)
                    PropertyChanged(this,new PropertyChangedEventArgs(name));
            }
        }
    }
}