using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [Header("Set  Dynimacly")]
    public GameObject prefDog;
    public List<GameObject> dogs;
    public int Lengths;
    public float radiuS = 50f;
    static public float radius;

    void Awake()
    {
        radius = radiuS;
        dogs = new List<GameObject>();
        for(int i = 0; i < Lengths; i++)
        {
            dogs.Add(Instantiate(prefDog, LocalSpace.globalPos.RandomSpawnPos(), Quaternion.identity));

        }
    }
}
