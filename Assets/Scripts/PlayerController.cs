using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
	[SerializeField] float walkSpeed = 3f;
	[SerializeField] float runSpeed = 6f;
	[SerializeField] float acceleration = 10f;
	[SerializeField] float lookSensitivity = 0.1f;
	[SerializeField] float jumpForce = 4f;
	[SerializeField] float playerCameraOffset = 1.5f;

	Rigidbody rigidBody;
	Camera playerCamera;
	Animator animator;

	Vector2 moveInputVector;
	Vector2 prevMoveInputVector;
	Vector2 lookInputVector;
	float currentSpeed;
	float maxSpeed;

	bool isGrounded = true;

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.IsOwner)
		{
			playerCamera = Camera.main;
			playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + playerCameraOffset, transform.position.z);
			playerCamera.transform.SetParent(transform);
		}
		else
		{
			gameObject.GetComponent<PlayerController>().enabled = false;
		}
	}

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
			Vector3 inputDirection;
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

			if (moveInputVector == Vector2.zero && rigidBody.velocity != Vector3.zero)
			{
				inputDirection = new Vector3(prevMoveInputVector.x, 0, prevMoveInputVector.y);
			}
			else
			{
				inputDirection = new Vector3(moveInputVector.x, 0, moveInputVector.y);
			}
			Vector3 worldSceneDirection = transform.TransformDirection(inputDirection);
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
			animator.SetBool("isJumping", true);
		}
	}

	void OnMove(InputValue value)
	{
		Vector2 newInputVector = value.Get<Vector2>();
		prevMoveInputVector = moveInputVector;
		moveInputVector = newInputVector;
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
			animator.SetBool("isJumping", false);
			isGrounded = true;
		}
	}
}
