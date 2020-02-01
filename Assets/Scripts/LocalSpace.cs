using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSpace : MonoBehaviour
{
    public static LocalSpace globalPos { get; private set; }

    [HideInInspector]
    public Vector3 pos;

    private void Awake()
    {
        globalPos = this;
    }

    public Vector3 RandomSpawnPos()
    {
        pos = new Vector3(Random.Range(-Spawner.radius, Spawner.radius), 0, Random.Range(4, -Spawner.radius));
        return transform.TransformPoint(pos);
    }
    public Vector3 RandomPointInRadius()
    {
        pos = new Vector3(Random.Range(-Spawner.radius, Spawner.radius), 0, Random.Range(Spawner.radius, -Spawner.radius));
        return transform.TransformPoint(pos);
    }
}
