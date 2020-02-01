using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public int Dimension = 10;

    public Color color;
    [Range(0, 1)]
    public float colorStrength;
    public float textureScale;

    public Octave[] Octaves;

    private MeshFilter meshFilter;
    private Mesh mesh;
    public Material material;

    private void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        mesh.uv = GenerateUVs();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.sharedMesh = mesh;
    }

    private Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[(Dimension + 1) * (Dimension + 1)];

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                vertices[index(x, z)] = new Vector3(x, 0, z);
            }
        }

        return vertices;
    }

    private int[] GenerateTriangles()
    {
        int[] triangles = new int[mesh.vertices.Length * 6];

        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                triangles[index(x, z) * 6] = index(x, z);
                triangles[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                triangles[index(x, z) * 6 + 2] = index(x + 1, z);
                triangles[index(x, z) * 6 + 3] = index(x + 1, z + 1);
                triangles[index(x, z) * 6 + 4] = index(x, z);
                triangles[index(x, z) * 6 + 5] = index(x, z + 1);
            }
        }

        return triangles;
    }

    private Vector2[] GenerateUVs()
    {
        Vector2[] uvs = new Vector2[mesh.vertices.Length];

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                Vector2 scaledPos = new Vector2(x % 2, z % 2);
                uvs[index(x, z)] = new Vector2(scaledPos.x <= 1 ? scaledPos.x : 2 - scaledPos.x, scaledPos.y <= 1 ? scaledPos.y : 2 - scaledPos.y);
            }
        }

        return uvs;
    }

    private int index(int x, int z)
    {
        return x * (Dimension + 1) + z;
    }

    private void Update()
    {
        Vector3[] vertices = mesh.vertices;
        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                float y = 0f;

                for (int i = 0; i < Octaves.Length; i++)
                {
                    if (Octaves[i].alternate)
                    {
                        float noiseValue = Mathf.PerlinNoise(
                            (x * Octaves[i].scale.x) / Dimension, 
                            (z * Octaves[i].scale.y) / Dimension) * Mathf.PI * 2;
                        y += (float)Math.Cos(noiseValue + Octaves[i].speed.magnitude * Time.time) * Octaves[i].height;
                    }
                    else
                    {
                        float noiseValue = Mathf.PerlinNoise(
                            (x * Octaves[i].scale.x + Time.time * Octaves[i].speed.x) / Dimension,
                            (z * Octaves[i].scale.y + Time.time * Octaves[i].speed.y) / Dimension) - 0.5f;
                        y += noiseValue * Octaves[i].height;
                    }
                }

                vertices[index(x, z)] = new Vector3(x, y, z);
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        material.SetColorArray("color", new Color[] { color });
        material.SetFloat("colorStrength", colorStrength);
        material.SetFloat("textureScale", textureScale);
    }
}

[System.Serializable]
public struct Octave
{
    public Vector2 speed;
    public Vector2 scale;
    public float height;
    public bool alternate;
}