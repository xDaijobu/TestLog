﻿using System;
using System.Diagnostics;
using Xamarin.Forms;

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

    public class CameraPreview : View
    {
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

        public static readonly BindableProperty DirectoryProperty = BindableProperty.Create(
            propertyName: nameof(Directory),
            returnType: typeof(string),
            declaringType: typeof(CameraPreview),
            defaultValue: string.Empty);

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

        public string Directory
        {
            get { return (string)GetValue(DirectoryProperty); }
            set { SetValue(DirectoryProperty, value); }
        }

        public Command CameraClick
        {
            get { return cameraClick; }
            set { cameraClick = value; }
        }

        public void PictureTaken()
        {
            PictureFinished?.Invoke();
        }

        public event Action PictureFinished;

        public void RaiseMediaCaptured(MediaCapturedEventArgs args)
        {
            Debug.WriteLine("RaiseMediaCaptured");
            MediaCaptured?.Invoke(this, args);
        }
    }
}
