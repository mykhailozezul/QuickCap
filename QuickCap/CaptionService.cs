using SkiaSharp;
using System.Text.RegularExpressions;

namespace QuickCap
{
    public class CaptionService
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string FontName { get; set; }
        public double Position { get; set; }
        public int FontSize { get; set; }
        public int LineHeight { get; set; }
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
            double position = 0.7, 
            int fontSize = 60, 
            int lineHeight = 60,
            uint color = 0xffffffff,
            SKTextAlign textAlign = SKTextAlign.Left,
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
            this.LineHeight = lineHeight;
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

            if (paint != null)
            {                
                for (int i = 0; i < this.Input.Count; i++)
                {
                    var item = this.Input[i];
                    ProcessSingleInput(item, canvasParams, paint, i, this.OutputFilename);
                }
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
            WriteTextOnCanvas(surface.Canvas, input, paint);
            var image = surface.Snapshot();
            return image;
        }

        public void WriteTextOnCanvas(SKCanvas canvas, CaptionInput input, SKPaint paint)
        {
            int lines = CountLines(input.Text);

            for (int i = 1; i <= lines; i++)
            {
                var line = GetLine(i, input.Text);
                var lineLengths = GetLineLengths(line, paint);
                var lineCoords = GetLineCoordinates(lineLengths);
                DrawLine(canvas, i, lineCoords, line, paint);
            }
        }

        private void DrawLine(SKCanvas canvas, int lineNum, List<int> coords, List<string> line, SKPaint paint)
        {
            int y = GetYCoordinate();
            for (int i = 0; i < line.Count; i++)
            {
                string text = line[i];
                int x = coords[i];
                canvas.DrawText(text, x, y + (lineNum * this.LineHeight), paint);
            }
        }

        private List<int> GetLineCoordinates(List<double> list)
        {
            List<int> result = new List<int>();
            double fullLineWidth = list.Aggregate(0.0, (acc, item) => acc + item);
            int spacerWidth = (int)((this.Width - fullLineWidth) / 2);

            result.Add(spacerWidth);
            for (int i = 0; i < list.Count-1; i++)
            {
                int itemWidth = (int)list[i];
                spacerWidth += itemWidth;
                result.Add(spacerWidth);
            }

            return result;
        }


        private List<double> GetLineLengths(List<string> list, SKPaint paint)
        {
            List<double> result = new List<double>();
            double spaceWidth = GetSpaceWidth(paint);

            foreach (string item in list)
            {
                result.Add(paint.MeasureText(item) + spaceWidth);
            }

            return result;
        }
        private double GetSpaceWidth(SKPaint paint)
        {
            double spaceWidth = paint.MeasureText(" ");
            return spaceWidth;
        }
        private List<string> GetLine(int num, List<string> list)
        {
            int counter = 1;
            List<string> result = new List<string>();

            foreach (string item in list)
            {
                if (counter == num && !IsCommandWord(item))
                {
                    result.Add(item);
                }
                if (string.Equals(item, "[newline]"))
                {
                    counter++;
                }
            }

            return result;
        }

        private bool IsCommandWord(string text)
        {
            return text.Contains("[newline]") || Regex.IsMatch(text, @"\[wait\(\d*\)\]");
        }

        private int CountLines(List<string> list)
        {
            int result = 1;

            foreach (string item in list)
            {
                if (string.Equals(item, "[newline]"))
                {
                    result++;
                }
            }

            return result;
        }

        public SKTypeface GetFontByName(string fontName)
        {
            return SKTypeface.FromFile(fontName);
        }

        private int GetYCoordinate()
        {
            return (int)(this.Height * this.Position);
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
