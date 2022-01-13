using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            cameraPreview.MediaOptions = new Camera2.MediaOptions()
            {
                //SaveToAlbum =
                CompressionQuality = 50, /*SATO SAMA HRTO PAKAINYA 50*/
                PhotoSize = Camera2.PhotoSize.Small,
                Directory = "/storage/emulated/0/Android/data/testing.testlog/files/Pictures/TestLog",
                MaxWidthHeight = 2000,
                Placemark = new Camera2.Placemark()
                {
                    Location = new Camera2.Location()
                    {
                        Latitude = -6.1649888,
                        Longitude = 106.9142543,
                    },
                    SubLocality = "Pegangsaan Dua",
                    Locality = "Kecamatan Kelapa Gading",
                    SubAdminArea = "Kota Jakarta Utara",
                    AdminArea = "Daerah Khusus Ibukota Jakarta",
                    PostalCode = "14250",
                    CountryName = "Indonesia",
                }
            };


            //await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            //var location = await Geolocation.GetLocationAsync();

            //var placemarks = await Geocoding.GetPlacemarksAsync(location);

            //var placemark = placemarks.FirstOrDefault();
            //Debug.WriteLine(placemark.ToString());
            /*
                Contoh isi dari Placemark
                Location: Latitude: -6.1649888,
                Longitude: 106.9142543,
                Altitude: , Accuracy: ,
                VerticalAccuracy: ,
                Speed: ,
                Course: ,
                Timestamp: 1 / 12 / 2022 8:48:09 AM + 00:00,
                CountryCode: ID, C
                ountryName: Indonesia,
                FeatureName: 5,
                PostalCode: 14250,
                SubLocality: Pegangsaan Dua,
                Thoroughfare: ,
                SubThoroughfare: ,
                Locality: Kecamatan Kelapa Gading,
                AdminArea: Daerah Khusus Ibukota Jakarta,
                SubAdminArea: Kota Jakarta Utara
            */

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


