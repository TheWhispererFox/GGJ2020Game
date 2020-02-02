using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minSpawnDistanceToPlayer;
    [SerializeField] private float maxSpawnDistanceToPlayer;

    [Space]
    [Header("References")]
    [SerializeField] private GameObject jackalPrefab;
    [SerializeField] private List<GameObject> jackals;

    private Transform player;

    private void Awake()
    {
        player = GameManager.Instance.PlayerTransform;
        jackals = new List<GameObject>();
    }

    public void Spawn(int count)
    {
        GameObject temp;
        AIController aiController;
        for (int i = 0; i < count; i++)
        {
            temp = Instantiate(jackalPrefab, GetSpawnPoint(), Quaternion.identity);
            aiController = temp.GetComponent<AIController>();
            aiController.Player = player;
            aiController.MinSpawnDistanceToPlayer = minSpawnDistanceToPlayer;
            aiController.MaxSpawnDistanceToPlayer = maxSpawnDistanceToPlayer;
            jackals.Add(temp);
        }
    }

    public void DeleteAllSpawned()
    {
        foreach (var obj in jackals)
        {
            Destroy(obj);
        }
    }

    private Vector3 GetSpawnPoint()
    {
        float distance = Random.Range(minSpawnDistanceToPlayer, maxSpawnDistanceToPlayer);
        Vector3 spawnPoint = Random.insideUnitSphere;
        if (spawnPoint.z > 0)
            spawnPoint.z *= -1;
        spawnPoint.y = 0;
        spawnPoint = spawnPoint.normalized * distance;
        spawnPoint = player.TransformPoint(spawnPoint);

        NavMeshHit hit;
        NavMesh.SamplePosition(spawnPoint, out hit, 20f, 1);
        return hit.position;
    }
}
