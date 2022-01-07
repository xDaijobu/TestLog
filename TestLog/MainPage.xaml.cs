using System;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace TestLog
{
    public partial class MainPage : ContentPage
    {
        
        public MainPage()
        {
            InitializeComponent();

            //ToLogger.Warn("TEST", "asd");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            
            cameraPreview.MediaCaptured += (s, args) =>
            {
                Debug.WriteLine("Data: " + args.ImageData);
                Debug.WriteLine("Path: " + args.Path);

                image.Source = ImageSource.FromFile(args.Path);
            };
        }

        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            cameraPreview.CameraClick.Execute(null);
        }

        async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (cameraPreview.CameraOptions == Camera2.CameraOptions.Rear)
                cameraPreview.CameraOptions = Camera2.CameraOptions.Front;
            else
                cameraPreview.CameraOptions = Camera2.CameraOptions.Rear;
        }

        void Button_Clicked_2(object sender, EventArgs e)
        {
            if (cameraPreview.FlashMode == Camera2.FlashMode.Off)
            {
                btnFlash.Text = "Flash On";
                cameraPreview.FlashMode = Camera2.FlashMode.On;
            }
            else
            {
                btnFlash.Text = "Flash Off";
                cameraPreview.FlashMode = Camera2.FlashMode.Off;
            }   
        }

        void Button_Clicked_3(System.Object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}


