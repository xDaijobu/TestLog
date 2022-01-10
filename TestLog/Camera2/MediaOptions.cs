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
    }

    public class Placemark
    {
        /// <summary>Gets or sets the location of the placemark.</summary>
        /// <value>The location of the placemark.</value>
        /// <remarks />
        public Location Location
        {
            get;
            set;
        }

        /// <summary>Gets or sets the country ISO code.</summary>
        /// <value>The country ISO code.</value>
        /// <remarks />
        public string CountryCode
        {
            get;
            set;
        }

        /// <summary>Gets or sets the country name.</summary>
        /// <value>The country name.</value>
        /// <remarks />
        public string CountryName
        {
            get;
            set;
        }

        /// <summary>Gets or sets the feature name.</summary>
        /// <value>The feature name.</value>
        /// <remarks />
        public string FeatureName
        {
            get;
            set;
        }

        /// <summary>Gets or sets the postal code.</summary>
        /// <value>The postal code.</value>
        /// <remarks />
        public string PostalCode
        {
            get;
            set;
        }

        /// <summary>Gets or sets the sub locality.</summary>
        /// <value>The sub locality.</value>
        /// <remarks />
        public string SubLocality
        {
            get;
            set;
        }

        /// <summary>Gets or sets the street name.</summary>
        /// <value>The street name.</value>
        /// <remarks />
        public string Thoroughfare
        {
            get;
            set;
        }

        /// <summary>Gets or sets optional info: sub street or region.</summary>
        /// <value>The sub thoroughfare.</value>
        /// <remarks />
        public string SubThoroughfare
        {
            get;
            set;
        }

        /// <summary>Gets or sets the city or town.</summary>
        /// <value>The city or town of the locality.</value>
        /// <remarks />
        public string Locality
        {
            get;
            set;
        }

        /// <summary>Gets or sets the administrative area name of the address, for example, "CA", or null if it is unknown.</summary>
        /// <value>The admin area.</value>
        /// <remarks />
        public string AdminArea
        {
            get;
            set;
        }

        /// <summary>Gets or sets the sub-administrative area name of the address, for example, "Santa Clara County", or null if it is unknown.</summary>
        /// <value>The sub-admin area.</value>
        /// <remarks />
        public string SubAdminArea
        {
            get;
            set;
        }
    }
    }
