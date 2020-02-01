using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOffGenerator
{
    public static float[,] GenerateFallOffMap(int size, float fallOffSmoothness, float fallOffOffset)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, fallOffSmoothness, fallOffOffset);
            }
        }

        return map;
    }

    private static float Evaluate(float value, float fallOffSmoothness, float fallOffOffset)
    {
        return Mathf.Pow(value, fallOffSmoothness) / (Mathf.Pow(value, fallOffSmoothness) + Mathf.Pow(1 - value, fallOffSmoothness) * fallOffOffset);
    }
}
