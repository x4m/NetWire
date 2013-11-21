using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NetWireUltimate
{
    public partial class BoxControl : UserControl
    {
        public BoxControl()
        {
            InitializeComponent();
        }

        private bool inProgress = false;

        public static bool UseAnimation { get; set; }

        private void BoxControl_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
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

        private void TurnStoryboadOnCompleted(object sender, EventArgs eventArgs)
        {
            _turnStoryboad.Completed-=TurnStoryboadOnCompleted;
            _angle.Angle = 0;
            var boxVm = DataContext as BoxVM;
            if (boxVm != null)
                boxVm.Tap();
            inProgress = false;
        }
    }
}
