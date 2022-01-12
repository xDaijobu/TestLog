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

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                if (image.Source is null)
                    return;

                await Navigation.PushAsync(new ImageView(image.Source));
            };
            image.GestureRecognizers.Add(tapGestureRecognizer);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            cameraPreview.MediaOptions = new Camera2.MediaOptions()
            {
                //SaveToAlbum =
                CompressionQuality = 50, /*SATO SAMA HRTO PAKAINYA 50*/
                PhotoSize = Camera2.PhotoSize.Small,
                MaxWidthHeight = 2000,
                Placemark = new Camera2.Placemark()
                {
                    Location = new Camera2.Location()
                    {
                        Latitude = 1.1231231,
                        Longitude = 108.555555,
                    },
                    CountryName = "Indonesia",
                    SubAdminArea = "Testing",
                }
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

            cameraPreview.CameraOptions = cameraPreview.CameraOptions == Camera2.CameraOptions.Rear ? Camera2.CameraOptions.Front : Camera2.CameraOptions.Rear;

            btnSwitchCam.Text = cameraPreview.CameraOptions.ToString();
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

        void cameraPreview_MediaCaptured(object sender, Camera2.MediaCapturedEventArgs e)
        {
            Debug.WriteLine("Data: " + e.ImageData.Length);
            Debug.WriteLine("Path: " + e.Path);

            Device.BeginInvokeOnMainThread(() =>
            {
                image.Source = e.Image;
                //image.Source = ImageSource.FromFile(e.Path);

                SetEnabledButtons(true);
            });
        }
    }
}


