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
    public partial class SettingsPage : PhoneApplicationPage
    {
        readonly ApplicationBarIconButton _okButton;
        public SettingsPage()
        {
            InitializeComponent();
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            _okButton = new ApplicationBarIconButton(new Uri("/check.png", UriKind.Relative));
            _okButton.Text = AppResources.OK;
            ApplicationBar.Buttons.Add(_okButton);
            _okButton.Click += (sender, args) =>
                                  {
                                      int number;
                                      int.TryParse(columnBox.Text, out number);
                                      
                                      if (number > 1 && number < 12)
                                      {
                                          if (number != GameVM.Instance.W || GameVM.SettedMirror != _mirror.IsChecked)
                                          {
                                              GameVM.SettedMirror = _mirror.IsChecked==true;
                                              GameVM.ChangeDimensions(number);
                                          }
                                      }
                                      BoxControl.UseAnimation = _useAnimation.IsChecked == true;
                                      Hint.Instance.IsVisible = _showHint.IsChecked == true;
                                      JournalEntry prev = NavigationService.BackStack.FirstOrDefault();
                                      if (prev != null && prev.Source.ToString().Contains("/MainPage.xaml"))
                                          NavigationService.GoBack();
                                      else
                                          NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                                  };

            var cancelButton = new ApplicationBarIconButton(new Uri("/cancel.png", UriKind.Relative));
            cancelButton.Text = AppResources.Cancel;
            ApplicationBar.Buttons.Add(cancelButton);
            cancelButton.Click += (sender, args) =>
                                      {
                                          JournalEntry prev = NavigationService.BackStack.FirstOrDefault();
                                          if (prev != null && prev.Source.ToString().Contains("/MainPage.xaml"))
                                              NavigationService.GoBack();
                                          else
                                              NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                                      };
        }

        private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (GameVM.Instance != null)
                columnBox.Text = GameVM.Instance.W.ToString();
            _useAnimation.IsChecked = BoxControl.UseAnimation;
            //_showHint.IsChecked = Hint.Instance.IsVisible;
            _mirror.IsChecked = GameVM.SettedMirror;
        }

        private void ColumnBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            int number;
            int.TryParse(columnBox.Text, out number);
            if (number > 1 && number < 11)
                _okButton.IsEnabled = true;
            else
                _okButton.IsEnabled = false;
        }

        private void ColumnBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.PlatformKeyCode == 190 || e.PlatformKeyCode == 188)
            {
                e.Handled = true;
            }
        }
    }
}