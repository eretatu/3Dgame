//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.Serialization;

namespace Arbor.BehaviourTree.Actions
{
#if ARBOR_DOC_JA
	/// <summary>
	/// 時間経過を待つ
	/// </summary>
#else
	/// <summary>
	/// Wait for the passage of time
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Wait")]
	[BuiltInBehaviour]
	public sealed class Wait : ActionBehaviour, INodeBehaviourSerializationCallbackReceiver
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
		/// 待つ秒数
		/// </summary>
#else
		/// <summary>
		/// Number of seconds to wait
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _Seconds = new FlexibleFloat();

		[SerializeField]
		[HideInInspector]
		private int _SerializeVersion = 0;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_TimeType")]
		private TimeType _OldTimeType = TimeType.Normal;

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 1;

		private Timer _Timer = new Timer();
		private float _DurationTime;

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
				return _DurationTime;
			}
		}

		protected override void OnStart()
		{
			_Timer.timeType = _TimeType.value;
			_Timer.Start();
			_DurationTime = _Seconds.value;
		}

		protected override void OnEnd()
		{
			_Timer.Stop();
		}

		protected override void OnExecute()
		{
			if (_Timer.elapsedTime >= _DurationTime)
			{
				FinishExecute(true);
			}
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
					default:
						_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		void INodeBehaviourSerializationCallbackReceiver.OnAfterDeserialize()
		{
			Serialize();
		}

		void INodeBehaviourSerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}
	}
}