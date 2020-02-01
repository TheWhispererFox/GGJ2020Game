using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPainter : MonoBehaviour
{
    public MeshRenderer mesh;

    void Start()
    {
        if (DominantColor.imageColor == null)
            return;

        mesh.material.SetColor("_Color", DominantColor.imageColor);
    }
}
