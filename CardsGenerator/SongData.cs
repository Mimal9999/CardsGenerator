using System.Windows.Controls;
using QRCoder;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Drawing;

public class SongData
{
    // Fields corresponding to JSON data
    public int ID { get; set; }

    [JsonPropertyName("Author")]
    public string Author { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; }

    [JsonPropertyName("URL")]
    public string URL { get; set; }

    [JsonPropertyName("Year")]
    public int Year { get; set; }

    [JsonPropertyName("Views")]
    public long Views { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    // Property to hold the generated QR code
    public System.Drawing.Image? QRCode { get; set; }

    // Default constructor
    public SongData() { }

    // Generates and assigns a QR code based on the URL
    public void GenerateQRCode()
    {
        QRCode = GenerateQRCode(URL);
    }

    // Helper method to generate a QR code image
    private System.Drawing.Image GenerateQRCode(string url)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new QRCode(qrCodeData);

        // Generate QR code with transparent background
        Bitmap qrImage = qrCode.GetGraphic(20, Color.Black, Color.Transparent, true);

        return qrImage;
    }
}

