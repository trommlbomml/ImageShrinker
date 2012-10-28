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
            string extension = Path.GetExtension(fileName);
            string baseName = Path.GetFileNameWithoutExtension(fileName);

            int index = 0;
            string newName = Path.GetFileName(fileName);
            while (UniqueUpperFileNames.Contains(newName.ToUpper()))
                newName = string.Format("{0}({1}){2}", baseName, ++index, extension);

            UniqueUpperFileNames.Add(newName.ToUpper());
            return newName;
        }

        public static void SaveScaled(ImageViewModel imageViewModel, string path)
        {
            string pathToSave = Path.Combine(path, imageViewModel.Name);
            using (FileStream fs = new FileStream(pathToSave, FileMode.Create))
            {
                Save(imageViewModel, fs);
            }

            if ((int) imageViewModel.Rotation%360 == 0) return;
            Image img = Image.FromFile(pathToSave);
            img.RotateFlip(GetRotationType(imageViewModel.Rotation));
            img.Save(pathToSave, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static double GetFileSizeScaledInMegaByte(ImageViewModel imageViewModel)
        {
            MemoryStream ms = new MemoryStream();
            Save(imageViewModel, ms);
            return (double)ms.Length / (1024 * 1024);
        }

        private static void Save(ImageViewModel imageViewModel, Stream stream)
        {
            ImageShrinkerViewModel imageShrinkerViewModel = imageViewModel.Parent;
            Bitmap original = new Bitmap(imageViewModel.Path);

            double factor = 1.0;
            if (imageShrinkerViewModel.DesiredWidth < imageViewModel.Width)
                factor = imageShrinkerViewModel.DesiredWidth / (double)imageViewModel.Width;
            if (imageShrinkerViewModel.DesiredHeight < imageViewModel.Height)
                factor = imageShrinkerViewModel.DesiredHeight / (double)imageViewModel.Height;
            
            Bitmap scaled = new Bitmap((int)(original.Width * factor), (int)(original.Height * factor));
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
            DrawImaging.ImageCodecInfo[] codecs = DrawImaging.ImageCodecInfo.GetImageEncoders();

            foreach (DrawImaging.ImageCodecInfo codec in codecs)
                if (codec.MimeType == mimeType)
                    return codec;

            throw new InvalidOperationException("There is no jpeg encoder");
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
            throw new ArgumentException("No valid image format was taken.", "url");
        }
    }
}
