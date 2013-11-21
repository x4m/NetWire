using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Bubbling
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            if (task == null)
            {
                task = new PhotoChooserTask {ShowCamera = true};
            }
            task.Completed += TaskOnCompleted;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        readonly PhotoChooserTask task;
        private bool waitingForPhoto;
        private bool gotPhoto;
        
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            task.Show();
            waitingForPhoto = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(waitingForPhoto && gotPhoto)
                NavigationService.Navigate(new Uri("/PhotoEdit.xaml", UriKind.Relative));

            waitingForPhoto = false;
            gotPhoto = false;
        }

        private void TaskOnCompleted(object sender, PhotoResult photoResult)
        {
            if (photoResult.TaskResult==TaskResult.OK)
            {
                if (waitingForPhoto)
                    gotPhoto = true;
                PhotoEdit.PassedArgument = photoResult.ChosenPhoto;
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private void RateClick(object sender, RoutedEventArgs e)
        {
            var review = new MarketplaceReviewTask();
            review.Show();
        }

        private void InfoClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
        }
    }
}