using SkiaSharp;

namespace QuickCap
{
    public class CaptionOutput
    {
        public string Text { get; set; }
        public bool Render { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int FileNumber { get; set; }
        public int GroupNumber { get; set; }
        public bool Enabled { get; set; }
        public SKPaint Paint { get; set; }
        public double Position { get; set; }
        public int LineHeight { get; set; }
        public bool IsStroke { get; set; }
        public int StrokeWidth { get; set; }
        public uint StrokeColor { get; set; }
    }
}
