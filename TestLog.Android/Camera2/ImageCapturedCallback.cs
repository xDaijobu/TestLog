using AndroidX.Camera.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static AndroidX.Camera.Core.ImageCapture;

namespace TestLog.Droid.Camera2
{
    public class ImageCapturedCallback : OnImageCapturedCallback
    {
        public TaskCompletionSource<bool> completionSource;
        private readonly Action<ImageCaptureException> onErrorCallback;
        private readonly Action<IImageProxy> onCapturedSuccessCallback;

        public ImageCapturedCallback(TaskCompletionSource<bool> completionSource, Action<IImageProxy> onCapturedSuccessCallback, Action<ImageCaptureException> onErrorCallback)
        {
            this.completionSource = completionSource;
            this.onCapturedSuccessCallback = onCapturedSuccessCallback;
            this.onErrorCallback = onErrorCallback;
        }

        public override void OnError(ImageCaptureException exc)
        {
            //Task task = Task.Run(() => onErrorCallback(exc));
            this.onErrorCallback?.Invoke(exc);
            //await task;

            //completionSource.TrySetCanceled();
        }

        public override void OnCaptureSuccess(IImageProxy image)
        {
            //Debug.WriteLine("Task.Run onCapturedSuccess");
            //Task task = Task.Run(() => onCapturedSuccessCallback(image));
            this.onCapturedSuccessCallback?.Invoke(image);
            //Debug.WriteLine("await Task oncapture done");
            //await task;
    
            base.OnCaptureSuccess(image);
            Debug.WriteLine("base on capture success");

            //Debug.WriteLine("completionSource try reset result TRUE");
            //completionSource.TrySetResult(true);
        }

        Task<T> AsAsync<T>(Action<Action<T>> target)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                target(t => tcs.SetResult(t));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}