using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public int damage;
    public float bulletSpeed;
    public int attackRadius;
    public float attackTime;
    public GameObject bullet;
    public Transform enemy;
    public bool attack;

    private bool isAttacking;
    private Transform target;

    public void Update()
    {
        // TODO: поиск противника
        target = GetNearestEnemy();
        if (target != null && attack)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                StartCoroutine("Attack");
            }
        }
        else
        {
            isAttacking = false;
            StopCoroutine("Attack");
        }
    }

    private Transform GetNearestEnemy()
    {
        return enemy;
    }

    private IEnumerator Attack()
    {
        while (true)
        {
            GameObject o = Instantiate(bullet, transform.position + Vector3.up * 3f, Quaternion.identity);
            Bullet b = o.GetComponent<Bullet>();
            b.damage = damage;
            b.speed = bulletSpeed;
            b.target = target.position;
            yield return new WaitForSeconds(attackTime);
        }
    }
}
