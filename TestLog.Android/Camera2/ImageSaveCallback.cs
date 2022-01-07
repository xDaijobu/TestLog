using System;
using AndroidX.Camera.Core;
using static AndroidX.Camera.Core.ImageCapture;

namespace TestLog.Droid.Camera2
{
    public class ImageSaveCallback : Java.Lang.Object, IOnImageSavedCallback
    {
        private readonly Action<ImageCaptureException> onErrorCallback;
        private readonly Action<OutputFileResults> onImageSaveCallback;

        public ImageSaveCallback(Action<OutputFileResults> onImageSaveCallback, Action<ImageCaptureException> onErrorCallback)
        {
            this.onImageSaveCallback = onImageSaveCallback;
            this.onErrorCallback = onErrorCallback;
        }

        public void OnError(ImageCaptureException exc)
        {
            onErrorCallback.Invoke(exc);
        }

        public void OnImageSaved(OutputFileResults photoFile)
        {
            onImageSaveCallback.Invoke(photoFile);
        }
    }
}
