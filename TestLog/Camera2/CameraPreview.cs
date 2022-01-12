using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace TestLog.Camera2
{
    public enum CameraOptions
    {
        Rear,
        Front
    }

    public enum FlashMode
    {
        Off,
        On,
        Auto,
    }

    public class CameraPreview : ContentView
    {
        [Preserve(Conditional = true)]
        public event EventHandler<MediaCapturedEventArgs> MediaCaptured;

        Command cameraClick;

        public static readonly BindableProperty CameraOptionsProperty = BindableProperty.Create(
            propertyName: nameof(CameraOptions),
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

        public static readonly BindableProperty FlashModeProperty = BindableProperty.Create(
            propertyName: nameof(FlashMode),
            returnType: typeof(FlashMode),
            declaringType: typeof(CameraPreview),
            defaultValue: FlashMode.Off);

        public static readonly BindableProperty MediaOptionsProperty = BindableProperty.Create(
            propertyName: nameof(MediaOptions),
            returnType: typeof(MediaOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: default(MediaOptions));

        public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(CameraPreview), false);

        public CameraOptions CameraOptions
        {
            get { return (CameraOptions)GetValue(CameraOptionsProperty); }
            set { SetValue(CameraOptionsProperty, value); }
        }

        public FlashMode FlashMode
        {
            get { return (FlashMode)GetValue(FlashModeProperty); }
            set { SetValue(FlashModeProperty, value); }
        }

        public MediaOptions MediaOptions
        {
            get { return (MediaOptions)GetValue(MediaOptionsProperty); }
            set { SetValue(MediaOptionsProperty, value); }
        }

        public Command CameraClick
        {
            get { return cameraClick; }
            set { cameraClick = value; }
        }

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public void RaiseMediaCaptured(MediaCapturedEventArgs args)
        {
            Debug.WriteLine("RaiseMediaCaptured");
            MediaCaptured?.Invoke(this, args);
        }

        public CameraPreview()
        {
        }
    }
}
