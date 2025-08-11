using SkiaSharp;
using System.Collections.Generic;

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
        private string FileName { get; set; }
        private string GroupName { get; set; }
        private SKImageInfo SurfaceImageInfo { get; set; }
        private int LineHeight { get; set; }

        private const string NEW_LINE_STRING = "[newline]";

        public CaptionsService(int width, int height)
        {
            SequenceCount = 0;
            GroupCount = 0;
            FileName = "caption";
            GroupName = "group";
            Width = width;
            Height = height;
            LineHeight = 50;//////////////////
            SurfaceImageInfo = new SKImageInfo(Width, Height);
        }

        public void SetInput(List<CaptionInput> input)
        {
            this.Input = input;
        }

        public void ProcessInputs()
        {
            Output = new List<CaptionOutput>();

            foreach (var input in this.Input)
            {
                GroupCount++;
                ConvertCaptionInput(input);
            }

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
                Output.Add(new CaptionOutput
                {
                    Text = item,
                    FileNumber = SequenceCount,
                    GroupNumber = GroupCount,
                });
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
                UpdateXCoord(line, spacer);
            }
        }
        private void UpdateXCoord(List<int> line, int spacer)
        {
            int runningX = spacer;
            for (int i = 0; i < line.Count; i++)
            {
                var el = line[i];
                Output[el].x = runningX;
                runningX += (int)GetTextWidth(Output[el].Text);
            }
        }
        private int GetWidthsSum(List<int> line)
        {
            int sum = 0;

            foreach (var el in line)
            {
                sum += (int)GetTextWidth(Output[el].Text);
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

        public double GetTextWidth(string text)
        {
            ///////////////////////////////////////////////////////////////////////////////////////
            SKPaint paint = new SKPaint
            {
                Typeface = SKTypeface.FromFile("Anton-Regular.ttf"),
                TextSize = 50,
                Color = new SKColor(0xffffffff),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };
            ///////////////////////////////////////////////////////////////////////////////////////
            return paint.MeasureText(text);
        }

        private void CreateImages()
        {
            int currentGroup = 1;
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
            ///////////////////////////////////////
            surface.Canvas.DrawText(output.Text, output.x, 500 + (output.y * LineHeight), new SKPaint
            {
                Typeface = SKTypeface.FromFile("Anton-Regular.ttf"),
                TextSize = 50,
                Color = new SKColor(0xffffffff),
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            });////////////////
            if (output.Render)
            {
                var image = surface.Snapshot();
                var imageBytes = EncodeImage(image);
                SaveImage(imageBytes, output);
            }
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
    }
}
