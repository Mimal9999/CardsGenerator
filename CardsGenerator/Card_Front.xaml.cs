using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardsGenerator
{
    /// <summary>
    /// Interaction logic for Card_Front.xaml
    /// </summary>
    public partial class Card_Front : UserControl
    {
        public GradientBrush BackgroundGradient { get; set; }
        public SongData Song { get; set; }
        public Card_Front(SongData song)
        {
            InitializeComponent();
            Song = song;
            DataContext = Song;
            QRCodeImage.Source = BitmapToImageSource(song.QRCode);
        }       
        private ImageSource BitmapToImageSource(System.Drawing.Image bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }        
    }
}
