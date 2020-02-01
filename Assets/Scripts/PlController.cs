using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlController : MonoBehaviour
{
    public GameObject cam;

    Quaternion StartingRotation;

    public float Ver, Hor, Jump;
    public float RotHor, RotVer, Sensivity = 5;

    bool IsGround;

    public float Speed = 5;
    public float JumpSpeed = 50;

    private void Start()
    {
        StartingRotation = transform.rotation;
    }
    void OnCollisionStay(Collision other)
    {
        if(other.gameObject.tag == "Ground")
        {
            IsGround = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            IsGround = false;
        }
    }
    void FixedUpdate()
    {
        RotHor += Input.GetAxis("Mouse X") * Sensivity;
        RotVer += Input.GetAxis("Mouse Y") * Sensivity;

        RotVer = Mathf.Clamp(RotVer, -60, 60);

        Quaternion RotX = Quaternion.AngleAxis(RotHor, Vector3.up);
        Quaternion RotY = Quaternion.AngleAxis(-RotVer, Vector3.right);

        cam.transform.rotation = StartingRotation * transform.rotation * RotX;
        transform.rotation = StartingRotation * RotY;

        if (IsGround)
        {
           Ver = Input.GetAxis("Vertical") * Time.deltaTime * Speed;
           Hor = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
           Jump = Input.GetAxis("Jump") * Time.deltaTime * JumpSpeed;

           GetComponent<Rigidbody>().AddForce(transform.up * Jump, ForceMode.Impulse);
        }

        transform.Translate(new Vector3(Hor, 0 , Ver));
    }
}
