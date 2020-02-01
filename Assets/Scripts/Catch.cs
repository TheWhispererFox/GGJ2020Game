using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Catch : MonoBehaviour
{
    public float speed = 10;
    Vector3 pos;

    public List<GameObject> objs = new List<GameObject>(5);
    public GameObject PrefSph;
    void Start()
    {
        objs = GameObject.FindGameObjectsWithTag("sphere").ToList();
    }
    void Update()
    {

        foreach (var obj in objs)
        {
            pos = objs[0].GetComponent<GameObject>().transform.position;
            obj.transform.position = new Vector3(obj.transform.position.x * speed, obj.transform.position.y, obj.transform.position.z);
            objs.Add(Instantiate(PrefSph, pos, Quaternion.identity));
        }

    }
}
