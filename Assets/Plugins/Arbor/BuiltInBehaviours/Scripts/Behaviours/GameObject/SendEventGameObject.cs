//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// イベントを送信する。
	/// </summary>
#else
	/// <summary>
	/// It will send the event.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("GameObject/SendEventGameObject")]
	[BuiltInBehaviour]
	public sealed class SendEventGameObject : StateBehaviour
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// OnStateAwakeによるイベント送信
		/// </summary>
#else
		/// <summary>
		/// Send event by OnStateAwake
		/// </summary>
#endif
		[SerializeField]
		private UnityEvent _OnStateAwake = new UnityEvent();

#if ARBOR_DOC_JA
		/// <summary>
		/// OnStateBeginによるイベント送信
		/// </summary>
#else
		/// <summary>
		/// Send event by OnStateBegin
		/// </summary>
#endif
		[SerializeField]
		[FormerlySerializedAs("_Event")]
		private UnityEvent _OnStateBegin = new UnityEvent();

#if ARBOR_DOC_JA
		/// <summary>
		/// OnStateEndによるイベント送信
		/// </summary>
#else
		/// <summary>
		/// Send event by OnStateEnd
		/// </summary>
#endif
		[SerializeField]
		private UnityEvent _OnStateEnd = new UnityEvent();

		#endregion // Serialize fields

		// Use this for awake state
		public override void OnStateAwake()
		{
			if (_OnStateAwake != null)
			{
				_OnStateAwake.Invoke();
			}
		}

		// Use this for enter state
		public override void OnStateBegin()
		{
			if (_OnStateBegin != null)
			{
				_OnStateBegin.Invoke();
			}
		}

		// Use this for exit state
		public override void OnStateEnd()
		{
			if (_OnStateEnd != null)
			{
				_OnStateEnd.Invoke();
			}
		}
	}
}
