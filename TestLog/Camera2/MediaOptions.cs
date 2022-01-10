using System;
namespace TestLog.Camera2
{
    public class MediaOptions
    {
        public string Directory { get; set; }
        public string Name { get; set; }

        public int? MaxWidthHeight { get; set; }

        /// <summary>
        // Get or set if the image should be stored public
        /// </summary>
        public bool SaveToAlbum { get; set; }

        /// <summary>
        /// Gets or sets the size of the photo.
        /// </summary>
        /// <value>The size of the photo.</value>
        public PhotoSize PhotoSize { get; set; } = PhotoSize.Full;

        int customPhotoSize = 100;
        /// <summary>
        /// The custom photo size to use, 100 full size (same as Full),
        /// and 1 being smallest size at 1% of original
        /// Default is 100
        /// </summary>
        public int CustomPhotoSize
        {
            get { return customPhotoSize; }
            set
            {
                if (value > 100)
                    customPhotoSize = 100;
                else if (value < 1)
                    customPhotoSize = 1;
                else
                    customPhotoSize = value;
            }
        }

        int quality = 100;
        /// <summary>
        /// The compression quality to use, 0 is the maximum compression (worse quality),
        /// and 100 minimum compression (best quality)
        /// Default is 100
        /// </summary>
        public int CompressionQuality
        {
            get { return quality; }
            set
            {
                if (value > 100)
                    quality = 100;
                else if (value < 0)
                    quality = 0;
                else
                    quality = value;
            }
        }

        /// <summary>
        /// Store provided location
        /// </summary>
        public Location Location { get; set; }

        bool rotateImage = true;
        /// <summary>
        /// Should the library rotate image according to received exif orientation.
        /// Set to true by default.
        /// </summary>
        public bool RotateImage
        {
            get { return rotateImage; }
            set { rotateImage = value; }
        }

        bool saveMetaData = true;
        /// <summary>
        /// Saves metadate/exif data from the original file.
        /// </summary>
        public bool SaveMetaData
        {
            get { return saveMetaData; }
            set { saveMetaData = value; }
        }
    }

    /// <summary>
    /// Photo size enum.
    /// </summary>
    public enum PhotoSize
    {
        /// <summary>
        /// 25% of original
        /// </summary>
        Small,
        /// <summary>
        /// 50% of the original
        /// </summary>
        Medium,
        /// <summary>
        /// 75% of the original
        /// </summary>
        Large,
        /// <summary>
        /// Untouched
        /// </summary>
        Full,
        /// <summary>
        /// Custom size between 1-100
        /// Must set the CustomPhotoSize value
        /// Only applies to iOS and Android
        /// Windows will auto configure back to small, medium, large, and full
        /// </summary>
        Custom,
        /// <summary>
        /// Use the Max Width or Height photo size.
        /// The property ManualSize must be set to a value. The MaxWidthHeight will be the max width or height of the image
        /// Currently this works on iOS and Android only.
        /// On Windows the PhotoSize will fall back to Full
        /// </summary>
        MaxWidthHeight
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double HorizontalAccuracy { get; set; }
        public double Speed { get; set; }
        public double Direction { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
