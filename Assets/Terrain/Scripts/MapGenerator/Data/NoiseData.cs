using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public NormalizeMode normalizeMode;

    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public bool randomSeed;

    public Vector2 offset;

    protected override void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;

        base.OnValidate();
    }

    public override void NotifyOfUpdatesValues()
    {
        if (randomSeed)
            seed = Random.Range(0, int.MaxValue);

        base.NotifyOfUpdatesValues();
    }
}
