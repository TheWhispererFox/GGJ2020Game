using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    public LayerMask terrainLayer;
    public int lookingDistance;
    public int attackDistance;
    public int damage = 10;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float height = 8.8f;
    public float attackTime;
    public Transform target;
    public Transform mainTarget;
    public List<Transform> targets;

    public UnitState currentState;
    public Animator animator;

    private Path path;
    private Grid grid;

    // Start is called before the first frame update
    void Start()
    {
        targets = new List<Transform>();
        currentState = UnitState.Run; 
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Transform t in targets)
        {
            if (t == null || (t.position - transform.position).sqrMagnitude > lookingDistance * lookingDistance)
            {
                targets.Remove(t);
                targets = targets.OrderBy(x => (x.position - transform.position).sqrMagnitude).ToList();
            }
        }

        if (targets.Count > 0)
        {
            target = targets[0];
        }
        else
        {
            target = mainTarget;
        }

        if ((target.position - transform.position).sqrMagnitude < attackDistance * attackDistance)
        {
            currentState = UnitState.Attack;
            StartCoroutine("Attack");
            StopCoroutine("FollowPath");
            animator.SetBool("Attack", true);
            animator.SetBool("Run", false);
        }
        else
        {
            currentState = UnitState.Run;
            StopCoroutine("Attack");
            StartCoroutine("FollowPath");
            animator.SetBool("Attack", false);
            animator.SetBool("Run", true);
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
                    animator.SetBool("Run", false);
                    currentState = UnitState.Idle;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            targets.Add(other.transform);
            targets = targets.OrderBy(t => (t.position - transform.position).sqrMagnitude).ToList();
        }
    }

    private IEnumerator Attack()
    {
        while (currentState == UnitState.Attack)
        {
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
                    yield return null;
                }
            }
            yield return new WaitForSeconds(attackTime);
        }
    }
}
