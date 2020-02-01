using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Idle,
    Run,
    RunToTarget,
    Attack
}

public class PathUnit : MonoBehaviour
{
    private const float minPathUpdateTime = .2f;
    private const float pathUpdateMoveThreshold = .5f;

    public LayerMask terrainLayer;
    public Transform target;
    public Vector3 destination;
    public float hp = 100;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 100;
    public float height;
    public float attackDistance;
    public float attackTime;
    public float damage;
    public bool isShooting;
    public Animator animator;
    public AudioSource stepSource;
    public AudioSource hitSource;

    public UnitState currentState;

    private Path path;
    private Grid grid;

    private void Start()
    {
        grid = FindObjectOfType<Grid>();
        animator.SetBool("Attack", false);
        animator.SetBool("Run", false);
        stepSource.volume = AudioController.volume;
    }

    public void Update()
    {
        if (isShooting)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.collider.tag == "Monster")
                    {
                        StartAttacking(hit.collider.transform);
                    }
                    else
                    {
                        StartPath(hit.point);
                    }
                }
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            }
            
        }
    }

    public void StartPath(Vector3 _destination)
    {
        destination = _destination;
        //StartCoroutine(UpdatePath());
        PathRequestManager.RequestPath(new PathRequest(transform.position, destination, OnPathFound));
        currentState = UnitState.Run;
        animator.SetBool("Run", true);
        animator.SetBool("Attack", false);
        StopCoroutine("Attack");
    }

    public void StartAttacking(Transform _target)
    {
        target = _target;
        StartCoroutine("Attack");
    }

    public void StopAll()
    {
        StopAllCoroutines();
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    public void GotDamage(float damage)
    {
        hp -= damage;
    }

    private IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 oldDestination = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - oldDestination).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                oldDestination = target.position;
            }
        }
    }

    private IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;

        while (followingPath && path != null)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {
                if (!stepSource.isPlaying)
                    stepSource.Play();

                if (currentState == UnitState.Run)
                {
                    if ((destination - transform.position).sqrMagnitude < stoppingDst * stoppingDst)
                    {
                        Debug.Log("Finished running");
                        followingPath = false;
                        animator.SetBool("Run", false);
                        currentState = UnitState.Idle;
                        if (stepSource.isPlaying)
                            stepSource.Stop();
                        yield return new WaitForSeconds(10f);
                    }
                }
                else if (currentState == UnitState.RunToTarget)
                {
                    if ((target.position - transform.position).sqrMagnitude < attackDistance * attackDistance)
                    {
                        Debug.Log("Finished running to target");
                        followingPath = false;
                        animator.SetBool("Run", false);
                        currentState = UnitState.Attack;
                        if (stepSource.isPlaying)
                            stepSource.Stop();
                        yield return new WaitForSeconds(10f);
                    }
                }
                
                Vector3 diff = path.lookPoints[pathIndex] - transform.position;
                diff.y = 0;
                transform.forward = diff.normalized;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward * speed, speed * Time.deltaTime);

                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * 100f, Vector3.down);
                if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000f, terrainLayer))
                {
                    transform.position = new Vector3(transform.position.x, hit.point.y + height, transform.position.z);
                }
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log("Started attacking");
        if ((target.position - transform.position).sqrMagnitude > attackDistance * attackDistance)
        {
            Debug.Log("Started running to target");
            animator.SetBool("Attack", false);
            animator.SetBool("Run", true);
            currentState = UnitState.RunToTarget;
            StopCoroutine("UpdatePath");
            StopCoroutine("FollowPath");
            StartCoroutine("UpdatePath");
            StartCoroutine("FollowPath");
            yield return new WaitUntil(() => currentState == UnitState.Attack);
        }

        while (currentState == UnitState.Attack)
        {
            Debug.Log("Attacking target");
            StopCoroutine("UpdatePath");
            StopCoroutine("FollowPath");
            animator.SetBool("Attack", true);

            Vector3 diff = target.position - transform.position;
            diff.y = 0;
            transform.forward = diff.normalized;

            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit, 20f))
            {
                PathUnit unit = hit.collider.GetComponent<PathUnit>();
                if (unit != null)
                {
                    unit.GotDamage(damage);
                    hitSource.Play();
                    if (unit.hp <= 0)
                    {
                        currentState = UnitState.Idle;
                        animator.SetBool("Attack", false);
                        StopAll();
                    }
                    yield return null;
                }
            }
            yield return new WaitForSeconds(attackTime);
        }
    }
}
