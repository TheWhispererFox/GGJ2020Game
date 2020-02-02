using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasso : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float throwForce;
    [SerializeField] private float maxHoldTime;
    [SerializeField] private float coolDownTime;
    [SerializeField] private float maxRotateSpeed;

    [Space]
    [Header("References")]
    [SerializeField] private Transform lasso;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider physicsCollider;
    [SerializeField] private TrailRenderer rope;
    [SerializeField] private Vector3 rotateOffset;

    private float holdTimer;
    private float coolDownTimer;
    private bool isThrown;
    private bool isRotating;

    private void Start()
    {
        Restore();
    }

    private void Update()
    {
        if (coolDownTimer > 0)
        {
            coolDownTimer -= Time.deltaTime;
            return;
        }
        else if (isThrown)
        {
            Restore();
        }
        
        if (holdTimer >= maxHoldTime)
        {
            Throw();
            return;
        }

        if (Input.GetButton("Fire1"))
        {
            if (!isRotating)
            {
                lasso.localPosition = rotateOffset;
            }
            float t = Mathf.Sqrt(holdTimer / maxHoldTime) / 2 + 0.5f;
            float rotateSpeed = Mathf.Lerp(0, maxRotateSpeed, t);
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            holdTimer += Time.deltaTime;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            Throw();
        }
    }

    private void Restore()
    {
        lasso.SetParent(transform);
        lasso.localPosition = Vector3.zero;
        rope.enabled = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        physicsCollider.enabled = false;
        isThrown = false;
        holdTimer = 0;
    }

    private void Throw()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        physicsCollider.enabled = true;
        rope.enabled = true;
        rope.Clear();
        lasso.localPosition = Vector3.zero;
        lasso.SetParent(null);
        rb.AddForce(Camera.main.transform.forward * throwForce * holdTimer / maxHoldTime, ForceMode.Impulse);
        coolDownTimer = coolDownTime;
        holdTimer = 0;
        isThrown = true;
    }
}