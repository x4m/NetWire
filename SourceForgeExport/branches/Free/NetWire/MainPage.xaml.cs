using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NetWireUltimate
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor

        public static MainPage Instance { get; private set; }

        public static bool FirstLaunch { get; set; }

        public MainPage()
        {
            Instance = this;
            InitializeComponent();
            
            BindGrid();

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
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
            ApplicationBar.MenuItems.Add(appBarMenuItem);*/
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string lockscreenKey = "WallpaperSettings";
            string lockscreenValue = "0";

            bool lockscreenValueExists = NavigationContext.QueryString.TryGetValue(lockscreenKey, out lockscreenValue);

            if (lockscreenValueExists)
            {
                // Navigate the user to your app's lock screen settings screen here, 
                // or indicate that the lock screen background image is updating.
                MessageBox.Show(AppResources.BigPuzzleOffer);
            }
            else
            {
                
            }
        }

        private void SaveButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/SaveLoadPage.xaml", UriKind.Relative));
        }

        private void SettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        internal void UpdateTile(BoxVM center)
        {
            var firstTile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri.ToString() == "/");
            if (firstTile != null)
            {
                var bitmap = new WriteableBitmap(180, 180);

                var uie = ContentPanel.Children.FirstOrDefault();
                if (uie == null)
                    return;

                var x = center.X;
                if (x == center.Game.W - 1)
                    x--;
                var y = center.Y;
                if (y == center.Game.H-1)
                    y--;

                
                    bitmap.Render(ContentPanel, new TranslateTransform
                                                    {
                                                        X = -uie.RenderSize.Width*x,
                                                        Y = -uie.RenderSize.Height * y,
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

                Uri uri = new Uri("isostore:" + fileName, UriKind.Absolute);

                firstTile.Update(new StandardTileData
                                     {
                                         BackgroundImage = uri,
                                     });
            }
        }

        private void InfoButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
        }

        public void BindGrid()
        {
            BindGameData(GameVM.Instance);
        }

        internal void BindGameData(GameVM game)
        {
            //Dispatcher.BeginInvoke(() =>{
                                           ContentPanel.Children.Clear();
                                           ContentPanel.ColumnDefinitions.Clear();
                                           ContentPanel.RowDefinitions.Clear();

                                           for (int x = 0; x < game.W; x++)
                                           {
                                               ContentPanel.ColumnDefinitions.Add(new ColumnDefinition
                                                                                      {
                                                                                          Width =
                                                                                              new GridLength(1,
                                                                                                             GridUnitType
                                                                                                                 .Star),
                                                                                      });
                                           }

                                           for (int y = 0; y < game.H; y++)
                                           {
                                               ContentPanel.RowDefinitions.Add(new RowDefinition
                                                                                   {
                                                                                       Height =
                                                                                           new GridLength(1,
                                                                                                          GridUnitType.
                                                                                                              Star),
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
                                       //});
        }

        private bool _welcomeSown;

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (FirstLaunch && !_welcomeSown)
            {
                _welcomeSown = true;
                InfoButtonOnClick(sender, e);
            }
            else
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                var adshown = "/adshown";
                if(!store.FileExists(adshown))
                {
                    store.CreateFile(adshown).Dispose();
                    var popUp = new PopUp() {Width = ActualWidth, Height = ActualHeight/2};

                    var Mypopup = new Popup
                        {
                            Child = popUp
                        };
                    popUp.ok.Click += (o,o1) => { Mypopup.IsOpen = false; };
                    Mypopup.Height = ActualHeight/2;
                    Mypopup.Width = ActualWidth;
                    Mypopup.VerticalOffset = ActualHeight / 5;
                    Mypopup.IsOpen = true;
                }
            }
        }
    }
}