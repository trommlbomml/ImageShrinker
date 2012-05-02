using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageShrinker.ImageFactory
{
    class ImageDataFactory
    {
        public static void GetImageData(string url,out int width,out int height)
        {
            BitmapDecoder decoder = GetSuitableBitmapDecoder(url);
            width = decoder.Frames[0].PixelWidth;
            height = decoder.Frames[0].PixelHeight;
        }

        private static BitmapDecoder GetSuitableBitmapDecoder(string url)
        {
            string extension = Path.GetExtension(url);
            if (!string.IsNullOrEmpty(extension))
            {
                switch (extension.ToUpper())
                {
                    case ".JPEG":
                    case ".JPG":
                        return new JpegBitmapDecoder(new Uri(url), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    case ".BMP":
                        return new BmpBitmapDecoder(new Uri(url), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    case ".PNG":
                        return new PngBitmapDecoder(new Uri(url), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                }    
            }

            throw new ArgumentException(@"No valid image format was taken.","url");
        }
    }
}
