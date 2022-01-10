using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestLog
{
    public partial class MainPage : ContentPage
    {
        
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            cameraPreview.MediaCaptured += (s, args) =>
            {
                Debug.WriteLine("Data: " + args.ImageData);
                Debug.WriteLine("Path: " + args.Path);

                Device.BeginInvokeOnMainThread(() =>
                {
                    image.Source = ImageSource.FromFile(args.Path);
                    SetEnabledButtons(true);
                });
            };
            cameraPreview.MediaOptions = new Camera2.MediaOptions()
            {
                //SaveToAlbum =
                CompressionQuality = 50,
                PhotoSize = Camera2.PhotoSize.Small,
                MaxWidthHeight = 2000,
            };

            cameraPreview.Placemark = new Camera2.Placemark()
            {
                Location = new Camera2.Location()
                {
                    Latitude = 1.1231231,
                    Longitude = 108.555555,
                },
                CountryName = "Indonesia",
                SubAdminArea = "Testing",
            };
        }

        protected override bool OnBackButtonPressed()
            => true;

        void Button_Clicked(object sender, EventArgs e)
        {
            SetEnabledButtons(false);
            cameraPreview.CameraClick.Execute(null);
        }

        void SetEnabledButtons(bool isEnabled)
        {
            btnTakePhoto.IsEnabled = isEnabled;
            btnFlash.IsEnabled = isEnabled;
            btnClose.IsEnabled = isEnabled;
            btnSwitchCam.IsEnabled = isEnabled;            
        }

        async void Button_Clicked_1(object sender, EventArgs e)
        {
            SetEnabledButtons(false);
            if (cameraPreview.CameraOptions == Camera2.CameraOptions.Rear)
                cameraPreview.CameraOptions = Camera2.CameraOptions.Front;
            else
                cameraPreview.CameraOptions = Camera2.CameraOptions.Rear;

            await Task.Delay(1000);
            SetEnabledButtons(true);
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

        void Button_Clicked_3(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}


