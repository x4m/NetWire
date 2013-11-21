using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetWireRT.Common;
using NetWireUltimate;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace NetWireRT
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : NetWireRT.Common.LayoutAwarePage
    {
        public MainPage()
        {
            Instance = this;
            this.InitializeComponent();
            BindGrid();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }



        /*// Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var infoButton = new ApplicationBarIconButton(new Uri("/i.png", UriKind.Relative));
            infoButton.Text = AppResources.Info;
            ApplicationBar.Buttons.Add(infoButton);
            infoButton.Click += InfoButtonOnClick;

            var refreshButton = new ApplicationBarIconButton(new Uri("/refresh.png", UriKind.Relative));
            refreshButton.Text = AppResources.Refresh;
            refreshButton.Click += (sender, args) => GameVM.Instance.StartNewGame();
            ApplicationBar.Buttons.Add(refreshButton);

            var settingsButton = new ApplicationBarIconButton(new Uri("/settings.png", UriKind.Relative));
            settingsButton.Text = AppResources.Settings;
            settingsButton.Click += SettingsButtonOnClick;
            ApplicationBar.Buttons.Add(settingsButton);

            var saveButton = new ApplicationBarIconButton(new Uri("/save.png", UriKind.Relative));
            saveButton.Text = AppResources.Save;
            saveButton.Click += SaveButtonOnClick;
            //ApplicationBar.Buttons.Add(saveButton);
            // Create a new menu item with the localized string from AppResources.
            /*
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem("Info");
            ApplicationBar.MenuItems.Add(appBarMenuItem);#1#

        }*/
        /*
        private void SaveButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/SaveLoadPage.xaml", UriKind.Relative));
        }

        private void SettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }*/

        internal void UpdateTile(BoxVM center)
        {/*
                var bitmap = new WriteableBitmap(150, 150);

                var uie = ContentPanel.Children.FirstOrDefault();
                if (uie == null)
                    return;

                /*var x = center.X;
                if (x == center.Game.W - 1)
                    x--;
                var y = center.Y;
                if (y == center.Game.H - 1)
                    y--;#1#


            bitmap.
                bitmap.Render(ContentPanel, new TranslateTransform
                {/*
                    X = -uie.RenderSize.Width * x,
                    Y = -uie.RenderSize.Height * y,#1#
                });

                bitmap.Invalidate();


                String fileName = String.Format("/Shared/ShellContent/Tile.jpg");

                var myStore = IsolatedStorageFile.GetUserStoreForApplication();

                if (myStore.FileExists(fileName))
                {
                    myStore.DeleteFile(fileName);
                }

                var myFileStream = myStore.CreateFile(fileName);

                bitmap.SaveJpeg(myFileStream, 180, 180, 0, 85);
                myFileStream.Close();

            */
        }
/*
        private void InfoButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
        }*/

        public void BindGrid()
        {
            BindGameData(GameVM.Instance);
            mirrorButton.IsChecked = GameVM.Instance.ActualMirror;
        }

        internal void BindGameData(GameVM game)
        {
            ContentPanel.Children.Clear();
            ContentPanel.ColumnDefinitions.Clear();
            ContentPanel.RowDefinitions.Clear();
            if (ContentPanel.ActualHeight < ContentPanel.ActualWidth)
            {
                BoxVM.RotateShift = false;
                for (int x = 0; x < game.W; x++)
                {
                    ContentPanel.ColumnDefinitions.Add(new ColumnDefinition
                                                           {
                                                               Width = new GridLength(1, GridUnitType.Star),
                                                           });
                }

                for (int y = 0; y < game.H; y++)
                {
                    ContentPanel.RowDefinitions.Add(new RowDefinition
                                                        {
                                                            Height = new GridLength(1, GridUnitType.Star),
                                                        });
                }

                for (int x = 0; x < game.W; x++)
                {
                    for (int y = 0; y < game.H; y++)
                    {
                        var ctrl = new BoxControl {DataContext = game.Boxes[x, y]};

                        ctrl.SetValue(Grid.RowProperty, y);
                        ctrl.SetValue(Grid.ColumnProperty, x);
                        ContentPanel.Children.Add(ctrl);
                    }
                }
            }
            else
            {
                BoxVM.RotateShift = true;
                for (int x = 0; x < game.H; x++)
                {
                    ContentPanel.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    });
                }

                for (int y = 0; y < game.W; y++)
                {
                    ContentPanel.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star),
                    });
                }

                for (int x = 0; x < game.H; x++)
                {
                    for (int y = 0; y < game.W; y++)
                    {
                        var ctrl = new BoxControl { DataContext = game.Boxes[y, x] };

                        ctrl.SetValue(Grid.RowProperty, y);
                        ctrl.SetValue(Grid.ColumnProperty, x);
                        ContentPanel.Children.Add(ctrl);
                    }
                }
                
            }
        }

        private bool _welcomeSown;


        public static MainPage Instance { get; private set; }

        public static bool FirstLaunch { get; set; }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (FirstLaunch && !_welcomeSown)
            {
                _welcomeSown = true;
                //InfoButtonOnClick(sender, e);
            }
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BindGrid();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            GameVM.Instance.StartNewGame();
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            var frame = Frame;
            if (frame != null) frame.Navigate(typeof(InfoPage));
        }

        private void mirrorButton_Checked(object sender, RoutedEventArgs e)
        {
            GameVM.SettedMirror = mirrorButton.IsChecked == true;
        }
    }
}
