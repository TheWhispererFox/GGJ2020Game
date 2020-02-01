using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NormalizeMode
{
    Local,
    Global
}

public static class Noise
{
    public static float[,] GenerateNoiseMap(
        int mapChunkSize,
        int seed,
        float scale,
        int octaves,
        float persistance,
        float lacunarity,
        Vector2 offset,
        NormalizeMode normalizeMode)
    {
        float[,] map = new float[mapChunkSize, mapChunkSize];

        System.Random r = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxNoiseHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = r.Next(-10000, 10000) + offset.x;
            float offsetY = r.Next(-10000, 10000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxNoiseHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapChunkSize / 2f;
        float halfHeight = mapChunkSize / 2f;

        for (int x = 0; x < mapChunkSize; x++)
        {
            for (int y = 0; y < mapChunkSize; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float noiseValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += noiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                    maxLocalNoiseHeight = noiseHeight;
                else if (noiseHeight < minLocalNoiseHeight)
                    minLocalNoiseHeight = noiseHeight;

                map[x, y] = noiseHeight;
            }
        }

        for (int x = 0; x < mapChunkSize; x++)
        {
            for (int y = 0; y < mapChunkSize; y++)
            {
                if (normalizeMode == NormalizeMode.Local)
                    map[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, map[x, y]);
                else
                    map[x, y] = Mathf.Clamp((map[x, y] + 1) / maxNoiseHeight, 0, int.MaxValue);
            }
        }
        return map;
    }
}
