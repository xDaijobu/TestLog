﻿using System;
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
using Android.Graphics.Drawables;
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

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace TestLog.Droid.Camera2
{
    public class CameraPreviewRenderer : ViewRenderer<CameraPreview, PreviewView>
    {
        bool isDisposed;
        #region Camera Manager
        File outputDirectory;
        private const string TAG = nameof(CameraPreviewRenderer);
        private const string FILENAME_FORMAT = "yyyy-MM-dd-HH-mm-ss-SSS";
        string directory = "ToCam";
        Preview cameraPreview;
        ImageAnalysis imageAnalyzer;
        PreviewView previewView;
        IExecutorService cameraExecutor;
        CameraSelector cameraSelector;
        ProcessCameraProvider cameraProvider;
        ICamera camera;
        ImageCapture imageCapture;
        CameraLocation cameraLocation = CameraLocation.Rear;
        PreviewObserver previewObserver;
        ILifecycleOwner lifecycleOwner
        {
            get
            {
                return _context is ILifecycleOwner _lifecycleOwner ? _lifecycleOwner : _context.GetActivity() as ILifecycleOwner;
            }
        }
        MediaOptions mediaOptions;
        Placemark placeMark;
        #endregion
        private CameraPreview _currentElement;
        private readonly Context _context;

        public CameraPreviewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected async Task<PreviewView> InitPreviewView()
        {
            if (!await CheckPermissions())
                throw new NotImplementedException("Permission Camera nyaaaaaaa");

            var previewView = new PreviewView(_context);
            UpdateCameraOptions(Element.CameraOptions);
            mediaOptions = Element.MediaOptions;
            placeMark = Element.Placemark;

            cameraExecutor = Executors.NewSingleThreadExecutor();
            Connect();

            // Callback for preview visibility
            previewObserver = new PreviewObserver(camera, previewView, placeMark);
            previewView.PreviewStreamState.Observe(lifecycleOwner, previewObserver);

            return previewView;
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);
            _currentElement = e.NewElement;
            previewView = await InitPreviewView();
            outputDirectory = GetOutputDirectory();

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

            if (e.PropertyName == nameof(CameraPreview.Directory))
                directory = Element.Directory;

            if (e.PropertyName == nameof(CameraPreview.MediaOptions))
                mediaOptions = Element.MediaOptions;

            if (e.PropertyName == nameof(CameraPreview.Placemark))
            {
                placeMark = Element.Placemark;
                System.Diagnostics.Debug.WriteLine("Lat: " + placeMark?.Location?.Latitude);
                System.Diagnostics.Debug.WriteLine("Lon: " + placeMark?.Location?.Longitude);
                previewObserver?.DrawAction(placeMark);
            }
        }

        public async Task<bool> CheckPermissions()
        {
            await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageRead>();
            await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageWrite>();
            return (await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.Camera>()) == Xamarin.Essentials.PermissionStatus.Granted;
        }

        public void Connect()
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(_context);

            cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
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
                        var result = await TakePicture();
                        System.Diagnostics.Debug.WriteLine("Result TakePicture: " + result + " | " + DateTime.Now);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error ! : " + ex);
                }

            }), ContextCompat.GetMainExecutor(_context)); //GetMainExecutor: returns an Executor that runs on the main thread.
        }

        [Obsolete]
        void ExportBitmapAsPNG(Bitmap bitmap)
        {
            var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var filePath = System.IO.Path.Combine(sdCardPath, $"test{System.DateTime.Now.Ticks}.png");
            var stream = new FileStream(filePath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Close();
        }

        private async Task<bool> TakePicture()
        {
            var completionSource = new TaskCompletionSource<bool>();

            // Get a stable reference of the modifiable image capture use case   
            var imageCapture = this.imageCapture;
            imageCapture.TargetRotation = 0;
            if (imageCapture == null)
                return false;
            //// Create time-stamped output file to hold the image
            //var photoFile = new File(outputDirectory, new Java.Text.SimpleDateFormat(FILENAME_FORMAT, Locale.Us).Format(JavaSystem.CurrentTimeMillis()) + ".jpg");

            //// Create output options object which contains file + metadata
            //var outputOptions = new OutputFileOptions.Builder(photoFile).Build();

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
                    int correctOrientation = imageProxy.ImageInfo.RotationDegrees;
                    System.Diagnostics.Debug.WriteLine("Correct Rotation: " + correctOrientation);

                    /* imageproxy to bitmap */
                    var data = ImageUtil.ImageToJpegByteArray(imageProxy);
                    System.Diagnostics.Debug.WriteLine("ImageProxy To Byte ~ " + DateTime.Now);
                    imageProxy.Close();
                    var imageBitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                    await FixOrientationAndResizeAsync(mediaOptions, imageBitmap, correctOrientation, path);

                    //System.Diagnostics.Debug.WriteLine("Array to Bitmap ~ " + DateTime.Now);
                    //var finalBitmap = RotateBitmap(imageBitmap, correctOrientation);
                    //System.Diagnostics.Debug.WriteLine("Rotate Bitmap ~ " + DateTime.Now);

                    //using FileStream os = new FileStream(path, FileMode.Create);
                    //await finalBitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, os);
                    //System.Diagnostics.Debug.WriteLine("Bitmap Compress ~ " + DateTime.Now);
                    //os.Close();

                    _currentElement?.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: data, path: path));

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
            File mediaDir = Environment.GetExternalStoragePublicDirectory(System.IO.Path.Combine(Environment.DirectoryPictures, directory));

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

        public void UpdateCamera()
        {
            if (cameraProvider != null)
            {
                // Unbind use cases before rebinding
                cameraProvider.UnbindAll();

                // Select back camera as a default, or front camera otherwise
                if (cameraLocation == CameraLocation.Rear && cameraProvider.HasCamera(CameraSelector.DefaultBackCamera))
                    cameraSelector = CameraSelector.DefaultBackCamera;
                else if (cameraLocation == CameraLocation.Front && cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera))
                    cameraSelector = CameraSelector.DefaultFrontCamera;
                else
                    cameraSelector = CameraSelector.DefaultBackCamera;

                if (cameraSelector == null)
                    throw new System.Exception("Camera not found");

                // The Context here SHOULD be something that's a lifecycle owner
                if (lifecycleOwner != null)
                    camera = cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, imageCapture, cameraPreview);//, imageAnalyzer);

                SetFlash(Element.FlashMode);

                // TODO Cris: bkin opsi hanya bisa di Zoom / Focus / 2 2 nya
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
            //previewObserver?.Dispose();

            base.Dispose(disposing);
        }

        // nyontek dari https://github.com/jamesmontemagno/MediaPlugin/blob/master/src/Media.Plugin/Android/MediaImplementation.cs#L652-L793
        // ngga 100% sama
        public Task<bool> FixOrientationAndResizeAsync(MediaOptions options, Bitmap originalImage, int rotation, string outputPath)
        {
            if (originalImage == null)
                return Task.FromResult(false);
            try
            {
                return Task.Run(() =>
                {
                    try
                    {
                        // if we don't need to rotate, aren't resizing, and aren't adjusting quality then simply return
                        if (rotation == 0 && mediaOptions.PhotoSize == PhotoSize.Full && mediaOptions.CompressionQuality == 100)
                            return false;

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
                                percent = (float)mediaOptions.CustomPhotoSize / 100f;
                                break;
                        }

                        if (mediaOptions.PhotoSize == PhotoSize.MaxWidthHeight && mediaOptions.MaxWidthHeight.HasValue)
                        {
                            var max = Math.Max(originalImage.Width, originalImage.Height);
                            if (max > mediaOptions.MaxWidthHeight)
                            {
                                percent = (float)mediaOptions.MaxWidthHeight / (float)max;
                            }
                        }

                        var finalWidth = (int)(originalImage.Width * percent);
                        var finalHeight = (int)(originalImage.Height * percent);
                        
                        if (originalImage == null)
                            return false;

                        if (finalWidth != originalImage.Width || finalHeight != originalImage.Height)
                        {
                            originalImage = Bitmap.CreateScaledBitmap(originalImage, finalWidth, finalHeight, true);
                        }

                        //if (rotation % 180 == 90)
                        //{
                        //    var a = finalWidth;
                        //    finalWidth = finalHeight;
                        //    finalHeight = a;
                        //}

                        //set scaled and rotated image dimensions
                        //exif?.SetAttribute(pixelXDimens, Java.Lang.Integer.ToString(finalWidth));
                        //exif?.SetAttribute(pixelYDimens, Java.Lang.Integer.ToString(finalHeight));

                        //if we need to rotate then go for it.
                        //then compresse it if needed
                        //var photoType = System.IO.Path.GetExtension(filePath)?.ToLower();
                        //var compressFormat = photoType == ".png" ? Bitmap.CompressFormat.Png : Bitmap.CompressFormat.Jpeg;
                        var compressFormat = Bitmap.CompressFormat.Jpeg;
                        if (rotation != 0)
                        {
                            var matrix = new Matrix();
                            matrix.PostRotate(rotation);
                            using (var rotatedImage = Bitmap.CreateBitmap(originalImage, 0, 0, originalImage.Width, originalImage.Height, matrix, true))
                            {
                                using FileStream stream = new FileStream(outputPath, FileMode.Create);
                                rotatedImage.Compress(compressFormat, mediaOptions.CompressionQuality, stream);
                                rotatedImage.Recycle();
                            }
                            //change the orienation to "not rotated"
                            //exif?.SetAttribute(ExifInterface.TagOrientation, Java.Lang.Integer.ToString((int)Orientation.Normal));

                        }
                        else
                        {
                            //always need to compress to save back to disk
                            using FileStream stream = new FileStream(outputPath, FileMode.Create);
                            originalImage.Compress(compressFormat, mediaOptions.CompressionQuality, stream);
                        }

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
    }
}
