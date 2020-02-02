using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public Text text;
    public RawImage image;
    public Button startGameButton;
    public Material material;
    public RenderTexture idealTex;
    public RenderTexture pixelatedTex;

    private void Start()
    {
        startGameButton.enabled = false;
    }

    public void LoadImage()
    {
        string[] openFile = StandaloneFileBrowser.OpenFilePanel(
            "Open file",
            "",
            new[] { new ExtensionFilter("Images", "jpg", "png") },
            false);

        Texture2D tempImage;
        if (TryLoadImage(openFile[0], out tempImage))
        {
            text.enabled = false;
            image.texture = tempImage;
            StartCoroutine(PixelateImageAfterTimer(5));
            ImageAnalyzer.AnalyzeImage(tempImage);
        }
    }

    private bool TryLoadImage(string filename, out Texture2D image)
    {
        image = new Texture2D(1920, 1080);

        if (!File.Exists(filename))
        {
            return false;
        }

        var bytes = File.ReadAllBytes(filename);
        image.LoadImage(bytes);

        material.SetFloat("_PixelAmount", 1000);
        material.SetFloat("_Pixelate", 0);
        Graphics.Blit(image, idealTex, material);

        return true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    private IEnumerator PixelateImageAfterTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        material.SetFloat("_PixelAmount", 100);
        material.SetFloat("_Pixelate", 1);
        Graphics.Blit(idealTex, pixelatedTex, material);
        image.texture = pixelatedTex;
        startGameButton.enabled = true;
    }
}