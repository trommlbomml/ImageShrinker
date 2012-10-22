using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using ImageShrinker2.ViewModels;

namespace ImageShrinker2.Model
{
    static class ImageModel
    {
        public static ImageViewModel CreateFromFile(string fileName)
        {
            int width, height;
            GetImageData(fileName, out width, out height);
            return new ImageViewModel
                       {
                           Height = height,
                           Width = width,
                           Name = Path.GetFileName(fileName),
                           Path = fileName
                       };
        }

        public static void SaveScaled(ImageViewModel imageViewModel, string path, int width, int height, long quality)
        {
            string pathToSave = Path.Combine(path, imageViewModel.Name);

            Bitmap original = new Bitmap(imageViewModel.Path);

            double factor = 1.0f;
            if (width < imageViewModel.Width)
                factor = width/(double)imageViewModel.Width;
            if (height < imageViewModel.Height)
                factor = height/(double) imageViewModel.Height;

            Bitmap scaled = new Bitmap((int) (original.Width * factor), (int)(original.Height * factor));
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.DrawImage(original, new Rectangle(0, 0, scaled.Width, scaled.Height));
                using (FileStream fs = new FileStream(pathToSave, FileMode.Create))
                {
                    Save(fs, scaled, quality);
                }
            }

            if ((int)imageViewModel.Rotation % 360 != 0)
            {
                Image img = Image.FromFile(pathToSave);
                img.RotateFlip(GetRotationType(imageViewModel.Rotation));
                img.Save(pathToSave, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private static void Save(Stream stream, Bitmap img, long quality)
        {
            System.Drawing.Imaging.EncoderParameter qualityParam =
                new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            System.Drawing.Imaging.ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(stream, jpegCodec, encoderParams);
        }

        private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        private static RotateFlipType GetRotationType(double rotation)
        {
            int rotationInt = (int)rotation;
            while (Math.Abs(rotationInt) > 360)
            {
                rotationInt += rotationInt < 0 ? -360 : 360;
            }
            switch (rotationInt)
            {
                case 0: return RotateFlipType.RotateNoneFlipNone;
                case 360: return RotateFlipType.RotateNoneFlipNone;
                case 90: return RotateFlipType.Rotate90FlipNone;
                case 180: return RotateFlipType.Rotate180FlipNone;
                case 270: return RotateFlipType.Rotate270FlipNone;
            }
            return RotateFlipType.RotateNoneFlipNone;
        }

        private static void GetImageData(string url, out int width, out int height)
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

            throw new ArgumentException(@"No valid image format was taken.", "url");
        }
    }
}
