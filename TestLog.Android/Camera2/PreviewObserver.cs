using System;
using Android.Animation;
using Android.App;
using Android.OS;
using AndroidX.Camera.Core;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using TestLog.Camera2;
using static Android.Animation.ValueAnimator;

namespace TestLog.Droid.Camera2
{
    public class PreviewObserver : Java.Lang.Object, IObserver, IAnimatorUpdateListener
    {
        Placemark _placemark;
        public Action<Placemark> UpdatePlacemark => SetPlacemark;
        readonly ICamera _camera;   
        readonly PreviewView _previewView;
        public PreviewObserver(ICamera camera, PreviewView previewView, Placemark placemark)
        {
            _camera = camera;
            _previewView = previewView;
            _placemark = placemark;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            float animatedValue = (float)animation.AnimatedValue;
            System.Diagnostics.Debug.WriteLine("animatedValue: " + animatedValue);
            _camera?.CameraControl?.SetLinearZoom(animatedValue);
        }

        public void OnChanged(Java.Lang.Object p0)
        {
            if (p0 is PreviewView.StreamState currentState)
            {
                System.Diagnostics.Debug.WriteLine(currentState);

                if (currentState == PreviewView.StreamState.Idle)
                {
                    System.Diagnostics.Debug.WriteLine("Idle!!!");
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

                    var placemarkView = new PlacemarkView(Application.Context, _previewView.Height, _previewView.Width, _placemark);

                    /* Hack ~ utk memperbaiki tampilan camera di lollipop */
                    if (Build.VERSION.SdkInt == BuildVersionCodes.LollipopMr1 ||
                        Build.VERSION.SdkInt == BuildVersionCodes.Lollipop)
                    {
                        float targetRotation = 180;
                        _previewView.Rotation = targetRotation;
                        placemarkView.Rotation = targetRotation;
                    }

                    _previewView.AddView(placemarkView);
                }
            }
        }

        private void SetPlacemark(Placemark placemark)
        {
            _placemark = placemark;
        }
    }
}
