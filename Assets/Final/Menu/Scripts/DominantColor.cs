using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// BUG: Make autoupdate pic color on image change
public class DominantColor : MonoBehaviour
{
    public static Color imageColor;

    private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>
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

    public Color _color;
    public Color oftenColor;
    
    public void CalculateDominantColor()
    {
        oftenColor = new ColorThief.ColorThief().GetColor(DuplicateTexture(GetComponent<Image>().sprite.texture))
            .UnityColor;
        Debug.Log($"Start often color: {oftenColor.r}, {oftenColor.g}, {oftenColor.b}");

        float dist = int.MaxValue;
        foreach (var color in _colors)
        {
            float r = oftenColor.r - color.Value.r;
            float g = oftenColor.g - color.Value.g;
            float b = oftenColor.b - color.Value.b;
            float tempDist = Mathf.Sqrt(r * r + g * g + b * b);

            if (tempDist < dist)
            {
                dist = tempDist;
                _color = color.Value;
                Debug.Log($"Changing color to {color.Key}");
            }
            Debug.Log($"Color: {color.Key}, dist: {tempDist}, {oftenColor.r} - {color.Value.r}, {oftenColor.g} - {color.Value.g}, {oftenColor.b} - {color.Value.b}");
        }

		Debug.Log($"Color: {_color.r}, {_color.g}, {_color.b}");
        imageColor = oftenColor;
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