using Android.Views;
using AndroidX.Camera.Core;

namespace TestLog.Droid.Camera2
{
    public class FocusTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly MeteringPointFactory _meteringPointFactory;
        private readonly ICameraControl _cameraControl;

        public FocusTouchListener(MeteringPointFactory meteringPointFactory, ICameraControl cameraControl)
        {
            _meteringPointFactory = meteringPointFactory;
            _cameraControl = cameraControl;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            // Convert UI coordinates into camera sensor coordinates
            var point = _meteringPointFactory.CreatePoint(e.GetX(), e.GetY());

            // Prepare focus action to be triggered
            var action = new FocusMeteringAction.Builder(point).SetAutoCancelDuration(5, Java.Util.Concurrent.TimeUnit.Seconds).Build();

            // Execute focus action
            _cameraControl.StartFocusAndMetering(action);

            return false;
        }
    }
}
