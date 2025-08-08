using SkiaSharp;

namespace QuickCap
{
    public class CaptionService
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string FontName { get; set; }
        public double Position { get; set; }
        public int FontSize { get; set; }
        public uint Color { get; set; }
        public SKTextAlign TextAlign { get; set; }
        public bool IsAdditiveMode { get; set; }
        public bool IsTimingSequence { get; set; }
        public string OutputFilename { get; set; }
        public string OutputFolderName { get; set; }
        private List<CaptionInput> Input { get; set; }
        public CaptionService(
            int width,
            int height,
            string fontName, 
            double position = 4/5, 
            int fontSize = 40, 
            uint color = 0xffffffff,
            SKTextAlign textAlign = SKTextAlign.Center,
            bool isAdditiveMode = true,
            bool isTimingSequence = false,
            string outPutFilename = "caption",
            string outputFolderName = "group"
        )
        {
            this.Height = height;
            this.Width = width;
            this.FontName = fontName;
            this.Position = position;
            this.FontSize = fontSize;
            this.Color = color;
            this.TextAlign = textAlign;
            this.IsAdditiveMode = isAdditiveMode;
            this.IsTimingSequence = isTimingSequence;
            this.OutputFilename = outPutFilename;
            this.OutputFolderName = outputFolderName;
        }

        public void SetInput(List<CaptionInput> input)
        {
            this.Input = input;
        }

        public void GenerateCaptions()
        {
            var font = GetFontByName(this.FontName);
            var paint = GetPaintStyle(font, this.FontSize, this.Color, this.TextAlign);
            var canvasParams = new SKImageInfo(this.Width, this.Height);

            for (int i = 0; i < this.Input.Count; i++)
            {
                var item = this.Input[i];
                ProcessSingleInput(item, canvasParams, paint, i, this.OutputFilename);
            }
        }

        public void ProcessSingleInput(CaptionInput item, SKImageInfo canvasParams, SKPaint paint, int index, string fileName)
        {
            var image = GetImage(canvasParams, paint, item);
            var imageBytes = EncodeImage(image);
            SaveImage(fileName, imageBytes, index);
        }

        public void SaveImage(string fileName, byte[] byteData, int index)
        {
            string sequenceCounter = index.ToString("D3");//separate
            File.WriteAllBytes(fileName + sequenceCounter + ".png", byteData);
        }

        public byte[] EncodeImage(SKImage image)
        {
            SKData pngImage = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] imageBytes = pngImage.ToArray();
            return imageBytes;
        }

        public SKImage GetImage(SKImageInfo canvasParams, SKPaint paint, CaptionInput input)
        {
            var surface = SKSurface.Create(canvasParams);
            surface.Canvas.DrawText("TEXT", 100, 500, paint);
            var image = surface.Snapshot();
            return image;
        }

        public SKTypeface GetFontByName(string fontName)
        {
            return SKTypeface.FromFile(fontName);
        }

        public SKPaint GetPaintStyle(SKTypeface font, int fontSize, uint textColor, SKTextAlign textAlign)
        {            
            if (font != null)
            {
                var paint = new SKPaint
                {
                    Typeface = font,
                    TextSize = fontSize,
                    Color = new SKColor(textColor),
                    IsAntialias = true,
                    TextAlign = textAlign
                };

                return paint;
            }
            else
            {
                return null;
            }
        }
    }
}
