using QuickCap;

int videoWidth = 1080;
int videoHeight = 1980;
string fontName = "Anton-Regular.ttf";

var input = new List<CaptionInput>()
{
    new CaptionInput(new List<string> { "For the longevity", "[newline]", "of ball", "[wait(15)]", "bearings" }),
    new CaptionInput(new List<string> { "it actually matters", "[newline]", "which ring is fixed", "and which one rotates" }),
    new CaptionInput(new List<string> { "Rotation of the outer ring", "[newline]", "causes the balls and separator" }),
    new CaptionInput(new List<string> { "to spin more", "[newline]", "compared to when only", "[newline]", "the inner ring rotates" })
};

var captionService = new CaptionService(videoWidth, videoHeight, fontName);

captionService.SetInput(input);
captionService.GenerateCaptions();