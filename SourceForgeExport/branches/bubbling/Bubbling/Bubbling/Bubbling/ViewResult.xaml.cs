using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bubbling.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace Bubbling
{
    public partial class ViewResult : PhoneApplicationPage
    {
        public ViewResult()
        {
            InitializeComponent();
            CreateMenu();
        }
        private void CreateMenu()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var _okButton = new ApplicationBarIconButton(new Uri("/Assets/share.png", UriKind.Relative));
            _okButton.Text = AppResources.ViewResult_CreateMenu_Share;
            ApplicationBar.Buttons.Add(_okButton);
            _okButton.Click += OnOkButtonOnClick;

            var saveButton = new ApplicationBarIconButton(new Uri("/Assets/save.png", UriKind.Relative));
            saveButton.Text = AppResources.ViewResult_CreateMenu_Save;
            ApplicationBar.Buttons.Add(saveButton);
            saveButton.Click += savebuttonClick;
        }

        private void savebuttonClick(object sender, EventArgs e)
        {
            var width = (int)WritableBitmap.PixelWidth;
            var height = (int)WritableBitmap.PixelHeight;
            using (var ms = new MemoryStream(width * height * 4))
            {
                WritableBitmap.SaveJpeg(ms, width, height, 0, 100);
                ms.Seek(0, SeekOrigin.Begin);
                var lib = new MediaLibrary();
                string format = string.Format("BUB_{0:yyyyMMddHHmmss}", DateTime.Now);
                var picture = lib.SavePictureToCameraRoll(format, ms);

                MessageBox.Show(string.Format(AppResources.ViewResult_savebuttonClick_Photo__0__saved_to_camera_roll, format));

            }
        }


        private void OnOkButtonOnClick(object sender, EventArgs args)
        {
            var width = (int)WritableBitmap.PixelWidth;
            var height = (int)WritableBitmap.PixelHeight;
            using (var ms = new MemoryStream(width * height * 4))
            {
                WritableBitmap.SaveJpeg(ms, width, height, 0, 100);
                ms.Seek(0, SeekOrigin.Begin);
                var lib = new MediaLibrary();
                var picture = lib.SavePicture(string.Format("bubbled.jpg"), ms);

                var task = new ShareMediaTask();

                task.FilePath = picture.GetPath();

                task.Show();
            }
        }

        
        public static Stream PassedArgument { get; set; }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            image1.Source = WritableBitmap;
        }
        public static WriteableBitmap WritableBitmap;
        private void Image1_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }

        private double totalScale = 1;

        private void Image1_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
                if (e.PinchManipulation != null)
                {
                    double scale = e.PinchManipulation.DeltaScale;
                    scaleTransform.CenterX = e.PinchManipulation.Original.Center.X;
                    scaleTransform.CenterY = e.PinchManipulation.Original.Center.Y;
                    var proposedScale = scale * totalScale;
                    if (scale != 0 && scale != 1 && proposedScale > 0.8 && proposedScale < 5)
                    {
                        scaleTransform.ScaleX *= scale;
                        scaleTransform.ScaleY *= scale;
                        totalScale *= scale;
                    }
                }
                else
                {
                    translateTransform.X += e.DeltaManipulation.Translation.X * totalScale;
                    translateTransform.Y += e.DeltaManipulation.Translation.Y * totalScale;
                }
        }

        private void Image1_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            TransferTransforms();
        }
        
        void TransferTransforms()
        {
            previousTransform.Matrix = Multiply(previousTransform.Matrix,
              currentTransform.Value);

            // Set current transforms to default values
            scaleTransform.ScaleX = scaleTransform.ScaleY = 1;
            scaleTransform.CenterX = scaleTransform.CenterY = 0;

            rotateTransform.Angle = 0;
            rotateTransform.CenterX = rotateTransform.CenterY = 0;

            translateTransform.X = translateTransform.Y = 0;
        }

        private void ResetTransform()
        {
            previousTransform.Matrix = Matrix.Identity;
        }

        Matrix Multiply(Matrix A, Matrix B)
        {
            return new Matrix(A.M11 * B.M11 + A.M12 * B.M21,
                              A.M11 * B.M12 + A.M12 * B.M22,
                              A.M21 * B.M11 + A.M22 * B.M21,
                              A.M21 * B.M12 + A.M22 * B.M22,
                              A.OffsetX * B.M11 + A.OffsetY * B.M21 + B.OffsetX,
                              A.OffsetX * B.M12 + A.OffsetY * B.M22 + B.OffsetY);
        }
    }
}