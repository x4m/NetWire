using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public partial class PhotoEdit : PhoneApplicationPage
    {
        public PhotoEdit()
        {
            InitializeComponent();
            CreateMenu();
        }
        ApplicationBarIconButton _okButton;

        private void CreateMenu()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            _okButton = new ApplicationBarIconButton(new Uri("/Assets/play.png", UriKind.Relative));
            _okButton.Text = AppResources.PhotoEdit_CreateMenu_Bubble;
            ApplicationBar.Buttons.Add(_okButton);
            _okButton.Click += OnBubbleButtonOnClick;

            var search = new ApplicationBarIconButton(new Uri("/Assets/search.png", UriKind.Relative));
            
            var edit = new ApplicationBarIconButton(new Uri("/Assets/edit.png", UriKind.Relative));

            search.Text = AppResources.PhotoEdit_CreateMenu_Search;
            ApplicationBar.Buttons.Add(search);
            search.Click += (sender, args) =>
                                {
                                    if (!editMode)
                                    {
                                        ResetTransform();
                                        _totalScale = 1;
                                    }
                                    edit.IsEnabled = true;
                                    Status.Text = move;
                                    editMode = false;
                                };

            edit.Text = AppResources.PhotoEdit_CreateMenu_Edit_area;
            edit.IsEnabled = false;
            ApplicationBar.Buttons.Add(edit);
            edit.Click += (sender, args) =>
            {
                edit.IsEnabled = false;
                Status.Text = encircle;
                editMode = true;
            };
        }

        readonly Random _rnd = new Random();
        List<Circle> _circles;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Touch.FrameReported-=TouchOnFrameReported;
            base.OnNavigatedFrom(e);
        }

        private void OnBubbleButtonOnClick(object sender, EventArgs args)
        {
            if (!_canvas.Children.OfType<Polygon>().SelectMany(pl => pl.Points).Any())
                return;
            BubblingSettings.Instance.GetHashCode();
            _okButton.IsEnabled = false;

            bool addfinishbutton = false;
            if (_circles != null)
            {
                foreach (var circle in _circles)
                {
                    _canvas.Children.Remove(circle.GetEllipse(this));
                }
            }
            else
            {
                addfinishbutton = true;
            }
            
            _circles = new List<Circle>();

            var progressIndicator = new ProgressIndicator
            {
                IsVisible = true,
                IsIndeterminate = false,
                Text = AppResources.PhotoEdit_OnBubbleButtonOnClick_Bubbling____,
                Value = 0,
            };

            SystemTray.SetProgressIndicator(this, progressIndicator);

            int actualWidth = (int)_canvas.ActualWidth;
            int actualHeight = (int)_canvas.ActualHeight;
            List<List<Point>> polygons = _canvas.Children.OfType<Polygon>().Select(polygon => polygon.Points.ToList()).ToList();

            Task.Factory.StartNew(() => Bubble(progressIndicator, addfinishbutton, actualWidth, actualHeight,polygons));
        }

        private void Bubble(ProgressIndicator progressIndicator, bool addfinishbutton,int width,int height,List<List<Point>> polygons)
        {
            try
            {
                int stepsCount = BubblingSettings.Instance.Bubbles;
                double percent = stepsCount*3;
                int maxR = Math.Min(width, height)/2;

                for (int i = 0; i < stepsCount; i++)
                {
                    var pt = new Point(_rnd.Next(width), _rnd.Next(height));
                    if (polygons.Any(pl => pt.InPolygon(pl)))
                        continue;
                    var r =
                        polygons
                               .SelectMany(pl => pl)
                               .Select(x => x.SqDistance(pt))
                               .Min();
                    r = Math.Sqrt(r);
                    if (_circles.Count > 0)
                    {
                        var pr = _circles.Min(circle => circle.MaxRadiusTo(pt));
                        if (pr < 0)
                            continue;
                        if (pr < r)
                            r = pr;
                    }
                    if(r>maxR)
                        continue;

                    var c = new Circle(pt, r);
                    _circles.Add(c);
                    Dispatcher.BeginInvoke(() =>
                                               {
                                                   if (i%10 == 0)
                                                       progressIndicator.Value = i/percent;
                                               });
                }

                MutateCircles(progressIndicator,polygons,maxR,width,height,0.33);
                Dispatcher.BeginInvoke(() =>
                                           {
                                               progressIndicator.Value = 0.67;
                                           });
                if(_circles.Count>20)
                {
                    foreach (var c in _circles.OrderBy(c => c.R).Take(_circles.Count / 2).ToList())
                    {
                        _circles.Remove(c);
                    }
                }

                MutateCircles(progressIndicator, polygons, maxR, width, height, 0.67);

                Dispatcher.BeginInvoke(() =>
                                           {
                                               foreach (var c in _circles)
                                               {
                                                   _canvas.Children.Add(c.GetEllipse(this));
                                               }
                                           });
            }
            finally
            {
                Dispatcher.BeginInvoke(() =>
                                           {
                                               if (addfinishbutton)
                                                   AddFinishButton();
                                               SystemTray.SetProgressIndicator(this, null);
                                               _okButton.IsEnabled = true;
                                           });
            }
        }

        private readonly Point[] _mutations = {new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0),};
        private int _mutationsMoveCnt;
        
        Point GetNextMutation()
        {
            return _mutations[(_mutationsMoveCnt++)%_mutations.Length];
        }


        private void MutateCircles(ProgressIndicator progressIndicator, List<List<Point>> polygons, int maxR, int width, int height, double d)
        {
            double percent = _circles.Count*3;
            for (int i = 0; i < _circles.Count; i++)
            {
                var c = _circles[i];
                MutateOne(c, polygons, maxR, width, height);
                Dispatcher.BeginInvoke(() =>
                {
                    if (i % 10 == 0)
                        progressIndicator.Value = d + i/percent;
                });
            }
        }

        private void MutateOne(Circle circle, List<List<Point>> polygons, int maxR, int width, int height)
        {
            bool finished = false;
            while (!finished)
            {
                finished = true;
                for(int i=0;i<_mutations.Length;i++)
                {
                    var mutation = GetNextMutation();
                    double pr = TestProposedRadius(circle, mutation, polygons, maxR, width, height);
                    if(pr>circle.R)
                    {
                        finished = false;
                        _mutationsMoveCnt--;
                        circle.R = pr;
                        circle.X += mutation.X;
                        circle.Y += mutation.Y;
                    }
                }
            }
        }

        private double TestProposedRadius(Circle circle, Point mutation, List<List<Point>> polygons, int maxR, int width, int height)
        {
            var pt = new Point(circle.X+mutation.X, circle.Y+mutation.Y);
            int halfMax = maxR/4;
            if (pt.X < -halfMax || pt.Y < -halfMax)
                return -1;
            if (pt.X > width+halfMax || pt.Y > height+halfMax)
                return -1;

            if (polygons.Any(pl => pt.InPolygon(pl)))
                return -1;

            var r = polygons.SelectMany(pl => pl).Select(x => x.SqDistance(pt)).Min();

            r = Math.Sqrt(r);

            if (_circles.Count > 0)
            {
                var pr = _circles.Except(new[]{circle}).Min(c => c.MaxRadiusTo(pt));
                if (pr < 0)
                    return -1;

                if (pr < r)
                    r = pr;
            }
            if (r > maxR)
                return -1;
            return r;
        }

        private void AddFinishButton()
        {
            var okButton = new ApplicationBarIconButton(new Uri("/Assets/check.png", UriKind.Relative));
            okButton.Text = AppResources.PhotoEdit_AddFinishButton_Show_result;
            ApplicationBar.Buttons.Add(okButton);
            okButton.Click +=FinishButtonClick;
        }

        private void FinishButtonClick(object sender, EventArgs args)
        {
            bool trial;
            ViewResult.WritableBitmap = GeometryExtensions.Paint(wbm, _circles, _canvas.ActualWidth/wbm.PixelWidth, _canvas.ActualHeight/wbm.PixelHeight, out trial);

            if (trial && (DateTime.Now - BubblingSettings.Instance.FirstUse) > new TimeSpan(2, 0, 0, 0))
            {
                var res = MessageBox.Show(AppResources.PhotoEdit_FinishButtonClick_This_is_a_trial_version, AppResources.PhotoEdit_FinishButtonClick_BUBBLE_UNDRESSER, MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {
                    var width = (int) ViewResult.WritableBitmap.PixelWidth;
                    var height = (int) ViewResult.WritableBitmap.PixelHeight;
                    using (var ms = new MemoryStream(width*height*4))
                    {
                        ViewResult.WritableBitmap.SaveJpeg(ms, width, height, 0, 100);
                        ms.Seek(0, SeekOrigin.Begin);
                        var lib = new MediaLibrary();
                        var picture = lib.SavePicture(string.Format("bubbled.jpg"), ms);

                        var task = new ShareMediaTask();

                        task.FilePath = picture.GetPath();

                        task.Show();
                    }
                }
            }
            else
            {
                NavigationService.Navigate(new Uri("/ViewResult.xaml", UriKind.Relative));
            }
        }


        private bool editMode = true;
        private readonly string encircle = AppResources.PhotoEdit_encircle_Encircle_covered_areas;
        private readonly string move = AppResources.PhotoEdit_move_Move_picture;

        public static Stream PassedArgument { get; set; }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Touch.FrameReported+=TouchOnFrameReported;
            // Get a dictionary of query string keys and values.
            IDictionary<string, string> queryStrings = NavigationContext.QueryString;

            // Ensure that there is at least one key in the query string, and check whether the "FileId" key is present.
            if (queryStrings.ContainsKey("FileId"))
            {
                // Retrieve the photo from the media library using the FileID passed to the app.
                MediaLibrary library = new MediaLibrary();
                Picture photoFromLibrary = library.GetPictureFromToken(queryStrings["FileId"]);

                // Create a BitmapImage object and add set it as the image control source.
                // To retrieve a full-resolution image, use the GetImage() method instead.
                BitmapImage bitmapFromPhoto = new BitmapImage();
                bitmapFromPhoto.SetSource(photoFromLibrary.GetPreviewImage());
                wbm = new WriteableBitmap(bitmapFromPhoto);
                image1.Source = wbm;
            }
            else
            {
                Stream passedArgument = PassedArgument;
                if (passedArgument != null)
                {
                    PassedArgument = null;
                    
                    BitmapImage bitmapFromPhoto = new BitmapImage();
                    bitmapFromPhoto.SetSource(passedArgument);
                    wbm = new WriteableBitmap(bitmapFromPhoto);
                    image1.Source = wbm;
                }
            }
        }

        WriteableBitmap wbm;

        private Polygon _currentPolygon;
        readonly SolidColorBrush _solidColorBrush = new SolidColorBrush(Colors.Blue) { Opacity = 0.3 };
        readonly SolidColorBrush _strokeColorBrush = new SolidColorBrush(Colors.Purple);
        internal SolidColorBrush RedColorBrush = new SolidColorBrush(Colors.Red);
        private void Image1_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (editMode)
            {
                _currentPolygon = new Polygon();
                _currentPolygon.Stroke=_strokeColorBrush;
                _currentPolygon.StrokeThickness = 2;
                _currentPolygon.Fill = _solidColorBrush;
                _canvas.Children.Add(_currentPolygon);
            }
        }

        private double _totalScale = 1;

        private bool _pinching;

        private void Image1_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!editMode)
            {
                if (e.PinchManipulation != null)
                {
                    if (!_pinching)
                    {
                        TransferTransforms();
                        _pinching = true;
                        scaleTransform.CenterX = e.PinchManipulation.Original.Center.X;
                        scaleTransform.CenterY = e.PinchManipulation.Original.Center.Y;
                    }
                    double scale = e.PinchManipulation.DeltaScale;
                    var proposedScale = scale*_totalScale;
                    if (scale != 0 && scale != 1 && proposedScale > 0.8 && proposedScale < 8)
                    {
                        scaleTransform.ScaleX *= scale;
                        scaleTransform.ScaleY *= scale;
                        _totalScale *= scale;
                    }
                }
                else
                {
                    translateTransform.X += e.DeltaManipulation.Translation.X*_totalScale;
                    translateTransform.Y += e.DeltaManipulation.Translation.Y*_totalScale;
                }
            }
        }

        private void TouchOnFrameReported(object sender, TouchFrameEventArgs e)
        {
            if(!editMode)
                return;
            try
            {
                // Determine if finger / mouse is down
                var point = e.GetPrimaryTouchPoint(image1);

                if (point.Position.X < 0 || point.Position.Y < 0)
                    return;

                if (point.Position.X > image1.Width || point.Position.Y > image1.Height)
                    return;

                if (_currentPolygon != null)
                    _currentPolygon.Points.Add(new Point(point.Position.X, point.Position.Y));
            }
            catch(Exception ex)
            {
                MessageBox.Show(AppResources.PhotoEdit_TouchOnFrameReported_Application_error);
            }

        }

        private void Image1_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!editMode)
            {
                TransferTransforms();
                _pinching = false;
            }
            else
            {
                if (_currentPolygon.Points.Count < 8)
                {
                    _canvas.Children.Remove(_currentPolygon);
                    return;
                }
                var avgx = _currentPolygon.Points.Average(p => p.X);
                var avgy = _currentPolygon.Points.Average(p => p.Y);

                var el = new Ellipse();
                Canvas.SetTop(el, avgy-25);
                Canvas.SetLeft(el, avgx - 25);
                el.Width = 50;
                el.Height = 50;
                el.Fill= RedColorBrush;
                _canvas.Children.Add(el);

                Line l = new Line { X1 = avgx - 10, X2 = avgx + 10, Y1 = avgy - 10, Y2 = avgy + 10 ,Stroke = _strokeColorBrush};

                _canvas.Children.Add(l);

                var l1 = new Line { X1 = avgx + 10, X2 = avgx - 10, Y1 = avgy - 10, Y2 = avgy + 10, Stroke = _strokeColorBrush };

                _canvas.Children.Add(l1);

                Polygon currentPolygon = _currentPolygon;
                el.Tap += (o, args) =>
                              {
                                  _canvas.Children.Remove(currentPolygon);
                                  _canvas.Children.Remove(el);
                                  _canvas.Children.Remove(l);
                                  _canvas.Children.Remove(l1);
                                  args.Handled = true;
                              };

                currentPolygon.Tap += (o, args) => { args.Handled = true; };
                FinishoPolygonConsistency(_currentPolygon);
                _currentPolygon = null;
            }
        }

        private void FinishoPolygonConsistency(Polygon poly)
        {
            var f = poly.Points.First();
            var l = poly.Points.Last();
            var v = new Point(l.X-f.X,l.Y-f.Y);
            var d = Math.Sqrt(v.SqDistance(0, 0));

            const double pointsPerReplacement = 16;
            v = new Point(pointsPerReplacement*v.X/d,pointsPerReplacement*v.Y/d);
            d/=pointsPerReplacement;
            for(int i=1;i<d-1;i++)
            {
                f = new Point(f.X+v.X,f.Y+v.Y);
                poly.Points.Add(f);
            }
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

        Matrix Multiply(Matrix a, Matrix b)
        {
            return new Matrix(a.M11 * b.M11 + a.M12 * b.M21,
                              a.M11 * b.M12 + a.M12 * b.M22,
                              a.M21 * b.M11 + a.M22 * b.M21,
                              a.M21 * b.M12 + a.M22 * b.M22,
                              a.OffsetX * b.M11 + a.OffsetY * b.M21 + b.OffsetX,
                              a.OffsetX * b.M12 + a.OffsetY * b.M22 + b.OffsetY);
        }
    }

    class Circle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double R { get; set; }

        public Circle(double x, double y, double r)
        {
            X = x;
            Y = y;
            R = r;
        }

        public Circle(Point pt, double r)
        {
            X = pt.X;
            Y = pt.Y;
            R = r;
        }

        private Ellipse _elipsis;
        public Ellipse GetEllipse(PhotoEdit ph)
        {
            if (_elipsis != null)
                return _elipsis;
            _elipsis = new Ellipse();
            Canvas.SetTop(_elipsis, Y - R);
            Canvas.SetLeft(_elipsis,X - R);
            _elipsis.Width = R * 2;
            _elipsis.Height = R * 2;
            _elipsis.Stroke = ph.RedColorBrush;
            return _elipsis;
        }

        public double MaxRadiusTo(Point pt)
        {
            var totalD = Math.Sqrt(pt.SqDistance(X, Y));
            return totalD - R;
        }
    }
}