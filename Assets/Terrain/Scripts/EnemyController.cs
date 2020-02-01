using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float spawnTimer;

    public List<GameObject> spawnUnits;
    public List<GameObject> incubators;

    public Transform mainTarget;

    private void Start()
    {
        if (incubators != null && incubators.Count > 0)
        {
            StartCoroutine("Spawn");
        }
    }

    private IEnumerator Spawn()
    {
        while (true)
        {
            // TODO: spawn;

            yield return new WaitForSeconds(spawnTimer);
        }
    }
}
