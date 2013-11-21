using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace NetWireUltimate
{
    public partial class InfoPage : PhoneApplicationPage
    {
        public InfoPage()
        {
            InitializeComponent();

            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton infoButton = new ApplicationBarIconButton(new Uri("/check.png", UriKind.Relative));
            infoButton.Text = AppResources.OK;
            ApplicationBar.Buttons.Add(infoButton);
            infoButton.Click += (sender, args) =>
            {
                JournalEntry prev = NavigationService.BackStack.FirstOrDefault();
                if (prev != null && prev.Source.ToString().Contains("/MainPage.xaml"))
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            };
            if(!Debugger.IsAttached)
            {
                
            }
        }

        private void InfoPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            versionBox.Text = AppResources.Version + " " + GetVersionNumber();
        }

        private static string GetVersionNumber()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var parts = asm.FullName.Split(',');
                return parts[1].Split('=')[1];
            }
            catch
            {
                return AppResources.UnknownForVersion;
            }
        }

        private void Rate_OnClick(object sender, RoutedEventArgs e)
        {
            var review = new MarketplaceReviewTask();
            review.Show();
        }
    }
}