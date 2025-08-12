namespace QuickCap
{
    public class CaptionInput
    {
        public List<string> Text { get; set; }
        public uint TextColor { get; set; }
        public int TextSize { get; set; }
        public string FontName { get; set; }
        public double Position { get; set; }
        public int LineHeight { get; set; }
        public bool IsStroke { get; set; }
        public int StrokeWidth { get; set; }
        public uint StrokeColor { get; set; }

        public CaptionInput(List<string> text, uint textColor = 0, int textSize = 0, string fontName = "", double position = 0, int lineHeight = 0, bool isStroke = false, int strokeWidth = 1, uint strokeColor = 0xff000000)
        {
            this.Text = text;
            TextColor = textColor;
            TextSize = textSize;
            FontName = fontName;
            Position = position;
            LineHeight = lineHeight;

            if (IsCustomTextSize(this) && !IsCustomLineHeight(this))
            {
                LineHeight = textSize;
            }

            IsStroke = isStroke;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
        }

        public static bool IsCustom(CaptionInput input)
        {
            return input.TextColor != 0 || input.TextSize != 0 || !string.IsNullOrEmpty(input.FontName);
        }

        public static bool IsCustomTextColor(CaptionInput input)
        {
            return input.TextColor != 0;
        }

        public static bool IsCustomTextSize(CaptionInput input)
        {
            return input.TextSize != 0;
        }

        public static bool IsCustomFontName(CaptionInput input)
        {
            return !string.IsNullOrEmpty(input.FontName);
        }

        public static bool IsCustomPosition(CaptionInput input)
        {
            return input.Position != 0;
        }

        public static bool IsCustomLineHeight(CaptionInput input)
        {
            return input.LineHeight != 0;
        }
    }
}
