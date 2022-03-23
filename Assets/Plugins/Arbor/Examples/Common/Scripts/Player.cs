//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Example
{
	using Arbor.Extensions;

	/// <summary>
	/// Behavior of the player character. (General MonoBehaviour script)
	/// </summary>
	[AddComponentMenu("Arbor/Example/Player")]
	[RequireComponent(typeof(CharacterController))]
	public sealed class Player : MonoBehaviour
	{
		/// <summary>
		/// Move speed
		/// </summary>
		public float speed = 10.0f;

		/// <summary>
		/// The value to divide to Animator's speed.
		/// </summary>
		public float divToAnimatorSpeed = 1f;

		/// <summary>
		/// Rotate speed
		/// </summary>
		public float rotateSpeed = 180.0f;

		public Animator animator;

		private int _SpeedHash;

		/// <summary>
		/// Transform cache
		/// </summary>
		private Transform _Transform;

		/// <summary>
		/// CharacterController cache
		/// </summary>
		private CharacterController _Controller;

		/// <summary>
		/// Next rotation
		/// </summary>
		private Quaternion _NextRotation;

		// Use this for initialization
		void Start()
		{
			// Cache Transform and CharacterController
			_Transform = transform;
			this.TryGetComponent<CharacterController>(out _Controller);

			_NextRotation = _Transform.rotation;

			_SpeedHash = Animator.StringToHash("Speed");
		}

		// Update is called once per frame
		void Update()
		{
			// Calculate moving direction.
			Vector3 moveDirection = Vector3.zero;
			if (_Controller.isGrounded)
			{
				Transform mainCameraTransform = Camera.main.transform;
				Vector3 cameraForward = mainCameraTransform.forward;
				cameraForward.y = 0f;
				cameraForward.Normalize();
				Vector3 cameraRight = mainCameraTransform.right;
				cameraRight.y = 0f;
				cameraRight.Normalize();

				Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
				input = input.normalized * Mathf.Clamp01(input.magnitude); // keyboard correction

				moveDirection = cameraForward * input.y + cameraRight * input.x;
				
				if (!Mathf.Approximately(moveDirection.sqrMagnitude, 0))
				{
					_NextRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
				}
			}

			// Calculate velocity
			Vector3 velocity = moveDirection * speed;

			if (animator != null)
			{
				float animatorSpeed = velocity.magnitude;
				if (divToAnimatorSpeed != 0.0f)
				{
					animatorSpeed /= divToAnimatorSpeed;
				}
				animator.SetFloat(_SpeedHash, animatorSpeed);
			}

			velocity += Physics.gravity * Time.deltaTime;

			// Move
			_Controller.Move(velocity * Time.deltaTime);

			// Rotate in moving direction.
			_Transform.rotation = Quaternion.RotateTowards(_Transform.rotation, _NextRotation, rotateSpeed * Time.deltaTime);
		}
	}
}