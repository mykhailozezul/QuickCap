using QuickCap;

int videoWidth = 1080;
int videoHeight = 1980;
string fontName = "Anton-Regular.ttf";

var input = new List<CaptionInput>()
{
    new CaptionInput(new List<string> { "For the longevity", "[newline]", "of ball", "[wait(15)]", "bearings" }, textColor: 0xffff55ff),
    new CaptionInput(new List<string> { "it actually matters", "[newline]", "which ring is fixed", "and which one rotates" }, position: 0.5, textSize: 70),
    new CaptionInput(new List<string> { "Rotation of the outer ring", "[newline]", "causes the balls and separator" }, fontName: "CormorantGaramond-VariableFont_wght.ttf", textColor: 0xff000000),
    new CaptionInput(new List<string> { "to spin more", "[newline]", "compared to when only", "[newline]", "", "the inner ring rotates" }, lineHeight: 80, isStroke: true, strokeWidth: 3, strokeColor: 0xffff0000)
};

var captionsService = new CaptionsService(videoWidth, videoHeight, fontName, input);

captionsService.ProcessInputs();