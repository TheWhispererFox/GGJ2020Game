using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LassoHandler : MonoBehaviour
{
	public float throwForce = 5f;
	public Transform lasso;
	public Vector3 lassoThrowingPosition = new Vector3(0, 1f, 2f);
	public Transform owner;
	public Slider powerMeter;
	public float maxHoldTime = 3f;

	private bool isThrown;
	private Vector3 originalPos;
	private Rigidbody rb;
	private float holdTime;
	private float debounce;

	private void Start()
	{
		originalPos = lasso.localPosition;
		rb = lasso.GetComponent<Rigidbody>() ?? lasso.gameObject.AddComponent<Rigidbody>();
		powerMeter.maxValue = maxHoldTime;
	}

	private void Update()
	{
		if (debounce <= 0)
		{
			debounce = 2f;
		}
		debounce -= Time.deltaTime;
		if (Input.GetButtonDown("Fire1"))
		{
			if (isThrown)
			{
				RestoreLasso();
				return;
			}
		}
		if (holdTime >= maxHoldTime)
		{
			ThrowLasso();
			return;
		}
		if (Input.GetButton("Fire1"))
		{
			if (isThrown && debounce > 0) return;
			holdTime += Time.deltaTime;
			powerMeter.value = holdTime;
		}
		if (Input.GetButtonUp("Fire1"))
		{
			if (isThrown) return;
			ThrowLasso();
		}
	}

	private void ThrowLasso()
	{
		lasso.localPosition = lassoThrowingPosition;
		rb.useGravity = true;
		rb.isKinematic = false;
		lasso.SetParent(null);
		rb.AddForce(throwForce * holdTime * Camera.main.transform.forward, ForceMode.Impulse);
		isThrown = !isThrown;
		holdTime = 0;
		powerMeter.value = 0;
	}

	private void RestoreLasso()
	{
		lasso.SetParent(owner);
		lasso.localPosition = originalPos;
		rb.useGravity = false;
		rb.isKinematic = true;
		isThrown = !isThrown;
	}
}
