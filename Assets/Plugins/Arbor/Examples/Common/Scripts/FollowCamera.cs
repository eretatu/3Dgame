//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Example
{
	/// <summary>
	/// Behavior of the following camera. (General MonoBehaviour script)
	/// </summary>
	[AddComponentMenu("Arbor/Example/FollowCamera")]
	public sealed class FollowCamera : MonoBehaviour
	{
		/// <summary>
		/// Distance to target
		/// </summary>
		public float distance = 10f;

		/// <summary>
		/// Look offset
		/// </summary>
		public Vector3 lookOffset = Vector3.up * 1.0f;

		/// <summary>
		/// Transform cache
		/// </summary>
		private Transform _Transform;

		/// <summary>
		/// Player Transform cache 
		/// </summary>
		private Transform _PlayerTransform;

		/// <summary>
		/// Direction
		/// </summary>
		private Vector3 _Direction;

		private void Start()
		{
			// Cache Transform
			_Transform = transform;

			// Get Player, Cache Player Transform
			Player player = FindObjectOfType<Player>();
			_PlayerTransform = player.transform;

			// Look at Player position + lookOffset
			_Transform.LookAt(_PlayerTransform.position + lookOffset, Vector3.up);

			// Calculate direction
			_Direction = (_PlayerTransform.position - _Transform.position).normalized;
		}

		// Update is called once per frame
		void LateUpdate()
		{
			// Calculate the position.
			Vector3 playerPosition = _PlayerTransform.position;

			_Transform.position = playerPosition - _Direction * distance;

			// Look at Player position + lookOffset
			_Transform.LookAt(_PlayerTransform.position + lookOffset, Vector3.up);
		}
	}
}