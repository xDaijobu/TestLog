using Android.Graphics;
using AndroidX.Camera.Core;
using AndroidX.Camera.Core.Internal.Utils;
using System;
using static AndroidX.Camera.Core.ImageCapture;

namespace TestLog.Droid.Camera2
{
    public class ImageCapturedCallback : OnImageCapturedCallback
    {
        private readonly Action<ImageCaptureException> onErrorCallback;
        private readonly Action<IImageProxy> onCapturedSuccessCallback;

        public ImageCapturedCallback(Action<IImageProxy> onCapturedSuccessCallback, Action<ImageCaptureException> onErrorCallback)
        {
            this.onCapturedSuccessCallback = onCapturedSuccessCallback;
            this.onErrorCallback = onErrorCallback;
        }

        public override void OnError(ImageCaptureException exc)
        {
            this.onErrorCallback?.Invoke(exc);
        }

        public override void OnCaptureSuccess(IImageProxy image)
        {
            //var data = ImageUtil.ImageToJpegByteArray(image);
            //var imageBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);

            this.onCapturedSuccessCallback?.Invoke(image);

            base.OnCaptureSuccess(image);
            //image.Close();
        }
    }
}