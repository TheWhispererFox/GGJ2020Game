using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureBluring : MonoBehaviour
{
    public Material blurMaterial;
    public RenderTexture src;
    public RenderTexture dest;

    private void Update()
    {
        Graphics.Blit(src, dest, blurMaterial);
    }
}
