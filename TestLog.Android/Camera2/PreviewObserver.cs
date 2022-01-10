using System;
using System.Reflection;
using Android.Animation;
using Android.Gms.Maps;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using TestLog.Camera2;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Animation.ValueAnimator;
using static Android.Views.ViewGroup;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace TestLog.Droid.Camera2
{
    public class PreviewObserver : Java.Lang.Object, IObserver, IAnimatorUpdateListener
    {
        Location _location;
        public Action<Location> DrawAction => DrawSomething;
        readonly ICamera _camera;
        readonly PreviewView _previewView;
        public PreviewObserver(ICamera camera, PreviewView previewView, Location location)
        {
            _camera = camera;
            _previewView = previewView;
            _location = location;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            float animatedValue = (float)animation.AnimatedValue;
            System.Diagnostics.Debug.WriteLine("animatedValue: " + animatedValue);
            _camera?.CameraControl?.SetLinearZoom(animatedValue);
        }

        public void OnChanged(Java.Lang.Object p0)
        {
            var currentState = p0 as PreviewView.StreamState;

            if (currentState != null)
            {
                System.Diagnostics.Debug.WriteLine(currentState);
                if (currentState == PreviewView.StreamState.Idle)
                {
                    //Bitmap currentView = BlurRenderScript(_previewView.Bitmap, 10);
                    //_previewView.SetBackgroundColor(Android.Graphics.Color.Red);
                    //_previewView.Background = new BitmapDrawable(Resources, currentView);w

                    // Initializing the animator with values to zoom - min:0f - max:1f
                    ValueAnimator animator = OfFloat(0.1f, 0.2f, 0.3f, 0.4f, 0.5f);

                    // Setting animation duration
                    animator.SetDuration(200);

                    // Adding listener for every value update of the animation
                    animator.AddUpdateListener(this);
                    // Start the zoom animation
                    animator.Start();
                }
                else if (currentState == PreviewView.StreamState.Streaming)
                {
                    System.Diagnostics.Debug.WriteLine("Streaming!!!");

                    //DrawAction?.Invoke(_location);
                    var firstLabel = CreateTextView($"Latitude: {_location?.Latitude} | Longitude: {_location?.Longitude}");
                    var secondLabel = CreateTextView("Testing saja");
                    LinearLayout linearLayout = new LinearLayout(MainActivity.Instance);
                    linearLayout.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    linearLayout.AddView(firstLabel);
                    linearLayout.AddView(secondLabel);
                    //var mapView = CreateGoogleMap();
                    //mapView
                    //linearLayout.AddView(CreateGoogleMap);
                    linearLayout.Orientation = Orientation.Vertical;
                    //linearLayout.SetBackgroundColor(Color.Red);
                    linearLayout.SetGravity(GravityFlags.Bottom);
                    linearLayout.SetPadding(0, 0, 0, 150);

                    _previewView.AddView(linearLayout);
                }
            }
        }

        public void DrawSomething(Location location)
        {
            _location = location;
        }

        private TextView CreateTextView(string text)
        {
            var x = new TextView(MainActivity.Instance);
            x.Text = text;
            System.Diagnostics.Debug.WriteLine("Current Location: " + x.Text);
            x.SetTextColor(Color.Blue);
            //x.Gravity = Android.Views.GravityFlags.Bottom;
            return x;
        }

        private MapView CreateGoogleMap()
        {
            var mapView = new MapView(MainActivity.Instance)
            {
                
            };
            return mapView;
        }

        public Bitmap GetBitmapFromView(View view)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            view.Draw(canvas);
            return bitmap;
        }
    }
}
