using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{

    GameObject player;
    NavMeshAgent nav;
   [Header("Dont change")]
    public float distToTarget;
    public float distToPlayer;
    public float distFromPlayerToTarg;

    public Vector3 target;


    [Header("Change")]
    public float stoppingDist;



    [Header("Если лоссо задело Шакала, то галачка")]
    public bool wasTouch;
    [Header("Вышел за радиус")]
    public bool moreRadius = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        nav = GetComponent<NavMeshAgent>();

        target = LocalSpace.globalPos.RandomPointInRadius();
        
        nav.SetDestination(target);
    }
    void Update()
    {
        if (wasTouch)
        {
            WasTouchedByLosso();
            return;
        }
        distToPlayer = Vector3.Distance(player.transform.position, transform.position);
        distToTarget = Vector3.Distance(target, transform.position);
        distFromPlayerToTarg = Vector3.Distance(target, player.transform.position);


        if (distToTarget < stoppingDist)
        {
            ChangeTarget();
        }
        if (distFromPlayerToTarg > Spawner.radius )
        {
            ChangeTarget();
        }
    }
    public void WasTouchedByLosso()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "losso")
        {
            wasTouch = true;
        }
    }
    private void ChangeTarget()
    {
            target = LocalSpace.globalPos.RandomPointInRadius();
            nav.SetDestination(target);
    }
}