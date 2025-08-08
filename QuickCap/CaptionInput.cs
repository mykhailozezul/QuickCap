namespace QuickCap
{
    public class CaptionInput
    {
        public List<string> Text { get; set; }
        public uint Color { get; set; }
        public double Position { get; set; }
        public int FontSize { get; set; }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public bool IsAdditiveMode { get; set; }


        public CaptionInput(
            List<string> text, 
            uint color = 0xffffff,
            double position = 4/5,
            int fontSize = 40,
            int startFrame = 0,
            int endFrame = 0,
            bool isAdditiveMode = true
        )
        {
            this.Text = text;
            this.Color = color;
            this.Position = position;
            this.FontSize = fontSize;
            this.StartFrame = startFrame;
            this.EndFrame = endFrame;
            this.IsAdditiveMode = isAdditiveMode;            
        }
    }
}
