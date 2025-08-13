# Video Caption PNG Generator
## Description
Video Caption PNG Generator is the simplest console application for generating text captions that can be overlaid on videos.
Input formatted text, and the app outputs PNG overlay files with transparent backgrounds, ready to be placed over video during editing.

## Requirements
* .NET 8

## Example
1. Input preparation

```
// Build your inputs in **data.jon** file
[
  {
    "Text": [ "For the longevity", "[newline]", "of ball", "bearings" ]
  },
  {
    "Text": [ "it actually matters", "[newline]", "which ring is fixed", "and which one rotates" ]
  },
  {
    "Text": [ "Rotation of the outer ring", "[newline]", "causes the balls and separator" ]
  }
]

// Setup configs in config.json
{
    "Width": 1080,
    "Height": 1920,
    "FontName": "Anton-Regular.ttf"
}

//run QuickCap.exe

```
2. Checking results

png files are located in:
```
output/
 ├── group001
 |   ├── caption001.png
 |   └── caption002.png
 ├── group002
 |   ├── caption003.png
 |   └── caption004.png
 ...
```
<img width="108" height="196" alt="caption005" src="https://github.com/user-attachments/assets/d8c0af5a-70ce-4755-b028-fb1f18fbec77" />
<img width="108" height="196" alt="caption003" src="https://github.com/user-attachments/assets/9bffb63c-6b20-49fa-8fbc-766e7e0f2aab" />
<img width="108" height="196" alt="caption009" src="https://github.com/user-attachments/assets/9495470e-8296-4f8b-9b62-4778b7a4ee12" />

## Features

1. **Split text by lines**  
   - Use `[newline]` tokens to force line breaks.
   - Example: ["For the longevity", "[newline]", "of ball"]
   - **Reveal rule:** One segment from each side of `[newline]` is shown together in the same image.  
   - If you need the next line to appear in **another** image instead of in the same reveal, insert an empty string segment (`,"",`) immediately after `[newline]`.
   - Example: ["For the longevity", "[newline]", **""**, "of ball"]

2. **Split text by segments**  
   - Each array item defines a caption segment, displayed in sequence. The application generates one PNG image for each segment.

3. **Styling & typography**
   - Each array element includes its own set of styling properties that apply only to that specific caption. (TextColor, TextSize, FontName, Position, LineHeight, IsStroke, StrokeWidth, StrokeColor)
   - The **config.json** also provides a set of global properties that control the styling of all captions. (LineHeight, FontSize, FontName, TextColor, Position, FileName, GroupName)
   - TextColor via ARGB (`0xff55aa66`)
   - TextSize integer that also sets LineHeight if it is not specified
   - FontName is exact file name of *.ttf font file in the folder
   - Position (0.0 - top, 0.5 - middle, 1.0 - bottom) and all the values in-between
   - LineHeight controls space between lines
   - IsStroke creates outline border around the text, StrokeWidth and StrokeColor take effect only if IsStroke is set to true
   - FileName changes names of png files
   - GroupName changes folders in which png files are located

Example snippet for styling and typography:

```
//**data.json**
[
  {
    "Text": [ "to spin more", "[newline]", "compared to when only", "[newline]", "", "the inner ring rotates" ],
    "LineHeight": 80,
    "IsStroke": true,
    "StrokeColor": 4294901760
  },
  {
    "Text": [ "it actually matters", "[newline]", "which ring is fixed", "and which one rotates" ]
  },
  {
    "Text": [ "Rotation of the outer ring", "[newline]", "causes the balls and separator" ]
  }
]

//**config.json**
{
  "Width": 1080,
  "Height": 1920,
  "FontName": "Anton-Regular.ttf",
  "TextColor": 2863258623,
  "Position": 0.5
}
```
<img width="216" height="396" alt="caption018" src="https://github.com/user-attachments/assets/698dc727-0ee6-4f17-9a41-6d749ba094af" />
<img width="216" height="396" alt="caption017" src="https://github.com/user-attachments/assets/48f87be8-cd6d-4ab3-b594-b6815619ea6d" />

