using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using Android.Util;
using Android.Content;
using Android.Graphics;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Android.Widget;
using Android.Views;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;
using TestLog.Camera;
using TestLog.Camera2;
using TestLog.Droid.Camera2;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidX.Lifecycle;
using static AndroidX.Camera.Core.ImageCapture;
using Math = Java.Lang.Math;
using Exception = System.Exception;
using File = Java.IO.File;
using Environment = Android.OS.Environment;
using Android.OS;
using AndroidX.Camera.Core.Internal.Utils;
using Android.Content.PM;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace TestLog.Droid.Camera2
{
    public class CameraPreviewRenderer : ViewRenderer<CameraPreview, PreviewView>
    {
        private bool isDisposed;
        #region Camera Manager
        private File outputDirectory;
        private const string TAG = nameof(CameraPreviewRenderer);
        private const string FILENAME_FORMAT = "yyyy-MM-dd-HH-mm-ss-SSS";

        private string AppName
        {
            get
            {
                ApplicationInfo applicationInfo = _context.PackageManager.GetApplicationInfo(_context.ApplicationInfo.PackageName, 0);
                return applicationInfo != null ? _context.PackageManager.GetApplicationLabel(applicationInfo) : "ToCam";
            }
        }

        private Preview cameraPreview;
        private ImageAnalysis imageAnalyzer;
        private PreviewView previewView;
        private IExecutorService cameraExecutor;
        private CameraSelector cameraSelector;
        private ProcessCameraProvider cameraProvider;
        private ICamera camera;
        private ImageCapture imageCapture;
        private CameraLocation cameraLocation = CameraLocation.Rear;
        private PreviewObserver previewObserver;
        private ILifecycleOwner lifecycleOwner
        {
            get
            {
                return _context is ILifecycleOwner _lifecycleOwner ? _lifecycleOwner : _context.GetActivity() as ILifecycleOwner;
            }
        }
        private MediaOptions mediaOptions;
        #endregion
        private CameraPreview _currentElement;
        private readonly Context _context;

        public CameraPreviewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected async Task<PreviewView> InitPreviewView()
        {
            if (!await CheckAndRequestPermissions())
                throw new NotImplementedException("Permission Camera nyaaaaaaa");

            var previewView = new PreviewView(_context);
            UpdateCameraOptions(Element.CameraOptions);
            mediaOptions = Element.MediaOptions;
            outputDirectory = GetOutputDirectory();
            cameraExecutor = Executors.NewSingleThreadExecutor();
            Connect();

            // Callback for preview visibility
            previewObserver = new PreviewObserver(camera, previewView, mediaOptions?.Placemark);
            previewView.PreviewStreamState.Observe(lifecycleOwner, previewObserver);
            return previewView;
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);
            _currentElement = e.NewElement;
            previewView = await InitPreviewView();

            SetNativeControl(previewView);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element == null || Control == null)
                return;

            if (e.PropertyName == nameof(CameraPreview.CameraOptions))
                UpdateCameraOptions(Element.CameraOptions);

            if (e.PropertyName == nameof(CameraPreview.FlashMode))
                SetFlash(Element.FlashMode);

            if (e.PropertyName == nameof(CameraPreview.MediaOptions))
            {
                mediaOptions = Element.MediaOptions;
                previewObserver?.UpdatePlacemark(mediaOptions?.Placemark);
            }
        }

        public async Task<bool> CheckAndRequestPermissions()
        {
            return
                (await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.Camera>()) == Xamarin.Essentials.PermissionStatus.Granted &&
                (await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageRead>()) == Xamarin.Essentials.PermissionStatus.Granted &&
                (await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageWrite>()) == Xamarin.Essentials.PermissionStatus.Granted;
        }

        public void Connect()
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(_context);

            cameraProviderFuture.AddListener(new Runnable(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Add Listener");

                    // Used to bind the lifecycle of cameras to the lifecycle owner
                    cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();

                    // Preview
                    cameraPreview = new Preview.Builder().Build();
                    cameraPreview.SetSurfaceProvider(previewView.SurfaceProvider);

                    // Take Photo
                    imageCapture = new Builder().Build();

                    UpdateCamera();

                    _currentElement.CameraClick = new Command(async () =>
                    {
                        _ = await TakePicture();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in cameraProviderFuture.AddListener ! : " + ex);
                }

            }), ContextCompat.GetMainExecutor(_context)); //GetMainExecutor: returns an Executor that runs on the main thread.
        }

        private async Task<bool> TakePicture()
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            // Get a stable reference of the modifiable image capture use case   
            var imageCapture = this.imageCapture;
            if (imageCapture == null)
                return false;

            // Set up image capture listener, which is triggered after photo has been taken
            #region Versi 1: This method provides an in-memory buffer of the captured image.
            imageCapture.TakePicture(ContextCompat.GetMainExecutor(_context), new ImageCapturedCallback(
                completionSource,
                onErrorCallback: (exc) =>
                {
                    var msg = $"Photo capture failed: {exc.Message}";

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                    {
                        if (!Environment.IsExternalStorageManager)
                        {
                            _context.GetActivity().StartActivityForResult(new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission), 0);
                        }
                    }

                    _currentElement?.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: null, path: null));
                    Log.Error(TAG, msg, exc);
                    Toast.MakeText(_context, msg, ToastLength.Short).Show();

                    completionSource.TrySetException(exc);
                },
                onCapturedSuccessCallback: async (imageProxy) =>
                {
                    string path = $"{outputDirectory}/{new Java.Text.SimpleDateFormat(FILENAME_FORMAT, Locale.Us).Format(JavaSystem.CurrentTimeMillis())}{".jpg"}";

                    /* https://developer.android.com/reference/androidx/camera/core/ImageInfo#getRotationDegrees() */
                    int correctRotation = imageProxy.ImageInfo.RotationDegrees;

                    System.Diagnostics.Debug.WriteLine("Correct Rotation: " + correctRotation);

                    if (Build.VERSION.SdkInt == BuildVersionCodes.LollipopMr1 || Build.VERSION.SdkInt == BuildVersionCodes.Lollipop)
                        correctRotation = cameraSelector == CameraSelector.DefaultFrontCamera ? -90 : 90;

                    /* imageproxy to bitmap */
                    var data = ImageUtil.ImageToJpegByteArray(imageProxy);
                    imageProxy.Close();
                    var imageBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);

                    var result = await ProcessImage(mediaOptions, imageBitmap, correctRotation, path);

                    imageBitmap.Recycle();
                    imageBitmap.Dispose();
                    GC.Collect();

                    if (!result)
                        completionSource.SetCanceled();
                    else
                        completionSource.TrySetResult(true);
                }
            ));

            var result = await completionSource.Task;
            return result;
            #endregion
            //#region Versi 2: This method saves the captured image to the provided file location.
            //imageCapture.TakePicture(outputOptions, ContextCompat.GetMainExecutor(_context), new ImageSaveCallback(

            //    onErrorCallback: (exc) =>
            //    {
            //        var msg = $"Photo capture failed: {exc.Message}";


            //        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            //        {
            //            if (!Android.OS.Environment.IsExternalStorageManager)
            //            {
            //                _context.GetActivity().StartActivityForResult(new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission), 0);
            //            }
            //        }

            //        Log.Error(TAG, msg, exc);
            //        Toast.MakeText(_context, msg, ToastLength.Short).Show();
            //    },

            //    onImageSaveCallback: (output) =>
            //    {
            //        var data = System.IO.File.ReadAllBytes(photoFile?.Path);
            //        _currentElement?.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: data, path: photoFile?.Path));
            //    }
            //));
            //#endregion
        }

        // Save photos to => /Pictures/CameraX/
        [Obsolete]
        private File GetOutputDirectory()
        {
            File mediaDir;
            if (string.IsNullOrEmpty(mediaOptions?.Directory))
                mediaDir = Environment.GetExternalStoragePublicDirectory(System.IO.Path.Combine(Environment.DirectoryPictures, AppName));
            else
                mediaDir = new File(mediaOptions.Directory);

            System.Diagnostics.Debug.WriteLine("MediaDir Path: " + mediaDir.Path);
            if (mediaDir != null && mediaDir.Exists())
                return mediaDir;

            var file = new File(mediaDir, string.Empty);
            file.Mkdirs();
            return file;
        }

        public void Disconnect()
        { }

        public void UpdateCameraOptions(CameraOptions cameraOptions)
        {
            CameraLocation camLocation;

            switch (cameraOptions)
            {
                case CameraOptions.Rear:
                    camLocation = CameraLocation.Rear;
                    break;
                case CameraOptions.Front:
                    camLocation = CameraLocation.Front;
                    break;
                default:
                    camLocation = CameraLocation.Front;
                    break;
            }

            if (camLocation == cameraLocation)
                return;

            cameraLocation = camLocation;
            UpdateCamera();
        }

        /// <summary>
        /// Set On/Off Tourch
        /// </summary>
        /// <param name="flashMode"></param>
        public void SetFlash(FlashMode flashMode)
        {
            if (cameraLocation == CameraLocation.Front)
                return;

            switch (flashMode)
            {
                case FlashMode.On:
                    camera?.CameraControl?.EnableTorch(true);
                    break;
                case FlashMode.Off:
                    camera?.CameraControl?.EnableTorch(false);
                    break;
            }
        }

        public void SetZoomAndFocusTouchListener()
        {
            //#region Pinch to Zoom
            //ScaleGestureListener listener = new ScaleGestureListener(camera.CameraControl, camera.CameraInfo);
            //ScaleGestureDetector scaleGestureDetector = new ScaleGestureDetector(_context, listener);
            //ScaleTouchListener scaleTouchListener = new ScaleTouchListener(scaleGestureDetector);
            //previewView.SetOnTouchListener(scaleTouchListener);
            //#endregion

            //#region Tap to Focus
            //FocusTouchListener focusTouchListener = new FocusTouchListener(previewView.MeteringPointFactory, camera.CameraControl);
            //previewView.SetOnTouchListener(focusTouchListener);
            ////previewView.listen
            //#endregion

            #region Pinch to Zoom & Tap To Focus
            ScaleGestureListener listener = new ScaleGestureListener(camera.CameraControl, camera.CameraInfo);
            ScaleGestureDetector scaleGestureDetector = new ScaleGestureDetector(_context, listener);
            ZoomAndFocusTouchListener zoomAndFocusTouchListener = new ZoomAndFocusTouchListener(scaleGestureDetector, previewView.MeteringPointFactory, camera.CameraControl);
            //ScaleTouchListener scaleTouchListener = new ScaleTouchListener(scaleGestureDetector);
            previewView.SetOnTouchListener(zoomAndFocusTouchListener);
            #endregion
        }


        public void UpdateCamera()
        {
            if (cameraProvider != null)
            {
                // Unbind use cases before rebinding
                cameraProvider.UnbindAll();

                // Select back camera as a default, or front camera otherwise
                switch (cameraLocation)
                {
                    case CameraLocation.Rear when cameraProvider.HasCamera(CameraSelector.DefaultBackCamera):
                        cameraSelector = CameraSelector.DefaultBackCamera;
                        break;
                    case CameraLocation.Front when cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera):
                        cameraSelector = CameraSelector.DefaultFrontCamera;
                        break;
                    default:
                        cameraSelector = CameraSelector.DefaultBackCamera;
                        break;
                }

                if (cameraSelector == null)
                    throw new Exception("Camera not found");

                // The Context here SHOULD be something that's a lifecycle owner
                if (lifecycleOwner != null)
                    camera = cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, imageCapture, cameraPreview);//, imageAnalyzer);
                
                SetFlash(Element.FlashMode);

                SetZoomAndFocusTouchListener();
            }
        }

        public void UpdateTorch(bool on)
        {
            camera?.CameraControl?.EnableTorch(on);
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            System.Diagnostics.Debug.WriteLine("Dispose !");
            cameraExecutor?.Shutdown();
            cameraExecutor?.Dispose();
            imageCapture?.Dispose();
            imageAnalyzer?.Dispose();
            cameraSelector?.Dispose();
            cameraProvider?.Dispose();
            camera?.Dispose();
            previewObserver?.Dispose();

            base.Dispose(disposing);
        }

        // Fix
        // - Orientation
        // - Resize
        // - Flip
        // - Scaled
        // - Placemark
        public Task<bool> ProcessImage(MediaOptions options, Bitmap originalImage, int rotation, string outputPath)
        {
            if (originalImage == null)
                return Task.FromResult(false);

            try
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var percent = 1.0f;
                        switch (mediaOptions.PhotoSize)
                        {
                            case PhotoSize.Large:
                                percent = .75f;
                                break;
                            case PhotoSize.Medium:
                                percent = .5f;
                                break;
                            case PhotoSize.Small:
                                percent = .25f;
                                break;
                            case PhotoSize.Custom:
                                percent = mediaOptions.CustomPhotoSize / 100f;
                                break;
                        }

                        if (originalImage == null)
                            return false;

                        if (mediaOptions.PhotoSize == PhotoSize.MaxWidthHeight && mediaOptions.MaxWidthHeight.HasValue)
                        {
                            var max = Math.Max(originalImage.Width, originalImage.Height);
                            if (max > mediaOptions.MaxWidthHeight)
                            {
                                percent = (float)mediaOptions.MaxWidthHeight / max;
                            }
                        }

                        var finalWidth = (int)(originalImage.Width * percent);
                        var finalHeight = (int)(originalImage.Height * percent);

                        // Scale Image
                        if (finalWidth != originalImage.Width || finalHeight != originalImage.Height)
                            originalImage = Bitmap.CreateScaledBitmap(originalImage, finalWidth, finalHeight, true);

                        var compressFormat = Bitmap.CompressFormat.Jpeg;

                        // Rotate Image
                        if (rotation != 0)
                        {
                            var matrix = new Matrix();
                            matrix.PostRotate(rotation);
                            originalImage = Bitmap.CreateBitmap(originalImage, 0, 0, originalImage.Width, originalImage.Height, matrix, true);
                        }

                        // Flip Image
                        originalImage = DoFlipHorizontal(originalImage, rotation);

                        // Draw Placemark
                        originalImage = DrawPlacemark(originalImage, mediaOptions?.Placemark);

                        //always need to compress to save back to disk
                        using FileStream stream = new FileStream(outputPath, FileMode.Create);
                        await originalImage.CompressAsync(compressFormat, mediaOptions.CompressionQuality, stream);

                        var bytes = await GetBytesAsync(stream);
                        _currentElement?.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: bytes, path: outputPath));

                        originalImage.Recycle();
                        originalImage.Dispose();
                        // Dispose of the Java side bitmap.
                        GC.Collect();
                        return true;

                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        throw ex;
#else
                        return false;
#endif
                    }
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#else
                return Task.FromResult(false);
#endif
            }
        }

        private Bitmap DrawPlacemark(Bitmap originalImage, Placemark placemark)
        {
            Bitmap mutableBitmap = originalImage.Copy(Bitmap.Config.Argb8888, true);
            Canvas canvas = new Canvas(mutableBitmap);

            CanvasHelper.DrawPlacemarkView(canvas, mutableBitmap.Width, mutableBitmap.Height, placemark);

            canvas.Dispose();
            return mutableBitmap;
        }

        /// <summary>
        /// Front camera ( mirror ), jdi harus gambar nya harus di flip
        /// </summary>
        private Bitmap DoFlipHorizontal(Bitmap originalImage, int rotation)
        {
            if (rotation <= 90 && rotation >= 0 || cameraLocation != CameraLocation.Front)
                return originalImage;

            Bitmap mutableBitmap = originalImage.Copy(Bitmap.Config.Argb8888, true);
            Canvas canvas = new Canvas(originalImage);
            Matrix flipHorizontalMatrix = new Matrix();
            flipHorizontalMatrix.SetScale(-1, 1);
            flipHorizontalMatrix.PostTranslate(mutableBitmap.Width, 0);
            canvas.DrawBitmap(mutableBitmap, flipHorizontalMatrix, null);
            canvas.Dispose();

            return originalImage;
        }

        // nyontek dari https://github.com/jamesmontemagno/MediaPlugin/blob/master/src/Media.Plugin/Android/MediaImplementation.cs#L795-L818
        int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            var height = options.OutHeight;
            var width = options.OutWidth;
            var inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                var halfHeight = height / 2;
                var halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        private async Task<byte[]> GetBytesAsync(Stream input)
        {
            using MemoryStream memoryStream = new MemoryStream();
            input.Position = 0;

            await input.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
