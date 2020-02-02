using System.Collections.Generic;
using UnityEngine;

public class ImageAnalyzer : MonoBehaviour
{
    public static Color mainImageColor;
    public static string mainImageColorName;

    private static readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>
    {
        {"Red", Color.red},
        {"Orange", new Color(255 / 255f, 165 / 255f, 0)},
        {"Yellow", Color.yellow},
        {"Green", Color.green},
        {"White", Color.white},
        {"Grey", Color.grey},
        {"Black", Color.black},
        {"Violet", new Color(138 / 255f, 43 / 255f, 226 / 255f)},
        {"Blue", Color.blue},
        {"Light blue", new Color(135 / 255f, 206 / 255f, 250 / 255f)}
    };

    public static void AnalyzeImage(Texture2D image)
    {
        Color mostOftenColor = new ColorThief.ColorThief().GetColor(image).UnityColor;

        float dist = int.MaxValue;
        foreach (var color in _colors)
        {
            float r = mostOftenColor.r - color.Value.r;
            float g = mostOftenColor.g - color.Value.g;
            float b = mostOftenColor.b - color.Value.b;
            float tempDist = Mathf.Sqrt(r * r + g * g + b * b);

            if (tempDist < dist)
            {
                dist = tempDist;
                mainImageColor = color.Value;
            }
        }
    }
}
