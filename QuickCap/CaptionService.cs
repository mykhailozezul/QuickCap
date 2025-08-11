using SkiaSharp;
using System;
using System.Reflection;
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
        public bool IsTimingSequence { get; set; }
        public string OutputFilename { get; set; }
        public string OutputFolderName { get; set; }
        private List<CaptionInput> Input { get; set; }
        private static int SequenceCount { get; set; }
        private SKPaint ServiceLevelPaint { get; set; }
        private SKImageInfo ServiceLevelCanvasParams { get; set; }
        public CaptionService(
            int width,
            int height,
            string fontName, 
            double position = 0.7, 
            int fontSize = 60, 
            int lineHeight = 60,
            uint color = 0xffffffff,
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
            this.IsTimingSequence = isTimingSequence;
            this.OutputFilename = outPutFilename;
            this.OutputFolderName = outputFolderName;

            var font = GetFontByName(this.FontName);
            this.ServiceLevelPaint = GetPaintStyle(font, this.FontSize, this.Color);
            this.ServiceLevelCanvasParams = new SKImageInfo(this.Width, this.Height);
        }

        public void SetInput(List<CaptionInput> input)
        {
            this.Input = input;
        }

        public void GenerateCaptions()
        {            
            if (this.ServiceLevelPaint != null)
            {                
                for (int i = 0; i < this.Input.Count; i++)
                {
                    var item = this.Input[i];
                    ProcessSingleInput(item, i);
                }
            }            
        }

        private void ProcessSingleInput(CaptionInput item, int index)
        {
            var surface = SKSurface.Create(this.ServiceLevelCanvasParams);
            int lines = CountLines(item.Text);

            for (int i = 1; i <= lines; i++)
            {
                var line = GetLine(i, item.Text);
                var lineLengths = GetLineLengths(line);
                var lineCoords = GetLineCoordinates(lineLengths);

                DrawLine(surface, i, lineCoords, line, this.ServiceLevelPaint, index);                
            }
            surface.Dispose();
        }

        private void SaveImage(byte[] byteData, int group)
        {
            string root = "output";
            string groupDir = root + "/" + this.OutputFolderName + group.ToString("D3");
            Directory.CreateDirectory(groupDir);
            string sequenceCounter = SequenceCount.ToString("D3");//auto calc TO DO
            File.WriteAllBytes(groupDir + "/" + this.OutputFilename + sequenceCounter + ".png", byteData);
        }

        private byte[] EncodeImage(SKImage image)
        {
            SKData pngImage = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] imageBytes = pngImage.ToArray();
            return imageBytes;
        }

        private void DrawLine(SKSurface surface, int lineNum, List<int> coords, List<string> line, SKPaint paint, int index)
        {
            var timing = GetTimingMap(line);
            int y = GetYCoordinate();
            for (int i = 0; i < line.Count; i++)
            {                
                string text = line[i];
                int x = coords[i];
                surface.Canvas.DrawText(text, x, y + (lineNum * this.LineHeight), paint);

                if (timing[i] == false)
                {
                    SequenceCount++;
                    var image = surface.Snapshot();
                    var imageBytes = EncodeImage(image);
                    SaveImage(imageBytes, index);
                }                
            }
        }

        private List<bool> GetTimingMap(List<string> list)
        {            
            List<int> indexes = new List<int>();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];
                if (string.Equals(item, "[newline]"))
                {
                    indexes.Add(i-1);
                    indexes.Add(i);
                }
            }

            List<bool> result = new List<bool>();

            for (int i = 0; i < list.Count; i++)
            {
                if (indexes.Contains(i))
                {
                    result.Add(true);
                }
                else
                {
                    result.Add(false);
                }
            }

            return result;
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


        private List<double> GetLineLengths(List<string> list)
        {
            List<double> result = new List<double>();
            double spaceWidth = GetSpaceWidth();

            foreach (string item in list)
            {
                result.Add(this.ServiceLevelPaint.MeasureText(item) + spaceWidth);
            }

            return result;
        }

        private double GetSpaceWidth()
        {
            double spaceWidth = this.ServiceLevelPaint.MeasureText(" ");
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

        private SKTypeface GetFontByName(string fontName)
        {
            return SKTypeface.FromFile(fontName);
        }

        private int GetYCoordinate()
        {
            return (int)(this.Height * this.Position);
        }

        private SKPaint GetPaintStyle(SKTypeface font, int fontSize, uint textColor)
        {            
            if (font != null)
            {
                var paint = new SKPaint
                {
                    Typeface = font,
                    TextSize = fontSize,
                    Color = new SKColor(textColor),
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
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
