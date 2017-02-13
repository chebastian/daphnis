using MVVMHelpers;
using MVVMHeplers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BorderlessAlphaWin
{
    internal class DraggableViewModel : ViewModelBase
    {
        public DraggableViewModel(ICloseHandler closer)
        {
            _closeHandler = closer;
            Opacity = 0.2f;
            TargetOpacity = 0.2f;
            Tags = new ObservableCollection<Tag>();
            IsUnsaved = true;
        } 

        public bool MouseHover
        {
            get
            {
                return false;
            }
        }
 
        public ICommand CloseCommand
        {
            get
            {
                return new MyCommandWrapper((x) => { _closeHandler.Close(); });
            }
        }

        public int UniqueIdentifier
        {
            get; set;
        }

        public Double ImgWidth
        {
            get; set;
        }

        public Double ImgHeight
        {
            set; get;
        }

        public String ImageSource
        {
            get; set;
        }

        public String Image64
        {
            get; set;
        }


        public int PosLeft
        {
            get
            {
                return _posLeft;
            }
            set
            {
                _posLeft = value;
                SetPropertyChanged();
            }
        }

        public int PosTop
        {
            get
            {
                return _posTop;
            }
            set
            {
                _posTop = value;
                SetPropertyChanged();
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new MyCommandWrapper((x) => { this.addTagToThing(TagName); });
            }
        }

        private void addTagToThing(String name)
        {
            Tags.Add(new Tag() { tName = name });
            TagName = String.Empty;
            SetPropertyChanged("Tags");
        }

        public ObservableCollection<Tag> Tags
        {
            get
            {
                return _tags;
            }

            set
            {
                _tags = value;
                SetPropertyChanged();
            }
        }

        private float _opacity;
        private bool _visible;
        private ICloseHandler _closeHandler;
        private string _tagName;
        private ObservableCollection<Tag> _tags;
        private string _img64;
        private int _posLeft;
        private int _posTop;

        public float TargetOpacity
        {
            get; set;
        }

        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = Math.Max(0.2f,value);
                _opacity = Math.Min(0.98f,_opacity);
                IsVisible = !(value < 1.0f);
                SetPropertyChanged();
            } 
        }

        public bool IsVisible
        {
            get
            {
                return _visible;
            }

            set
            {
                _visible = value;
                SetPropertyChanged();
            }
        }

        public String TagName
        {
            get
            {
                return _tagName;
            }

            set
            {
                _tagName = value;
                SetPropertyChanged();
            }
        }

        public bool IsUnsaved { get; set; }
        public BitmapImage BitmapImage { get; internal set; }

        public interface ICloseHandler 
        {
            void Close();
        }
    }
}