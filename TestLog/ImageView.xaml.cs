using Xamarin.Forms;

namespace TestLog
{
    public partial class ImageView : ContentPage
    {
        public ImageView(ImageSource imageSource)
        {
            InitializeComponent();

            imgView.Source = imageSource;
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
        }
    }
}
