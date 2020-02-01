using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
	public float gravity = 9.81f;
	public float speed = 5f;
	public float sprintSpeedModifier = 1.5f;
	public float mouseSensitivity = 25f;
	public Transform cameraTransform;
	public float jumpHeight = 10f;
	public LayerMask groundMask;
	public Transform groundCheckObject;
	public float groundDistance = 0.4f;
	public bool lockCursor = false;

	private CharacterController _character;
	private float yRotation = 0f;
	private Vector3 velocity;
	private bool isGrounded;

	private void Start()
	{
		_character = GetComponent<CharacterController>();
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
	private void Update()
	{
		isGrounded = Physics.CheckSphere(groundCheckObject.position, groundDistance, groundMask);

		float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

		float x = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
		float z = Input.GetAxis("Vertical") * Time.deltaTime * speed;

		if (Input.GetKey(KeyCode.LeftShift))
		{
			x *= sprintSpeedModifier;
			z *= sprintSpeedModifier;
		}

		Vector3 move = transform.right * x + transform.forward * z;

		yRotation -= mouseY;
		yRotation = Mathf.Clamp(yRotation, -90f, 90f);

		cameraTransform.transform.localRotation = Quaternion.Euler(yRotation, 0, 0);
		transform.Rotate(Vector3.up * mouseX);
		_character.Move(move * Time.deltaTime * speed);

		// ---------------------------------------------------------------------------

		if (isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}
		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * -gravity);
		}
		velocity.y -= gravity * Time.deltaTime;
		_character.Move(velocity * Time.deltaTime);
	}
}
