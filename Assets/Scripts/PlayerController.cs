using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] float walkSpeed = 3f;
	[SerializeField] float runSpeed = 6f;
	[SerializeField] float acceleration = 10f;
	[SerializeField] float lookSensitivity = 0.1f;
	[SerializeField] float jumpForce = 4f;

	Rigidbody rigidBody;
	Camera playerCamera;
	Animator animator;

	Vector2 moveInputVector;
	Vector2 lookInputVector;
	float currentSpeed;
	float maxSpeed;

	bool isGrounded = true;

	public Vector3 ClampViewAngle(Transform playerCamera)
	{
		if (playerCamera.up.y < 0f)
		{
			if (playerCamera.forward.y > 0f)
			{
				return new Vector3(270f, playerCamera.localEulerAngles.y, playerCamera.localEulerAngles.z);
			}
			return new Vector3(90f, playerCamera.localEulerAngles.y, playerCamera.localEulerAngles.z);
		}

		return playerCamera.localEulerAngles;
	}

	void Start()
	{
		currentSpeed = 0f;
		maxSpeed = walkSpeed;

		Cursor.lockState = CursorLockMode.Locked;
		animator = GetComponent<Animator>();
		rigidBody = GetComponent<Rigidbody>();
		playerCamera = GetComponentInChildren<Camera>();
	}

	void Update()
	{
		Move();
	}

	private void LateUpdate()
	{
		Look();
	}

	void Move()
	{
		if (isGrounded)
		{
			if (moveInputVector == Vector2.zero)
			{
				animator.SetBool("isWalking", false);
				currentSpeed -= acceleration * Time.deltaTime;
				currentSpeed = Mathf.Max(currentSpeed, 0f);
			}
			else
			{
				animator.SetBool("isWalking", true);
				currentSpeed += acceleration * Time.deltaTime;
				currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
			}
			Vector3 inputDirection = new Vector3(moveInputVector.x, 0, moveInputVector.y);      // input direction
			Vector3 worldSceneDirection = transform.TransformDirection(inputDirection);         // world space direction

			Debug.Log("currentSpeed: " + currentSpeed);

			float velocityX = worldSceneDirection.x * currentSpeed;
			float velocityY = rigidBody.velocity.y;
			float velocityZ = worldSceneDirection.z * currentSpeed;

			rigidBody.velocity = new Vector3(velocityX, velocityY, velocityZ);
		}
	}

	void Look()
	{
		Vector2 delta = lookInputVector * lookSensitivity;
		playerCamera.transform.Rotate(-delta.y, 0f, 0f);
		playerCamera.transform.localEulerAngles = ClampViewAngle(playerCamera.transform);
		Quaternion rigidRot = rigidBody.rotation;
		Quaternion deltaRot = Quaternion.Euler(0f, delta.x, 0f);
		rigidBody.MoveRotation(rigidRot * deltaRot);
	}

	void Jump()
	{
		if (isGrounded)
		{
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpForce, rigidBody.velocity.z);
			isGrounded = false;
		}
	}

	void OnMove(InputValue value)
	{
		moveInputVector = value.Get<Vector2>();
	}

	void OnRun(InputValue value)
	{
		if (value.isPressed)
		{
			animator.SetBool("isRunning", true);
			maxSpeed = runSpeed;
		}
		else
		{
			animator.SetBool("isRunning", false);
			maxSpeed = walkSpeed;
		}

	}

	void OnLook(InputValue value)
	{
		lookInputVector = value.Get<Vector2>();
	}

	void OnJump(InputValue value)
	{
		if (value.isPressed)
		{
			Jump();
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.transform.tag == "Ground")
		{
			isGrounded = true;
		}
	}
}
