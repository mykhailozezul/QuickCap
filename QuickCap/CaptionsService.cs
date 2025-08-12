using SkiaSharp;
using System.Drawing;
using System.Text.RegularExpressions;

namespace QuickCap
{
    public class CaptionsService
    {
        private int Width { get; set; }
        private int Height { get; set; }
        private List<CaptionInput> Input { get; set; }
        private List<CaptionOutput> Output { get; set; }
        private int SequenceCount { get; set; }
        private int GroupCount { get; set; }                
        private SKImageInfo SurfaceImageInfo { get; set; }
        private SKPaint Paint { get; set; }


        public int LineHeight { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public uint TextColor { get; set; }
        public double Position { get; set; }
        public string FileName { get; set; }
        private string GroupName { get; set; }


        private const string NEW_LINE_STRING = "[newline]";

        public CaptionsService(int width, int height, string fontName, List<CaptionInput> input, int fontSize = 50, uint textColor = 0xffffffff, double position = 0.7)
        {
            SequenceCount = 0;
            GroupCount = 0;
            FileName = "caption";
            GroupName = "group";
            Width = width;
            Height = height;
            FontSize = fontSize;
            LineHeight = FontSize;
            FontName = fontName;
            TextColor = textColor;
            Position = position;
            SurfaceImageInfo = new SKImageInfo(Width, Height);
            Paint = GetStyle(FontName, FontSize, TextColor);
            this.Input = input;
        }

        private SKPaint GetStyle(string fontName, int fontSize, uint color)
        {            
            var paint = new SKPaint
            {
                Typeface = SKTypeface.FromFile(fontName),
                TextSize = fontSize,
                Color = new SKColor(color),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
            };
            return paint;
        }

        private SKPaint ConvertToStrokeStyle(SKPaint inputPaint, int strokeWidth, uint strokeColor)
        {
            var paint = new SKPaint
            {
                Typeface = inputPaint.Typeface,
                TextSize = inputPaint.TextSize,
                Color = strokeColor,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                StrokeWidth = strokeWidth,
                Style = SKPaintStyle.Stroke
            };
            return paint;
        }

        public void ProcessInputs()
        {
            Output = new List<CaptionOutput>();

            foreach (var input in this.Input)
            {
                GroupCount++;
                ConvertCaptionInput(input);
            }

            DisableCommandWords();
            CreateTimingMap();
            CreateYCoordinates();
            CreateXCoordinates();
            CreateImages();
        }

        private void ConvertCaptionInput(CaptionInput input)
        {
            foreach (var item in input.Text)
            {
                SequenceCount++;
                SKPaint paint = Paint;
                if (CaptionInput.IsCustom(input))
                {
                    paint = GetCustomPaint(input);
                }

                double position = Position;
                if (CaptionInput.IsCustomPosition(input))
                {
                    position = input.Position;
                }

                int lineHeight = LineHeight;
                if (CaptionInput.IsCustomLineHeight(input))
                {
                    lineHeight = input.LineHeight;
                }

                Output.Add(new CaptionOutput
                {
                    Text = item,
                    FileNumber = SequenceCount,
                    GroupNumber = GroupCount,
                    Paint = paint,
                    Position = position,
                    LineHeight = lineHeight,
                    IsStroke = input.IsStroke,
                    StrokeColor = input.StrokeColor,
                    StrokeWidth = input.StrokeWidth
                });
            }
        }

        private SKPaint GetCustomPaint(CaptionInput input)
        {
            int fontSize = FontSize;
            uint textColor = TextColor;
            string fontName = FontName;

            if (CaptionInput.IsCustomTextSize(input))
            {
                fontSize = input.TextSize;
            }

            if (CaptionInput.IsCustomTextColor(input))
            {
                textColor = input.TextColor;
            }

            if (CaptionInput.IsCustomFontName(input))
            {
                fontName = input.FontName;
            }

            return GetStyle(fontName, fontSize, textColor);
        }

        private void DisableCommandWords()
        {
            foreach (var item in Output)
            {
                if (!IsCommandWord(item.Text))
                {
                    item.Enabled = true;
                }
            }
        }

        private void CreateTimingMap()
        {
            for (int i = 0; i < Output.Count; i++)
            {
                var item = Output[i];
                if (IsWithinLine(i) && IsWithinSameGroup(i))
                {
                    item.Render = false;
                }
                else
                {
                    item.Render = true;
                }
            }
        }

        private bool IsWithinLine(int index)
        {
            if (index + 1 < Output.Count)
            {
                if (Output[index + 1].Text == NEW_LINE_STRING)
                {
                    return true;
                }
                if (Output[index].Text == NEW_LINE_STRING)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private bool IsWithinSameGroup(int index)
        {
            if (index + 1 < Output.Count)
            {
                if (Output[index + 1].GroupNumber == Output[index].GroupNumber)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        private void CreateYCoordinates()
        {
            int currentGroup = 1;
            int lineNumber = 0;

            foreach (var item in Output)
            {
                if (currentGroup == item.GroupNumber)
                {
                    if (item.Text == NEW_LINE_STRING)
                    {
                        lineNumber++;
                    }
                }
                else
                {
                    currentGroup = item.GroupNumber;
                    lineNumber = 0;
                }

                item.y = lineNumber;
            }
        }

        private void CreateXCoordinates()
        {
            var lines = GetLines();
            foreach (var line in lines)
            {
                var sum = GetWidthsSum(line);
                var spacer = (Width - sum) / 2;

                if (spacer < 0)
                {
                    string msg = line.Aggregate("", (acc, item) => acc + Output[item].Text);
                    Warning("\"" + msg + "\" => is outside of bounds");
                }

                UpdateXCoord(line, spacer);
            }
        }

        private void UpdateXCoord(List<int> line, int spacer)
        {
            int runningX = spacer;
            for (int i = 0; i < line.Count; i++)
            {
                var el = line[i];
                var item = Output[el];

                item.x = runningX;
                if (item.Enabled && !string.IsNullOrEmpty(item.Text))
                {                    
                    runningX += GetTextWidth(item.Text, item.Paint) + GetSpaceWidth(item.Paint);                                                              
                }                
            }
        }

        private int GetWidthsSum(List<int> line)
        {
            int sum = 0;

            foreach (var el in line)
            {
                var item = Output[el];
                if (!item.Enabled)
                {
                    continue;
                }
                sum += GetTextWidth(item.Text, item.Paint);
            }

            return sum;
        }

        private List<List<int>> GetLines()
        {
            var result = new List<List<int>>();
            var list = new List<int>();
            result.Add(list);

            int currentGroup = 1;
            int currentLine = 0;

            for (int i = 0; i < Output.Count; i++)
            {
                var item = Output[i];
                if (item.GroupNumber == currentGroup)
                {
                    if (item.y == currentLine)
                    {
                        list.Add(i);
                    }
                    else
                    {
                        list = new List<int>();
                        result.Add(list);
                        currentLine = item.y;
                        list.Add(i);
                    }
                }
                else
                {
                    list = new List<int>();
                    result.Add(list);
                    currentGroup = item.GroupNumber;
                    list.Add(i);
                    currentLine = item.y;
                }
            }

            return result;
        }

        private int GetTextWidth(string text, SKPaint paint)
        {
            return (int)paint.MeasureText(text);
        }

        private int GetSpaceWidth(SKPaint paint)
        {            
            return (int)paint.MeasureText(" ");
        }

        private void CreateImages()
        {
            int currentGroup = 0;
            var surface = SKSurface.Create(SurfaceImageInfo);

            foreach (var item in Output)
            {
                if (currentGroup != item.GroupNumber)
                {
                    currentGroup = item.GroupNumber;
                    surface.Dispose();
                    surface = SKSurface.Create(SurfaceImageInfo);
                }
                ProcessSingleOutput(item, surface);
            }
        }

        private void ProcessSingleOutput(CaptionOutput output, SKSurface surface)
        {
            if (output.Enabled)
            {
                surface.Canvas.DrawText(output.Text, output.x, GetPosition(output.Position) + (output.y * output.LineHeight), output.Paint);

                if (output.IsStroke)
                {
                    var strokePaint = ConvertToStrokeStyle(output.Paint, output.StrokeWidth, output.StrokeColor);
                    surface.Canvas.DrawText(output.Text, output.x, GetPosition(output.Position) + (output.y * output.LineHeight), strokePaint);
                    strokePaint.Dispose();
                }

                if (output.Render)
                {
                    var image = surface.Snapshot();
                    var imageBytes = EncodeImage(image);
                    SaveImage(imageBytes, output);
                }
            }            
        }

        private int GetPosition(double position)
        {
            return (int)(Height * position);
        }

        private byte[] EncodeImage(SKImage image)
        {
            SKData pngImage = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] imageBytes = pngImage.ToArray();
            return imageBytes;
        }

        private void SaveImage(byte[] byteData, CaptionOutput output)
        {
            string root = "output";
            string groupDir = root + "/" + GetGroupName(output.GroupNumber);
            Directory.CreateDirectory(groupDir);            
            File.WriteAllBytes(groupDir + "/" + GetFileName(output.FileNumber), byteData);
        }

        private string GetFileName(int num)
        {
            return FileName + num.ToString("D3") + ".png";
        }

        private string GetGroupName(int num)
        {
            return GroupName + num.ToString("D3");
        }

        private bool IsCommandWord(string text)
        {
            return text.Contains("[newline]") || Regex.IsMatch(text, @"\[wait\(\d*\)\]");
        }

        private void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
