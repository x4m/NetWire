using System.ComponentModel;
using System.Windows;
using NetWireUltimate.Annotations;
using Windows.UI.Xaml;

namespace NetWireUltimate
{
    public class Hint : INotifyPropertyChanged
    {
        private Visibility _visibility;
        public static Hint Instance { get; set; }
        public Hint()
        {
            Instance = this;
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static void Reset()
        {
            if (Instance != null) Instance.IsVisible = false;
        }

        public bool IsVisible
        {
            get { return _visibility == Visibility.Visible; }
            set {
                _visibility = value ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged("Visibility");
            }
        }
    }
}