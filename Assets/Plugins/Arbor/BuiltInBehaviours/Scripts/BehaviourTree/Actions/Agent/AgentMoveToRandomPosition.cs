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
	/// 指定位置を中心とする半径内のランダム移動。
	/// </summary>
#else
	/// <summary>
	/// Random movement within a radius centered on a specified position.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Agent/AgentMoveToRandomPosition")]
	[BuiltInBehaviour]
	public sealed class AgentMoveToRandomPosition : AgentMoveBase
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// Agentの更新タイプ。
		/// </summary>
#else
		/// <summary>
		/// Agent update type.
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(AgentUpdateType))]
		private FlexibleAgentUpdateType _UpdateType = new FlexibleAgentUpdateType(AgentUpdateType.Time);

#if ARBOR_DOC_JA
		/// <summary>
		/// Intervalの時間タイプ。
		/// </summary>
#else
		/// <summary>
		/// Interval time type.
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(TimeType))]
		private FlexibleTimeType _TimeType = new FlexibleTimeType(TimeType.Normal);

#if ARBOR_DOC_JA
		/// <summary>
		/// 移動先を変更するまでのインターバル(秒)。
		/// </summary>
#else
		/// <summary>
		/// Interval (seconds) before moving destination is changed.
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _Interval = new FlexibleFloat();

#if ARBOR_DOC_JA
		/// <summary>
		/// 移動半径
		/// </summary>
#else
		/// <summary>
		/// Moving radius
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _Radius = new FlexibleFloat();

#if ARBOR_DOC_JA
		/// <summary>
		/// 停止する距離
		/// </summary>
#else
		/// <summary>
		/// Distance to stop
		/// </summary>
#endif
		[SerializeField]
		private FlexibleFloat _StoppingDistance = new FlexibleFloat();

#if ARBOR_DOC_JA
		/// <summary>
		/// 移動範囲の中心タイプ
		/// </summary>
#else
		/// <summary>
		/// Center type of movement range
		/// </summary>
#endif
		[SerializeField]
		[Internal.DocumentType(typeof(PatrolCenterType))]
		private FlexiblePatrolCenterType _CenterType = new FlexiblePatrolCenterType(PatrolCenterType.InitialPlacementPosition);

#if ARBOR_DOC_JA
		/// <summary>
		/// 中心Transformの指定(CenterTypeがTransformのみ)
		/// </summary>
#else
		/// <summary>
		/// Specifying the center transform (CenterType is Transform only)
		/// </summary>
#endif
		[SerializeField]
		private FlexibleTransform _CenterTransform = new FlexibleTransform();

#if ARBOR_DOC_JA
		/// <summary>
		/// 中心の指定(CenterTypeがCustomのみ)
		/// </summary>
#else
		/// <summary>
		/// Specify the center (CenterType is Custom only)
		/// </summary>
#endif
		[SerializeField]
		private FlexibleVector3 _CenterPosition = new FlexibleVector3();

		[SerializeField]
		[HideInInspector]
		private int _AgentPatrol_SerializeVersion = 0;

		#region old

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_UpdateType")]
		private AgentUpdateType _OldUpdateType = AgentUpdateType.Time;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_TimeType")]
		private TimeType _OldTimeType = TimeType.Normal;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("_CenterType")]
		private PatrolCenterType _OldCenterType = PatrolCenterType.InitialPlacementPosition;

		#endregion // old

		#endregion // Serialize fields

		private const int kCurrentSerializeVersion = 1;

		Vector3 _ActionStartPosition;

		private bool _IsStartExecuted = false;
		private Timer _Timer = new Timer();
		private float _NextInterval = 0f;
		private bool _IsDone = false;

		private AgentUpdateType _CacheUpdateType;
		
		protected override void OnStart()
		{
			base.OnStart();

			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				_ActionStartPosition = agentController.agentTransform.position;
			}

			_CacheUpdateType = _UpdateType.value;

			_IsStartExecuted = false;
			_IsDone = false;

			_Timer.timeType = _TimeType.value;
			_Timer.Start();

			_NextInterval = 0f;
		}

		void OnUpdateAgent()
		{
			AgentController agentController = cachedAgentController;
			if (agentController != null)
			{
				switch (_CenterType.value)
				{
					case PatrolCenterType.InitialPlacementPosition:
						agentController.MoveToRandomPosition(_Speed.value, _Radius.value, _StoppingDistance.value);
						break;
					case PatrolCenterType.StateStartPosition:
						agentController.MoveToRandomPosition(_ActionStartPosition, _Speed.value, _Radius.value, _StoppingDistance.value);
						break;
					case PatrolCenterType.Transform:
						Transform centerTransform = _CenterTransform.value;
						if (centerTransform != null)
						{
							agentController.MoveToRandomPosition(centerTransform.position, _Speed.value, _Radius.value, _StoppingDistance.value);
						}
						break;
					case PatrolCenterType.Custom:
						agentController.MoveToRandomPosition(_CenterPosition.value, _Speed.value, _Radius.value, _StoppingDistance.value);
						break;
				}
			}
		}

		protected override void OnExecute()
		{
			switch (_CacheUpdateType)
			{
				case AgentUpdateType.Time:
					{
						if (!_IsStartExecuted || _Timer.elapsedTime >= _NextInterval)
						{
							OnUpdateAgent();
							_Timer.Stop();
							_Timer.Start();
							_NextInterval = _Interval.value;

							_IsStartExecuted = true;
						}
					}
					break;
				case AgentUpdateType.Done:
					{
						if (_IsStartExecuted)
						{
							if (!_IsDone)
							{
								AgentController agentController = cachedAgentController;
								if (agentController != null && agentController.isDone)
								{
									_IsDone = true;
									_Timer.Stop();
									_Timer.Start();
									_NextInterval = _Interval.value;
								}
								else
								{
									_IsDone = false;
								}
							}
						}
						else
						{
							OnUpdateAgent();
							_IsDone = false;
							_NextInterval = 0f;

							_IsStartExecuted = true;
						}

						if (_IsDone)
						{
							if (_Timer.elapsedTime >= _NextInterval)
							{
								OnUpdateAgent();
								_Timer.Stop();
								_IsDone = false;
							}
						}
					}
					break;
				case AgentUpdateType.StartOnly:
					if (!_IsStartExecuted)
					{
						OnUpdateAgent();
						_IsStartExecuted = true;
					}
					break;
				case AgentUpdateType.Always:
					{
						OnUpdateAgent();

						_IsStartExecuted = true;
					}
					break;
			}
		}

		protected override void OnEnd()
		{
			base.OnEnd();

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

		protected override void Reset()
		{
			base.Reset();

			_AgentPatrol_SerializeVersion = kCurrentSerializeVersion;
		}

		void SerializeVer1()
		{
			_UpdateType = (FlexibleAgentUpdateType)_OldUpdateType;
			_TimeType = (FlexibleTimeType)_OldTimeType;
			_CenterType = (FlexiblePatrolCenterType)_OldCenterType;
		}

		void Serialize()
		{
			while (_AgentPatrol_SerializeVersion != kCurrentSerializeVersion)
			{
				switch (_AgentPatrol_SerializeVersion)
				{
					case 0:
						SerializeVer1();
						_AgentPatrol_SerializeVersion++;
						break;
					default:
						_AgentPatrol_SerializeVersion = kCurrentSerializeVersion;
						break;
				}
			}
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();

			Serialize();
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			Serialize();
		}
	}
}