using System.IO;
using ImageShrinker.ImageFactory;
using System.Drawing;
using System;

namespace ImageShrinker.ViewModels
{
    public class ImageThumbViewModel : ViewModel
    {
        private readonly ImageShrinkerViewModel _imageShrinkerViewModel;

        public ImageThumbViewModel(string url, ImageShrinkerViewModel imageShrinkerViewModel)
        {
            _imageShrinkerViewModel = imageShrinkerViewModel;

            AbsolutePath = url;
            FileName = Path.GetFileName(url);

            int width, height;
            ImageDataFactory.GetImageData(url, out width, out height);
            Width = width;
            Height = height;

            OriginalFileSize = (new FileInfo(url)).Length / (1024.0 * 1024.0);

            IsSelected = true;
        }

        public string AbsolutePath {get;set;}

        private string _filename;
        public string FileName
        {
            get { return _filename;}
            set
            {
                _filename = value;
                NotifyPropertyChanged("FileName");
            }
        }

        private double _rotation;
        public double Rotation
        {
            get { return _rotation; }
            set { _rotation = value; NotifyPropertyChanged("Rotation"); }
        }

        public void ChangeFileName(string nameWithOutExtension)
        {
            FileName = nameWithOutExtension + ".jpg";
            AbsolutePath = Path.Combine(Path.GetDirectoryName(AbsolutePath), FileName);
        }

        private int _width;
        public int Width 
        {
            get { return _width; }
            set { _width = value; NotifyPropertyChanged("Width"); }
        }
        private int _height;
        public int Height
        {
            get { return _height; }
            set { _height = value; NotifyPropertyChanged("Height"); }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value; 
                NotifyPropertyChanged("IsSelected");
                _imageShrinkerViewModel.NotifySelectedImageCountChanged();
            }
        }

        public double OriginalFileSize
        {
            get;
            set;
        }

        public void SaveScaled(int width, string path, long quality)
        {
            string pathToSave = Path.Combine(path, FileName);

            Bitmap original = new Bitmap(AbsolutePath);
            double factor = Width > width ? width / (double)Width : 1.0;

            Bitmap scaled = new Bitmap((int)(original.Width * factor), (int)(original.Height * factor));
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.DrawImage(original, new Rectangle(0, 0, scaled.Width, scaled.Height));

                using (FileStream fs = new FileStream(pathToSave, FileMode.Create))
                {
                    Save(fs, scaled, quality);
                }
            }

            if ((int)Rotation % 360 != 0)
            {
                Image img = Image.FromFile(pathToSave);
                img.RotateFlip(GetRotationType());
                img.Save(pathToSave, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private RotateFlipType GetRotationType()
        {
            int rotation = (int)Rotation;
            while (Math.Abs(rotation) > 360)
            {
                rotation += rotation < 0 ? -360 : 360;
            }

            switch (rotation)
            {
                case 0: return RotateFlipType.RotateNoneFlipNone;
                case 360: return RotateFlipType.RotateNoneFlipNone;
                case 90: return RotateFlipType.Rotate90FlipNone;
                case 180: return RotateFlipType.Rotate180FlipNone;
                case 270: return RotateFlipType.Rotate270FlipNone;
            }

            return RotateFlipType.Rotate270FlipNone;
        }

        public double GetFileSizeScaled(int width,long quality)
        {
            MemoryStream ms = new MemoryStream();
            Bitmap original = new Bitmap(AbsolutePath);

            double factor = Width > width ? width / (double)Width : 1.0;
            Bitmap scaled = new Bitmap((int)(original.Width * factor), (int)(original.Height * factor));
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.DrawImage(original, new Rectangle(0, 0, scaled.Width, scaled.Height));
                Save(ms, scaled, quality);
            }

            return (double)ms.Length / (1024 * 1024);
        }

        private void Save(Stream stream, Bitmap img, long quality)
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

        private System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }
    }
}
