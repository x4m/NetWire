using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using Bubbling7;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Bubbling
{
    public partial class InfoPage : PhoneApplicationPage
    {
        public InfoPage()
        {
            InitializeComponent();

            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton infoButton = new ApplicationBarIconButton(new Uri("/Assets/check.png", UriKind.Relative));
            infoButton.Text = "OK";
            ApplicationBar.Buttons.Add(infoButton);
            infoButton.Click += (sender, args) =>
            {
                uint bubbles;
                if(uint.TryParse(numberBox.Text,out bubbles))
                {
                    BubblingSettings.Instance.Bubbles = (int) bubbles;
                }
                var colorItem = listBox.SelectedItem as ColorItem;
                if (colorItem != null)
                    BubblingSettings.Instance.Color = colorItem.Color;
                BubblingSettings.Instance.Save();
                JournalEntry prev = NavigationService.BackStack.FirstOrDefault();
                if (prev != null && prev.Source.ToString().Contains("/MainPage.xaml"))
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            };

            var colors = new List<ColorItem>()
                             {
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_current, Color = BubblingSettings.Instance.Color},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_transparent, Color = Colors.Transparent},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_orange, Color = Colors.Orange},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_red, Color = Colors.Red},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_blue, Color = Colors.Blue},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_magenta, Color = Colors.Magenta},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_purple, Color = Colors.Purple},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_green, Color = Colors.Green},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_cyan, Color = Colors.Cyan},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_brown, Color = Colors.Brown},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_yellow, Color = Colors.Yellow},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_black, Color = Colors.Black},
                                 new ColorItem() {Text = AppResources.InfoPage_InfoPage_white, Color = Colors.White},
                             };
            this.listBox.ItemsSource = colors;
            listBox.SelectedItem = colors[0];
            numberBox.Text = BubblingSettings.Instance.Bubbles.ToString();
        }
        private void InfoPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            versionBox.Text = AppResources.InfoPage_InfoPage_OnLoaded_Version_ + GetVersionNumber();
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
                return AppResources.InfoPage_GetVersionNumber_unknown;
            }
        }

        private void Rate_OnClick(object sender, RoutedEventArgs e)
        {
            var review = new MarketplaceReviewTask();
            review.Show();
        }
    }

    public class ColorItem
    {
        public string Text { get; set; }
        public Color Color { get; set; }
    }

    public class ColorToBrushConverter : IValueConverter
    {

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return new SolidColorBrush((Color)(value));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}