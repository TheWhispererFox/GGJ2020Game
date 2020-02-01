using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// BUG: Make autoupdate pic color on image change
public class DominantColor : MonoBehaviour
{
    private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>
    {
        {"Red", Color.red},
        {"Orange", new Color(255, 165, 0)},
        {"Yellow", Color.yellow},
        {"Green", Color.green},
        {"White", Color.white},
        {"Grey", Color.grey},
        {"Black", Color.black},
        {"Violet", new Color(138, 43, 226)},
        {"Blue", Color.blue},
        {"Light blue", new Color(135, 206, 250)}
    };
    private Color _color;
    public void Start()
    {
        if (GetComponent<Image>().sprite != null) CalculateDominantColor();
    }

    private void CalculateDominantColor()
    {
        var oftenColor = new ColorThief.ColorThief().GetColor(DuplicateTexture(GetComponent<Image>().sprite.texture))
            .UnityColor;

        (Color, double) nearestColor = (Color.white, 0);

        foreach (var color in _colors)
        {
            var distance = Math.Sqrt(Math.Pow(nearestColor.Item1.r - oftenColor.r, 2) +
                                     Math.Pow(nearestColor.Item1.g - oftenColor.g, 2) +
                                     Math.Pow(nearestColor.Item1.b - oftenColor.b, 2));
            if (nearestColor.Item2 < 255) nearestColor = (color.Value, distance);
        }

        _color = nearestColor.Item1;
    }

    private static Texture2D DuplicateTexture(Texture2D source)
    {
        var renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        var readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}