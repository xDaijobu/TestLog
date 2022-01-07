using Android.Animation;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Android.Animation.ValueAnimator;
using Color = Android.Graphics.Color;

namespace TestLog.Droid.Camera2
{
    public class PreviewObserver : Java.Lang.Object, IObserver, IAnimatorUpdateListener
    {
        Location location;
        
        readonly ICamera _camera;
        readonly PreviewView _previewView;
        public PreviewObserver(ICamera camera, PreviewView previewView)
        {
            _camera = camera;
            _previewView = previewView;

            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    location = await Geolocation.GetLastKnownLocationAsync();
            //});
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

                    _previewView?.AddView(textView);
                }
            }
        }

        private TextView textView
        {
            get
            {
                var x = new TextView(MainActivity.Instance);
                x.Text = $"Latitude: {location?.Latitude} | Longitude: {location?.Longitude}";
                x.SetTextColor(Color.Blue);
                x.Gravity = Android.Views.GravityFlags.Bottom;
                return x;
            }
        }
    }
}
