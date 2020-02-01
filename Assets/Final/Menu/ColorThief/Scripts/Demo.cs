using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public Image dominantColor;
    public RawImage image;
    public Image[] paletteColors;

    public Texture2D texture;

    public InputField urlField;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void Download()
    {
        StartCoroutine(DownloadImage());
    }

    [Obsolete]
    private IEnumerator DownloadImage()
    {
        var www = new WWW(urlField.text);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            texture = www.texture;
            image.texture = texture;
            var ratio = 700f / texture.height;
            var w = texture.width * ratio;
            var h = texture.height * ratio;
            image.rectTransform.sizeDelta = new Vector2(w, h);
            var dominant = new ColorThief.ColorThief();
            dominantColor.color = dominant.GetColor(texture).UnityColor;

            var palette = new ColorThief.ColorThief();
            var colors = palette.GetPalette(texture, paletteColors.Length);
            for (var i = 0; i < colors.Count; i++)
                paletteColors[i].color = colors[i].UnityColor;
        }
        else
        {
            Debug.Log(www.error);
        }
    }
}