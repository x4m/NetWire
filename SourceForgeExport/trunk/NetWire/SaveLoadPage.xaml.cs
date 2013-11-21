using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NetWireUltimate
{
    public partial class SaveLoadPage : PhoneApplicationPage
    {
        SavedGamesVM _vm;

        public SaveLoadPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _nameBox.Text = DateTime.Now.ToString();

            _vm = new SavedGamesVM();
            DataContext = _vm;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_vm != null)
                _vm.Dispose();
            _vm = null;
            base.OnNavigatedFrom(e);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameBox.Text))
                return;
            _saveButton.IsEnabled = false;
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    GameVM.Instance.Serialize(binaryWriter);

                    _vm.Save(_nameBox.Text, memoryStream.ToArray());
                }
            }
        }

        private void Play_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                var sg = button.DataContext as SavedGame;

                if (sg != null)
                {
                    using (var binaryReader = new BinaryReader(new MemoryStream(sg.Data)))
                    {
                        GameVM.LoadFromData(binaryReader);

                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    }
                }
            }
        }

        private void Trash_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                var sg = button.DataContext as SavedGame;

                if (sg != null)
                {
                    var res = MessageBox.Show(AppResources.AreYouSureDrop, "NET WIRE", MessageBoxButton.OKCancel);
                    if (res == MessageBoxResult.OK)
                        _vm.Drop(sg);
                }
            }
        }
    }
}