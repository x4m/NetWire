using System;
using System.Windows.Navigation;

namespace Bubbling
{
    class CustomUriMapper : UriMapperBase
    {

        public override Uri MapUri(Uri uri)
        {
            string tempUri = uri.ToString();
            string mappedUri;

            // Launch from the photo edit picker.
            // This is only for Windows Phone 8 apps.
            // Incoming URI example: /MainPage.xaml?Action=EditPhotoContent&FileId=%7Bea74a960-3829-4007-8859-cd065654fbbc%7D
            if ((tempUri.Contains("EditPhotoContent")) && (tempUri.Contains("FileId")))
            {
                // Redirect to PhotoEdit.xaml.
                mappedUri = tempUri.Replace("MainPage", "PhotoEdit");
                return new Uri(mappedUri, UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}