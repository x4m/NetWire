using System;
using System.ComponentModel;
using System.Windows;
using NetWireRT;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace NetWireUltimate
{
    public class BoxVM : INotifyPropertyChanged
    {
        private readonly GameVM _game;
        private readonly int _x;
        private readonly int _y;

        public GameVM Game
        {
            get { return _game; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private Visibility _l, _t, _r, _b;

        static readonly Brush ShadowBrush =
            //new SolidColorBrush(Colors.DarkBlue);
            new SolidColorBrush(Colors.Blue);
        static readonly Brush PoweredBrush =
            //new SolidColorBrush(Colors.YellowGreen);
            new SolidColorBrush(Colors.Red);
        private Brush _color = ShadowBrush;
        private Visibility _startVisibility = Visibility.Collapsed;

        public BoxVM(GameVM game, int x, int y)
        {
            _game = game;
            _x = x;
            _y = y;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public Visibility Lv
        {
            get
            {
                if (RotateShift)
                {
                    return T;
                }
                return L;
            }
        }

        public Visibility Rv
        {
            get
            {
                if (RotateShift)
                {
                    return B;
                }
                return R;
            }
        }

        public Visibility Tv
        {
            get
            {
                if (RotateShift)
                {
                    return L;
                }
                return T;
            }
        }

        public Visibility Bv
        {
            get
            {
                if (RotateShift)
                {
                    return R;
                }
                return B;
            }
        }

        public Visibility L
        {
            get { return _l; }
            set
            {
                _l = value;
                OnPropertyChanged("Lv");
                OnPropertyChanged("Tv");
            }
        }

        public Visibility T
        {
            get { return _t; }
            set
            {
                _t = value;
                OnPropertyChanged("Lv");
                OnPropertyChanged("Tv");
            }
        }

        public Visibility R
        {
            get { return _r; }
            set
            {
                _r = value;
                OnPropertyChanged("Rv");
                OnPropertyChanged("Bv");
            }
        }

        public Visibility B
        {
            get { return _b; }
            set
            {
                _b = value;
                OnPropertyChanged("Rv");
                OnPropertyChanged("Bv");
            }
        }

        public Brush Color
        {
            get { return _color; }
            set
            {
                var changed = _color != value;
                _color = value;
                if (changed)
                    OnPropertyChanged("Color");
            }
        }

        public Visibility StartVisibility
        {
            get { return _startVisibility; }
            set { _startVisibility = value; }
        }

        public bool Visited { get; set; }

        public short Number { get; set; }

        public static bool RotateShift { get; set; }

        public void Tap()
        {
            Rotate();
            _game.TotalRotations++;
            _game.RecalcAvailability(this);
            if (_rnd.Next(20) == 15)
                if (MainPage.Instance != null)
                    MainPage.Instance.UpdateTile(this);
        }

        readonly Random _rnd = new Random();

        public void Rotate()
        {
            if (!RotateShift)
            {
                var l = L;
                L = B;
                B = R;
                R = T;
                T = l;
            }
            else
            {
                var l = L;
                L = T;
                T = R;
                R = B;
                B = l;
            }
        }

        public void SetPowered(bool power)
        {
            Color = power ? PoweredBrush : ShadowBrush;
        }

        public byte Serialize()
        {
            byte b = new byte();
            if (L == Visibility.Visible)
                b += 1;
            if (R == Visibility.Visible)
                b += 2;
            if (T == Visibility.Visible)
                b += 4;
            if (B == Visibility.Visible)
                b += 8;
            if (Color == PoweredBrush)
                b += 16;
            if (StartVisibility == Visibility.Visible)
                b += 32;
            return b;
        }

        public void Deserealize(byte b)
        {
            bool c = (b & 1) != 0;
            L = c ? Visibility.Visible : Visibility.Collapsed;
            c = (b & 2) != 0;
            R = c ? Visibility.Visible : Visibility.Collapsed;
            c = (b & 4) != 0;
            T = c ? Visibility.Visible : Visibility.Collapsed;
            c = (b & 8) != 0;
            B = c ? Visibility.Visible : Visibility.Collapsed;
            c = (b & 16) != 0;
            Color = c ? PoweredBrush : ShadowBrush;
            c = (b & 32) != 0;
            StartVisibility = c ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}