using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BorderlessAlphaWin
{
    internal class Tag : INotifyPropertyChanged
    {
        private string _name;

        [JsonProperty("tName")]
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

        public bool MouseHover
        {
            get
            {
                return false;
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