using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardsGenerator;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private List<SongData> songs = new List<SongData>();
    private List<Tuple<Card_Front, Card_Back>> cards = new List<Tuple<Card_Front, Card_Back>>();
    private List<Canvas> canvases = new List<Canvas>();
    private void OpenJsonButton_Click(object sender, RoutedEventArgs e)
    {
        // Open file dialog to select a JSON file
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select a JSON file"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            // Get the JSON file path
            string jsonFilePath = openFileDialog.FileName;
            JsonFilePathTextBox.Text = jsonFilePath;

            // Read the content of the JSON file
            try
            {
                string json = File.ReadAllText(jsonFilePath);

                // Deserialize JSON into a list of SongData objects
                songs = JsonSerializer.Deserialize<List<SongData>>(json);

                if (songs == null) return;

                // Generate QR codes for each song
                foreach (var song in songs)
                {
                    song.GenerateQRCode();
                }
            }
            catch (Exception ex)
            {
                // Show an error message if something goes wrong
                MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    private GradientBrush GetRandomGradient()
    {
        var gradientBrushes = Application.Current.Resources
            .OfType<DictionaryEntry>()
            .Where(entry => entry.Value is GradientBrush && entry.Key.ToString().StartsWith("Gradient"))
            .Select(entry => entry.Value as GradientBrush)
            .ToList();

        if (gradientBrushes.Count > 0)
        {
            Random random = new Random();
            int index = random.Next(gradientBrushes.Count);
            return gradientBrushes[index];
        }

        return new LinearGradientBrush(Colors.White, Colors.Gray, 0);
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        cards.Clear();  // Clear the cards list
        canvases.Clear();  // Clear the canvases list

        // Create front and back cards for each song
        foreach (var song in songs)
        {
            var gradient = GetRandomGradient();
            var frontCard = new Card_Front(song) { BackgroundGradient = gradient };
            frontCard.CardGrid.Background = frontCard.BackgroundGradient;
            var backCard = new Card_Back(song) { BackgroundGradient = gradient };
            backCard.CardGrid.Background = backCard.BackgroundGradient;

            // Add the card pair (front, back) to the list
            cards.Add(new Tuple<Card_Front, Card_Back>(frontCard, backCard));
        }

        // Calculate the number of canvases required
        int numberOfCanvases = (int)Math.Ceiling((double)cards.Count / 15) * 2;

        // Create canvases and set their size (300 DPI)
        for (int i = 0; i < numberOfCanvases; i++)
        {
            Canvas canvas = new Canvas
            {
                Width = 2480,  // A4 width in pixels (300 DPI)
                Height = 3508 // A4 height in pixels (300 DPI)
            };
            canvases.Add(canvas);
        }
        FillCanvasWithCards(canvases, cards);

        string filePath = SaveImagePathTextBox.Text;
        //SaveCanvasesToImages(canvases, filePath); // Uncomment to also save images as PNG
        SaveCanvasesAsPdf(canvases, filePath);
        MessageBox.Show("Done saving files!");
    }
    private void FillCanvasWithCards(List<Canvas> canvases, List<Tuple<Card_Front, Card_Back>> cards)
    {
        const int cardsPerRow = 3; // Number of cards per row
        const int cardsPerColumn = 5; // Number of cards per column
        const int cardsPerCanvas = cardsPerRow * cardsPerColumn;

        const double topBottomMarginCm = 0.3; // Top and bottom margin

        // Convert to pixels at 300 DPI
        const double dpi = 300.0;
        double cardWidthPx = 217; // Card width in pixels
        double cardHeightPx = 217; // Card height in pixels
        double leftRightMarginPx = 47; // Horizontal margin in pixels
        double topBottomMarginPx = topBottomMarginCm * dpi / 2.54; // Vertical margin in pixels

        topBottomMarginPx /= 3.15789473684;
        leftRightMarginPx /= 3.15789473684;

        // Calculate real left margin
        double totalWidth = cardsPerRow * cardWidthPx * 3.15789473684 + 7; // Those values works for my printer. Experiment with them to fit your needs
        double rightMarginPx = (2480 - leftRightMarginPx - totalWidth) / 3.15789473684;

        for (int i = 0; i < canvases.Count; i++)
        {
            var canvas = canvases[i];
            canvas.Children.Clear();

            bool isFrontCanvas = i % 2 == 0; // Even indices for fronts, odd indices for backs
            int startIndex = (i / 2) * cardsPerCanvas;

            for (int j = 0; j < cardsPerCanvas; j++)
            {
                int cardIndex = startIndex + j;
                if (cardIndex >= cards.Count) break;

                // Get the correct card based on the type of canvas (front or back)
                UIElement card;
                if (isFrontCanvas)
                {
                    card = cards[cardIndex].Item1; // Get front card
                }
                else
                {
                    card = cards[cardIndex].Item2; // Get back card
                }

                // Set position for back cards (flip horizontally only)
                if (isFrontCanvas)
                {
                    // Calculate position in grid for front cards (aligned to the right)
                    int row = j / cardsPerRow;
                    int col = j % cardsPerRow;
                    double x = rightMarginPx + col * cardWidthPx; // Align to the right
                    double y = topBottomMarginPx + row * cardHeightPx;

                    Canvas.SetLeft(card, x);
                    Canvas.SetTop(card, y);
                }
                else
                {
                    // Calculate position in grid for back cards, flip horizontally
                    int row = j / cardsPerRow;
                    int col = j % cardsPerRow;

                    // Flip horizontally (preserving the center card)
                    if (col == 0)
                    {
                        col = 2;
                    }
                    else if (col == 2)
                    {
                        col = 0;
                    }

                    double x = leftRightMarginPx + col * cardWidthPx;
                    double y = topBottomMarginPx + row * cardHeightPx;

                    Canvas.SetLeft(card, x);
                    Canvas.SetTop(card, y);
                }

                canvas.Children.Add(card);
            }

            // Add grid lines
            for (int row = 0; row <= cardsPerColumn; row++)
            {
                double y = topBottomMarginPx + row * cardHeightPx;
                var line = new Line
                {
                    X1 = (isFrontCanvas) ? rightMarginPx : leftRightMarginPx,
                    Y1 = y,
                    X2 = (isFrontCanvas) ? rightMarginPx + cardsPerRow * cardWidthPx : leftRightMarginPx + cardsPerRow * cardWidthPx,
                    Y2 = y,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }

            for (int col = 0; col <= cardsPerRow; col++)
            {
                double x = (isFrontCanvas) ? rightMarginPx + col * cardWidthPx : leftRightMarginPx + col * cardWidthPx;
                var line = new Line
                {
                    X1 = x,
                    Y1 = topBottomMarginPx,
                    X2 = x,
                    Y2 = topBottomMarginPx + cardsPerColumn * cardHeightPx,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }
        }
    }
    private void SaveCanvasesToImages(List<Canvas> canvases, string path)
    {
        path = System.IO.Path.GetDirectoryName(path);

        // Create the directory if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // Iterate through all canvases
        for (int i = 0; i < canvases.Count; i++)
        {
            var canvas = canvases[i];

            // Force layout calculation
            canvas.Measure(new System.Windows.Size(canvas.Width, canvas.Height));
            canvas.Arrange(new Rect(0, 0, canvas.Width, canvas.Height));
            canvas.UpdateLayout();

            // Create a RenderTargetBitmap to save the Canvas as an image
            var renderTargetBitmap = new RenderTargetBitmap(
                (int)canvas.Width,
                (int)canvas.Height,
                300, 300, // 300 DPI
                System.Windows.Media.PixelFormats.Pbgra32);

            renderTargetBitmap.Render(canvas);

            // Define the file name for saving
            string fileName = System.IO.Path.Combine(path, $"card_{i + 1}.png");

            // Save to file
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(fileStream);
            }
        }
    }

    public void SaveCanvasesAsPdf(List<Canvas> canvases, string outputPath)
    {
        // Constants for A4 page size in points
        const double A4WidthInPoints = 595.0;  // 210mm at 72 DPI
        const double A4HeightInPoints = 842.0; // 297mm at 72 DPI

        // Create a new PDF document
        using (PdfDocument document = new PdfDocument())
        {
            foreach (var canvas in canvases)
            {
                // Force layout calculation
                canvas.Measure(new System.Windows.Size(canvas.Width, canvas.Height));
                canvas.Arrange(new Rect(0, 0, canvas.Width, canvas.Height));
                canvas.UpdateLayout();

                // Create a RenderTargetBitmap to render the Canvas
                var renderTargetBitmap = new RenderTargetBitmap(
                    (int)canvas.Width,
                    (int)canvas.Height,
                    300, 300, // 300 DPI
                    System.Windows.Media.PixelFormats.Pbgra32);
                renderTargetBitmap.Render(canvas);

                // Convert to PNG format
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    encoder.Save(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position

                    // Load image into XImage
                    XImage image = XImage.FromStream(memoryStream);

                    // Calculate scaling factors for A4 page
                    double scaleX = A4WidthInPoints / image.PixelWidth;
                    double scaleY = A4HeightInPoints / image.PixelHeight;
                    double scale = Math.Min(scaleX, scaleY);

                    double scaledWidth = image.PixelWidth * scale;
                    double scaledHeight = image.PixelHeight * scale;

                    // Create a new PDF page
                    PdfPage page = document.AddPage();
                    page.Width = A4WidthInPoints;
                    page.Height = A4HeightInPoints;

                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    {
                        // Draw the image centered on the page
                        double offsetX = (A4WidthInPoints - scaledWidth) / 2;
                        double offsetY = (A4HeightInPoints - scaledHeight) / 2;
                        gfx.DrawImage(image, offsetX, offsetY, scaledWidth, scaledHeight);
                    }
                }
            }

            // Save the PDF document to file
            document.Save(outputPath);
        }
    }
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        // Create a dialog to select a file path and name
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf", // Filter for PDF files
            DefaultExt = ".pdf", // Default file extension
            FileName = "Cards.pdf" // Default file name
        };

        // If the user selects a path, set it in the TextBox
        if (saveFileDialog.ShowDialog() == true)
        {
            SaveImagePathTextBox.Text = saveFileDialog.FileName;
        }
    }
}