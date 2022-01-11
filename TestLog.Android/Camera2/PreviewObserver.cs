﻿using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using TestLog.Camera2;
using static Android.Animation.ValueAnimator;
using static Android.Views.ViewGroup;
using Color = Android.Graphics.Color;

namespace TestLog.Droid.Camera2
{
    public class PreviewObserver : Java.Lang.Object, IObserver, IAnimatorUpdateListener
    {
        Placemark _placemark;
        public Action<Placemark> DrawAction => DrawSomething;
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
            var currentState = p0 as PreviewView.StreamState;

            if (currentState != null)
            {
                System.Diagnostics.Debug.WriteLine(currentState);
                if (currentState == PreviewView.StreamState.Idle)
                {
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

                    //var firstLabel = CreateTextView($"Latitude: {_placemark?.Location?.Latitude}");
                    //var secondLabel = CreateTextView($"Longitude: {_placemark?.Location?.Longitude}");
                    //var thirdLabel = CreateTextView(_placemark.CountryName);
                    //View drawView = new View(MainActivity.Instance);
                    //drawView.SetBackgroundColor(Color.Red);
                    //drawView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    //linearLayout.AddView(firstLabel);
                    //linearLayout.AddView(secondLabel);
                    //linearLayout.AddView(thirdLabel);
                    //linearLayout.Orientation = Orientation.Vertical;
                    ////linearLayout.SetPadding(0, 500, 5, 0);
                    ////linearLayout.SetGravity(GravityFlags.Right);
                    //linearLayout.SetHorizontalGravity(GravityFlags.End);
                    //linearLayout.SetVerticalGravity(GravityFlags.Center);

                    //Bitmap a = null;
                    //ImageView imageView = new ImageView(MainActivity.Instance);
                    //imageView.SetImageBitmap(a);

                   

                    /* Hack ~ utk memperbaiki tampilan camera di lollipop */
                    if (Build.VERSION.SdkInt == BuildVersionCodes.LollipopMr1 ||
                        Build.VERSION.SdkInt == BuildVersionCodes.Lollipop)
                    {
                        float targetRotation = 180;
                        _previewView.Rotation = targetRotation;
                        //canvas.Rotate(targetRotation);
                        //linearLayout.Rotation = targetRotation;
                    }
                    var drawView = new DrawView(MainActivity.Instance, _previewView.Height, _previewView.Width, _placemark);
                    //drawView.Draw(canvas);
                    _previewView.AddView(drawView);
                    //_previewView.Draw(canvas);
                    //canvas.Restore();
                    //canvas.Dispose();
                    //canvas?.Dispose();
                    //_previewView?.AddView(linearLayout);
                }
            }
        }

        public void DrawSomething(Placemark placemark)
        {
            _placemark = placemark;
        }
    }
}
