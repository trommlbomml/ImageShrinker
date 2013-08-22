using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using ImageShrinker2.ViewModels;
using DrawImaging = System.Drawing.Imaging;

namespace ImageShrinker2.Model
{
    static class ImageModel
    {
        private static readonly HashSet<string> UniqueUpperFileNames = new HashSet<string>(); 

        private static string GetUnifyNameFormat(int imageCount)
        {
            if (imageCount > 9999) return "{0:00000}.jpg";
            if (imageCount > 999) return "{0:0000}.jpg";
            if (imageCount > 99) return "{0:000}.jpg";
            if (imageCount > 9) return "{0:00}.jpg";
            return "{0}.jpg";
        }

        public static void UnifyImageNames(ImageShrinkerViewModel imageShrinkerViewModel, string baseName)
        {
            var format = baseName + " " + GetUnifyNameFormat(imageShrinkerViewModel.Images.Count);
            var baseIndex = 0;
            foreach (var imageViewModel in imageShrinkerViewModel.Images)
            {
                imageViewModel.Name = string.Format(format, baseIndex++);
            }
        }

        public static ImageViewModel CreateFromFile(string fileName)
        {
            int width, height;
            GetImageData(fileName, out width, out height);
            return new ImageViewModel
                       {
                           Height = height,
                           Width = width,
                           Name = GetUniqueFileName(fileName),
                           Path = fileName,
                           IsSelected = true,
                       };
        }

        private static string GetUniqueFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var newName = Path.GetFileName(fileName);
            var index = 0;

            while (UniqueUpperFileNames.Contains(newName.ToUpper())) newName = string.Format("{0}({1}){2}", baseName, ++index, extension);
            UniqueUpperFileNames.Add(newName.ToUpper());
            return newName;
        }

        public static void SaveScaled(ImageViewModel imageViewModel, string path)
        {
            string pathToSave = Path.Combine(path, imageViewModel.Name);
            using (var fs = new FileStream(pathToSave, FileMode.Create))
            {
                Save(imageViewModel, fs);
            }

            if ((int) imageViewModel.Rotation%360 == 0) return;
            var img = Image.FromFile(pathToSave);
            img.RotateFlip(GetRotationType(imageViewModel.Rotation));
            img.Save(pathToSave, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static double GetFileSizeScaledInMegaByte(ImageViewModel imageViewModel)
        {
            var ms = new MemoryStream();
            Save(imageViewModel, ms);
            return (double)ms.Length / (1024 * 1024);
        }

        private static void Save(ImageViewModel imageViewModel, Stream stream)
        {
            var imageShrinkerViewModel = imageViewModel.Parent;
            var original = new Bitmap(imageViewModel.Path);

            var factor = 1.0;
            if (imageShrinkerViewModel.DesiredWidth < imageViewModel.Width)
                factor = imageShrinkerViewModel.DesiredWidth / (double)imageViewModel.Width;
            if (imageShrinkerViewModel.DesiredHeight < imageViewModel.Height)
                factor = imageShrinkerViewModel.DesiredHeight / (double)imageViewModel.Height;
            
            var scaled = new Bitmap((int)(original.Width * factor), (int)(original.Height * factor));
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.DrawImage(original, new Rectangle(0, 0, scaled.Width, scaled.Height));
                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParams.Param[0] = new DrawImaging.EncoderParameter(DrawImaging.Encoder.Quality, imageShrinkerViewModel.Quality);
                scaled.Save(stream, GetEncoderInfo("image/jpeg"), encoderParams);
            }
        }

        private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var codecs = DrawImaging.ImageCodecInfo.GetImageEncoders();

            foreach (var codec in codecs)
            {
                if (codec.MimeType == mimeType) return codec;
            }

            throw new InvalidOperationException("There is no jpeg encoder");
        }

        private static RotateFlipType GetRotationType(double rotation)
        {
            var rotationInt = (int)rotation;
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
            var decoder = GetSuitableBitmapDecoder(url);
            width = decoder.Frames[0].PixelWidth;
            height = decoder.Frames[0].PixelHeight;
        }

        private static BitmapDecoder GetSuitableBitmapDecoder(string url)
        {
            var extension = Path.GetExtension(url);
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
            throw new ArgumentException("No valid image format was taken.", "url");
        }
    }
}
