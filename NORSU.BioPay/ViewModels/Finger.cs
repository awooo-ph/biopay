using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NORSU.BioPay.ViewModels
{
    class FingerSample : ViewModelBase
    {
        private byte[] _Picture;

        public byte[] Picture
        {
            get => _Picture;
            set
            {
                if(value == _Picture)
                    return;
                _Picture = value;
                OnPropertyChanged(nameof(Picture));
            }
        }

        private byte[] _Template;

        public byte[] Template
        {
            get => _Template;
            set
            {
                if(value == _Template)
                    return;
                _Template = value;
                OnPropertyChanged(nameof(Template));
            }
        }

        private bool _IsWaiting = true;

        public bool IsWaiting
        {
            get => _IsWaiting;
            set
            {
                if(value == _IsWaiting)
                    return;
                _IsWaiting = value;
                OnPropertyChanged(nameof(IsWaiting));
            }
        }

        private bool _IsValid;

        public bool IsValid
        {
            get => _IsValid;
            set
            {
                if(value == _IsValid)
                    return;
                _IsValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }
}
