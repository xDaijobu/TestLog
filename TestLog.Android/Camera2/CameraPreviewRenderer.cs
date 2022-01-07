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
using Android.Graphics.Drawables;
using Android.Views;
using Java.Lang;
using Java.Nio;
using Java.Util;
using Java.Util.Concurrent;
using TestLog.Camera;
using TestLog.Camera2;
using TestLog.Droid.Camera2;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidX.Lifecycle;
using Android.Renderscripts;
using static AndroidX.Camera.Core.ImageCapture;
using Math = Java.Lang.Math;
using Exception = System.Exception;
using File = Java.IO.File;
using Environment = Android.OS.Environment;

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

            //previewView.SetImplementationMode(PreviewView.ImplementationMode.Performance);
            //var textView = new TextView(MainActivity.Instance);
            //textView.Text = "hayyyy HAYY HELLLLOO OWORLDD";
            //previewView.AddView(textView);
            //textView.SetHeight(200);
            //textView.SetWidth(500);
            //previewView.Overlay.Add(convertViewToBitmap(textView));

            cameraExecutor = Executors.NewSingleThreadExecutor();
            Connect();


            // Callback for preview visibility
            previewObserver = new PreviewObserver(camera, previewView);
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
        }

        public async Task<bool> CheckPermissions()
        {
            await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageRead>();
            await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.StorageWrite>();
            return (await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.Camera>()) == Xamarin.Essentials.PermissionStatus.Granted;
        }

        public Drawable convertViewToBitmap(Android.Views.View view)
        {
            view.Measure(
                MeasureSpec.MakeMeasureSpec(0, Android.Views.MeasureSpecMode.Unspecified),
                MeasureSpec.MakeMeasureSpec(0, Android.Views.MeasureSpecMode.Unspecified)
            );
            view.Layout(0, 0, view.MeasuredWidth, view.MeasuredHeight);
            view.BuildDrawingCache();
            Bitmap bitmap = view.DrawingCache;
            Drawable d = new BitmapDrawable(_context.Resources, bitmap);
            return d;
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

                    _currentElement.CameraClick = new Command(() =>
                    {
                        TakePicture();
                    });
                }
                catch(Exception ex)
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

        private void TakePicture()
        {
            //var bitmap = previewView.Bitmap;
            //System.Diagnostics.Debug.WriteLine("" +
            //    "Camera Clicked\n" +
            //    $"Height: {bitmap.Height}\n" +
            //    $"Width: {bitmap.Width}");

            //System.Diagnostics.Debug.WriteLine("export bitmap as png.");
            //ExportBitmapAsPNG(bitmap);

            // Get a stable reference of the modifiable image capture use case   
            var imageCapture = this.imageCapture;
            if (imageCapture == null)
                return;

            // Create time-stamped output file to hold the image
            var photoFile = new File(outputDirectory, new Java.Text.SimpleDateFormat(FILENAME_FORMAT, Locale.Us).Format(JavaSystem.CurrentTimeMillis()) + ".jpg");

            // Create output options object which contains file + metadata
            var outputOptions = new OutputFileOptions.Builder(photoFile).Build();

            // Set up image capture listener, which is triggered after photo has been taken
            imageCapture.TakePicture(outputOptions, ContextCompat.GetMainExecutor(_context), new ImageSaveCallback(

                onErrorCallback: (exc) =>
                {
                    var msg = $"Photo capture failed: {exc.Message}";
                    Log.Error(TAG, msg, exc);
                    Toast.MakeText(_context, msg, ToastLength.Short).Show();
                },

                onImageSaveCallback: (output) =>
                {
                    var data = System.IO.File.ReadAllBytes(photoFile?.Path);
                    _currentElement?.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: data, path: photoFile?.Path));
                }
            ));
        }

        // Save photos to => /Pictures/CameraX/
        [Obsolete]
        private File GetOutputDirectory()
        {
            //var mediaDir = GetExternalMediaDirs().FirstOrDefault();  
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
            
            base.Dispose(disposing);
        }

        private static byte[] GetByteArrayFromByteBuffer(ByteBuffer byteBuffer)
        {
            byte[] bytesArray = new byte[byteBuffer.Remaining()];
            byteBuffer.Get(bytesArray, 0, bytesArray.Length);
            return bytesArray;
        }

        private Bitmap BlurRenderScript(Bitmap smallBitmap, int radius)
        {
            float defaultBitmapScale = 0.1f;

            int width = Math.Round(smallBitmap.Width * defaultBitmapScale);
            int height = Math.Round(smallBitmap.Height * defaultBitmapScale);

            Bitmap inputBitmap = Bitmap.CreateScaledBitmap(smallBitmap, width, height, false);
            Bitmap outputBitmap = Bitmap.CreateBitmap(inputBitmap);

            RenderScript renderScript = RenderScript.Create(_context);
            ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(renderScript, Android.Renderscripts.Element.U8_4(renderScript));
            Allocation tmpIn = Allocation.CreateFromBitmap(renderScript, inputBitmap);
            Allocation tmpOut = Allocation.CreateFromBitmap(renderScript, outputBitmap);
            theIntrinsic.SetRadius(radius);
            theIntrinsic.SetInput(tmpIn);
            theIntrinsic.ForEach(tmpOut);
            tmpOut.CopyTo(outputBitmap);

            return outputBitmap;
        }
    }
}
