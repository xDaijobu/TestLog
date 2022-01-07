using Android.Views;

namespace TestLog.Droid.Camera2
{
    public class ScaleTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly ScaleGestureDetector _scaleGestureDetector;
        public ScaleTouchListener(ScaleGestureDetector scaleGestureDetector)
        {
            _scaleGestureDetector = scaleGestureDetector;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _scaleGestureDetector.OnTouchEvent(e);

            return false;
        }
    }
}
