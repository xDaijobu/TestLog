﻿using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using TestLog.Camera2;

namespace TestLog.Droid.Camera2
{
    public class DrawView : View
    {
        private int _height, _width;
        private Placemark _placemark;
        public DrawView(Context context, int height, int width, Placemark placemark) : base(context)
        {
            _height = height;
            _width = width;
            _placemark = placemark;
        }
        public DrawView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }
        public DrawView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            //canvas.Save();
            //canvas.Translate(_height, _width);

            string text = $"Latitude: {_placemark?.Location?.Latitude}\n" +
                          $"Longitude: {_placemark?.Location?.Longitude}\n" +
                          $"{_placemark?.CountryName}";
            float x = Width / 2, y = Height / 2;
            Paint paint = new Paint();
            paint.Color = Color.White;
            paint.AntiAlias = true;
            paint.TextAlign = Paint.Align.Right;
            //paint.SetTypeface(Typeface.DefaultBold);

            //paint.TextSize = 25;
            // set text size for width
            float testTextSize = 48f;
            // Get the bounds of the text, using our testTextSize.  
            paint.TextSize = testTextSize;
            Rect bounds = new Rect();
            paint.GetTextBounds(text, 0, text.Length, bounds);

            // Calculate the desired size as a proportion of our testTextSize.
            float desiredTextSize = testTextSize * _width / bounds.Width();

            // Set the paint for that size.
            System.Diagnostics.Debug.WriteLine("Desired Text Size: " + desiredTextSize);
            paint.TextSize = desiredTextSize;

            foreach (var line in text.Split("\n"))
            {
                canvas.DrawText(line, (float)(Width * 0.99), y, paint);
                y += paint.Descent() - paint.Ascent();
            }
        }
    }
}