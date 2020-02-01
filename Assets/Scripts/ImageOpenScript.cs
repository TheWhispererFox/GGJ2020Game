using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public class ImageOpenScript : MonoBehaviour
{
    public void OpenAndReplaceTexture()
    {
        var canvasRenderer = GetComponent<CanvasRenderer>();
        var image = canvasRenderer.GetComponent<Image>();
        var openFile = StandaloneFileBrowser.OpenFilePanel(
            "Open file",
            "",
            new[] {new ExtensionFilter("Images", "jpg", "png")},
            false);

        
        image.sprite = Sprite.Create(
            LoadImage(openFile[0]),
            new Rect(0, 0, 1280, 720),
            image.sprite.textureRectOffset);
    }

    private static Texture2D LoadImage(string filename)
    {
        if (!File.Exists(filename)) return null;

        var texture = new Texture2D(1920, 1080);
        var bytes = File.ReadAllBytes(filename);
        texture.LoadImage(bytes);
        return texture;
    }
}