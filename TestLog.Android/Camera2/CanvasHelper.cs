using Android.Graphics;
using TestLog.Camera2;

namespace TestLog.Droid.Camera2
{
    public class CanvasHelper
    {
        public static void DrawPlacemarkView(Canvas canvas, float width, float height, Placemark placemark)
        {
            //contoh datanya
            //SubLocality = "Pegangsaan Dua",
            //Locality = "Kecamatan Kelapa Gading",
            //SubAdminArea = "Kota Jakarta Utara",
            //AdminArea = "Daerah Khusus Ibukota Jakarta",
            //PostalCode = "14250",
            //CountryName = "Indonesia",

            if (placemark is null)
                return;

            string text = $"Latitude: {placemark.Location?.Latitude}\n" +
                          $"Longitude: {placemark.Location?.Longitude}\n" +
                          $"{placemark.SubLocality}\n" +
                          $"{placemark.Locality}\n" +
                          $"{placemark.SubAdminArea}\n" +
                          $"{placemark.AdminArea}\n" +
                          $"{placemark.PostalCode}\n" +
                          $"{placemark.CountryName}";

            float y = height / 2;
            Paint paint = new Paint
            {
                Color = Color.White,
                AntiAlias = true,
                TextAlign = Paint.Align.Right
            };
            //paint.SetTypeface(Typeface.DefaultBold);

            // set text size for width
            float testTextSize = 45f;

            // Get the bounds of the text, using our testTextSize.  
            paint.TextSize = testTextSize;
            Rect bounds = new Rect();
            paint.GetTextBounds(text, 0, text.Length, bounds);

            // Calculate the desired size as a proportion of our testTextSize.
            float desiredTextSize = (float)(testTextSize * width / (bounds.Width() * 0.75));

            // Set the paint for that size.
            //System.Diagnostics.Debug.WriteLine("Desired Text Size: " + desiredTextSize);
            paint.TextSize = desiredTextSize;
            //System.Diagnostics.Debug.WriteLine("first y: " + y);
            foreach (var line in text.Split("\n"))
            {
                canvas.DrawText(line, (float)(width * 0.99), y, paint);
                y += paint.Descent() - paint.Ascent();

                //System.Diagnostics.Debug.WriteLine("Paint.Descent: " + paint.Descent());
                //System.Diagnostics.Debug.WriteLine("Paint.Ascent:" + paint.Ascent());
                //System.Diagnostics.Debug.WriteLine("y: " + y);
            }
        }
    }
}
