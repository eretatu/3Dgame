//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor.StateMachine.StateBehaviours
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 時間経過後にステートを遷移する。
	/// </summary>
#else
	/// <summary>
	/// It will transition the state after the lapse of time.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Transition/TimeTransition")]
	[BuiltInBehaviour]
	public sealed class TimeTransition : StateBehaviour, INodeBehaviourSerializationCallbackReceiver
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 時間の種類。
		/// </summary>
#else
		/// <summary>
		/// Type of time.
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(TimeType))]
		private FlexibleTimeType _TimeType = new FlexibleTimeType(TimeType.Normal);

#if ARBOR_DOC_JA
		/// <summary>
		/// 遷移するまでの秒数。
		/// </summary>
#else
		/// <summary>
		/// The number of seconds until the transition.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _Seconds = new FlexibleFloat(0.0f);

#if ARBOR_DOC_JA
		/// <summary>
		/// 遷移先ステート。<br />
		/// 遷移メソッド : OnStateUpdate
		/// </summary>
#else
		/// <summary>
		/// Transition destination state.<br />
		/// Transition Method : OnStateUpdate
		/// </summary>
#endif
		[SerializeField]
		private StateLink _NextState = new StateLink();

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[FormerlySerializedAs("_Seconds")]
		[SerializeField]
		[HideInInspector]
		private float _OldSeconds = 0;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_TimeType")]
		private TimeType _OldTimeType = TimeType.Normal;

		#endregion

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 2;

		float _Duration = 0.0f;
		
		private Timer _Timer = new Timer();

		public float elapsedTime
		{
			get
			{
				return _Timer.elapsedTime;
			}
		}

		public float duration
		{
			get
			{
				return _Duration;
			}
		}

		public override void OnStateBegin()
		{
			_Timer.timeType = _TimeType.value;
			_Timer.Start();
			_Duration = _Seconds.value;
		}

		public override void OnStateUpdate()
		{
			if (_Timer.elapsedTime >= _Duration)
			{
				Transition(_NextState);
			}
		}

		public override void OnStateEnd()
		{
			_Timer.Stop();
		}

		protected override void OnGraphPause()
		{
			_Timer.Pause();
		}

		protected override void OnGraphResume()
		{
			_Timer.Resume();
		}

		void Reset()
		{
			_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_Seconds = (FlexibleFloat)_OldSeconds;
		}

		void SerializeVer2()
		{
			_TimeType = (FlexibleTimeType)_OldTimeType;
		}

		void Serialize()
		{
			while (_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_SerializeVersion++;
						break;
					case 1:
						SerializeVer2();
						_SerializeVersion++;
						break;
					default:
						_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		void INodeBehaviourSerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}

		void INodeBehaviourSerializationCallbackReceiver.OnAfterDeserialize()
		{
			Serialize();
		}
	}
}
