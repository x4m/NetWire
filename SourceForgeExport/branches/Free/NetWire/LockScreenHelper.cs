using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Phone.System.UserProfile;

namespace NetWireUltimate
{
    public class LockScreenHelper
    {
        public static void Update(Grid contentPanel)
        {
            LockHelper(contentPanel);
        }

        private static void LockHelper(Grid contentPanel)
        {
            bool isAppResource = false;
            try
            {
                var isProvider = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;
                if (!isProvider)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var ra = Windows.Phone.System.UserProfile.LockScreenManager.RequestAccessAsync();
                    Task<LockScreenRequestResult> t = ra.AsTask();
                    t.Wait();
                    var op = t.Result;

                    // Only do further work if the access was granted.
                    isProvider = op == Windows.Phone.System.UserProfile.LockScreenRequestResult.Granted;
                }

                if (isProvider)
                {

                    string filePathOfTheImage = CreateImage(contentPanel);
                    if (filePathOfTheImage == null)
                        return;
                    // At this stage, the app is the active lock screen background provider.

                    // The following code example shows the new URI schema.
                    // ms-appdata points to the root of the local app data folder.
                    // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
                    var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                    var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);

                    // Set the lock screen background image.
                    Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);

                    // Get the URI of the lock screen background image.
                    var currentImage = Windows.Phone.System.UserProfile.LockScreen.GetImageUri();
                    System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private static string CreateImage(Grid ContentPanel)
        {
            int w = (int) Application.Current.Host.Content.ActualWidth;
            int h = (int) Application.Current.Host.Content.ActualHeight;
            var bitmap = new WriteableBitmap(w, h);

            var uie = ContentPanel.Children.FirstOrDefault();
            if (uie == null)
                return null;

            double ratio = w/ContentPanel.ActualWidth;
            bitmap.Render(ContentPanel,new TransformGroup()
                                           {
                                               Children =
                                                   {
                                                       new ScaleTransform()
                                                           {
                                                               ScaleX = ratio,
                                                               ScaleY = ratio
                                                           },
                                                           new TranslateTransform()
                                                               {
                                                                   Y = (h-ContentPanel.ActualHeight*ratio)/2
                                                               }
                                                   }
                                           });

            bitmap.Invalidate();

            
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            string template = "LiveLockBackground_{0}.jpg";
            string fileName = template;
            int cnt = 0;
            while (myStore.FileExists(string.Format(template, cnt)))
            {
                cnt++;
            }
            /*var currentImage = LockScreen.GetImageUri();*/

            /*if (currentImage.ToString().EndsWith("_A.jpg"))
            {
                fileName = "LiveLockBackground_B.jpg";
            }
            else
            {
                fileName = "LiveLockBackground_A.jpg";
            }*/

            fileName = string.Format(fileName, cnt);


            if (myStore.FileExists(fileName))
            {
                myStore.DeleteFile(fileName);
            }

            var myFileStream = myStore.CreateFile(fileName);
            bitmap.SaveJpeg(myFileStream, w, h, 0, 85);
            myFileStream.Close();
            for (int i = 0; i < cnt; i++)
            {
                try
                {
                    myStore.DeleteFile(string.Format(template, i));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return fileName;
        }
    }
}