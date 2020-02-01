using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Success : MonoBehaviour
{

    bool success;
    Collider col;
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Shacal")
        {
            success = true;
        }
    }
    void Update()
    {       
        if(success) { }
            // stop spawn;
    }
}
