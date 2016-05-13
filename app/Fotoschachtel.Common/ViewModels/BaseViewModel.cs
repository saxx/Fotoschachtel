using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fotoschachtel.Common.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        public const string TitlePropertyName = "Title";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _subtitle = string.Empty;
        public const string SubtitlePropertyName = "Subtitle";
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }

        private string _icon;
        public const string IconPropertyName = "Icon";
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }


        protected bool SetProperty<T>(
            ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;

            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
