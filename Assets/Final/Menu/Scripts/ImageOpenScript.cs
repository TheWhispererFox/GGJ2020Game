using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageOpenScript : MonoBehaviour
{
	public DominantColor colorAnalyzer;
    public Text text;

    public void OpenAndReplaceTexture()
    {
        var image = GetComponent<Image>();
        var openFile = StandaloneFileBrowser.OpenFilePanel(
            "Open file",
            "",
            new[] {new ExtensionFilter("Images", "jpg", "png")},
            false);

		Texture2D tempImage = LoadImage(openFile[0]);
        if (tempImage == null)
            return;

        text.enabled = false;
		Sprite temp = Sprite.Create(
			tempImage,
			new Rect(0, 0, tempImage.width, tempImage.height),
			new Vector2(0.5f, 0.5f));
		image.sprite = temp;
		colorAnalyzer.CalculateDominantColor();
    }

    private static Texture2D LoadImage(string filename)
    {
        if (!File.Exists(filename)) return null;

        var texture = new Texture2D(1920, 1080);
        var bytes = File.ReadAllBytes(filename);
		texture.LoadImage(bytes);
        return texture;
    }

	public void StartGame()
	{

		SceneManager.LoadScene(1);
	}
}