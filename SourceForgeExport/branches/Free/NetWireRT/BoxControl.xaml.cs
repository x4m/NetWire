using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using NetWireRT;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;

namespace NetWireUltimate
{
    public partial class BoxControl : UserControl
    {
        public BoxControl()
        {
            InitializeComponent();
        }

        private bool inProgress = false;
        private static bool _useAnimation = true;

        public static bool UseAnimation
        {
            get { return _useAnimation; }
            set { _useAnimation = value; }
        }

        private void BoxControl_OnManipulationStarted(object sender, object e)
        {
            if (UseAnimation || MainPage.FirstLaunch)
            {
                StartAnimation();
            }
            else
            {
                QuickChange();
            }
        }

        private void QuickChange()
        {
            var boxVm = DataContext as BoxVM;

            _storyboad.Begin();
            if (boxVm != null)
                boxVm.Tap();
        }

        private void StartAnimation()
        {
            if (inProgress)
                return;
            inProgress = true;
            _storyboad.Begin();
            _turnStoryboad.Begin();
            _angle.CenterX = ActualHeight/2;
            _angle.CenterY = ActualHeight/2;
            _turnStoryboad.Completed += TurnStoryboadOnCompleted;
        }

        private void TurnStoryboadOnCompleted(object sender, object eventArgs)
        {
            _turnStoryboad.Completed-=TurnStoryboadOnCompleted;
            _angle.Angle = 0;
            var boxVm = DataContext as BoxVM;
            if (boxVm != null)
                boxVm.Tap();
            inProgress = false;
        }

        private void UserControl_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

        }
    }
}
