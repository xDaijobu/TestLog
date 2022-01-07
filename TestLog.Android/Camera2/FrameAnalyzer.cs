using System;
using Android.Util;
using AndroidX.Camera.Core;
using Java.Nio;

namespace TestLog.Droid.Camera2
{
	internal class FrameAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
	{
		readonly Action<ByteBuffer, Size> frameCallback;

		public FrameAnalyzer(Action<ByteBuffer, Size> callback)
		{
			frameCallback = callback;
		}

		public void Analyze(IImageProxy image)
		{
			var buffer = image.GetPlanes()[0].Buffer;

			var s = new Size(image.Width, image.Height);

			frameCallback?.Invoke(buffer, s);

			image.Close();
		}
	}
}
