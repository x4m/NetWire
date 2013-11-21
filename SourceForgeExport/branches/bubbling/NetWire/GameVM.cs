using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Marketplace;

namespace NetWireUltimate
{
    public class GameVM
    {
        static GameVM()
        {
            Instance = new GameVM(2, 3);
        }
        public static GameVM Instance
        {
            get;
            private set;
        }
        BoxVM[,] _boxes;
        readonly int _w = 2;
        readonly int _h = 3;

        int _startX;
        int _startY;

        public int W
        {
            get { return _w; }
        }

        public int H
        {
            get { return _h; }
        }

        public BoxVM[,] Boxes
        {
            get { return _boxes; }
        }

        public int TotalRotations
        {
            get { return _totalRotations; }
            set { _totalRotations = value; }
        }

        static readonly Random Rnd = new Random();

        public GameVM(int w, int h)
        {
            Hint.Reset();
            _w = w;
            _h = h;
            _steps = new Action<int, int>[] { StepL, StepR, StepB, StepT };
            StartNewGame();
        }

        public static bool SettedMirror { get; set; }
        bool ActualMirror { get; set; }

        private GameVM(BinaryReader data)
        {
            _w = data.ReadInt32();
            _h = data.ReadInt32();
            _steps = new Action<int, int>[] { StepL, StepR, StepB, StepT };

            _boxes = new BoxVM[_w, _h];

            for (int i = 0; i < _w; i++)
                for (int o = 0; o < _h; o++)
                {
                    var boxVM = new BoxVM(this, i, o);
                    boxVM.Deserealize(data.ReadByte());
                    _boxes[i, o] = boxVM;
                }
            _startX = data.ReadInt32();
            _startY = data.ReadInt32();

            _boxes[_startX, _startY].StartVisibility = Visibility.Visible;

            _totalRotations = data.ReadInt32();

            for (int i = 0; i < _w; i++)
                for (int o = 0; o < _h; o++)
                {
                    if (data.BaseStream.Position < data.BaseStream.Length)
                        _boxes[i, o].Number = data.ReadInt16();
                }

            BoxControl.UseAnimation = data.ReadBoolean();
            SettedMirror = data.ReadBoolean();
            ActualMirror = data.ReadBoolean();

            RecalcAvailability(null);
        }

        public void StartNewGame()
        {
            _cnt = 0;
            ActualMirror = SettedMirror;
            InitBoxes();
            Randomize();
            RecalcAvailability(null);
            if (MainPage.Instance != null)
                MainPage.Instance.BindGameData(this);
        }

        int _totalRotations;
        private void Randomize()
        {
            _totalRotations = 0;
            for (int i = 0; i < _w; i++)
                for (int o = 0; o < _h; o++)
                {
                    var b = _boxes[i, o];
                    int maxReasonableTurns = 4;
                    if ((b.L == Visibility.Visible && b.R == Visibility.Visible && b.B != Visibility.Visible && b.T != Visibility.Visible) ||
                        (b.L != Visibility.Visible && b.R != Visibility.Visible && b.B == Visibility.Visible && b.T == Visibility.Visible))
                        maxReasonableTurns = 2;
                    int turns = Rnd.Next(maxReasonableTurns);
                    for (int x = 0; x < turns; x++)
                    {
                        b.Rotate();
                    }

                    if (turns != 0)
                    {
                        _totalRotations += turns - maxReasonableTurns;
                    }
#if DEBUG
                        if (_totalRotations <= -2)
                            return;
#endif
                }
        }

        private void InitBoxes()
        {
            _boxes = new BoxVM[_w, _h];
            for (int i = 0; i < _w; i++)
                for (int o = 0; o < _h; o++)
                {
                    _boxes[i, o] = new BoxVM(this, i, o)
                    {
                        B = Visibility.Collapsed,
                        L = Visibility.Collapsed,
                        R = Visibility.Collapsed,
                        T = Visibility.Collapsed
                    };
                }
            _startX = Rnd.Next(_w);

            _startY = Rnd.Next(_h);
            StartPopulation(_startX, _startY);
        }

        private void StartPopulation(int x, int y)
        {
            var b = _boxes[x, y];
            b.StartVisibility = Visibility.Visible;
            b.Visited = true;
            RandomStepAside(x, y);
        }

        private void StepL(int x, int y)
        {
            int xm = x;
            if (x == 0)
            {
                if (ActualMirror)
                {
                    x = _w;
                }
                else
                {
                    return;
                }
            }
            x--;
            var b = _boxes[x, y];
            if (b.Visited)
                return;
            b.Visited = true;

            b.R = Visibility.Visible;
            
            _boxes[xm, y].L = Visibility.Visible;

            RandomStepAside(x, y);
        }

        private void StepR(int x, int y)
        {
            int xm = x;
            if (x == _w - 1)
            {
                if (ActualMirror)
                    x = -1;
                else
                    return;
            }
            x++;
            var b = _boxes[x, y];
            if (b.Visited)
                return;
            b.Visited = true;

            b.L = Visibility.Visible;
            
            _boxes[xm, y].R = Visibility.Visible;

            RandomStepAside(x, y);
        }

        private void StepT(int x, int y)
        {
            int ym = y;
            if (y == 0)
            {
                if (ActualMirror)
                    y = _h;
                else
                    return;
            }
            y--;
            var b = _boxes[x, y];
            if (b.Visited)
                return;
            b.Visited = true;

            b.B = Visibility.Visible;
            
            _boxes[x, ym].T = Visibility.Visible;

            RandomStepAside(x, y);
        }

        private void StepB(int x, int y)
        {
            int ym1 = y;
            if (y == _h - 1)
            {
                if (ActualMirror)
                    y = -1;
                else
                    return;
            }
            y++;
            var b = _boxes[x, y];
            if (b.Visited)
                return;
            b.Visited = true;

            b.T = Visibility.Visible;
            
            _boxes[x, ym1].B = Visibility.Visible;

            RandomStepAside(x, y);
        }

        private readonly Action<int, int>[] _steps;

        private int _cnt;

        private void RandomStepAside(int x, int y)
        {
            _boxes[x, y].Number = (short)_cnt++;
            var bools = new bool[4];
            while (bools.Any(b => !b))
            {
                var n = Rnd.Next(4);
                if (bools[n])
                    continue;
                bools[n] = true;
                _steps[n](x, y);
            }
        }

        public void RecalcAvailability(BoxVM box)
        {
            var visible = new Dictionary<Tuple, bool>();

            Action<int, int> visibilityCalculator = null;
            visibilityCalculator = (x, y) =>
            {
                if (x < 0 || x >= _w || y < 0 || y >= _h)
                    return;
                var t = Tuple.Create(x, y);

                if (!visible.ContainsKey(t))
                {
                    visible.Add(t, true);
                    var b = _boxes[x, y];
                    int xm1 = x - 1;
                    if (xm1 == -1)
                        xm1 = _w-1;
                    int xp1 = x + 1;
                    if (xp1 == _w)
                        xp1 = 0;
                    int ym1 = y - 1;
                    if (ym1 == -1)
                        ym1 = _h-1;
                    int yp1 = y + 1;
                    if (yp1 == _h)
                        yp1 = 0;

                    if (b.L == Visibility.Visible && (x != 0 || ActualMirror) && _boxes[xm1, y].R == Visibility.Visible)
                        visibilityCalculator(xm1, y);

                    if (b.R == Visibility.Visible && (x != _w - 1 || ActualMirror) && _boxes[xp1, y].L == Visibility.Visible)
                        visibilityCalculator(xp1, y);

                    if (b.T == Visibility.Visible && (y != 0 || ActualMirror) && _boxes[x, ym1].B == Visibility.Visible)
                        visibilityCalculator(x, ym1);
                    
                    if (b.B == Visibility.Visible && (y != _h - 1|| ActualMirror) && _boxes[x, yp1].T == Visibility.Visible)
                        visibilityCalculator(x, yp1);
                }
            };

            visibilityCalculator(_startX, _startY);

            for (int i = 0; i < _w; i++)
                for (int o = 0; o < _h; o++)
                {
                    var b = _boxes[i, o];
                    b.SetPowered(visible.ContainsKey(Tuple.Create(i, o)));
                }

            if (visible.Count == _w * _h && MainPage.Instance != null)
            {
                MainPage.Instance.Dispatcher.BeginInvoke(() =>
                {
                    string messageBoxText = AppResources.YouWin + " " + StepsText() + ".";
                    MessageBox.Show(messageBoxText, AppResources.Congratulations, MessageBoxButton.OK);
                    if (W >= 8)
                        StartNewGame();
                    else
                        ChangeDimensions(W + 1);
                });
            }
        }

        private string StepsText()
        {
            int totalRotations = _totalRotations / 4;
            if (totalRotations <= 0)
                return AppResources.Perfect;
            return string.Format(AppResources.FarFromPerfect, totalRotations);
        }

        public static void ChangeDimensions(int width)
        {
            if(width>4)
            {
                if (new LicenseInformation().IsTrial())
                {
                    width = 4;
                    MessageBox.Show(AppResources.DemoConstraint);
                }
            }
            int height = (int)Math.Round(width * 8.0 / 5);
            ChangeDimensions(width, height);
        }

        public void Serialize(BinaryWriter br)
        {
            unchecked
            {
                br.Write(_w);
                br.Write(_h);

                for (int i = 0; i < _w; i++)
                    for (int o = 0; o < _h; o++)
                    {
                        br.Write(_boxes[i, o].Serialize());
                    }
                br.Write(_startX);
                br.Write(_startY);


                br.Write(_totalRotations);

                for (int i = 0; i < _w; i++)
                    for (int o = 0; o < _h; o++)
                    {
                        br.Write(_boxes[i, o].Number);
                    }

                br.Write(BoxControl.UseAnimation);
                br.Write(SettedMirror);
                br.Write(ActualMirror);
            }
        }

        public static void LoadFromData(BinaryReader data)
        {
            Instance = new GameVM(data);

            if (MainPage.Instance != null)
                MainPage.Instance.BindGameData(Instance);
        }

        private static void ChangeDimensions(int width, int height)
        {
            Instance = new GameVM(width, height);

            if (MainPage.Instance != null)
                MainPage.Instance.BindGameData(Instance);
        }
    }
}
