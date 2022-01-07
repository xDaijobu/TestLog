using System;
namespace TestLog.Camera2
{
	public class CameraFrameBufferEventArgs : EventArgs
	{
		public CameraFrameBufferEventArgs(PixelBufferHolder pixelBufferHolder) : base()
			=> Data = pixelBufferHolder;

		public readonly PixelBufferHolder Data;
	}
}
