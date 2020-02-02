using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public Transform Player { get; set; }
    public float MinSpawnDistanceToPlayer { get; set; }
    public float MaxSpawnDistanceToPlayer { get; set; }

    private float TargetDistanceToPlayer => (Player.position - target).magnitude;
    private float SelfDistanceToPlayer => (Player.position - transform.position).magnitude;
    private float DistanceToTarget => (transform.position - target).magnitude;

    [Header("Settings")]
    [SerializeField] private float minTargetDistanceToPlayer;
    [SerializeField] private float maxTargetDistanceToPlayer;
    [SerializeField] private float maxTotalDistanceToPlayer;
    [SerializeField] private float stoppingDistance;

    [Space]
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private Vector3 target;

    private void Start()
    {
        transform.position = GetNewSpawnPoint();
        target = GetNewTargetPoint();
        agent.SetDestination(target);
        animator.SetInteger("Walk", 1);
    }

    private void Update()
    {
        if (target == null || TargetDistanceToPlayer > maxTargetDistanceToPlayer || DistanceToTarget <= stoppingDistance)
        {
            target = GetNewTargetPoint();
            agent.SetDestination(target);
        }
        if (SelfDistanceToPlayer > maxTotalDistanceToPlayer)
        {
            transform.position = GetNewSpawnPoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lasso"))
        {
            other.GetComponent<Collider>().enabled = false;
            other.GetComponent<Rigidbody>().isKinematic = true;
            agent.isStopped = true;
            agent.enabled = false;
            animator.SetInteger("Walk", 0);
            transform.up = Vector3.right;
            GameManager.Instance.RestorePixelation();
            enabled = false;
        }
    }

    private Vector3 GetNewSpawnPoint()
    {
        float distance = Random.Range(MinSpawnDistanceToPlayer, MaxSpawnDistanceToPlayer);
        Vector3 spawnPoint = Random.insideUnitSphere;
        if (spawnPoint.z > 0)
            spawnPoint.z *= -1;
        spawnPoint.y = 0;
        spawnPoint = spawnPoint.normalized * distance;
        spawnPoint = Player.TransformPoint(spawnPoint);

        NavMeshHit hit;
        NavMesh.SamplePosition(spawnPoint, out hit, 20f, 1);
        return hit.position;
    }

    private Vector3 GetNewTargetPoint()
    {
        float distance = Random.Range(minTargetDistanceToPlayer, maxTargetDistanceToPlayer);
        Vector3 target = Random.insideUnitSphere;
        target.y = 0;
        target = target.normalized * distance;
        target = Player.TransformPoint(target);

        NavMeshHit hit;
        NavMesh.SamplePosition(target, out hit, 20f, 1);
        //Debug.Log($"{hit.position.x}, {hit.position.x}, {hit.position.x}");
        return hit.position;
    }
}
